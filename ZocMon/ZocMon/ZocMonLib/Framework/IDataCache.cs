using System.Collections.Concurrent;

namespace ZocMonLib
{
    public interface IDataCache
    {
        /// <summary>
        /// Monitor Info by config name
        /// </summary>
        ConcurrentDictionary<string, MonitorInfo> MonitorInfo { get; }

        /// <summary>
        /// Stores the config settings for a given event type
        /// </summary>
        ConcurrentDictionary<string, MonitorConfig> MonitorConfigs { get; } 

        //This is BROKEN - not anywhere near safe let alone thread safe
        MonitorRecord<double> Empty { get; }
    }
}