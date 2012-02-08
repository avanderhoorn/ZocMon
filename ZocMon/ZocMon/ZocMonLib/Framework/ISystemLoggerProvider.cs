using System;

namespace ZocMonLib
{
    public interface ISystemLoggerProvider
    {
        ISystemLogger CreateLogger();

        ISystemLogger CreateLogger(Type name);

        ISystemLogger CreateLogger(string name);
    }
}
