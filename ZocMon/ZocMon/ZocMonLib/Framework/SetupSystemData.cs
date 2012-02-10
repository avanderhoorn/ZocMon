using System;
using System.Collections.Generic;
using System.Data;
using ZocMonLib;

namespace ZocMonLib
{
    public class SetupSystemData : ISetupSystemData
    {
        private readonly ISystemLogger _logger;
        private readonly IDataCache _cache;
        private readonly IStorageCommandsSetup _storageCommands;
        private readonly ISettings _settings;

        public SetupSystemData(IDataCache cache, IStorageCommandsSetup storageCommands, ISettings settings)
        {
            _logger = settings.LoggerProvider.CreateLogger(typeof(SetupSystemData));
            _cache = cache;
            _storageCommands = storageCommands;
            _settings = settings;
        }

        public void LoadAndValidateData(IDbConnection conn)
        {
            try
            {
                var aggregatedReduceLevels = new Dictionary<string, List<ReduceLevel>>();

                ProcessReduceLevels(aggregatedReduceLevels, conn);

                ProcessMonitorConfigs(aggregatedReduceLevels, conn);
            }
            catch (Exception e)
            {
                _logger.Fatal("Exception swallowed: ", e);
                if (_settings.Debug)
                    throw;
            }
        }

        private void ProcessReduceLevels(Dictionary<string, List<ReduceLevel>> aggregatedReduceLevels, IDbConnection conn)
        {
            //Basically resoving the foreign key reference
            var allReduceLevels = _storageCommands.SelectListAllReduceLevels(conn);
            foreach (var level in allReduceLevels)
            {
                level.AggregationClass = _settings.ReduceMethodProvider.Retrieve(level.AggregationClassName);

                //TODO This is a problem... this really doesn't work!
                var reduceLevelList = aggregatedReduceLevels.SetDefault(level.MonitorConfigName, new List<ReduceLevel>());
                reduceLevelList.Add(level);
            }

            //Validate that the reduce level resolutions are all integral multiples of one another
            foreach (var reduceLevels in aggregatedReduceLevels.Values)
            {
                long lastResolution = 0;
                foreach (var reduceLevel in reduceLevels)
                {
                    if (reduceLevel.Resolution < Constant.MinResolutionForDbWrites)
                        throw new DataException("Will not write to DB at a resolution higher than " + Constant.MinResolutionForDbWrites + " ms; make sure your first reduce table has at least this resolution.  Monitor Config \"" + reduceLevel.MonitorConfigName + "\"");

                    if (Constant.MsPerDay % reduceLevel.Resolution != 0)
                        throw new DataException("Only resolutions that divide evenly into a day are supported");

                    if (lastResolution == 0)
                    {
                        lastResolution = reduceLevel.Resolution;
                        continue;
                    }

                    //if the resolutions are devisable by each other then we have a problem
                    if (((double)reduceLevel.Resolution / (double)lastResolution) != (long)(reduceLevel.Resolution / lastResolution))
                        throw new DataException("Reduce level resolutions must be integral multiples of each other.  Failed for \"" + reduceLevel.MonitorConfigName + "\" levels " + lastResolution + " and " + reduceLevel.Resolution);

                    lastResolution = reduceLevel.Resolution;
                }
            }
        }

        private void ProcessMonitorConfigs(IDictionary<string, List<ReduceLevel>> aggregatedReduceLevels, IDbConnection conn)
        {
            // process monitor configs to, e.g., instantiate class instances from their names, and link-up reduce levels with their configs.
            var monitorConfigList = _storageCommands.SelectListAllMonitorConfigs(conn);
            foreach (var monitorConfig in monitorConfigList)
            {
                // Don't load invalid config names.
                var configName = Support.ValidateConfigName(monitorConfig.Name);
                if (!monitorConfig.Name.Equals(configName))
                    continue;

                List<ReduceLevel> reduceLevelList;
                // There must be at least one "reduce level" to store the source data
                if (!aggregatedReduceLevels.TryGetValue(monitorConfig.Name, out reduceLevelList) || reduceLevelList.Count == 0)
                    throw new DataException("Monitor configuration \"" + monitorConfig.Name + "\" has no reduce levels");
                monitorConfig.ReduceLevels = reduceLevelList;

                // Create the processing instruction class, to create comparison values
                if (monitorConfig.ComparisonCalculatorClassName != null)
                    monitorConfig.ComparisonCalculator = _settings.ProcessingInstructionProvider.Retrieve(monitorConfig.ComparisonCalculatorClassName);

                _cache.MonitorConfigs.TryAdd(monitorConfig.Name, monitorConfig);
            }
        }
    }
}