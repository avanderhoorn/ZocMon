using ZocMonLib;

namespace ZocMonLib
{
    public interface IConfigSeed
    {
        /// <summary>
        /// Seed the local data for a monitor configuration.
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="monitorReductionType"></param>
        /// <returns></returns>
        MonitorInfo Seed(string configName, MonitorReductionType monitorReductionType);
    }
}