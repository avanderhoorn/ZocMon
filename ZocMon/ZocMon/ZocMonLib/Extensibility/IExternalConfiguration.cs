namespace ZocMonLib.Extensibility
{
    public interface IExternalConfiguration
    {
        bool Enabled { get; set; }

        bool LoggingEnabled { get; set; }
    }
}
