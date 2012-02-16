using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using ZocMonLib;

namespace ZocMonLib
{
    public class StorageCommands : IStorageCommands
    {
        //TODO: Instead of passing in tableName pass in configName?

        private readonly ISystemLogger _logger;
        private readonly IStorageFactory _dbFactory;

        public StorageCommands(IStorageFactory dbFactory, ISettings settings)
        {
            _logger = settings.LoggerProvider.CreateLogger(typeof(StorageCommands));
            _dbFactory = dbFactory;
        }
         
        public void Insert<T>(string tableName, IEnumerable<T> values, IDbConnection conn, IDbTransaction transaction = null)
        {
            tableName.ThrowIfNull("tableName");
            values.ThrowIfNull("values");
            conn.ThrowIfNull("conn");

            const string sqlInsertFormat = "insert into [{0}] ({1}) values ({2})";

            var valueType = typeof(T); 
            var properties = valueType.GetProperties();
            var parameters = new List<IDbDataParameter>(properties.Length);
            var valuesList = new StringBuilder();
            var parametersList = new StringBuilder();
            for (var i = 0; i < properties.Length; ++i)
            {
                parameters[i] = _dbFactory.CreateParameter(properties[i].Name, GetDbType(properties[i].PropertyType));
                valuesList.Append(properties[i].Name).Append(", ");
                parametersList.Append("@").Append(properties[i].Name).Append(", ");
            }
            valuesList.Remove(valuesList.Length - StorageCommandsSql.Comma.Length, StorageCommandsSql.Comma.Length);
            parametersList.Remove(parametersList.Length - StorageCommandsSql.Comma.Length, StorageCommandsSql.Comma.Length);

            var sql = string.Format(sqlInsertFormat, tableName, valuesList, parametersList);

            Execute(sql, parameters, properties, values, conn, transaction);
        }
         
        public void Update<T>(string tableName, IEnumerable<T> values, IDbConnection conn, IDbTransaction transaction = null) where T : ITimeStamped
        {
            tableName.ThrowIfNull("tableName");
            values.ThrowIfNull("values");
            conn.ThrowIfNull("conn");

            const string sqlUpdateFormat = "update [{0}] set {1} where " + StorageCommandsSql.TimeStampParameterName + " = " + StorageCommandsSql.TimeStampParameter;

            var valueType = typeof(T); 
            var properties = valueType.GetProperties();
            var parameters = new List<IDbDataParameter>(properties.Length);
            var setList = new StringBuilder();
            for (var i = 0; i < properties.Length; ++i)
            {
                parameters[i] = _dbFactory.CreateParameter(properties[i].Name, GetDbType(properties[i].PropertyType));
                if (!StorageCommandsSql.TimeStampParameterName.Equals(properties[i].Name))
                    setList.Append(properties[i].Name).Append(" = @").Append(properties[i].Name).Append(StorageCommandsSql.Comma);

            }
            setList.Remove(setList.Length - StorageCommandsSql.Comma.Length, StorageCommandsSql.Comma.Length);

            var sql = string.Format(sqlUpdateFormat, tableName, setList);

            Execute(sql, parameters, properties, values, conn, transaction);
        }

        public bool UpdateIfExists(string tableName, MonitorRecord<double> update, bool knownToExist, IDbConnection conn)
        {
            var ret = false;
            var rowCount = 0;
            var sql = "";
            if (!knownToExist)
            {
                const string rowExistsFormat = "select count(1) from [{0}] where TimeStamp = '{1}'";
                sql = string.Format(rowExistsFormat, tableName, update.TimeStamp);
                rowCount = DatabaseSqlHelper.ExecuteScalarWithConnection(conn, sql);
            }
            if (knownToExist || rowCount == 1)
            {
                var updateSql = string.Format(StorageCommandsSql.UpdateFormat, tableName);
                var result = DatabaseSqlHelper.ExecuteNonQueryWithConnection(conn, updateSql, update);
                if (result != 1)
                    throw new DataException("Expected to update 1 row, but updated " + result + " for sql: \"" + updateSql + "\"");
                ret = true;
            }
            else if (rowCount > 1)
                throw new DataException("Too many rows (" + rowCount + ") for: \"" + sql + "\" trying to insert: " + update);
            else
            {
                // In principal, this is ok; but in practice, it should hardly ever happen, so I want to know about it
                _logger.Warn("Zero Rows in UpdateIfExists for \"" + sql + "\"");
            }

            return ret;
        }

        public void Flush(string tableName, IEnumerable<MonitorRecord<double>> updateList, IDbConnection conn)
        {
            tableName.ThrowIfNull("configName");
            updateList.ThrowIfNull("updateList");
            conn.ThrowIfNull("conn");

            var insertSql = string.Format(StorageCommandsSql.InsertFormat, tableName);

            var transaction = conn.BeginTransaction();
            try
            {
                var command = _dbFactory.CreateCommand(insertSql, conn, transaction);

                var timeStampParameter = _dbFactory.CreateParameter(StorageCommandsSql.TimeStampParameter, DbType.DateTime);
                var valueParameter = _dbFactory.CreateParameter(StorageCommandsSql.ValueParameter, DbType.Double);
                var numberParameter = _dbFactory.CreateParameter(StorageCommandsSql.NumberParameter, DbType.Int32);
                var sumParameter = _dbFactory.CreateParameter(StorageCommandsSql.IntervalSumParameter, DbType.Double);
                var sumOfSquaresParameter = _dbFactory.CreateParameter(StorageCommandsSql.IntervalSumOfSquaresParameter, DbType.Double);
                command.Parameters.Add(timeStampParameter);
                command.Parameters.Add(valueParameter);
                command.Parameters.Add(numberParameter);
                command.Parameters.Add(sumParameter);
                command.Parameters.Add(sumOfSquaresParameter);
                command.Prepare();

                foreach (MonitorRecord<double> update in updateList)
                {
                    timeStampParameter.Value = update.TimeStamp;
                    valueParameter.Value = update.Value;
                    numberParameter.Value = update.Number;
                    sumParameter.Value = update.IntervalSum;
                    sumOfSquaresParameter.Value = update.IntervalSumOfSquares;
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw new DataException("Falied to save data for \"" + tableName + "\"", e);
            }
        }
        
        public IEnumerable<MonitorRecord<double>> SelectListForUpdateExisting(string tableName, DateTime timeStamp, IDbConnection conn, IDbTransaction transaction = null)
        {
            const string getExistingFormat = "select * from [{0}] where {1} >= '{2}' order by {1}";
            var getExistingSql = string.Format(getExistingFormat, tableName, StorageCommandsSql.TimeStampParameterName, timeStamp);

            return DatabaseSqlHelper.CreateListWithConnection<MonitorRecord<double>>(conn, getExistingSql, transaction);
        }

        public IEnumerable<MonitorRecord<double>> SelectListForLastReduced(string tableName, IDbConnection conn)
        {
            var loadLastReducedUpdateSql = string.Format(StorageCommandsSql.LoadLastUpdateFormat, tableName);

            return DatabaseSqlHelper.CreateListWithConnection<MonitorRecord<double>>(conn, loadLastReducedUpdateSql);
        }

        public StorageLastReduced RetrieveLastReducedData(string tableName, long resolution, IDbConnection conn)
        {
            var lastReducedList = SelectListForLastReduced(tableName, conn);

            var lastReducedListCount = lastReducedList.Count();
            var lastReducedUpdate = lastReducedList.FirstOrDefault();
            var lastReductionTime = Constant.MinDbDateTime;

            //0 means we have no reduced data yet and 1 means we've loaded the last reduced update, many means we have a problem
            if (lastReducedListCount > 0)   
            { 
                lastReductionTime = lastReducedUpdate.TimeStamp - TimeSpan.FromMilliseconds((long)(resolution / 2));

                if (lastReducedListCount > 1)
                {
                    //No primary key on these table, so have to delete all with that timestamp and insert one back in 
                    var sqlDelete = "DELETE FROM " + ParseTableName(tableName) + " WHERE TimeStamp = @time";
                    DatabaseSqlHelper.ExecuteNonQueryWithConnection(conn, sqlDelete, new { time = lastReducedUpdate.TimeStamp });

                    DatabaseSqlHelper.InsertRecordWithConnection(conn, lastReducedUpdate, tableName);

                    _logger.Warn("Expected 0 or 1 updates, but got: " + lastReducedListCount + " for \"" + tableName + "\"");
                } 
            }

            return new StorageLastReduced { Record = lastReducedUpdate, Time = lastReductionTime };
        }

        public IEnumerable<MonitorRecord<double>> SelectListRequiringReduction(string tableName, bool hasTargetReducedRecord, DateTime lastReductionTime, IDbConnection conn)
        {
            var whereClause = !hasTargetReducedRecord ? "" : string.Format(StorageCommandsSql.LoadDataWhereFormat, lastReductionTime);
            var loadToBeReducedSql = string.Format(StorageCommandsSql.LoadDataFormat, tableName, whereClause);

            return DatabaseSqlHelper.CreateListWithConnection<MonitorRecord<double>>(conn, loadToBeReducedSql);
        }

        public void ClearReducedData(string configName, DateTime reducedTo, ReduceLevel reduceLevel, IDbConnection conn)
        {
            var history = DateTime.Now - TimeSpan.FromMilliseconds(reduceLevel.HistoryLength);
            var deleteBeforeDate = reducedTo < history ? reducedTo : history;

            var deleteSql = string.Format(StorageCommandsSql.DeleteDataFormat, Support.MakeReducedName(configName, reduceLevel.Resolution), deleteBeforeDate);

            DatabaseSqlHelper.ExecuteNonQueryWithConnection(conn, deleteSql);
        }

        public void PergeDuplicateReducedData(string tableName, IDbConnection conn)
        {
            var dupCheckSql = @"select rl.TimeStamp as TimeStamp, count(1) as Count from [" + tableName + "] rl group by TimeStamp having count(1) > 1";
            var dups = DatabaseSqlHelper.CreateListWithConnection<DupInfo>(conn, dupCheckSql);

            if (dups.Any())
            {
                var msg = "";
                foreach (var dupInfo in dups)
                    msg += "TimeStamp: " + dupInfo.TimeStamp.ToString("yyyyMMdd HH:mm:ss") + "; Count: " + dupInfo.Count + Environment.NewLine;

                throw new Exception("Duplicates found for " + tableName + ": " + msg);
            }
        }


        public IEnumerable<MonitorRecord<double>> SelectListLastComparisonData(string comparisonTableName, IDbConnection conn)
        {
            var loadLastComparisonSql = string.Format(StorageCommandsSql.LoadLastComparisonFormat, comparisonTableName);
            var lastComparison = DatabaseSqlHelper.CreateListWithConnection<MonitorRecord<double>>(conn, loadLastComparisonSql).SingleOrDefault();

            return new List<MonitorRecord<double>> {lastComparison};
        }

        public IEnumerable<MonitorRecord<double>> SelectListNeedingToBeReduced(string reducedTableName, bool hasLastPrediction, DateTime reducedDataStartTime, IDbConnection conn)
        {
            var whereClause = (!hasLastPrediction ? "" : string.Format(StorageCommandsSql.LoadDataWhereFormat, reducedDataStartTime));
            var reducedSql = string.Format(StorageCommandsSql.LoadComparisonFormat, reducedTableName, whereClause);

            return DatabaseSqlHelper.CreateListWithConnection<MonitorRecord<double>>(conn, reducedSql);
        }

        public void CreateConfigAndReduceLevels(MonitorConfig monitorConfig, IEnumerable<ReduceLevel> reduceLevels, IDbConnection conn)
        {
            using (var transaction = conn.BeginTransaction())
            {
                //The "if this doesn't already exist" check is inside the query string
                DatabaseSqlHelper.ExecuteNonQueryWithConnection(conn, StorageCommandsSql.MonitorConfigInsert,
                    new
                    {
                        name = monitorConfig.Name,
                        monitorReductionType = monitorConfig.MonitorReductionType
                    }, transaction);

                foreach (var reduceLevel in reduceLevels)
                {
                    DatabaseSqlHelper.ExecuteNonQueryWithConnection(conn, StorageCommandsSql.ReduceLevelInsert,
                        new
                        {
                            name = reduceLevel.MonitorConfigName,
                            res = reduceLevel.Resolution,
                            historyLength = reduceLevel.HistoryLength,
                            aggregationClassName = reduceLevel.AggregationClassName
                        }, transaction);
                }

                transaction.Commit();
            }
        }

        #region Support Methods

        /// <summary>
        /// Execute parameterized SQL, in a transaction, optionally specified by the caller.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="properties"></param>
        /// <param name="values"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        private void Execute<T>(string sql, IList<IDbDataParameter> parameters, PropertyInfo[] properties, IEnumerable<T> values, IDbConnection conn, IDbTransaction transaction = null)
        {
            sql.ThrowIfNull("sql");
            parameters.ThrowIfNull("parameters");
            properties.ThrowIfNull("properties");
            values.ThrowIfNull("values");
            conn.ThrowIfNull("conn");

            bool closeTran = transaction == null;
            transaction = transaction ?? conn.BeginTransaction();
            try
            {
                IDbCommand command = _dbFactory.CreateCommand(sql, conn, transaction);
                foreach (DbParameter sqlParameter in parameters)
                {
                    command.Parameters.Add(sqlParameter);
                }

                command.Prepare();

                foreach (T value in values)
                {
                    for (int i = 0; i < properties.Length; ++i)
                        parameters[i].Value = properties[i].GetValue(value, null);
                    command.ExecuteNonQuery();
                }
                if (closeTran) transaction.Commit();
            }
            catch (Exception e)
            {
                if (closeTran)
                    transaction.Rollback();
                throw new DataException("Falied to save data for \"" + sql + "\"", e);
            }
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/ms131092.aspx for more.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private DbType GetDbType(Type type)
        {
            type.ThrowIfNull("type");
            DbType ret;
            if (typeof(string).Equals(type))
            {
                //ret = SqlDbType.VarChar;
                ret = DbType.String;
            }
            else if (typeof(DateTime).Equals(type))
            {
                //ret = SqlDbType.DateTime;
                ret = DbType.DateTime;
            }
            else if (typeof(float).Equals(type) || typeof(double).Equals(type))
            {
                //ret = SqlDbType.Float;
                ret = DbType.Double;
            }
            else if (typeof(int).Equals(type))
            {
                //ret = SqlDbType.Int; 
                ret = DbType.Int32;
            }
            else if (typeof(long).Equals(type))
            {
                //ret = SqlDbType.BigInt;
                ret = DbType.Int64;
            }
            else if (typeof(TimeSpan).Equals(type))
            {
                //ret = SqlDbType.Time;
                ret = DbType.Time;
            }
            else
            {
                throw new SqlTypeException("Unknow type translation for \"" + type + "\"");
            }
            return ret;
        }

        private string ParseTableName(string tableName)
        {
            var table = tableName;
            if (char.IsNumber(tableName[0]))
                table = "[" + tableName + "]";

            return table;
        }

        #endregion

        #region Support Types
        
        private class DupInfo
        {
            public DateTime TimeStamp { get; set; }
            public int Count { get; set; }
        }

        #endregion
    }
}