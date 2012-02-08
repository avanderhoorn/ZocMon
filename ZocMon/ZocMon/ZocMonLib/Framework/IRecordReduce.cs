using System.Data;

namespace ZocMonLib
{
    public interface IRecordReduce
    {
        /// <summary>
        /// Reduce all known data.
        /// </summary>
        string ReduceAll(bool deleteReducedData);

        /// <summary>
        /// Calculate and store all reductions for the given configuration.
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="deleteReducedData">If true, will actually delete the reduced data</param>
        /// <param name="conn">s</param>
        /// <param name="isInner"></param>
        string Reduce(string configName, bool deleteReducedData, IDbConnection conn, bool isInner);
    }
}