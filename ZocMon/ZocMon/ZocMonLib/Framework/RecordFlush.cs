using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ZocMonLib.Extensibility;
using ZocMonLib.Extension;
using ZocMonLib;

namespace ZocMonLib
{
    public class RecordFlush : IRecordFlush
    {
        private readonly ISystemLogger _logger;
        private readonly ISetupMonitorConfig _setupMonitorConfig;
        private readonly IDataCache _cache;
        private readonly IStorageCommands _storageCommands;
        private readonly IRecordFlushUpdate _logic;
        private readonly IStorageFactory _dbFactory;
        private readonly ISettings _settings;

        public RecordFlush(ISetupMonitorConfig setupMonitorConfig, IDataCache cache, IStorageCommands storageCommands, IRecordFlushUpdate logic, IStorageFactory dbFactory, ISettings settings)
        {
            _logger = settings.LoggerProvider.CreateLogger(typeof(RecordFlush));
            _setupMonitorConfig = setupMonitorConfig;
            _cache = cache;
            _storageCommands = storageCommands;
            _logic = logic;
            _dbFactory = dbFactory;
            _settings = settings;
        }

        /// <summary>
        /// Flush all data accumulated thus far.
        /// </summary>
        public void FlushAll()
        {
            try
            {
                using (var conn = _dbFactory.CreateConnection())
                {
                    conn.Open();
                    foreach (var configName in _cache.MonitorInfo.Keys) 
                        Flush(configName, conn); 
                }
            }
            catch (Exception e)
            {
                _logger.Fatal("Exception swallowed: ", e);
                if (_settings.Debug) 
                    throw;
            }
        }

        /// <summary>
        /// Flush data for the lowest reduce resolution for the given configuration.
        /// (The rest is only written on Reduce.)
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="conn"></param>
        public void Flush(string configName, IDbConnection conn = null)
        {
            try
            {
                configName = Support.ValidateConfigName(configName);

                MonitorInfo monitorInfo;
                if (!_cache.MonitorInfo.TryGetValue(configName, out monitorInfo)) 
                    throw new ArgumentException("No updates for monitor \"" + configName + "\"");

                if (!monitorInfo.TablesCreated)
                {
                    if (_setupMonitorConfig.CreateDefaultReduceLevels(monitorInfo.MonitorConfig, monitorInfo.MonitorConfig.ReduceLevels, conn)) 
                        monitorInfo.TablesCreated = true;
                }

                var reduceLevel = GetFirstReduceLevel(configName);
                var updateList = monitorInfo.MonitorRecords;
                var updateListClone = CloneRecordList(updateList);

                //If we have records to work with
                if (updateListClone.Count > 0)
                {
                    var tableName = Support.MakeReducedName(configName, reduceLevel.Resolution);
                    var sortedUpdateListClone = ConvertRecordToSortedDictionary(updateListClone);
                    var shouldClose = false;

                    if (conn == null)
                    {
                        conn = _dbFactory.CreateConnection();
                        conn.Open();
                        shouldClose = true;
                    }

                    try
                    {
                        // Get the isolation level (is there a better way?)
                        var transaction = conn.BeginTransaction();
                        var initialIsolationLevel = transaction.IsolationLevel;
                        transaction.Rollback();

                        // Now open the real transaction
                        transaction = conn.BeginTransaction(IsolationLevel.Serializable);
                        try
                        {
                            _logic.UpdateExisting(configName, sortedUpdateListClone, conn, transaction);
                            _storageCommands.Insert(tableName, sortedUpdateListClone.Values, conn, transaction);
                            transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
                            _logger.Fatal("Failed to flush data for table " + tableName + "\n", e);
                            throw new Exception("Failed to flush data for table " + tableName + "\n" + e.Message);
                        }

                        //reset the isolation level
                        transaction = conn.BeginTransaction(initialIsolationLevel);
                        transaction.Rollback();
                    }
                    catch (Exception e)
                    {
                        var msg = "Failed to flush \"" + configName + "\" with " + updateListClone.FormatAsString();
                        _logger.Fatal(msg, e);
                        throw new DataException(msg, e);
                    }
                    finally
                    {
                        if (shouldClose) 
                            conn.Close(); 
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Fatal("Exception swallowed: ", e);
                if (_settings.Debug) 
                    throw;
            }
        }

        /// <summary>
        /// The first reduce level is the table for the primary data.
        /// </summary>
        /// <param name="configName"></param>
        /// <returns></returns>
        private ReduceLevel GetFirstReduceLevel(string configName)
        { 
            MonitorConfig monitorConfig;
            if (!_cache.MonitorConfigs.TryGetValue(configName, out monitorConfig))
                throw new ArgumentException("Unknown monitor configuration \"" + configName + "\"");

            return monitorConfig.ReduceLevels.First();
        }

        /// <summary>
        /// Clone the update list before writting it to the DB, but leave the last element
        /// (since it may be an incomplete accumulation).
        /// </summary>
        /// <param name="updateList"></param>
        /// <returns></returns>
        private IList<MonitorRecord<double>> CloneRecordList(IList<MonitorRecord<double>> updateList)
        {
            IList<MonitorRecord<double>> updateListClone;
            lock (updateList)
            {
                if (updateList.Count > 0)
                {
                    //UpdateList is necessarily sorted, since the values are inserted in time order by the Record method
                    updateListClone = new List<MonitorRecord<double>>(updateList);
                    updateList.Clear();
                }
                else
                {
                    updateListClone = new List<MonitorRecord<double>>();
                }
            }

            return updateListClone;
        }

        private SortedDictionary<long, MonitorRecord<double>> ConvertRecordToSortedDictionary(IEnumerable<MonitorRecord<double>> updateList)
        {
            var sortedUpdateListClone = new SortedDictionary<long, MonitorRecord<double>>();
            foreach (var update in updateList)
                sortedUpdateListClone.Add(update.TimeStamp.Ticks, update);

            return sortedUpdateListClone;
        }
    }
}