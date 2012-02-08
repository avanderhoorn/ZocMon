using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using ZocMonLib.Extensibility;
using ZocMonLib;

namespace ZocMonLib
{
    public class RecordReduce : IRecordReduce
    {
        private readonly ISystemLogger _logger;
        private readonly IConfigSeed _configSeed;
        private readonly IRecordReduceStatus _reduceStatus;
        private readonly IRecordReduceAggregate _reduceAggregater;
        private readonly IDataCache _cache; 
        private readonly IRecordCompare _recordCompare;
        private readonly IStorageCommands _storageCommands;
        private readonly IStorageFactory _dbFactory;
        private readonly ISettings _settings;

        public RecordReduce(IRecordReduceStatus reduceStatus, IRecordReduceAggregate reduceAggregater, IDataCache cache, IRecordCompare recordCompare, IStorageCommands storageCommands, IStorageFactory dbFactory, ISettings settings)
        {
            _logger = settings.LoggerProvider.CreateLogger(typeof(RecordReduce));
            _configSeed = settings.ConfigSeed;
            _reduceStatus = reduceStatus;
            _reduceAggregater = reduceAggregater;
            _cache = cache; 
            _recordCompare = recordCompare;
            _storageCommands = storageCommands;
            _dbFactory = dbFactory;
            _settings = settings;
        }

        /// <summary>
        /// Reduce all known data.
        /// </summary>
        public string ReduceAll(bool deleteReducedData = false)
        {
            var errorReturn = "";
            if (!_reduceStatus.IsReducing())
            {
                using (var conn = _dbFactory.CreateConnection())
                {
                    conn.Open();
                    foreach (var monitorInfo in _cache.MonitorConfigs)
                    {
                        try
                        {
                            var ret = Reduce(monitorInfo.Key, deleteReducedData, conn, true);
                            if (!"".Equals(ret)) 
                                errorReturn += ret + Environment.NewLine;
                        }
                        catch (Exception e)
                        {
                            _logger.Fatal("Exception swallowed: ", e); 
                        }
                    }
                }

                if (!"".Equals(errorReturn)) 
                    _logger.Fatal("Errors with: \"" + errorReturn + "\"");

                _reduceStatus.DoneReducing();
            }
            else 
                _logger.Fatal("Already Reducing Event Occurred for ProcessId: " + Process.GetCurrentProcess().Id);

            return errorReturn;
        }

        /// <summary>
        /// Calculate and store all reductions for the given configuration.
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="deleteReducedData">If true, will actually delete the reduced data</param>
        /// <param name="conn"></param>
        /// <param name="isInner"></param>
        public string Reduce(string configName, bool deleteReducedData = false, IDbConnection conn = null, bool isInner = false)
        {
            var errorReturn = "";
            var shouldClose = false;
            if (isInner || !_reduceStatus.IsReducing())
            {
                try
                { 
                    configName = Support.ValidateConfigName(configName);

                    MonitorInfo monitorInfo;
                    if (!_cache.MonitorInfo.TryGetValue(configName, out monitorInfo)) 
                        monitorInfo = _configSeed.Seed(configName, MonitorReductionType.Custom);  //NOT sure this is supposed to be custom

                    if (conn == null)
                    {
                        conn = _dbFactory.CreateConnection();
                        conn.Open();
                        shouldClose = true;
                    }

                    try
                    {
                        var deleteInfo = ProcessConfig(configName, monitorInfo, deleteReducedData, conn);
                         
                        OtherCalculations(configName, monitorInfo, conn);

                        DeleteData(deleteReducedData, deleteInfo, conn);
                    }
                    finally
                    {
                        if (shouldClose) 
                            conn.Close(); 
                    }
                }
                catch (Exception e)
                {
                    _logger.Fatal("Exception swallowed: ", e);
                    errorReturn = configName + ", ProcessId == " + Process.GetCurrentProcess().Id;
                    if (_settings.Debug)
                        throw;
                }
                finally
                {
                    if (!isInner)
                        _reduceStatus.DoneReducing();
                }
            }
            return errorReturn;
        }

        private IEnumerable<DeleteInfo> ProcessConfig(string configName, MonitorInfo monitorInfo, bool deleteReducedData, IDbConnection conn)
        {
            var deleteInfo = new List<DeleteInfo>();

            ReduceLevel lastReduceLevel = null;
            foreach (var targetReduceLevel in monitorInfo.MonitorConfig.ReduceLevels)
            {
                if (lastReduceLevel == null)
                {
                    lastReduceLevel = targetReduceLevel;
                    continue;
                }

                //Get the most recent data point that's already reduced
                var targetReducedTableName = Support.MakeReducedName(configName, targetReduceLevel.Resolution);
                var targetReducedData = _storageCommands.RetrieveLastReducedData(targetReducedTableName, targetReduceLevel.Resolution, conn);

                //Get the data to be reduced, starting from the last point that was already reduced
                var toBeReducedTableName = Support.MakeReducedName(configName, lastReduceLevel.Resolution);
                var toBeReducedData = _storageCommands.SelectListRequiringReduction(toBeReducedTableName, targetReducedData.Record != null, targetReducedData.Time, conn);

                //Reduce the data down and aggregate it together 
                var aggregatedList = new Dictionary<DateTime, IList<MonitorRecord<double>>>();
                var lastAggregatedTime = _reduceAggregater.Aggregate(targetReducedData.Time, targetReduceLevel, toBeReducedData, aggregatedList);

                //Write it out the data
                var reducedList = CalculateReductions(aggregatedList, targetReduceLevel);
                if (reducedList.Count > 0)
                {
                    var updated = _storageCommands.UpdateIfExists(targetReducedTableName, reducedList.First(), targetReducedData.Record != null, conn);
                    if (updated)
                        reducedList.RemoveAt(0);

                    _storageCommands.Flush(targetReducedTableName, reducedList, conn);

                    //Prepare to delete old data
                    if (deleteReducedData)
                        deleteInfo.Add(new DeleteInfo { ConfigName = configName, LastAggregatedTime = lastAggregatedTime, LastReduceLevel = lastReduceLevel });
                }

                lastReduceLevel = targetReduceLevel;

                //Remove duplicate reduce data if any exists
                _storageCommands.PergeDuplicateReducedData(targetReducedTableName, conn);
            }

            return deleteInfo;
        }

        private void OtherCalculations(string configName, MonitorInfo monitorInfo, IDbConnection conn)
        {
            //After we've calculated all the reductions, we can do calculations of other data
            foreach (var targetReduceLevel in monitorInfo.MonitorConfig.ReduceLevels) 
                _recordCompare.CalculateComparisons(configName, monitorInfo, targetReduceLevel, conn);  
        }

        private void DeleteData(bool deleteReducedData, IEnumerable<DeleteInfo> deleteInfo, IDbConnection conn)
        {
            // Finally delete any data that is old
            if (deleteReducedData)
            {
                foreach (var info in deleteInfo)
                    _storageCommands.ClearReducedData(info.ConfigName, info.LastAggregatedTime, info.LastReduceLevel, conn);
            }
        }


        private static IList<MonitorRecord<double>> CalculateReductions(IDictionary<DateTime, IList<MonitorRecord<double>>> aggregatedList, ReduceLevel reduceLevel)
        { 
            return aggregatedList.Select(updates => reduceLevel.AggregationClass.Reduce(updates.Key, updates.Value)).ToList();
        }

        #region Supporting Types

        private struct DeleteInfo
        {
            public string ConfigName { get; set; }
            public DateTime LastAggregatedTime { get; set; }
            public ReduceLevel LastReduceLevel { get; set; }
        }

        #endregion
    }
}