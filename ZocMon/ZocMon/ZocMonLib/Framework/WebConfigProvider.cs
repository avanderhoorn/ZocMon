using System.Configuration;
using System.Data.Common;

namespace ZocMonLib
{
    public class WebConfigProvider : IWebConfigProvider
    {
        public string GetAppSetting(string name)
        {
            return ConfigurationManager.AppSettings[name]; 
        }

        public DbProviderFactory GetDbProviderFactory(string name)
        {
            return DbProviderFactories.GetFactory(name);
        }
    }
}