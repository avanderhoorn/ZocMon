using System;
using NLog;
using NLog.Config;
using NLog.Targets;
using ZocMonLib;

namespace ZocMonLib
{
    public class SystemLoggerProvider : ISystemLoggerProvider
    {
        private readonly ISettings _settings; 
        private readonly LogFactory _factory;

        public SystemLoggerProvider(ISettings configuration)
        {
            _settings = configuration;
            _factory = BuildFactory();
        }
        
        public ISystemLogger CreateLogger()
        {
            return CreateLogger("ZocMon");
        }

        public ISystemLogger CreateLogger(Type name)
        {
            return CreateLogger(name.FullName);
        }

        public ISystemLogger CreateLogger(string name)
        {
            if (!_settings.LoggingEnabled)
                return new SystemLogger(LogManager.CreateNullLogger());

            return new SystemLogger(_factory.GetLogger(name));
        }

        private LogFactory BuildFactory()
        {
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget();
            fileTarget.FileName = String.IsNullOrEmpty(_settings.LoggingDirectory) ? "${basedir}/ZocMon.log" : _settings.LoggingDirectory;
            fileTarget.Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}|${exception:maxInnerExceptionLevel=5:format=type,message,stacktrace:separator=--:innerFormat=shortType,message,method}";
            config.AddTarget("file", fileTarget);

            var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule2);

            return new LogFactory(config);
        }
    }
}
