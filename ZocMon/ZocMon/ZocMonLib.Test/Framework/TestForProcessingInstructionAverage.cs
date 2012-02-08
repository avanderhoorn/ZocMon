using Xunit;
using ZocMonLib;

namespace ZocMonLib.Test
{
    public class TestForProcessingInstructionAverage
    {
        public class UsingCalculateExpectedValue
        {
            [Fact]
            public void ShouldBeAbleToUse()
            {
                var processingInstruction = new ProcessingInstructionAverage();
                var result = processingInstruction.CalculateExpectedValues(null, null, null, null);

                Assert.Equal(0, result.Count);
            }
        }
    }
}