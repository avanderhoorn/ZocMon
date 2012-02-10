using System;
using System.Collections.Generic;
using System.Data;
using ZocMonLib;

namespace ZocMonLib
{
    public class SetupMonitorConfig : ISetupMonitorConfig
    {    
        private readonly ISystemLogger _logger;
        private readonly IStorageCommands _storageCommands;
        private readonly ISetupSystemTables _setupSystemTables;
        private readonly IStorageFactory _storageFactory;
        private readonly IDataCache _cache;

        public SetupMonitorConfig(IStorageCommands storageCommands, ISetupSystemTables setupSystemTables, IDataCache cache, IStorageFactory storageFactory, ISettings settings)
        {
            _storageCommands = storageCommands;
            _setupSystemTables = setupSystemTables;
            _logger = settings.LoggerProvider.CreateLogger(typeof(SetupMonitorConfig));
            _cache = cache;
            _storageFactory = storageFactory; 
        }

        public bool CreateDefaultReduceLevels(MonitorConfig monitorConfig, List<ReduceLevel> reduceLevels, IDbConnection conn = null)
        {
            var shouldClose = false;
            if (conn == null)
            {
                conn = _storageFactory.CreateConnection();
                conn.Open();
                shouldClose = true;
            }

            try
            {
                monitorConfig.ThrowIfNull("monitorConfig");
                reduceLevels.ThrowIfNull("reduceLevels");

                _storageCommands.CreateConfigAndReduceLevels(monitorConfig, reduceLevels, conn);

                // Load the new monitor configurations from the DB into our static Config
                if (!_cache.MonitorConfigs.ContainsKey(monitorConfig.Name))
                    _cache.MonitorConfigs.TryAdd(monitorConfig.Name, monitorConfig); 

                //create tables for these reduce levels
                _setupSystemTables.ValidateAndCreateDataTables(conn);
            }
            catch(Exception e)
            {
                var msg = "Failed to Create Default Reduce Levels for " + monitorConfig.Name;
                _logger.Fatal(msg, e);
                throw new DataException(msg, e);
            }
            finally
            {
                if (shouldClose)
                    conn.Close();
            }
            return true;
        }
    }
}