using System.Collections.Generic;
using System.Data;

namespace ZocMonLib
{
    public class ProcessingInstructionAverage : IProcessingInstruction
    {
        public IList<MonitorRecord<double>> CalculateExpectedValues(string configName, ReduceLevel comparisonReduceLevel, IDictionary<long, MonitorRecordComparison<double>> expectedValues, IDbConnection conn)
        { 
            return new List<MonitorRecord<double>>(); 
        }
    }
}
