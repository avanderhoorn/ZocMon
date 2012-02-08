using System;

namespace ZocMonLib
{
    public class Constant
    {
        public static readonly DateTime MinDbDateTime = new DateTime(1753, 1, 1);

        public static readonly MonitorRecord<double> DefaultUpdate = new MonitorRecord<double>(DateTime.MinValue, 0.0);
         
        public const long TicksInMillisecond = 10000;

        public const long MinResolutionForDbWrites = 60 * 1000;

        public const long MsPerDay = 24 * 60 * 60 * 1000;

        public const int MaxConfigNameLength = 116;

        public const int MaxDataPointsPerLine = 2000; // roughly 3 months of hourly data
    }
}