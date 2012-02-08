using System.Data;

namespace ZocMonLib
{
    public interface IRecordCompare
    {
        /// <summary>
        /// Create the comparison data needed 
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="monitorInfo"></param>
        /// <param name="reduceLevel"></param>
        /// <param name="conn"></param>
        void CalculateComparisons(string configName, MonitorInfo monitorInfo, ReduceLevel reduceLevel, IDbConnection conn);
    }
}