using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Xunit;
using ZocMonLib;

namespace ZocMonLib.Test
{
    public class TestForRecorder
    {
        public class UsingRecord : TestBase
        {
            #region Support

            private MonitorInfo BuildDefaultMonitorInfo(long resolution)
            {
                var monitorRecord = new MonitorRecord<double>(new DateTime(2001, 06, 06, 7, 50, 15, 234), 5);

                var reductionMethod = new Mock<IReduceMethod<double>>();
                reductionMethod.Setup(x => x.IntervalAggregate(It.IsAny<DateTime>(), null, 1)).Returns(monitorRecord);

                var monitorInfo = new MonitorInfo
                    {
                        MonitorRecords = new List<MonitorRecord<double>>(),
                        MonitorConfig = new MonitorConfig(),
                        FirstReduceLevel = new ReduceLevel
                        {
                            Resolution = resolution,
                            //AggregationClass = reductionMethod.Object
                            AggregationClass = new ReduceMethodAccumulate()
                        }
                    };

                return monitorInfo;
            }

            public class RecordMocks
            {
                public Mock<IConfigSeed> Seeder { get; set; }

                public Mock<ISettings> Settings { get; set; }

                public Mock<IDataCache> Cache { get; set; }
            }

            public RecordMocks BuildRecordMocks(MonitorInfo monitorInfo)
            {
                var seeder = new Mock<IConfigSeed>();
                seeder.Setup(x => x.Seed("Test", MonitorReductionType.DefaultAccumulate)).Returns(monitorInfo);

                var settings = BuildSettings();
                settings.SetupGet(x => x.ConfigSeed).Returns(seeder.Object);

                var cache = new Mock<IDataCache>();
                cache.SetupGet(x => x.Empty).Returns(new MonitorRecord<double>());

                return new RecordMocks { Cache = cache, Settings = settings, Seeder = seeder };
            }

            #endregion

            [Fact]
            public void ShouldThrowExceptionWhenNoSeedConfigIsFound()
            {
                var seeder = new Mock<IConfigSeed>();
                seeder.Setup(x => x.Seed("Test", MonitorReductionType.DefaultAccumulate)).Verifiable();

                var settings = BuildSettings();
                settings.SetupGet(x => x.Debug).Returns(true);
                settings.SetupGet(x => x.ConfigSeed).Returns(seeder.Object);

                var cache = new Mock<IDataCache>();

                var recorder = new Recorder(cache.Object, settings.Object);

                Assert.Throws<Exception>(() => recorder.Record("Test", DateTime.Now, 1, MonitorReductionType.DefaultAccumulate));
                
                seeder.VerifyAll();
            }

            [Fact]
            public void WhenResolutionIsZeroShouldAddToUpdateList()
            {
                var monitorInfo = BuildDefaultMonitorInfo(0);
                var recordMocks = BuildRecordMocks(monitorInfo);

                var recorder = new Recorder(recordMocks.Cache.Object, recordMocks.Settings.Object);
                recorder.RecordEvent("Test");

                Assert.Equal(monitorInfo.MonitorRecords.Count, 1);
            }

            [Fact]
            public void WhenHasResolutionAllLikeTimesShouldBeGrouped()
            {
                var monitorInfo = BuildDefaultMonitorInfo(1000);
                var recordMocks = BuildRecordMocks(monitorInfo);
                
                var recorder = new Recorder(recordMocks.Cache.Object, recordMocks.Settings.Object);
                recorder.Record("Test", new DateTime(1995, 06, 06, 1, 32, 23, 452), 1, MonitorReductionType.DefaultAccumulate);
                recorder.Record("Test", new DateTime(1995, 06, 06, 1, 32, 23, 453), 1, MonitorReductionType.DefaultAccumulate);
                recorder.Record("Test", new DateTime(1995, 06, 06, 1, 32, 23, 454), 1, MonitorReductionType.DefaultAccumulate); 

                Assert.Equal(monitorInfo.MonitorRecords.Count, 1);
                Assert.Equal(monitorInfo.MonitorRecords[0].Number, 3);
            }

            [Fact]
            public void WhenHasResolutionAllUnlikeTimesShouldNotBeGrouped()
            {
                var monitorInfo = BuildDefaultMonitorInfo(1000);
                var recordMocks = BuildRecordMocks(monitorInfo);

                var recorder = new Recorder(recordMocks.Cache.Object, recordMocks.Settings.Object);
                recorder.Record("Test", new DateTime(1995, 06, 06, 1, 32, 23, 267), 1, MonitorReductionType.DefaultAccumulate);
                recorder.Record("Test", new DateTime(1995, 06, 06, 1, 32, 23, 452), 1, MonitorReductionType.DefaultAccumulate);
                recorder.Record("Test", new DateTime(2000, 06, 06, 7, 50, 15, 234), 1, MonitorReductionType.DefaultAccumulate);
                recorder.Record("Test", new DateTime(2012, 12, 14, 1, 23, 12, 124), 1, MonitorReductionType.DefaultAccumulate);

                Assert.Equal(monitorInfo.MonitorRecords.Count, 3);
                Assert.Equal(monitorInfo.MonitorRecords[0].Number, 2);
                Assert.Equal(monitorInfo.MonitorRecords[1].Number, 1);
            }
        }
    }
}
