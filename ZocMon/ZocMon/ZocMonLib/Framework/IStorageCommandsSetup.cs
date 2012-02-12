using System;
using System.Collections.Generic;
using System.Data;

namespace ZocMonLib
{
    public interface IStorageCommandsSetup
    {
        /// <summary>
        /// Pulls out all reduce levels 
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        IEnumerable<ReduceLevel> SelectListAllReduceLevels(IDbConnection conn);

        /// <summary>
        /// Pulls out all monitor configs 
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        IEnumerable<MonitorConfig> SelectListAllMonitorConfigs(IDbConnection conn);

        /// <summary>
        /// Pulls out the current tables we have to check if any are missing 
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        IEnumerable<string> SelectListAllExistingTables(IDbConnection conn);

        /// <summary>
        /// Builds the new tables that are required
        /// </summary>
        /// <param name="needCreatingTable"></param>
        /// <param name="needCreatingTableComparison"></param>
        /// <param name="tablesConfigResolution"></param>
        /// <param name="conn"></param>
        void BuildTables(IEnumerable<string> needCreatingTable, IEnumerable<string> needCreatingTableComparison, Dictionary<string, Tuple<string, long>> tablesConfigResolution, IDbConnection conn);
    }
}