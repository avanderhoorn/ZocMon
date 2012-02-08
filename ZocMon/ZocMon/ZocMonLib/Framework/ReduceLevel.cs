using System;
using ZocMonLib;

namespace ZocMonLib
{
    public class ReduceLevel : IComparable<ReduceLevel>
    {
        /// <summary>
        /// Which config this level belongs too
        /// </summary>
        public string MonitorConfigName { get; set; }

        /// <summary>
        /// How long in ms data at this resolution should be kept for
        /// </summary>
        public long HistoryLength { get; set; }

        /// <summary>
        /// Resolution in ms of what timespan the reduction represents
        /// </summary>
        public long Resolution { get; set; }

        /// <summary>
        /// Type of aggregation that is being used for this reduction 
        /// </summary>
        public string AggregationClassName { get; set; }

        /// <summary>
        /// Aggregation object that is assigned and can be use
        /// </summary>
        public IReduceMethod<double> AggregationClass { get; set; }

        public string DataTablename { get; set; }

        public int CompareTo(ReduceLevel other)
        {
            long diff = Resolution - other.Resolution;
            return diff > 0 ? 1 : (diff == 0 ? 0 : -1);
        }
    }
}