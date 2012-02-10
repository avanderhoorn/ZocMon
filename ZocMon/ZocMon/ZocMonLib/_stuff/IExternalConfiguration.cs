namespace ZocMonLib
{
    /// <summary>
    /// NOTE: Not currently been setup, when i get a chance I'll implement.
    /// </summary>
    public interface IExternalConfiguration
    {
        bool Enabled { get; set; }

        bool LoggingEnabled { get; set; }
    }
}
