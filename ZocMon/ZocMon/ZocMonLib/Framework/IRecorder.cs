using System;
using ZocMonLib;

namespace ZocMonLib
{
    public interface IRecorder
    {
        /// <summary>
        /// Record the ocurrance of a given monitor configuration.
        /// </summary>
        /// <param name="configName">Name of the config</param> 
        void RecordEvent(string configName);

        /// <summary>
        /// Record the ocurrance of a given monitor configuration.
        /// </summary>
        /// <param name="configName">Name of the config</param> 
        /// <param name="monitorReductionType">Reduction type that is being used</param>
        void RecordEvent(string configName, MonitorReductionType monitorReductionType);

        /// <summary>
        /// Record the given value to the given monitor configuration.
        /// </summary>
        /// <param name="configName">Name of the config</param>
        /// <param name="value">Value that is being recorded</param>
        /// <param name="monitorReductionType">Reduction type that is being used</param>
        void Record(string configName, double value, MonitorReductionType monitorReductionType);

        /// <summary>
        /// Record the given value to the given monitor configuration, for the given time.
        /// </summary>
        /// <param name="configName">Name of the config</param>
        /// <param name="time">Time of the recrod</param>
        /// <param name="value">Value that is being recorded</param>
        /// <param name="monitorReductionType">Reduction type that is being used</param>
        void Record(string configName, DateTime time, double value, MonitorReductionType monitorReductionType);
    }
}