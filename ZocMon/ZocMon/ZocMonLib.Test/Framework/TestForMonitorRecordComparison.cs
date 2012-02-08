using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ZocMonLib.Test
{
    public class TestForMonitorRecordComparison
    {
        public class UsingProperties
        {
            [Fact]
            public void ShouldReturnNotNullInstances()
            {
                var cache = new MonitorRecordComparison<double>(10);

                Assert.Equal(1, cache.Number);
                Assert.Equal(DateTime.Now.Date, cache.TimeStamp.Date);
                Assert.Equal(10, cache.Value);
            }
        }

        public class UsingCompareTo
        {
            [Fact]
            public void ShouldPickUpSame()
            {
                var date = new DateTime(2012, 06, 06);

                var cache = new MonitorRecordComparison<double> { TimeStamp = date };

                Assert.Equal(0, cache.CompareTo(new MonitorRecord<double>(date, 1)));
            }
            [Fact]
            public void ShouldPickUpHigher()
            {
                var cache = new MonitorRecordComparison<double> { TimeStamp = new DateTime(2012, 06, 06) };

                Assert.Equal(-1, cache.CompareTo(new MonitorRecord<double>(new DateTime(2012, 06, 07), 1)));
            }
            [Fact]
            public void ShouldPickUpLower()
            {
                var cache = new MonitorRecordComparison<double> { TimeStamp = new DateTime(2012, 06, 06) };

                Assert.Equal(1, cache.CompareTo(new MonitorRecord<double>(new DateTime(2012, 06, 05), 1)));
            }
        }
    }
}
