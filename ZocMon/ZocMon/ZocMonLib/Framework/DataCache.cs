using System.Collections.Concurrent;

namespace ZocMonLib
{
    public class DataCache : IDataCache
    {
        private readonly ConcurrentDictionary<string, MonitorInfo> _monitorInfo;
        private readonly MonitorRecord<double> _empty;
        private readonly ConcurrentDictionary<string, MonitorConfig> _monitorConfigs;

        public DataCache()
        {
            _monitorInfo = new ConcurrentDictionary<string, MonitorInfo>();
            _empty = new MonitorRecord<double>();
            _monitorConfigs = new ConcurrentDictionary<string, MonitorConfig>();
        }

        public ConcurrentDictionary<string, MonitorInfo> MonitorInfo
        {
            get { return _monitorInfo; }
        }

        public MonitorRecord<double> Empty
        {
            get { return _empty; }
        }

        public ConcurrentDictionary<string, MonitorConfig> MonitorConfigs
        {
            get { return _monitorConfigs; }
        }
    }
}