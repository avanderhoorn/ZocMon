namespace ZocMonLib
{
    public interface IStorageFactoryProvider
    {
        IStorageFactory CreateProvider();

        IStorageFactory CreateProvider(string providerName);
    }
}