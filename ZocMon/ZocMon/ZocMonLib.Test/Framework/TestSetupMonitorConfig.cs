using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Moq;
using Xunit;
using ZocMonLib.Extensibility;

namespace ZocMonLib.Test
{
    public class TestSetupMonitorConfig
    {
        public class UsingCreateDefaultReduceLevels : TestBase
        {
            [Fact]
            public void ShouldCreateSystem()
            {
                var monitorConfigDictionary = new ConcurrentDictionary<string, MonitorConfig>();
                var monitorConfig = new MonitorConfig { Name = "Test" };
                var reductionLevels = new List<ReduceLevel>();

                var connection = new Mock<IDbConnection>();
                connection.Setup(x => x.Open());
                connection.Setup(x => x.Close());
                var connectionInstance = connection.Object;

                var storageCommands = new Mock<IStorageCommands>();
                storageCommands.Setup(x => x.CreateConfigAndReduceLevels(monitorConfig, reductionLevels, connectionInstance)).Verifiable(); 

                var setupSystemTables = new Mock<ISetupSystemTables>();
                setupSystemTables.Setup(x => x.ValidateAndCreateDataTables(connectionInstance)).Verifiable();

                var cache = new Mock<IDataCache>();
                cache.SetupGet(x => x.MonitorConfigs).Returns(monitorConfigDictionary).Verifiable();

                var storageFactory = new Mock<IStorageFactory>();
                storageFactory.Setup(x => x.CreateConnection()).Returns(connectionInstance).Verifiable();

                var settings = BuildSettings();

                var system = new SetupMonitorConfig(storageCommands.Object, setupSystemTables.Object, cache.Object, storageFactory.Object, settings.Object);
                system.CreateDefaultReduceLevels(monitorConfig, reductionLevels, null);

                Assert.True(monitorConfigDictionary.ContainsKey("Test"));

                storageCommands.VerifyAll();
                setupSystemTables.VerifyAll();
                cache.VerifyAll();
                storageFactory.VerifyAll();
            }
        }
    }
}
