using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ZocMonLib
{
    public class RecordFlushUpdate : IRecordFlushUpdate
    {
        private readonly IDataCache _cache;
        private readonly IStorageCommands _storageCommands;

        public RecordFlushUpdate(IDataCache cache, IStorageCommands storageCommands)
        {
            _cache = cache;
            _storageCommands = storageCommands;
        }

        /// <summary>
        /// Update any existing rows for the given values, and remove them from the values collection.
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="values"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public bool UpdateExisting(string configName, IDictionary<long, MonitorRecord<double>> values, IDbConnection conn, IDbTransaction transaction = null)
        {
            var result = false;

            if (values.Count == 0)
                return true;

            MonitorInfo monitorInfo;
            if (_cache.MonitorInfo.TryGetValue(configName, out monitorInfo))
            {
                var reduceLevel = monitorInfo.FirstReduceLevel;
                var reduceMethod = reduceLevel.AggregationClass;
                var startTime = values.First().Value.TimeStamp;

                var tableName = Support.MakeReducedName(configName, reduceLevel.Resolution);

                var updateList = new List<MonitorRecord<double>>();
                var combineList = new List<MonitorRecord<double>>(); 

                //Based on the timestamp we have we pull out from the database all recrods that have a date 
                //grater than the timestamp we have. For each value we find loop through and find the value 
                //that is the next greatest time past our current value from the database
                var existingList = _storageCommands.SelectListForUpdateExisting(tableName, startTime, conn, transaction);
                foreach (var existingValue in existingList)
                {
                    var hasNext = false;
                    var matchingValue = _cache.Empty;

                    var valuesEnumerator = values.GetEnumerator();
                    while ((hasNext = valuesEnumerator.MoveNext()) && ((matchingValue = valuesEnumerator.Current.Value).TimeStamp < existingValue.TimeStamp));

                    combineList.Clear();

                    if (hasNext && !_cache.Empty.Equals(matchingValue))
                    {
                        combineList.Add(existingValue);
                        combineList.Add(matchingValue);
                        values.Remove(matchingValue.TimeStamp.Ticks);
                    }
                    else 
                        continue; 

                    //Reduce the value we have from the database with the value we have here (thats what is in the combined list)
                    var update = reduceMethod.Reduce(existingValue.TimeStamp, combineList);
                    updateList.Add(update);
                }

                //Update everything in the database
                _storageCommands.Update(tableName, updateList, conn, transaction);

                result = true;
            }
            else 
                throw new DataException("No monitor config found for " + configName); 

            return result;
        }

    }
}