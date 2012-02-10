using System;
using System.Collections.Generic;
using System.Linq;
using ZocMonLib;

namespace ZocMonLib
{
    public class Recorder : IRecorder
    {
        private readonly ISystemLogger _logger;
        private readonly IConfigSeed _configSeed;
        private readonly IDataCache _cache;
        private readonly ISettings _settings;

        public Recorder(IDataCache cache, ISettings settings)
        { 
            _logger = settings.LoggerProvider.CreateLogger(typeof(Recorder));
            _configSeed = settings.ConfigSeed;
            _cache = cache;
            _settings = settings;
        }

        /// <summary>
        /// Record the ocurrance of a given monitor configuration.
        /// </summary>
        /// <param name="configName">Name of the config</param> 
        public void RecordEvent(string configName)
        {
            RecordEvent(configName, MonitorReductionType.DefaultAccumulate);
        }

        /// <summary>
        /// Record the ocurrance of a given monitor configuration.
        /// </summary>
        /// <param name="configName">Name of the config</param> 
        /// <param name="monitorReductionType">Reduction type that is being used</param>
        public void RecordEvent(string configName, MonitorReductionType monitorReductionType)
        {
            Record(configName, DateTime.Now, 1.0, monitorReductionType);
        }

        /// <summary>
        /// Record the given value to the given monitor configuration.
        /// </summary>
        /// <param name="configName">Name of the config</param>
        /// <param name="value">Value that is being recorded</param>
        /// <param name="monitorReductionType">Reduction type that is being used</param>
        public void Record(string configName, double value, MonitorReductionType monitorReductionType)
        {
            Record(configName, DateTime.Now, value, monitorReductionType);
        }

        /// <summary>
        /// Record the given value to the given monitor configuration, for the given time.
        /// </summary>
        /// <param name="configName">Name of the config</param>
        /// <param name="time">Time of the recrod</param>
        /// <param name="value">Value that is being recorded</param>
        /// <param name="monitorReductionType">Reduction type that is being used</param>
        public void Record(string configName, DateTime time, double value, MonitorReductionType monitorReductionType)
        {
            try
            {
                configName.ThrowIfNull("configName");
                value.ThrowIfNaN("value");

                // todo: If this is called too quickly by different threads, there will be a problem....
                MonitorInfo monitorInfo = _configSeed.Seed(configName, monitorReductionType);
                if (monitorInfo == null)
                    throw new Exception("Seed failed to find/create a monitorInfo: \"" + configName + "\"");

                MonitorConfig monitorConfig = monitorInfo.MonitorConfig;
                ReduceLevel reduceLevel = monitorInfo.FirstReduceLevel;
                IReduceMethod<double> reduceMethod = reduceLevel.AggregationClass;
                IList<MonitorRecord<double>> updateList = monitorInfo.MonitorRecords;

                MonitorRecord<double> next;
                lock (updateList)
                {
                    if (reduceLevel.Resolution == 0)
                    {
                        next = reduceMethod.IntervalAggregate(time, _cache.Empty, value);
                        updateList.Add(next);
                    }
                    else
                    {
                        DateTime timeBin;
                        DateTime recordTime;

                        //The core purpose of this logic is just to group all events into one or more time buckets.
                        //As a result the actual time that the record occurd is irrelevent. The only time that 
                        //matters is the first time that occured since the last reduce and even this is rounded 
                        //in accordance with the resolution of the reduceLevel.

                        var last = updateList.LastOrDefault();
                        if (last == null)
                        {
                            //Starting bucket for the events to be aggregated into
                            timeBin = Support.RoundToResolution(time, reduceLevel.Resolution);
                            //As we doing have a previous event to work off 
                            recordTime = timeBin.AddMilliseconds((long)(reduceLevel.Resolution / 2));
                        }
                        else
                        {
                            recordTime = last.TimeStamp;
                            //Since the we didn't keep a record of the original timeBin that was worked out 
                            //when "last == null", we need to derive the original timeBin from the recorded TimeStamp. 
                            timeBin = recordTime.AddMilliseconds(-(long)(reduceLevel.Resolution / 2));
                        }

                        //This logic seems off - it will group like items together per timebin but when the
                        //event time changes beyound the threshold of the current timebin, the timebin isn't
                        //the one that matches the event time, its simply one increment from the last timebin
                        if (last != null && time.Ticks < timeBin.AddMilliseconds(reduceLevel.Resolution).Ticks)
                        {
                            next = reduceMethod.IntervalAggregate(recordTime, last, value);
                            updateList[updateList.Count - 1] = next;
                        }
                        else
                        {
                            if (last != null) recordTime = recordTime.AddMilliseconds(reduceLevel.Resolution);
                            next = reduceMethod.IntervalAggregate(recordTime, _cache.Empty, value);
                            updateList.Add(next);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Fatal("Exception swallowed: ", e);
                if (_settings.Debug)
                    throw;
            }
        }
    }
}