using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using ZocMonLib;

namespace ZocMonLib
{
    public class StorageCommandsSetup : IStorageCommandsSetup
    {
        public IList<ReduceLevel> SelectListAllReduceLevels(IDbConnection conn)
        {
            return ZocMonSqlHelper.CreateListWithConnection<ReduceLevel>(conn, StorageCommandsSql.ReduceLevelSql);
        }

        public IList<MonitorConfig> SelectListAllMonitorConfigs(IDbConnection conn)
        {
            return ZocMonSqlHelper.CreateListWithConnection<MonitorConfig>(conn, StorageCommandsSql.MonitorConfigSql);
        }

        public IList<string> SelectListAllExistingTables(IDbConnection conn)
        {
            return ZocMonSqlHelper.CreateListWithConnection<string>(conn, StorageCommandsSql.GetExistingTables);
        }

        public void BuildTables(IEnumerable<string> needCreatingTable, IEnumerable<string> needCreatingTableComparison, Dictionary<string, Tuple<string, long>> tablesConfigResolution, IDbConnection conn)
        {
            var builder = new StringBuilder();
            //add to sql string for reduced data table creation
            foreach (var needsCreating in needCreatingTable)
                builder.AppendFormat(StorageCommandsSql.DataTableCreateFormat, needsCreating, tablesConfigResolution[needsCreating].Item1, tablesConfigResolution[needsCreating].Item2);

            //add to sql string for comparison table creation
            foreach (var needsCreating in needCreatingTableComparison)
                builder.AppendFormat(StorageCommandsSql.ComparisonTableCreateFormat, needsCreating);

            ZocMonSqlHelper.ExecuteNonQueryWithConnection(conn, builder.ToString());
        }
    }
}