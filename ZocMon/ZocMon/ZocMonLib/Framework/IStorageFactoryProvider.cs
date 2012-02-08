namespace ZocMonLib.Extensibility
{
    public interface IStorageFactoryProvider
    {
        IStorageFactory CreateProvider();

        IStorageFactory CreateProvider(string providerName);
    }
}