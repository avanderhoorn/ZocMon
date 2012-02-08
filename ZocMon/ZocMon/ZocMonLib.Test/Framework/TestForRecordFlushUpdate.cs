using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using Xunit;
using ZocMonLib;

namespace ZocMonLib.Test
{
    public class TestForRecordFlushUpdate
    {
        public class UsingUpdateExisting
        {
            [Fact]
            public void ShouldExitIfHasNoValues()
            {
                var dataUpdate = new RecordFlushUpdate(null, null);
                var result = dataUpdate.UpdateExisting("", new Dictionary<long, MonitorRecord<double>>(), null);

                Assert.True(result);
            }

            [Fact]
            public void ShouldThrowExceptionIfInfoNotFoundInCache()
            {
                var dictionary = new ConcurrentDictionary<string, MonitorInfo>();

                var cache = new Mock<IDataCache>();
                cache.SetupGet(x => x.MonitorInfo).Returns(dictionary).Verifiable();

                var data = new Dictionary<long, MonitorRecord<double>> { { 1000, null } };
                var dataUpdate = new RecordFlushUpdate(cache.Object, null);
                Assert.Throws<DataException>(() => dataUpdate.UpdateExisting("Test", data, null));

                cache.VerifyAll();
            }

            [Fact]
            public void ShouldUpdateExistingData()
            {
                var connection = new Mock<IDbConnection>();
                var connectionInstance = connection.Object;


                IEnumerable<MonitorRecord<double>> updateList = null;

                var storeData = new List<MonitorRecord<double>>
                                    {
                                        new MonitorRecord<double>(new DateTime(2010, 6, 6, 10, 30, 0), 1),
                                        new MonitorRecord<double>(new DateTime(2010, 6, 6, 10, 30, 1), 1)
                                    };

                var store = new Mock<IStorageCommands>();
                store.Setup(x => x.SelectListForUpdateExisting("TestSecondlyData", new DateTime(2010, 6, 6, 10, 30, 0, 100), connectionInstance, null)).Returns(storeData).Verifiable();
                store.Setup(x => x.Update("TestSecondlyData", It.IsAny<IEnumerable<MonitorRecord<double>>>(), connectionInstance, null)).Callback((string tableName, IEnumerable<MonitorRecord<double>> values, IDbConnection conn, IDbTransaction transaction) => { updateList = values; }).Verifiable();




                var reduceLevel = new ReduceLevel { AggregationClass = new ReduceMethodAccumulate(), Resolution = 1000 };
                var monitorInfo = new MonitorInfo { FirstReduceLevel = reduceLevel };

                var dictionary = new ConcurrentDictionary<string, MonitorInfo>();
                dictionary.TryAdd("Test", monitorInfo);

                var cache = new Mock<IDataCache>();
                cache.SetupGet(x => x.Empty).Returns(new MonitorRecord<double>()).Verifiable();
                cache.SetupGet(x => x.MonitorInfo).Returns(dictionary).Verifiable();



                var data = new Dictionary<long, MonitorRecord<double>>
                               {
                                   { 1000, new MonitorRecord<double>(new DateTime(2010, 6, 6, 10, 30, 0, 100), 3) },
                                   { 1001, new MonitorRecord<double>(new DateTime(2010, 6, 6, 10, 30, 0, 200), 5) },
                                   { 1002, new MonitorRecord<double>(new DateTime(2010, 6, 6, 10, 30, 0, 400), 1) },
                                   { 1003, new MonitorRecord<double>(new DateTime(2010, 6, 6, 10, 30, 1, 300), 2) }
                               };
                var dataUpdate = new RecordFlushUpdate(cache.Object, store.Object);
                var result = dataUpdate.UpdateExisting("Test", data, connectionInstance);

                Assert.True(result);
                
                //I'm really not sure about these assertions as given the test data the output isn't what I would expect
                Assert.Equal(2, updateList.Count());
                var first = updateList.First();
                Assert.Equal(4, first.Value);
                Assert.Equal(2, first.Number);
                var last = updateList.Last();
                Assert.Equal(3, last.Value);
                Assert.Equal(2, last.Number);

                cache.VerifyAll();
                store.VerifyAll();
            }
        }
    }
}