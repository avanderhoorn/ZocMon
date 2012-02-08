using System.Collections.Generic;
using System.Data;

namespace ZocMonLib
{
    public interface ISetupMonitorConfig
    {
        /// <summary>
        /// Define monitor configurations
        /// </summary>
        bool CreateDefaultReduceLevels(MonitorConfig monitorConfig, List<ReduceLevel> reduceLevels, IDbConnection conn);
    }
}