using System;
using System.Collections.Generic;
using System.Data;

namespace ZocMonLib
{
    public class ReduceMethodAccumulate : IReduceMethod<double>
    {
        public bool ValidateMonitorConfiguration(MonitorReductionType monitorReductionType)
        {
            return MonitorReductionType.DefaultAccumulate.Equals(monitorReductionType);
        }

        public MonitorRecord<double> Reduce(DateTime time, IList<MonitorRecord<double>> values)
        {
            double sum = 0;
            var totalCount = 0;
            foreach (var update in values)
            {
                sum += update.Value;
                totalCount += update.Number;
            }

            if (totalCount == 0)
                throw new DataException("No Data");

            return new MonitorRecord<double>(time, sum, totalCount);
        }

        public MonitorRecord<double> IntervalAggregate(DateTime timeBin, MonitorRecord<double> lastUpdate, double newValue)
        {
            var tempCount = lastUpdate.Number + 1;
            var tempValue = lastUpdate.Value + newValue;

            return new MonitorRecord<double>(timeBin, tempValue, tempCount);
        }
    }
}
