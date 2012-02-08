using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace ZocMonLib
{
    /// <summary>
    /// Data point that contains multiple occurances of a given event
    /// </summary>
    public class MonitorInfo
    {
        public MonitorInfo()
        {
            TablesCreated = false;
        }

        /// <summary>
        /// Config associated with this info
        /// </summary>
        public MonitorConfig MonitorConfig { get; set; }

        /// <summary>
        /// Reference to the first reduce level
        /// </summary>
        public ReduceLevel FirstReduceLevel { get; set; }

        /// <summary>
        /// Records that have been recorded for this event
        /// </summary>
        public IList<MonitorRecord<double>> MonitorRecords { get; set; }
         
        /// <summary>
        /// If a config record exists for this info
        /// </summary>
        /// <returns>
        /// By extension this also says whether the core record for the event exists
        /// in the data store.
        /// </returns>
        public bool TablesCreated { get; set; }
    }
}
