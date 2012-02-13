using System;
using System.Collections.Generic;
using System.Data;

namespace ZocMonLib
{
    public interface IStorageCommands
    {
        /// <summary>
        /// Bulk insert values of the give type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="values"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        void Insert<T>(string tableName, IEnumerable<T> values, IDbConnection conn, IDbTransaction transaction = null);

        /// <summary>
        /// Bulk insert values of the give type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="values"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        void Update<T>(string tableName, IEnumerable<T> values, IDbConnection conn, IDbTransaction transaction = null) where T : ITimeStamped;

        /// <summary>
        /// Update any existing rows for the given values, and remove them from the values collection.
        /// </summary>
        /// <param name="tableName"> </param>
        /// <param name="update"> </param>
        /// <param name="knownToExist"> </param>
        /// <param name="conn"></param>
        /// <returns></returns>
        bool UpdateIfExists(string tableName, MonitorRecord<double> update, bool knownToExist, IDbConnection conn);

        /// <summary>
        /// Flush the given list of data to the DB.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="updateList"></param>
        /// <param name="conn"></param> 
        void Flush(string tableName, IEnumerable<MonitorRecord<double>> updateList, IDbConnection conn);
        
        /// <summary>
        /// Pulls out a list of recrods that need to be updated for the given table and timeStamp
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="timeStamp"></param>
        /// <param name="conn"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        IEnumerable<MonitorRecord<double>> SelectListForUpdateExisting(string tableName, DateTime timeStamp, IDbConnection conn, IDbTransaction transaction = null);

        /// <summary>
        /// Select the list of records that where previously reduced, get the most recent data point 
        /// that's already reduced
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        IEnumerable<MonitorRecord<double>> SelectListForLastReduced(string tableName, IDbConnection conn);

        /// <summary>  
        /// Pulls out the lastReducedUpdate record and the lastReductionTime
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="resolution"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        StorageLastReduced RetrieveLastReducedData(string tableName, long resolution, IDbConnection conn);

        /// <summary>
        /// Select the list of records that is to be reduced, get the data to be reduced, starting from 
        /// the last point that was already reduced
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="hasTargetReducedRecord"></param>
        /// <param name="lastReductionTime"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        IEnumerable<MonitorRecord<double>> SelectListRequiringReduction(string tableName, bool hasTargetReducedRecord, DateTime lastReductionTime, IDbConnection conn);

        /// <summary>
        /// Delete data before the earlier of the reducedTo date, or the history length configured in for the reduce level.
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="reducedTo">The start time of the last reduced (or partially reduced) interval, of the next higher reduction level.  Everything before this will be deleted.</param>
        /// <param name="reduceLevel">The reduce level to clear.</param>
        /// <param name="conn"></param>
        void ClearReducedData(string configName, DateTime reducedTo, ReduceLevel reduceLevel, IDbConnection conn);

        /// <summary>
        /// Removes any duplicate records for a given timeStamp.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="conn"></param>
        void PergeDuplicateReducedData(string tableName, IDbConnection conn);

        /// <summary>
        /// Pulls out the last comparison data that was calculated
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        IEnumerable<MonitorRecord<double>> SelectListLastComparisonData(string tableName, IDbConnection conn);

        /// <summary>
        /// Get the data to be reduced, starting from the last point that was already reduced 
        /// </summary>
        /// <param name="reducedTableName"> </param>
        /// <param name="hasLastPrediction"> </param>
        /// <param name="reducedDataStartTime"> </param>
        /// <param name="conn"></param>
        /// <returns></returns> 
        IEnumerable<MonitorRecord<double>> SelectListNeedingToBeReduced(string reducedTableName, bool hasLastPrediction, DateTime reducedDataStartTime, IDbConnection conn);

        /// <summary>
        /// Creates a given monitorConfig and its reduce levels
        /// </summary>
        /// <param name="monitorConfig"></param>
        /// <param name="reduceLevels"></param>
        /// <param name="conn"></param>
        void CreateConfigAndReduceLevels(MonitorConfig monitorConfig, IEnumerable<ReduceLevel> reduceLevels, IDbConnection conn);
    }
}