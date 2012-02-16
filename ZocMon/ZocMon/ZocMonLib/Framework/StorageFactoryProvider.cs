using System;
using System.Configuration;
using System.Data.Common;
using ZocMonLib;

namespace ZocMonLib
{
    public class StorageFactoryProvider : IStorageFactoryProvider
    {
        private const string ZocMonDataSourceKey = "ZocMonDataSourceProvider";
        
        private readonly ISystemLogger _logger;
        private readonly IWebConfigProvider _configProvider;

        public StorageFactoryProvider(IWebConfigProvider configProvider, ISettings settings)
        {
            _logger = settings.LoggerProvider.CreateLogger(typeof(StorageFactoryProvider));
            _configProvider = configProvider;
        }

        public IStorageFactory CreateProvider()
        {
            var providerName = _configProvider.GetAppSetting(ZocMonDataSourceKey); 
            if (string.IsNullOrEmpty(providerName))
            { 
                _logger.Fatal(string.Format("No database config has been provided as the source for the ZocMon data store. Please specify an app config value under {0} which indicates which DbProviderFactory to use.", ZocMonDataSourceKey));
                throw new NullReferenceException(string.Format("App.config setting {0} is empty", ZocMonDataSourceKey)); 
            }

            return CreateProvider(providerName);
        }

        public IStorageFactory CreateProvider(string providerName)
        {
            var innerProvider = _configProvider.GetDbProviderFactory(providerName);
            return new StorageFactory(innerProvider);
        }
    }
}