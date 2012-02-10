using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Moq;
using Xunit;
using ZocMonLib;

namespace ZocMonLib.Test
{
    public class TestForRecordFlush : TestBase
    {
        #region Support

        private class TestData
        {
            public Mock<IDataCache> Cache { get; set; }
            public Mock<IStorageCommands> Storage { get; set; }
            public Mock<IRecordFlushUpdate> Logic { get; set; }
            public Mock<IStorageFactory> DbProviderFactory { get; set; }

            public Mock<IDbConnection> Connection { get; set; }
            public Mock<IDbTransaction> Transaction { get; set; }

            public Mock<ISetupMonitorConfig> DefineDefaults { get; set; }

            public Mock<ISettings> Settings { get; set; }

            public void Verify()
            { 
                Connection.VerifyAll();
                Transaction.VerifyAll();
                Storage.VerifyAll();
                Logic.VerifyAll();
            }
        }

        private static TestData PopulateTestData(bool populateAll)
        {
            //Define Defaults
            var defineDefaults = new Mock<ISetupMonitorConfig>();
            defineDefaults.Setup(x => x.CreateDefaultReduceLevels(It.IsAny<MonitorConfig>(), It.IsAny<List<ReduceLevel>>(), It.IsAny<IDbConnection>())).Verifiable();

            //Cache
            var monitorInfoDictionary = new ConcurrentDictionary<string, MonitorInfo>();
            var monitorConfigsDictionary = new ConcurrentDictionary<string, MonitorConfig>();

            var reductionLevel = new ReduceLevel { Resolution = 1000 };
            var monitorConfig = new MonitorConfig { Name = "Test", ReduceLevels = new List<ReduceLevel> { reductionLevel } };
            var monitorInfo = new MonitorInfo { TablesCreated = false, MonitorRecords = new List<MonitorRecord<double>> { new MonitorRecord<double>(DateTime.Now, 5) }, MonitorConfig = monitorConfig };

            monitorInfoDictionary.TryAdd("Test", monitorInfo);
            monitorConfigsDictionary.TryAdd("Test", monitorConfig);
             
            reductionLevel = new ReduceLevel { Resolution = 1000 };
            monitorConfig = new MonitorConfig { Name = "Jester", ReduceLevels = new List<ReduceLevel> { reductionLevel } };
            monitorInfo = new MonitorInfo { TablesCreated = false, MonitorRecords = new List<MonitorRecord<double>> { new MonitorRecord<double>(DateTime.Now, 5) }, MonitorConfig = monitorConfig };

            monitorInfoDictionary.TryAdd("Jester", monitorInfo);
            monitorConfigsDictionary.TryAdd("Jester", monitorConfig); 

            var cache = new Mock<IDataCache>();
            cache.SetupGet(x => x.MonitorInfo).Returns(monitorInfoDictionary);
            cache.SetupGet(x => x.MonitorConfigs).Returns(monitorConfigsDictionary);

            //Storage
            var storage = new Mock<IStorageCommands>();
            storage.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<IEnumerable<MonitorRecord<double>>>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>())).Verifiable();

            //Logic
            var logic = new Mock<IRecordFlushUpdate>();
            logic.Setup(x => x.UpdateExisting(It.IsAny<string>(), It.IsAny<SortedDictionary<long, MonitorRecord<double>>>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>())).Verifiable();

            //Db Provider
            var transaction = new Mock<IDbTransaction>();
            transaction.SetupGet(x => x.IsolationLevel).Returns(IsolationLevel.Serializable).Verifiable();
            transaction.Setup(x => x.Rollback()).Verifiable();
            transaction.Setup(x => x.Commit()).Verifiable();

            var connection = new Mock<IDbConnection>();
            connection.Setup(x => x.Open()).Verifiable();
            connection.Setup(x => x.BeginTransaction()).Returns(transaction.Object).Verifiable();
            connection.Setup(x => x.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(transaction.Object).Verifiable();

            var dbProviderFactory = new Mock<IStorageFactory>();
            dbProviderFactory.Setup(x => x.CreateConnection()).Returns(connection.Object).Verifiable();

            //Settings 
            var settings = BuildSettings();

            return new TestData { Settings = settings, Cache = cache, DbProviderFactory = dbProviderFactory, Logic = logic, Storage = storage, Transaction = transaction, Connection = connection, DefineDefaults = defineDefaults };
        }

        #endregion

        public class UsingFlush
        {
            [Fact]
            public void ShouldBeAbleToFlush()
            {
                var testData = PopulateTestData(false);

                testData.Connection.Setup(x => x.Close()).Verifiable();

                var flusher = new RecordFlush(testData.DefineDefaults.Object, testData.Cache.Object, testData.Storage.Object, testData.Logic.Object, testData.DbProviderFactory.Object, testData.Settings.Object);
                flusher.Flush("Test");

                testData.Verify();
            }

            [Fact]
            public void ShouldThrowExceptionIfInfoNotFound()
            {
                var settings = BuildSettings();
                settings.SetupGet(x => x.Debug).Returns(true); 

                var monitorInfoDictionary = new ConcurrentDictionary<string, MonitorInfo>();

                var cache = new Mock<IDataCache>();
                cache.SetupGet(x => x.MonitorInfo).Returns(monitorInfoDictionary);

                var flusher = new RecordFlush(null, cache.Object, null, null, null, settings.Object);
                Assert.Throws<ArgumentException>(() => flusher.Flush("Test"));
            }

            [Fact]
            public void ShouldThrowExceptionIfMatchingConfigForInfoNotFound()
            {
                var settings = BuildSettings();
                settings.SetupGet(x => x.Debug).Returns(true); 

                var monitorConfigsDictionary = new ConcurrentDictionary<string, MonitorConfig>();
                var monitorInfoDictionary = new ConcurrentDictionary<string, MonitorInfo>();
                monitorInfoDictionary.TryAdd("Test", new MonitorInfo { TablesCreated = true });

                var cache = new Mock<IDataCache>();
                cache.SetupGet(x => x.MonitorInfo).Returns(monitorInfoDictionary);
                cache.SetupGet(x => x.MonitorConfigs).Returns(monitorConfigsDictionary);

                var flusher = new RecordFlush(null, cache.Object, null, null, null, settings.Object);
                Assert.Throws<ArgumentException>(() => flusher.Flush("Test"));
            }
        }

        public class UsingFlushAll
        {
            [Fact]
            public void ShouldBeAbleToFlushAll()
            {
                var testData = PopulateTestData(true);

                var flusher = new RecordFlush(testData.DefineDefaults.Object, testData.Cache.Object, testData.Storage.Object, testData.Logic.Object, testData.DbProviderFactory.Object, testData.Settings.Object);
                flusher.FlushAll();

                testData.Verify();
            }
        }
    }
}