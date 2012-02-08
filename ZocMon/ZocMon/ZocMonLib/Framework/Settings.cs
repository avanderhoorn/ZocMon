using ZocMonLib.Extensibility;
using ZocMonLib;
using ZocMonLib.Plumbing;

namespace ZocMonLib
{
    public class Settings : ISettings
    {
        public IConfigSeed ConfigSeed { get; private set; }

        public IRecorder Recorder { get; private set; }

        public IRecordFlush RecordFlush { get; private set; }

        public IRecordReduce RecordReduce { get; private set; }


        public ISystemLoggerProvider LoggerProvider { get; private set; }
        
        public bool Enabled { get; set; }

        public bool LoggingEnabled { get; set; }

        public string LoggingDirectory { get; set; }

        public bool Debug { get; set; }

        public IReduceMethodProvider ReduceMethodProvider { get; private set; }

        public IProcessingInstructionProvider ProcessingInstructionProvider { get; private set; }


        public void Initialize()
        {
            Initialize(new SettingsExtensionOptions());
        }

        public void Initialize(SettingsExtensionOptions settingsExtensionOptions)
        {
            //Need to setup the logger first
            LoggerProvider = settingsExtensionOptions.LoggerProvider ?? new SystemLoggerProvider(this);

            var storageFactory = settingsExtensionOptions.StorageFactory;
            if (settingsExtensionOptions.StorageFactory == null)
            {
                var storageFactoryProvider = new StorageFactoryProvider();
                storageFactory = storageFactoryProvider.CreateProvider();
            }
            var storageCommands = settingsExtensionOptions.StorageCommands ?? new StorageCommands(storageFactory, this);
            var storageCommandsSetup = settingsExtensionOptions.StorageCommandsSetup ?? new StorageCommandsSetup();

            var cache = new DataCache();
            var flusherUpdate = new RecordFlushUpdate(cache, storageCommands);
            var reduceStatus = new RecordReduceStatus(new RecordReduceStatusSourceProviderFile(this));
            var reduceAggregate = new RecordReduceAggregate();
            var compare = new RecordCompare(storageCommands, this);

            var setupSystemTables = new SetupSystemTables(cache, storageCommandsSetup, this);
            var setupSystemData = new SetupSystemData(cache, storageCommandsSetup, this);
            var setupSystem = new SetupSystem(setupSystemTables, setupSystemData, storageFactory);
            var defineDefaults = new SetupMonitorConfig(storageCommands, setupSystemTables, cache, storageFactory, this);

            ConfigSeed = new ConfigSeed(cache, this);
            Recorder = new Recorder(cache, this);
            RecordFlush = new RecordFlush(defineDefaults, cache, storageCommands, flusherUpdate, storageFactory, this);
            RecordReduce = new RecordReduce(reduceStatus, reduceAggregate, cache, compare, storageCommands, storageFactory, this);

            ReduceMethodProvider = new ReduceMethodProvider();
            ReduceMethodProvider.Register(new ReduceMethodAccumulate());
            ReduceMethodProvider.Register(new ReduceMethodAverage());
            ProcessingInstructionProvider = new ProcessingInstructionProvider();
            ProcessingInstructionProvider.Register(new ProcessingInstructionAccumulate(storageCommands));
            ProcessingInstructionProvider.Register(new ProcessingInstructionAverage());

            //Run system setup
            setupSystem.Initialize();
        }
    }
}