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
    public class TestForSetupMonitorConfig
    {
        public class UsingCreateDefaultReduceLevels : TestBase
        {
            [Fact]
            public void ShouldBeAbleToPullExistingInfoFromCache()
            {
                var monitorConfig = new MonitorConfig { Name = "Test" };
                var reduceLevels = new List<ReduceLevel>();

                var connection = new Mock<IDbConnection>();
                var connectionInstance = connection.Object;

                var storageCommands = new Mock<IStorageCommands>();
                storageCommands.Setup(x => x.CreateConfigAndReduceLevels(monitorConfig, reduceLevels, connectionInstance));

                var setupSystemTables = new Mock<ISetupSystemTables>();
                setupSystemTables.Setup(x => x.ValidateAndCreateDataTables(connectionInstance)).Verifiable();
                
                var monitorConfigsDictionary = new ConcurrentDictionary<string, MonitorConfig>(); 

                var cache = new Mock<IDataCache>();
                cache.SetupGet(x => x.MonitorConfigs).Returns(monitorConfigsDictionary).Verifiable();

                var storageFactory = new Mock<IStorageFactory>();
                storageFactory.Setup(x => x.CreateConnection()).Returns(connectionInstance).Verifiable();

                var settings = BuildSettings();


                var defaults = new SetupMonitorConfig(storageCommands.Object, setupSystemTables.Object, cache.Object, storageFactory.Object, settings.Object);
                defaults.CreateDefaultReduceLevels(monitorConfig, reduceLevels);

                Assert.Equal(1, monitorConfigsDictionary.Count);
                Assert.True(monitorConfigsDictionary.ContainsKey("Test"));

                storageCommands.VerifyAll();
                setupSystemTables.VerifyAll();
                storageFactory.VerifyAll();
                cache.VerifyAll();
            }
        }
    }
}
