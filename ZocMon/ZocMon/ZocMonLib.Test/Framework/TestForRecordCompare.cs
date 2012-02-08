using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Moq;
using Xunit;

namespace ZocMonLib.Test
{
    public class TestForRecordCompare 
    {
        public class UsingCalculateComparisons : TestBase
        {
            [Fact]
            public void ShouldDoNothingWhenComparisonCalculatorIsNull()
            {
                var monitorInfo = new MonitorInfo {MonitorConfig = new MonitorConfig()};

                var settings = BuildSettings();

                var compare = new RecordCompare(null, settings.Object);
                compare.CalculateComparisons("Test", monitorInfo, null, null);
            }

            [Fact]
            public void ShouldRun()
            {
                var level = new ReduceLevel { Resolution = 1000 };
                var connection = new Mock<IDbConnection>().Object; 

                var calculator = new Mock<IProcessingInstruction>();
                calculator.Setup(x => x.CalculateExpectedValues("Test", level, It.IsAny<IDictionary<long, MonitorRecordComparison<double>>>(), connection)).Verifiable();

                var monitorInfo = new MonitorInfo { MonitorConfig = new MonitorConfig { ComparisonCalculator = calculator.Object } };


                var settings = BuildSettings();


                var commands = new Mock<IStorageCommands>();
                commands.Setup(x => x.Insert("TestSecondlyData", It.IsAny<IEnumerable<MonitorRecord<double>>>(), connection, null));

                var compare = new RecordCompare(commands.Object, settings.Object);
                compare.CalculateComparisons("Test", monitorInfo, level, connection);

                calculator.VerifyAll();
            }
        }
    }
}
