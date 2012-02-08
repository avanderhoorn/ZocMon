using System;
using ZocMonLib;

namespace ZocMonLib
{
    /// <summary>
    /// Class of static methods for recording and reducing time series data.
    /// </summary>
    public static class ZocMon
    { 
        static ZocMon()
        {
            Settings = new Settings();   
        }

        /// <summary>
        /// Configuration settings for the system
        /// </summary>
        public static ISettings Settings { get; private set; }

        /// <summary>
        /// Record the ocurrance of a given monitor configuration.
        /// </summary>
        /// <param name="configName">Name of the config</param> 
        public static void RecordEvent(string configName)
        {
            Settings.Recorder.RecordEvent(configName);
        }

        /// <summary>
        /// Record the ocurrance of a given monitor configuration.
        /// </summary>
        /// <param name="configName">Name of the config</param> 
        /// <param name="monitorReductionType">Reduction type that is being used</param>
        public static void RecordEvent(string configName, MonitorReductionType monitorReductionType)
        {
            Settings.Recorder.RecordEvent(configName, monitorReductionType);
        }

        /// <summary>
        /// Record the given value to the given monitor configuration.
        /// </summary>
        /// <param name="configName">Name of the config</param>
        /// <param name="value">Value that is being recorded</param>
        /// <param name="monitorReductionType">Reduction type that is being used</param>
        public static void Record(string configName, double value, MonitorReductionType monitorReductionType)
        {
            Settings.Recorder.Record(configName, value, monitorReductionType);
        }

        /// <summary>
        /// Record the given value to the given monitor configuration, for the given time.
        /// </summary>
        /// <param name="configName">Name of the config</param>
        /// <param name="time">Time of the recrod</param>
        /// <param name="value">Value that is being recorded</param>
        /// <param name="monitorReductionType">Reduction type that is being used</param>
        public static void Record(string configName, DateTime time, double value, MonitorReductionType monitorReductionType)
        {
            Settings.Recorder.Record(configName, time, value, monitorReductionType);
        }

        /// <summary>
        /// Seed the local data for a monitor configuration.
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="monitorReductionType"></param>
        /// <returns></returns>
        public static MonitorInfo Seed(string configName, MonitorReductionType monitorReductionType)
        {
            return Settings.ConfigSeed.Seed(configName, monitorReductionType);
        }
    }
}