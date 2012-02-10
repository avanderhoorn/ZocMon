using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Moq;
using Xunit;
using ZocMonLib;

namespace ZocMonLib.Test
{
    public class TestForSetupSystemData
    {
        public class UsingLoadAndValidateData : TestBase
        {
            [Fact]
            public void ShouldBeAbleToUse()
            {
                var connection = new Mock<IDbConnection>();
                var connectionInstance = connection.Object;

                var monitorConfigs = new ConcurrentDictionary<string, MonitorConfig>();
                var cache = new Mock<IDataCache>();
                cache.SetupGet(x => x.MonitorConfigs).Returns(monitorConfigs).Verifiable();

                var reduceLevelData = new List<ReduceLevel>
                    {
                        new ReduceLevel { MonitorConfigName = "Test1", Resolution = 60*1000, HistoryLength = 7*24*60*60*1000 },
                        new ReduceLevel { MonitorConfigName = "Test1", Resolution = 5*60*1000, HistoryLength = 7*24*60*60*1000 },
                        new ReduceLevel { MonitorConfigName = "Test1", Resolution = 60*60*1000, HistoryLength = 7*24*60*60*1000 },
                        new ReduceLevel { MonitorConfigName = "Test1", Resolution = 24*60*60*1000, HistoryLength = 7*24*60*60*1000 },
                        new ReduceLevel { MonitorConfigName = "Test2", Resolution = 60*1000, HistoryLength = 7*24*60*60*1000 },
                        new ReduceLevel { MonitorConfigName = "Test2", Resolution = 5*60*1000, HistoryLength = 7*24*60*60*1000 },
                        new ReduceLevel { MonitorConfigName = "Test2", Resolution = 60*60*1000, HistoryLength = 7*24*60*60*1000 },
                        new ReduceLevel { MonitorConfigName = "Test2", Resolution = 24*60*60*1000, HistoryLength = 7*24*60*60*1000 },
                        new ReduceLevel { MonitorConfigName = "Test3", Resolution = 60*1000, HistoryLength = 7*24*60*60*1000 },
                        new ReduceLevel { MonitorConfigName = "Test3", Resolution = 5*60*1000, HistoryLength = 7*24*60*60*1000 },
                        new ReduceLevel { MonitorConfigName = "Test3", Resolution = 60*60*1000, HistoryLength = 7*24*60*60*1000 },
                        new ReduceLevel { MonitorConfigName = "Test3", Resolution = 24*60*60*1000, HistoryLength = 7*24*60*60*1000 },
                        new ReduceLevel { MonitorConfigName = "Test4", Resolution = 60*1000, HistoryLength = 7*24*60*60*1000 },
                        new ReduceLevel { MonitorConfigName = "Test4", Resolution = 5*60*1000, HistoryLength = 7*24*60*60*1000 }
                    };
                var monitorConfigData = new List<MonitorConfig>
                    {
                        new MonitorConfig { Name = "Test1" },
                        new MonitorConfig { Name = "Test2" },
                        new MonitorConfig { Name = "Test3" },
                        new MonitorConfig { Name = "Test4" }
                    };
                var storageCommands = new Mock<IStorageCommandsSetup>();
                storageCommands.Setup(x => x.SelectListAllReduceLevels(connectionInstance)).Returns(reduceLevelData);
                storageCommands.Setup(x => x.SelectListAllMonitorConfigs(connectionInstance)).Returns(monitorConfigData);


                var reduceMethodProvider = new Mock<IReduceMethodProvider>();
                reduceMethodProvider.Setup(x => x.Retrieve(It.IsAny<string>())).Returns(new Mock<IReduceMethod<double>>().Object);

                var settings = BuildSettings();
                settings.SetupGet(x => x.Debug).Returns(true);
                settings.SetupGet(x => x.ReduceMethodProvider).Returns(reduceMethodProvider.Object);


                var setupSystem = new SetupSystemData(cache.Object, storageCommands.Object, settings.Object);
                setupSystem.LoadAndValidateData(connectionInstance);

                Assert.Equal(4, monitorConfigs.Count);
                Assert.True(monitorConfigs.ContainsKey("Test1"));
                Assert.Equal(4, monitorConfigs["Test1"].ReduceLevels.Count);
                Assert.Equal("Test1", monitorConfigs["Test1"].Name);
                Assert.True(monitorConfigs.ContainsKey("Test2"));
                Assert.Equal(4, monitorConfigs["Test2"].ReduceLevels.Count);
                Assert.Equal("Test2", monitorConfigs["Test2"].Name);
                Assert.True(monitorConfigs.ContainsKey("Test3"));
                Assert.Equal(4, monitorConfigs["Test3"].ReduceLevels.Count);
                Assert.Equal("Test3", monitorConfigs["Test3"].Name);
                Assert.True(monitorConfigs.ContainsKey("Test4"));
                Assert.Equal(2, monitorConfigs["Test4"].ReduceLevels.Count);
                Assert.Equal("Test4", monitorConfigs["Test4"].Name);

                cache.VerifyAll();
                storageCommands.VerifyAll();
            }
        }
    }
}