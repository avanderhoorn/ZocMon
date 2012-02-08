using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ZocMonLib.Extensibility;
using ZocMonLib;

namespace ZocMonLib
{
    public class ConfigSeed : IConfigSeed
    {
        private readonly IDataCache _cache;
        private readonly ISettings _settings;

        public ConfigSeed(IDataCache cache, ISettings settings)
        { 
            _cache = cache;
            _settings = settings;
        }

        /// <summary>
        /// Seed the local data for a monitor configuration.
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="monitorReductionType"></param>
        /// <returns></returns>
        public MonitorInfo Seed(string configName, MonitorReductionType monitorReductionType)
        {
            configName.ThrowIfNull("configName");
            monitorReductionType.ThrowIfNull("monitorReductionType");

            MonitorInfo monitorInfo = null;
            bool tablesCreated = false;

            configName = Support.ValidateConfigName(configName);

            // Look for static cached data/state (i.e. its already been seeded)
            if (!_cache.MonitorInfo.TryGetValue(configName, out monitorInfo))
            {
                // Look for an existing monitor configuration
                MonitorConfig monitorConfig;
                if (!_cache.MonitorConfigs.TryGetValue(configName, out monitorConfig))
                {
                    // Create new monitorconfig record (will get inserted into db during Flush())
                    var aggregationClassName = monitorReductionType == MonitorReductionType.DefaultAccumulate ? "ZocMonLib.ReduceMethods.ReduceMethodAccumulate" : "ZocMonLib.ReduceMethods.ReduceMethodAverage";
                    var aggregationClass = _settings.ReduceMethodProvider.Retrieve(aggregationClassName);
                    monitorConfig = new MonitorConfig
                                        {
                                            Name = configName,
                                            MonitorReductionType = monitorReductionType,
                                            ReduceLevels = new List<ReduceLevel>
                                                               {
                                                                   new ReduceLevel
                                                                       {
                                                                           MonitorConfigName = configName,
                                                                           Resolution = 60 * 1000, 
                                                                           HistoryLength = 7 * 24 * 60 * 60 * 1000,
                                                                           AggregationClassName = aggregationClassName,
                                                                           AggregationClass = aggregationClass
                                                                       },
                                                                   new ReduceLevel
                                                                       {
                                                                           MonitorConfigName = configName,
                                                                           Resolution = 5 * 60 * 1000, 
                                                                           HistoryLength = 7 * 24 * 60 * 60 * 1000,
                                                                           AggregationClassName = aggregationClassName,
                                                                           AggregationClass = aggregationClass
                                                                       },
                                                                   new ReduceLevel
                                                                       {
                                                                           MonitorConfigName = configName,
                                                                           Resolution = 60 * 60 * 1000, 
                                                                           HistoryLength = 7 * 24 * 60 * 60 * 1000,
                                                                           AggregationClassName = aggregationClassName,
                                                                           AggregationClass = aggregationClass
                                                                       },
                                                                   new ReduceLevel
                                                                       {
                                                                           MonitorConfigName = configName,
                                                                           Resolution = 24 * 60 * 60 * 1000, 
                                                                           HistoryLength = 7 * 24 * 60 * 60 * 1000,
                                                                           AggregationClassName = aggregationClassName,
                                                                           AggregationClass = aggregationClass
                                                                       }
                                                               }
                                        };
                }
                else
                {
                    // If this monitor already exists in monitorconfigs, we know the tables have already been created
                    tablesCreated = true;
                }

                // Don't validate custom configs (as they could be anything....)
                if (!MonitorReductionType.Custom.Equals(monitorReductionType))
                {
                    if (!monitorReductionType.Equals(monitorConfig.MonitorReductionType))
                        throw new DataException("Wrong reduction type for monitor \"" + configName + "\"");
                }

                // WTF why only the first
             
                var reduceLevel = monitorConfig.ReduceLevels.First(); 

                monitorInfo = new MonitorInfo
                                  {
                                      MonitorConfig = monitorConfig,
                                      FirstReduceLevel = reduceLevel,
                                      MonitorRecords = new List<MonitorRecord<double>>(), 
                                      TablesCreated = tablesCreated
                                  };

                _cache.MonitorInfo.AddOrUpdate(configName, monitorInfo, (key, oldValue) => monitorInfo);
            }

            return monitorInfo;
        } 
    }
}