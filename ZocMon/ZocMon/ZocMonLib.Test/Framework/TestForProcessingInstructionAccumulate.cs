using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Moq;
using Xunit;
using ZocMonLib;

namespace ZocMonLib.Test
{
    public class TestForProcessingInstructionAccumulate
    {
        public class UsingCalculateExpectedValues
        {
            [Fact]
            public void ShouldBeAbleToUse()
            {
                var connection = new Mock<IDbConnection>();
                var connectionInstance = connection.Object;

                var reductionLevel = new ReduceLevel();
                reductionLevel.Resolution = 1000;

                var comparisons = new SortedDictionary<long, MonitorRecordComparison<double>>();

                var lastData = new List<MonitorRecord<double>> { new MonitorRecord<double>() { TimeStamp = new DateTime(2011, 6, 6, 12, 30, 30) } };
                var needingData = new List<MonitorRecord<double>>
                                      {
                                          new MonitorRecord<double>() { TimeStamp = new DateTime(2011, 6, 6, 12, 30, 30), Number = 1, Value = 20 },  
                                          new MonitorRecord<double>() { TimeStamp = new DateTime(2011, 6, 6, 12, 30, 31), Number = 2, Value = 19 }, 
                                          new MonitorRecord<double>() { TimeStamp = new DateTime(2011, 6, 6, 12, 30, 32), Number = 3, Value = 18 }, 
                                          new MonitorRecord<double>() { TimeStamp = new DateTime(2011, 6, 6, 12, 30, 43), Number = 4, Value = 10 }, 
                                          new MonitorRecord<double>() { TimeStamp = new DateTime(2011, 6, 13, 12, 30, 40), Number = 5, Value = 16 }, 
                                          new MonitorRecord<double>() { TimeStamp = new DateTime(2011, 6, 13, 12, 30, 41), Number = 6, Value = 17 }, 
                                          new MonitorRecord<double>() { TimeStamp = new DateTime(2011, 6, 13, 12, 30, 42), Number = 7, Value = 18 }, 
                                          new MonitorRecord<double>() { TimeStamp = new DateTime(2011, 6, 13, 12, 30, 43), Number = 8, Value = 10 }, 
                                          new MonitorRecord<double>() { TimeStamp = new DateTime(2011, 6, 20, 12, 30, 43), Number = 10, Value = 10 }
                                      };

                var storageCommands = new Mock<IStorageCommands>();
                storageCommands.Setup(x => x.SelectListLastComparisonData("TestSecondlyComparison", connectionInstance)).Returns(lastData).Verifiable();
                storageCommands.Setup(x => x.SelectListNeedingToBeReduced("TestSecondlyData", true, new DateTime(2011, 5, 9, 12, 30, 30, 500), connectionInstance)).Returns(needingData).Verifiable();

                var processingInstruction = new ProcessingInstructionAccumulate(storageCommands.Object);
                var result = processingInstruction.CalculateExpectedValues("Test", reductionLevel, comparisons, connectionInstance);

                Assert.Equal(18, comparisons.Count);

                Assert.Equal((new DateTime(2011, 6, 13, 12, 30, 30)).Ticks, comparisons.Keys.ToList()[0]);
                Assert.Equal(1, comparisons.Values.ToList()[0].Number);
                Assert.Equal(20, comparisons.Values.ToList()[0].Value);
                Assert.Equal((new DateTime(2011, 6, 13, 12, 30, 31)).Ticks, comparisons.Keys.ToList()[1]);
                Assert.Equal(2, comparisons.Values.ToList()[1].Number);
                Assert.Equal(19, comparisons.Values.ToList()[1].Value);
                Assert.Equal((new DateTime(2011, 6, 13, 12, 30, 32)).Ticks, comparisons.Keys.ToList()[2]);
                Assert.Equal(3, comparisons.Values.ToList()[2].Number);
                Assert.Equal(18, comparisons.Values.ToList()[2].Value);

                //This is important
                Assert.Equal((new DateTime(2011, 6, 13, 12, 30, 43)).Ticks, comparisons.Keys.ToList()[3]);
                Assert.Equal(4, comparisons.Values.ToList()[3].Number);
                Assert.Equal(10, comparisons.Values.ToList()[3].Value);

                Assert.Equal((new DateTime(2011, 6, 20, 12, 30, 30)).Ticks, comparisons.Keys.ToList()[4]);
                Assert.Equal(1, comparisons.Values.ToList()[4].Number);
                Assert.Equal(20, comparisons.Values.ToList()[4].Value);
                Assert.Equal((new DateTime(2011, 6, 20, 12, 30, 31)).Ticks, comparisons.Keys.ToList()[5]);
                Assert.Equal(2, comparisons.Values.ToList()[5].Number);
                Assert.Equal(19, comparisons.Values.ToList()[5].Value);
                Assert.Equal((new DateTime(2011, 6, 20, 12, 30, 32)).Ticks, comparisons.Keys.ToList()[6]);
                Assert.Equal(3, comparisons.Values.ToList()[6].Number);
                Assert.Equal(18, comparisons.Values.ToList()[6].Value);
                Assert.Equal((new DateTime(2011, 6, 20, 12, 30, 40)).Ticks, comparisons.Keys.ToList()[7]);
                Assert.Equal(5, comparisons.Values.ToList()[7].Number);
                Assert.Equal(16, comparisons.Values.ToList()[7].Value);
                Assert.Equal((new DateTime(2011, 6, 20, 12, 30, 41)).Ticks, comparisons.Keys.ToList()[8]);
                Assert.Equal(6, comparisons.Values.ToList()[8].Number);
                Assert.Equal(17, comparisons.Values.ToList()[8].Value);
                Assert.Equal((new DateTime(2011, 6, 20, 12, 30, 42)).Ticks, comparisons.Keys.ToList()[9]);
                Assert.Equal(7, comparisons.Values.ToList()[9].Number);
                Assert.Equal(18, comparisons.Values.ToList()[9].Value);

                //This is important
                Assert.Equal((new DateTime(2011, 6, 20, 12, 30, 43)).Ticks, comparisons.Keys.ToList()[10]);
                Assert.Equal(12, comparisons.Values.ToList()[10].Number);
                Assert.Equal(10, comparisons.Values.ToList()[10].Value);

                Assert.Equal((new DateTime(2011, 6, 27, 12, 30, 30)).Ticks, comparisons.Keys.ToList()[11]);
                Assert.Equal(1, comparisons.Values.ToList()[11].Number);
                Assert.Equal(20, comparisons.Values.ToList()[11].Value);
                Assert.Equal((new DateTime(2011, 6, 27, 12, 30, 31)).Ticks, comparisons.Keys.ToList()[12]);
                Assert.Equal(2, comparisons.Values.ToList()[12].Number);
                Assert.Equal(19, comparisons.Values.ToList()[12].Value);
                Assert.Equal((new DateTime(2011, 6, 27, 12, 30, 32)).Ticks, comparisons.Keys.ToList()[13]);
                Assert.Equal(3, comparisons.Values.ToList()[13].Number);
                Assert.Equal(18, comparisons.Values.ToList()[13].Value);
                Assert.Equal((new DateTime(2011, 6, 27, 12, 30, 40)).Ticks, comparisons.Keys.ToList()[14]);
                Assert.Equal(5, comparisons.Values.ToList()[14].Number);
                Assert.Equal(16, comparisons.Values.ToList()[14].Value);
                Assert.Equal((new DateTime(2011, 6, 27, 12, 30, 41)).Ticks, comparisons.Keys.ToList()[15]);
                Assert.Equal(6, comparisons.Values.ToList()[15].Number);
                Assert.Equal(17, comparisons.Values.ToList()[15].Value);
                Assert.Equal((new DateTime(2011, 6, 27, 12, 30, 42)).Ticks, comparisons.Keys.ToList()[16]);
                Assert.Equal(7, comparisons.Values.ToList()[16].Number);
                Assert.Equal(18, comparisons.Values.ToList()[16].Value);

                //This is important
                Assert.Equal((new DateTime(2011, 6, 27, 12, 30, 43)).Ticks, comparisons.Keys.ToList()[17]);
                Assert.Equal(22, comparisons.Values.ToList()[17].Number);
                Assert.Equal(10, comparisons.Values.ToList()[17].Value);

                Assert.Equal(0, result.Count);

                storageCommands.VerifyAll();
            }
        }
    }
}
