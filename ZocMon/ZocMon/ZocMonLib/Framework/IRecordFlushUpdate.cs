using System.Collections.Generic;
using System.Data;

namespace ZocMonLib
{
    public interface IRecordFlushUpdate
    {
        /// <summary>
        /// Update any existing rows for the given values, and remove them from the values collection.
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="values"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        bool UpdateExisting(string configName, IDictionary<long, MonitorRecord<double>> values, IDbConnection conn, IDbTransaction transaction = null);
    }
}