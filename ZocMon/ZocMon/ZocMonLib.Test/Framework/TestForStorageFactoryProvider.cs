using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Moq;
using Xunit;

namespace ZocMonLib.Test
{
    public class TestForStorageFactoryProvider : TestBase
    {
        public class UsingCreateProvider
        {
            [Fact]
            public void ShouldThrowExceptionWhenConfigValueIsPresent()
            {
                var settings = BuildSettings();

                var configProvider = new Mock<IWebConfigProvider>();
                configProvider.Setup(x => x.GetAppSetting("ZocMonDataSourceProvider")).Returns("").Verifiable();

                var provider = new StorageFactoryProvider(configProvider.Object, settings.Object);
                Assert.Throws<NullReferenceException>(() => provider.CreateProvider()); 

                configProvider.VerifyAll();
            }

            [Fact]
            public void ShouldReturnInstance()
            {
                var settings = BuildSettings();

                var configProvider = new Mock<IWebConfigProvider>();
                configProvider.Setup(x => x.GetAppSetting("ZocMonDataSourceProvider")).Returns("Test").Verifiable();
                configProvider.Setup(x => x.GetDbProviderFactory("Test")).Returns((DbProviderFactory)null).Verifiable();

                var provider = new StorageFactoryProvider(configProvider.Object, settings.Object);
                var result = provider.CreateProvider();

                Assert.NotNull(result);

                configProvider.VerifyAll();
            }
        }
    }
}
