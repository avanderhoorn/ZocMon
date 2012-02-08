using System.Collections.Concurrent;
using System.Collections.Generic;
using Moq;
using Xunit;
using ZocMonLib;

namespace ZocMonLib.Test
{
    public class TestForConfigSeed
    {
        public class UsingSeed : TestBase
        {
            [Fact]
            public void ShouldBeAbleToPullExistingInfoFromCache()
            {
                var monitorInfo = new MonitorInfo();

                var monitorInfoDictionary = new ConcurrentDictionary<string, MonitorInfo>();
                monitorInfoDictionary.TryAdd("Test", monitorInfo);

                var cache = new Mock<IDataCache>();
                cache.SetupGet(x => x.MonitorInfo).Returns(monitorInfoDictionary);

                var settings = BuildSettings();

                var seeder = new ConfigSeed(cache.Object, settings.Object);
                var result = seeder.Seed("Test", MonitorReductionType.DefaultAccumulate);

                Assert.NotNull(result);
                Assert.Same(monitorInfo, result);
            }

            [Fact]
            public void ShouldBeAbleToPullExistingConfigFromCache()
            { 
                var monitorInfoDictionary = new ConcurrentDictionary<string, MonitorInfo>();

                var reduceLevel = new ReduceLevel();

                var monitorConfig = new MonitorConfig();
                monitorConfig.MonitorReductionType = MonitorReductionType.DefaultAccumulate;
                monitorConfig.ReduceLevels = new List<ReduceLevel> { reduceLevel };

                var monitorConfigsDictionary = new ConcurrentDictionary<string, MonitorConfig>();
                monitorConfigsDictionary.TryAdd("Test", monitorConfig);

                var cache = new Mock<IDataCache>();
                cache.SetupGet(x => x.MonitorInfo).Returns(monitorInfoDictionary);
                cache.SetupGet(x => x.MonitorConfigs).Returns(monitorConfigsDictionary);

                var settings = BuildSettings();

                var seeder = new ConfigSeed(cache.Object, settings.Object);
                var result = seeder.Seed("Test", MonitorReductionType.DefaultAccumulate);

                Assert.NotNull(result);
                Assert.Same(monitorConfig, result.MonitorConfig);
                Assert.Same(reduceLevel, result.FirstReduceLevel);
            }

            [Fact]
            public void ShouldCreatConfigWhenNotRecordExists()
            {
                var monitorInfoDictionary = new ConcurrentDictionary<string, MonitorInfo>();
                var monitorConfigsDictionary = new ConcurrentDictionary<string, MonitorConfig>();

                var cache = new Mock<IDataCache>();
                cache.SetupGet(x => x.MonitorInfo).Returns(monitorInfoDictionary);
                cache.SetupGet(x => x.MonitorConfigs).Returns(monitorConfigsDictionary);

                var reduceMethodProvider = new Mock<IReduceMethodProvider>();
                reduceMethodProvider.Setup(x => x.Retrieve(It.IsAny<string>())).Returns(new Mock<IReduceMethod<double>>().Object);

                var settings = BuildSettings();
                settings.SetupGet(x => x.ReduceMethodProvider).Returns(reduceMethodProvider.Object);

                var seeder = new ConfigSeed(cache.Object, settings.Object);
                var result = seeder.Seed("Test", MonitorReductionType.DefaultAccumulate);

                Assert.NotNull(result);
                Assert.Equal(4, result.MonitorConfig.ReduceLevels.Count);
            }
        }
    }
}