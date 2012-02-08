using Xunit;

namespace ZocMonLib.Test
{
    public class TestForDataCache
    {
        public class UsingProperties
        {
            [Fact]
            public void ShouldReturnNotNullInstances()
            {
                var cache = new DataCache();

                Assert.NotNull(cache.Empty);
                Assert.NotNull(cache.MonitorConfigs);
                Assert.NotNull(cache.MonitorInfo);
            }
        }
    }
}
