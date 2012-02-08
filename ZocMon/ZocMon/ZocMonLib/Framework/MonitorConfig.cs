using System.Collections.Generic;
using ZocMonLib;

namespace ZocMonLib
{
    public class MonitorConfig
    {
        /// <summary>
        /// Name of the monitor in place
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Reduction type that is currently being used
        /// </summary>
        public MonitorReductionType MonitorReductionType { get; set; }

        /// <summary>
        /// Key that will be used to lookup which IProcessingInstruction should be used for this Config
        /// </summary>
        public string ComparisonCalculatorClassName { get; set; }

        /// <summary>
        /// The ProcessingInstruction should be used during the reduce process
        /// </summary>
        public IProcessingInstruction ComparisonCalculator { get; set; }

        /// <summary>
        /// Reduce levels that are relevent for this config
        /// </summary>
        public List<ReduceLevel> ReduceLevels { get; set; }
    }
}
