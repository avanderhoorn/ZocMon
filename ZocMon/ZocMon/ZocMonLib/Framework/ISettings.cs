using ZocMonLib.Extensibility;
using ZocMonLib;

namespace ZocMonLib
{
    public interface ISettings
    {
        IConfigSeed ConfigSeed { get; }

        IRecorder Recorder { get; }

        IRecordFlush RecordFlush { get; }

        IRecordReduce RecordReduce { get; }


        ISystemLoggerProvider LoggerProvider { get; }

        bool Enabled { get; set; }

        bool LoggingEnabled { get; set; }

        string LoggingDirectory { get; set; }

        bool Debug { get; set; }

        IReduceMethodProvider ReduceMethodProvider { get; }

        IProcessingInstructionProvider ProcessingInstructionProvider { get; }



        void Initialize();

        void Initialize(SettingsExtensionOptions settingsExtensionOptions);
    }
}