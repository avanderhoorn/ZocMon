using System;
using System.Collections.Generic;

namespace ZocMonLib
{
    public interface IRecordReduceAggregate
    {
        /// <summary>
        /// Divide the given updates into sub-lists to be reduced to a single data point.
        /// </summary>
        /// <param name="lastReductionTime"></param>
        /// <param name="targetReduceLevel"></param>
        /// <param name="sourceAggregationList"></param>
        /// <param name="destinationAggregatedList"></param>
        /// <returns>The DateTime of the last aggregated data point.</returns>
        DateTime Aggregate(DateTime lastReductionTime, ReduceLevel targetReduceLevel, IList<MonitorRecord<double>> sourceAggregationList, IDictionary<DateTime, IList<MonitorRecord<double>>> destinationAggregatedList);
    }
}