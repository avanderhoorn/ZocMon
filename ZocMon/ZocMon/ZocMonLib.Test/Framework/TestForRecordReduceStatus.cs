using System;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace ZocMonLib.Test
{
    public class TestForRecordReduceStatus
    {
        public class UsingIsReducing
        {
            [Fact]
            public void ShouldTurnOnIfOff()
            {
                var sourceProvider = new Mock<IRecordReduceStatusSourceProvider>();
                sourceProvider.Setup(x => x.ReadValue()).Returns("0").Verifiable();
                sourceProvider.Setup(x => x.WriteValue("1")).Verifiable();

                var dataStatus = new RecordReduceStatus(sourceProvider.Object);
                var result = dataStatus.IsReducing(); 

                Assert.Equal(false, result);

                sourceProvider.VerifyAll();
            }

            [Fact]
            public void ShouldReportOnIfOn()
            {
                var sourceProvider = new Mock<IRecordReduceStatusSourceProvider>();
                sourceProvider.Setup(x => x.ReadValue()).Returns("1").Verifiable(); 

                var dataStatus = new RecordReduceStatus(sourceProvider.Object);
                var result = dataStatus.IsReducing();

                Assert.Equal(true, result);

                sourceProvider.VerifyAll();
            }
        }

        public class UsingDoneReducing
        {
            [Fact]
            public void ShouldSaveValue()
            {
                var sourceProvider = new Mock<IRecordReduceStatusSourceProvider>(); 
                sourceProvider.Setup(x => x.WriteValue("0")).Verifiable();

                var dataStatus = new RecordReduceStatus(sourceProvider.Object);
                dataStatus.DoneReducing();

                sourceProvider.VerifyAll();
            }
        }
    }
}