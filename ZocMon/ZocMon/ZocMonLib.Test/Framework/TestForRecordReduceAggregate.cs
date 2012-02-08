using System;
using System.Linq;
using System.Collections.Generic; 
using Xunit;

namespace ZocMonLib.Test
{
    public class TestForRecordReduceAggregate
    {
        public class UsingAggregate
        {
            [Fact] 
            public void ShouldReturnMinDateWhenHasNoSourceData()
            {
                var aggregater = new RecordReduceAggregate();
                var result = aggregater.Aggregate(DateTime.Now, null, new List<MonitorRecord<double>>(), new Dictionary<DateTime, IList<MonitorRecord<double>>>());
                 
                Assert.Equal(Constant.MinDbDateTime, result);
            }

            [Fact]
            public void AggregatesDataIntoDictionary()
            {
                var lastReductionTime = new DateTime(2011, 11, 11, 5, 30, 0, 0);
                var reduceLevel = new ReduceLevel { Resolution = 5000 };
                var sourceAggregationList = new List<MonitorRecord<double>>
                                                {
                                                    new MonitorRecord<double>(new DateTime(2011, 11, 11, 5, 30, 0, 500), 5, 5),
                                                    new MonitorRecord<double>(new DateTime(2011, 11, 11, 5, 30, 1, 0), 25, 4),
                                                    new MonitorRecord<double>(new DateTime(2011, 11, 11, 5, 30, 1, 500), 7, 8),
                                                    new MonitorRecord<double>(new DateTime(2011, 11, 11, 5, 30, 40, 0), 3, 3)
                                                };
                var destination = new Dictionary<DateTime, IList<MonitorRecord<double>>>();

                var aggregater = new RecordReduceAggregate();
                var result = aggregater.Aggregate(lastReductionTime, reduceLevel, sourceAggregationList, destination);


                Assert.Equal(2, destination.Count());

                var firstItem = destination.First();
                Assert.Equal(new DateTime(2011, 11, 11, 5, 30, 2, 500), firstItem.Key);
                Assert.Equal(3, firstItem.Value.Count);

                var lastItem = destination.Last();
                Assert.Equal(new DateTime(2011, 11, 11, 5, 30, 42, 500), lastItem.Key);
                Assert.Equal(1, lastItem.Value.Count);
            }
        }
    }
}