using System;
using Moq;
using ZocMonLib;

namespace ZocMonLib.Test
{
    public class TestBase
    {
        protected static Mock<ISettings> BuildSettings()
        {
            var systemLogging = new Mock<ISystemLogger>();

            var systemLoggingProvider = new Mock<ISystemLoggerProvider>();
            systemLoggingProvider.Setup(x => x.CreateLogger(It.IsAny<Type>())).Returns(systemLogging.Object);

            var settings = new Mock<ISettings>();
            settings.SetupGet(x => x.LoggerProvider).Returns(systemLoggingProvider.Object);

            return settings;
        }
    }
}