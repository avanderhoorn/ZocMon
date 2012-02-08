using System;
using System.Collections.Generic;
using Xunit;
using ZocMonLib;

namespace ZocMonLib.Test
{
    public class TestForReduceMethodAccumulate
    {
        public class UsingReduce
        {
            [Fact]
            public void ShouldReduceDown()
            {
                var record1 = new MonitorRecord<double> { Value = 5, Number = 1 };
                var record2 = new MonitorRecord<double> { Value = 10, Number = 2 };
                var record3 = new MonitorRecord<double> { Value = 15, Number = 4 };

                var receords = new List<MonitorRecord<double>> { record1, record2, record3 };

                var reduceMethod = new ReduceMethodAccumulate();
                var result = reduceMethod.Reduce(new DateTime(2012, 05, 12), receords);

                Assert.Equal(new DateTime(2012, 05, 12), result.TimeStamp);
                Assert.Equal(7, result.Number);
                Assert.Equal(30, result.Value); 
            }
        }

        public class UsingIntervalAggregate
        {
            [Fact]
            public void ShoudlAccumulate()
            {
                var record = new MonitorRecord<double> { Value = 10, Number = 5 };

                var reduceMethod = new ReduceMethodAccumulate();
                var result = reduceMethod.IntervalAggregate(new DateTime(2012, 05, 12), record, 2);

                Assert.Equal(new DateTime(2012, 05, 12), result.TimeStamp);
                Assert.Equal(6, result.Number);
                Assert.Equal(12, result.Value);
                Assert.NotSame(record, result);

                result = reduceMethod.IntervalAggregate(new DateTime(2012, 05, 12), result, 4);

                Assert.Equal(new DateTime(2012, 05, 12), result.TimeStamp);
                Assert.Equal(7, result.Number);
                Assert.Equal(16, result.Value);
            }
        }

        public class UsingValidateMonitorConfiguration
        {
            [Fact]
            public void ShouldConfirmTypeIsSame()
            {
                var reduceMethod = new ReduceMethodAccumulate();

                Assert.True(reduceMethod.ValidateMonitorConfiguration(MonitorReductionType.DefaultAccumulate));
                Assert.False(reduceMethod.ValidateMonitorConfiguration(MonitorReductionType.DefaultAverage));
            }
        }
    }
}
