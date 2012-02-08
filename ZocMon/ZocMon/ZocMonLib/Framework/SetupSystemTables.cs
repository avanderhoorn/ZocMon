using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ZocMonLib.Extensibility;
using ZocMonLib;

namespace ZocMonLib
{
    public class SetupSystemTables : ISetupSystemTables
    {
        private readonly ISystemLogger _logger;
        private readonly IDataCache _cache;
        private readonly IStorageCommandsSetup _storageCommands;
        private readonly ISettings _settings;

        public SetupSystemTables(IDataCache cache, IStorageCommandsSetup storageFactory, ISettings settings)
        {
            _logger = settings.LoggerProvider.CreateLogger(typeof(SetupSystemTables));
            _cache = cache;
            _storageCommands = storageFactory;
            _settings = settings;
        }

        public void ValidateAndCreateDataTables(IDbConnection conn)
        {
            try
            {
                // This is in case multiple threads try to create new configurations at the same time.
                // The issue isn't concurrent modification of the dictionary, but duplicate attempts to
                // insert the same data in the DB.
                lock (_cache.MonitorConfigs)
                {
                    //get the known table names
                    var tablesKnown = new List<string>();
                    var tablesKnownComparison = new List<string>();
                    var tablesConfigResolution = new Dictionary<string, Tuple<string, long>>();

                    foreach (var monitorConfig in _cache.MonitorConfigs.Values)
                    {
                        // validate/create the tables for reduced data
                        foreach (var reduceLevel in monitorConfig.ReduceLevels)
                        {
                            var tableName = Support.MakeReducedName(monitorConfig.Name, reduceLevel.Resolution);

                            tablesKnown.Add(tableName);
                            if (monitorConfig.ComparisonCalculator != null)
                                tablesKnownComparison.Add(Support.MakeComparisonName(monitorConfig.Name, reduceLevel.Resolution));
                            tablesConfigResolution.Add(tableName, new Tuple<string, long>(monitorConfig.Name, reduceLevel.Resolution));
                        }
                    }

                    //get list of existing tables 
                    var existingTables = _storageCommands.SelectListAllExistingTables(conn);

                    //only generate sql for the ones that we know about that don't already exist
                    var needCreatingTable = tablesKnown.Except(existingTables).ToList();
                    var needCreatingTableComparison = tablesKnownComparison.Except(existingTables).ToList();

                    _storageCommands.BuildTables(needCreatingTable, needCreatingTableComparison, tablesConfigResolution, conn);
                }
            }
            catch (Exception e)
            {
                _logger.Fatal("Exception swallowed: ", e);
                if (_settings.Debug)
                    throw;
            }
        }
    }
}