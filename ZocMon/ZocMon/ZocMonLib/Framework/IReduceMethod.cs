using System;
using System.Collections.Generic;
using ZocMonLib;

namespace ZocMonLib
{
    public interface IReduceMethod<T>
    {
        /// <summary>
        /// Given a list of values for a particular bin (which starts at time time),
        /// calculate a reduced value for that time bin.
        /// </summary>
        /// <param name="time">The beginning of the time interval.</param>
        /// <param name="values">The values for that time interval.</param>
        /// <returns>The reduced value.</returns>
        MonitorRecord<T> Reduce(DateTime time, IList<MonitorRecord<T>> values);

        /// <summary>
        /// Given a single update that belongs in the same time bin as a list of
        /// previous updates, calculate the new value for that time bin.
        /// </summary>
        /// <param name="timeBin">The beginning of the time interval.</param>
        /// <param name="lastUpdate">Previous updates for this time bin.</param>
        /// <param name="newValue">A new update for this time bin.</param>
        /// <returns>An aggregated value for this time bin.</returns>
        MonitorRecord<T> IntervalAggregate(DateTime timeBin, MonitorRecord<T> lastUpdate, T newValue);

        /// <summary>
        /// Validate that the given monitor configuration is consistant with this reduction method.
        /// </summary>
        /// <param name="monitorReductionType"></param>
        /// <returns></returns>
        bool ValidateMonitorConfiguration(MonitorReductionType monitorReductionType);
    }
}
