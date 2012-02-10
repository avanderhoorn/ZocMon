using System.Configuration;
using System.Data.Common;
using ZocMonLib;

namespace ZocMonLib
{
    public class StorageFactoryProvider : IStorageFactoryProvider
    {
        private const string ZocMonDataSourceKey = "ZocMonDataSourceProvider";

        public IStorageFactory CreateProvider()
        {
            var providerName = ConfigurationManager.AppSettings[ZocMonDataSourceKey];
            return CreateProvider(providerName);
        }

        public IStorageFactory CreateProvider(string providerName)
        {
            var innerProvider = DbProviderFactories.GetFactory(providerName);
            return new StorageFactory(innerProvider);
        }
    }
}