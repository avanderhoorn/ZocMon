using System;
using System.Collections.Generic;
using System.Data;

namespace ZocMonLib
{
    public class ReduceMethodAverage : IReduceMethod<double>
    {
        public bool ValidateMonitorConfiguration(MonitorReductionType monitorReductionType)
        {
            return MonitorReductionType.DefaultAverage.Equals(monitorReductionType);
        }

        public MonitorRecord<double> Reduce(DateTime time, IList<MonitorRecord<double>> values)
        {
            double sum = 0;
            double sumOfSquares = 0;
            var totalCount = 0;
            foreach (var update in values)
            {
                sum += update.Value * update.Number;
                sumOfSquares += ((update.Number == 1) ? update.Value * update.Value : update.IntervalSumOfSquares);
                totalCount += update.Number;
            }

            if (totalCount == 0)
                throw new DataException("No Data");
                
            return new MonitorRecord<double>()
                {
                    TimeStamp = time,
                    Value = sum / totalCount,
                    Number = totalCount,
                    IntervalSum = sum,
                    IntervalSumOfSquares = sumOfSquares
                };
        }


        public MonitorRecord<double> IntervalAggregate(DateTime timeBin, MonitorRecord<double> lastUpdate, double newValue)
        {
            var sum = lastUpdate.Value * lastUpdate.Number + newValue;
            var sumOfSquares = lastUpdate.IntervalSumOfSquares + newValue * newValue;
            var newCount = lastUpdate.Number + 1;

            return new MonitorRecord<double>()
                {
                    TimeStamp = timeBin,
                    Value = sum / newCount,
                    Number = newCount,
                    IntervalSum = sum,
                    IntervalSumOfSquares = sumOfSquares
                };
        }
    }
}
