using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Moq;
using Xunit;
using ZocMonLib;

namespace ZocMonLib.Test
{
    public class TestForRecordReduce : TestBase
    {   
        #region Support

        private class TestData
        {
            public int DeletedCount { get; set; }
            public int CalculateCount { get; set; } 
            public int AggregateCount { get; set; }
            public int UpdateIfExistsCount { get; set; }

            public Mock<IDbConnection> Connection { get; set; }
            public Mock<IRecordReduceStatus> ReduceStatus { get; set; }
            public Mock<IRecordReduceAggregate> ReduceAggregater { get; set; }
            public Mock<IDataCache> Cache { get; set; }
            public Mock<IConfigSeed> Seeder { get; set; }
            public Mock<IRecordCompare> ComparisonsData { get; set; }
            public Mock<IStorageCommands> Storage { get; set; }
            public Mock<IStorageFactory> DbProviderFactory { get; set; } 
            public Mock<ISettings> Settings { get; set; }

            public void Verify()
            { 
                ReduceStatus.VerifyAll();
                Cache.VerifyAll();
                Seeder.VerifyAll();
                Connection.VerifyAll();
                DbProviderFactory.VerifyAll();
                Storage.VerifyAll();
                ComparisonsData.VerifyAll(); 
            }
        }

        private static TestData PopulateTestData(bool isAll)
        {
            var testData = new TestData();

            var reduceStatus = new Mock<IRecordReduceStatus>();
            reduceStatus.Setup(x => x.IsReducing()).Returns(false).Verifiable();
            reduceStatus.Setup(x => x.DoneReducing()).Verifiable();


            var reductionLevel1 = new ReduceLevel { Resolution = 1000, AggregationClass = new ReduceMethodAccumulate() };
            var reductionLevel2 = new ReduceLevel { Resolution = 60000, AggregationClass = new ReduceMethodAccumulate() };

            var monitorConfigSingle = new MonitorConfig { Name = "Single", ReduceLevels = new List<ReduceLevel> { reductionLevel1, reductionLevel2 } };
            var monitorInfoSingle = new MonitorInfo { MonitorConfig = monitorConfigSingle };
             
            var monitorInfoDictionary = new ConcurrentDictionary<string, MonitorInfo>();

            var monitorConfigDictionary = new ConcurrentDictionary<string, MonitorConfig>();
            monitorConfigDictionary.TryAdd("Single", monitorConfigSingle);

            var seeder = new Mock<IConfigSeed>();
            seeder.Setup(x => x.Seed("Single", MonitorReductionType.Custom)).Returns(monitorInfoSingle).Verifiable();


            var cache = new Mock<IDataCache>();
            cache.SetupGet(x => x.MonitorInfo).Returns(monitorInfoDictionary).Verifiable();
            if (isAll)
                cache.SetupGet(x => x.MonitorConfigs).Returns(monitorConfigDictionary).Verifiable();


            var connection = new Mock<IDbConnection>();
            connection.Setup(x => x.Open()).Verifiable();
            if (!isAll)
                connection.Setup(x => x.Close()).Verifiable();
            var connectionInstance = connection.Object;

            var dbProviderFactory = new Mock<IStorageFactory>();
            dbProviderFactory.Setup(x => x.CreateConnection()).Returns(connectionInstance).Verifiable();


            var requiringReduction = new List<MonitorRecord<double>> { new MonitorRecord<double>(new DateTime(2012, 1, 15, 6, 30, 5), 1), new MonitorRecord<double>(new DateTime(2012, 1, 15, 6, 30, 10), 2), new MonitorRecord<double>(new DateTime(2012, 1, 15, 6, 30, 15), 4) };
            var reducedRecord = new MonitorRecord<double>(new DateTime(2012, 1, 15, 6, 30, 30), 7);
            var reduced = new StorageLastReduced { Record = reducedRecord, Time = new DateTime(2012, 1, 15, 6, 30, 0) };

            var storage = new Mock<IStorageCommands>();
            storage.Setup(x => x.RetrieveLastReducedData("SingleMinutelyData", 60000, connectionInstance)).Returns(reduced).Verifiable();
            storage.Setup(x => x.SelectListRequiringReduction("SingleSecondlyData", true, new DateTime(2012, 1, 15, 6, 30, 0), connectionInstance)).Returns(requiringReduction).Verifiable();
            storage.Setup(x => x.UpdateIfExists("SingleMinutelyData", It.IsAny<MonitorRecord<double>>(), true, connectionInstance)).Callback(() => testData.UpdateIfExistsCount++).Returns(true).Verifiable();
            storage.Setup(x => x.Flush("SingleMinutelyData", It.IsAny<IEnumerable<MonitorRecord<double>>>(), connectionInstance)).Verifiable();
            storage.Setup(x => x.ClearReducedData("Single", It.IsAny<DateTime>(), It.IsAny<ReduceLevel>(), connectionInstance)).Callback(() => testData.DeletedCount++).Verifiable();


            var reduceAggregater = new Mock<IRecordReduceAggregate>();
            reduceAggregater.Setup(x => x.Aggregate(new DateTime(2012, 1, 15, 6, 30, 0), reductionLevel2, requiringReduction, It.IsAny<IDictionary<DateTime, IList<MonitorRecord<double>>>>()))
                .Callback(() => testData.AggregateCount++)
                .Returns((DateTime lastReductionTime, ReduceLevel targetReduceLevel, IList<MonitorRecord<double>> sourceAggregationList, IDictionary<DateTime, IList<MonitorRecord<double>>> destinationAggregatedList) => { destinationAggregatedList.Add(new DateTime(2012, 1, 15, 6, 30, 0), requiringReduction); return new DateTime(2012, 1, 15, 6, 30, 0); });


            var comparisonsData = new Mock<IRecordCompare>();
            comparisonsData.Setup(x => x.CalculateComparisons("Single", monitorInfoSingle, It.IsAny<ReduceLevel>(), connectionInstance)).Callback(() => testData.CalculateCount++).Verifiable();


            var settings = BuildSettings();
            settings.Setup(x => x.ConfigSeed).Returns(seeder.Object);


            testData.Settings = settings;
            testData.Connection = connection;
            testData.ReduceStatus = reduceStatus;
            testData.ReduceAggregater = reduceAggregater;
            testData.Cache = cache;
            testData.Seeder = seeder;
            testData.ComparisonsData = comparisonsData;
            testData.Storage = storage;
            testData.DbProviderFactory = dbProviderFactory;

            return testData;
        }

        #endregion

        //NOTE these aren't really good tests as I am making no assertions, but 

        public class UsingReduceAll
        {
            [Fact]
            public void ShouldNotRunIfWeAreAlreadyReducing()
            {
                var reduceStatus = new Mock<IRecordReduceStatus>();
                reduceStatus.Setup(x => x.IsReducing()).Returns(true).Verifiable();

                var settings = BuildSettings();

                var reducer = new RecordReduce(reduceStatus.Object, null, null, null, null, null, settings.Object);
                reducer.ReduceAll();

                reduceStatus.VerifyAll();
            }

            [Fact]
            public void ShouldBeAbleToReduce()
            {
                var testData = PopulateTestData(true);

                var reducer = new RecordReduce(testData.ReduceStatus.Object, testData.ReduceAggregater.Object, testData.Cache.Object, testData.ComparisonsData.Object, testData.Storage.Object, testData.DbProviderFactory.Object, testData.Settings.Object);
                reducer.ReduceAll(true);

                Assert.Equal(1, testData.AggregateCount);
                Assert.Equal(1, testData.UpdateIfExistsCount);
                Assert.Equal(2, testData.CalculateCount); 
                Assert.Equal(1, testData.DeletedCount); 

                testData.Verify();
            }
        }

        public class UsingReduce
        {
            [Fact]
            public void ShouldNotRunIfWeAreAlreadyReducing()
            {
                var reduceStatus = new Mock<IRecordReduceStatus>();
                reduceStatus.Setup(x => x.IsReducing()).Returns(true).Verifiable();

                var settings = BuildSettings();

                var reducer = new RecordReduce(reduceStatus.Object, null, null, null, null, null, settings.Object);
                reducer.Reduce("");

                reduceStatus.VerifyAll();
            }

            [Fact]
            public void ShouldBeAbleToReduce()
            {
                var testData = PopulateTestData(false);

                var reducer = new RecordReduce(testData.ReduceStatus.Object, testData.ReduceAggregater.Object, testData.Cache.Object, testData.ComparisonsData.Object, testData.Storage.Object, testData.DbProviderFactory.Object, testData.Settings.Object);
                reducer.Reduce("Single", true); 

                Assert.Equal(1, testData.AggregateCount);
                Assert.Equal(1, testData.UpdateIfExistsCount); 
                Assert.Equal(2, testData.CalculateCount);  
                Assert.Equal(1, testData.DeletedCount); 

                testData.Verify();
            }
        }
    }
}