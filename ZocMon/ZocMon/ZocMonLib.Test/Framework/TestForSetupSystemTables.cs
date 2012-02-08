using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Moq;
using Xunit;
using ZocMonLib;

namespace ZocMonLib.Test
{
    public class TestForSetupSystemTables
    {
        public class UsingValidateAndCreateDataTables : TestBase
        {
            [Fact]
            public void ShouldBeAbleToUse()
            {
                var connection = new Mock<IDbConnection>();
                var connectionInstance = connection.Object;

                var settings = BuildSettings();

                var monitorConfigs = new ConcurrentDictionary<string, MonitorConfig>();
                monitorConfigs.TryAdd("Test1", new MonitorConfig { Name = "Test1", ComparisonCalculator = new ProcessingInstructionAverage(), ReduceLevels = new List<ReduceLevel> { new ReduceLevel { Resolution = 1000 }, new ReduceLevel { Resolution = 60000 } } });
                monitorConfigs.TryAdd("Test2", new MonitorConfig { Name = "Test2", ComparisonCalculator = new ProcessingInstructionAverage(), ReduceLevels = new List<ReduceLevel> { new ReduceLevel { Resolution = 1000 }, new ReduceLevel { Resolution = 60000 } } });
                monitorConfigs.TryAdd("Test3", new MonitorConfig { Name = "Test3", ComparisonCalculator = new ProcessingInstructionAverage(), ReduceLevels = new List<ReduceLevel> { new ReduceLevel { Resolution = 1000 }, new ReduceLevel { Resolution = 60000 } } });
                monitorConfigs.TryAdd("Test4", new MonitorConfig { Name = "Test4", ComparisonCalculator = new ProcessingInstructionAverage(), ReduceLevels = new List<ReduceLevel> { new ReduceLevel { Resolution = 1000 }, new ReduceLevel { Resolution = 60000 } } });

                var cache = new Mock<IDataCache>();
                cache.SetupGet(x => x.MonitorConfigs).Returns(monitorConfigs).Verifiable();


                IEnumerable<string> needCreatingTable = null;
                IEnumerable<string> needCreatingTableComparison = null;
                Dictionary<string, Tuple<string, long>> tablesConfigResolution = null;

                var existingTables = new List<string> { "Test1SecondlyData", "Test1MinutelyData", "Test2SecondlyData", "Test2MinutelyData", "Test1SecondlyComparison", "Test1MinutelyComparison", "Test2SecondlyComparison", "Test2MinutelyComparison" };
                var storageCommands = new Mock<IStorageCommandsSetup>();
                storageCommands.Setup(x => x.SelectListAllExistingTables(connectionInstance)).Returns(existingTables);
                storageCommands.Setup(x => x.BuildTables(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<Dictionary<string, Tuple<string, long>>>(), connectionInstance))
                    .Callback((IEnumerable<string> nCT, IEnumerable<string> nCTC, Dictionary<string, Tuple<string, long>> tCR, IDbConnection c) => { needCreatingTable = nCT; needCreatingTableComparison = nCTC; tablesConfigResolution = tCR; }).Verifiable();


                var setupSystemTables = new SetupSystemTables(cache.Object, storageCommands.Object, settings.Object);
                setupSystemTables.ValidateAndCreateDataTables(connectionInstance);

                Assert.Equal(4, needCreatingTable.Count());
                Assert.True(needCreatingTable.Contains("Test3SecondlyData"));
                Assert.True(needCreatingTable.Contains("Test3MinutelyData"));
                Assert.True(needCreatingTable.Contains("Test4SecondlyData"));
                Assert.Equal(4, needCreatingTableComparison.Count());
                Assert.True(needCreatingTableComparison.Contains("Test3SecondlyComparison"));
                Assert.True(needCreatingTableComparison.Contains("Test3MinutelyComparison"));
                Assert.True(needCreatingTableComparison.Contains("Test4SecondlyComparison"));
                Assert.Equal(8, tablesConfigResolution.Count());
                Assert.True(tablesConfigResolution.ContainsKey("Test3SecondlyData"));
                Assert.True(tablesConfigResolution.ContainsKey("Test3MinutelyData"));
                Assert.True(tablesConfigResolution.ContainsKey("Test4SecondlyData"));

                cache.VerifyAll();
                storageCommands.VerifyAll();
            }
        }
    }
}
