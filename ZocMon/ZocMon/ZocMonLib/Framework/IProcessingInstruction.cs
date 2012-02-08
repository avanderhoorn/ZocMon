using System.Collections.Generic;
using System.Data;

namespace ZocMonLib
{
    public interface IProcessingInstruction
    {
        IList<MonitorRecord<double>> CalculateExpectedValues(string configName, ReduceLevel comparisonReduceLevel, IDictionary<long, MonitorRecordComparison<double>> expectedValues, IDbConnection conn);
    }
}
