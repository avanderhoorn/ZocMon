using ZocMonLib;

namespace ZocMonLib
{
    public class SettingsExtensionOptions
    {
        public IStorageCommands StorageCommands { get; set; }

        public IStorageCommandsSetup StorageCommandsSetup { get; set; }

        public IStorageFactory StorageFactory { get; set; }

        public ISystemLoggerProvider LoggerProvider { get; set; }
    }
}
