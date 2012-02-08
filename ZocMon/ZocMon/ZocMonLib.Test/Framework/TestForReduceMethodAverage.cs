using System;
using System.Collections.Generic;
using Xunit;
using ZocMonLib;

namespace ZocMonLib.Test
{
    public class TestForReduceMethodAverage
    {
        public class UsingReduces
        {
            [Fact]
            public void ShouldReduceDown()
            {
                var record1 = new MonitorRecord<double> { Value = 5, Number = 1 };
                var record2 = new MonitorRecord<double> { Value = 9, Number = 2 };
                var record3 = new MonitorRecord<double> { Value = 3, Number = 4 };

                var receords = new List<MonitorRecord<double>> { record1, record2, record3 };

                var reduceMethod = new ReduceMethodAverage();
                var result = reduceMethod.Reduce(new DateTime(2012, 05, 12), receords);

                Assert.Equal(new DateTime(2012, 05, 12), result.TimeStamp);
                Assert.Equal(7, result.Number);
                Assert.Equal(5, result.Value);
                Assert.Equal(35, result.IntervalSum);
            }
        }

        public class UsingIntervalAggregates
        {
            [Fact]
            public void ShoudlAverage()
            {
                var record = new MonitorRecord<double> { Value = 10, Number = 5 };

                var reduceMethod = new ReduceMethodAverage();
                var result = reduceMethod.IntervalAggregate(new DateTime(2012, 05, 12), record, 100);

                Assert.Equal(new DateTime(2012, 05, 12), result.TimeStamp);
                Assert.Equal(6, result.Number);
                Assert.Equal(25, result.Value);
                Assert.Equal(10000, result.IntervalSumOfSquares);
                Assert.Equal(150, result.IntervalSum);
                Assert.NotSame(record, result);

                result = reduceMethod.IntervalAggregate(new DateTime(2012, 05, 12), result, 4);
                Assert.Equal(7, result.Number);
                Assert.Equal(22, result.Value);
                Assert.Equal(10016, result.IntervalSumOfSquares);
                Assert.Equal(154, result.IntervalSum);
            }
        }

        public class UsingValidateMonitorConfigurations
        {
            [Fact]
            public void ShouldConfirmTypeIsSame()
            {
                var reduceMethod = new ReduceMethodAverage();

                Assert.True(reduceMethod.ValidateMonitorConfiguration(MonitorReductionType.DefaultAverage));
                Assert.False(reduceMethod.ValidateMonitorConfiguration(MonitorReductionType.DefaultAccumulate));
            }
        }
    }
}