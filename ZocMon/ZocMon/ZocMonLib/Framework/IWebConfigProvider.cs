using System.Data.Common;

namespace ZocMonLib
{
    public interface IWebConfigProvider
    {
        string GetAppSetting(string name);

        DbProviderFactory GetDbProviderFactory(string name);
    }
}