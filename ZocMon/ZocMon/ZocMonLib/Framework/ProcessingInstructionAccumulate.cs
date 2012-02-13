using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ZocMonLib
{ 
    public class ProcessingInstructionAccumulate : IProcessingInstruction
    {
        public const int WeeksHistory = 4;
        private readonly IStorageCommands _commands;

        public ProcessingInstructionAccumulate(IStorageCommands commands)
        {
            _commands = commands;
        }

        public IList<MonitorRecord<double>> CalculateExpectedValues(string configName, ReduceLevel comparisonReduceLevel, IDictionary<long, MonitorRecordComparison<double>> expectedValues, IDbConnection conn)
        { 
            configName.ThrowIfNull("configName");
            comparisonReduceLevel.ThrowIfNull("comparisonReduceLevel");
            expectedValues.ThrowIfNull("expectedValues");
            conn.ThrowIfNull("conn");
             
            //Pulls out the last comparison data that was calculated
            var comparisonTableName = Support.MakeComparisonName(configName, comparisonReduceLevel.Resolution);
            var lastComparisonList = _commands.SelectListLastComparisonData(comparisonTableName, conn);

            //Get data we need for the processing
            var processingData = GetProcessingData(lastComparisonList, comparisonReduceLevel.Resolution);
             
            //Get the data to be reduced, starting from the last point that was already reduced 
            var reducedTableName = Support.MakeReducedName(configName, comparisonReduceLevel.Resolution);
            var reducedOrdered = _commands.SelectListNeedingToBeReduced(reducedTableName, processingData.LastPrediction != null, processingData.ReducedDataStartTime, conn);

            //Start working through the result
            ProcessResult(comparisonReduceLevel.Resolution, processingData.LastPredictionTime, reducedOrdered, expectedValues);

            return new List<MonitorRecord<double>>(); 
        }

        private ProcessingInstructionData GetProcessingData(IEnumerable<MonitorRecord<double>> lastComparisonList, long resolution)
        {
            var data = new ProcessingInstructionData();

            //We have no reduced data yet
            data.LastPrediction = null;
            data.LastPredictionTime = Constant.MinDbDateTime;
            data.ReducedDataStartTime = DateTime.Now.AddTicks(-TimeSpan.FromDays(WeeksHistory * 7).Ticks);  //Load data from 4 weeks ago 

            //If we have some previous data to work with lets use it
            if (lastComparisonList.Any())
            {
                //We've loaded the last available prediction
                data.LastPrediction = lastComparisonList.First();
                data.LastPredictionTime = data.LastPrediction.TimeStamp + TimeSpan.FromMilliseconds((long)(resolution / 2));
                data.ReducedDataStartTime = data.LastPredictionTime.AddTicks(-TimeSpan.FromDays(WeeksHistory * 7).Ticks);  //Load data from 4 weeks before the last prediction
            }

            return data;
        }

        private void ProcessResult(long reduceResolution, DateTime lastPredictionTime, IEnumerable<MonitorRecord<double>> reducedOrdered, IDictionary<long, MonitorRecordComparison<double>> expectedValues)
        {
            var halfResolution = TimeSpan.FromMilliseconds((long)(reduceResolution / 2));

            //If we have some data to be reduced, reduece it down
            if (reducedOrdered.Any())
            {
                //Loop through and index each to-be-reduced item
                var updates = new Dictionary<long, MonitorRecord<double>>();
                foreach (var update in reducedOrdered)
                    updates.Add(update.TimeStamp.Ticks - halfResolution.Ticks, update);

                //Going between the first date we have and the last date plus 7 days we loop around.
                //The loop increments on what ever the resolution we have for the ReductionLevel provided.
                //Then for each value in the loop we go backwards in time a week at a time for 4 weeks.
                //For each of these weeks we look to see if there is a week in the list passed in that
                //matches. This results in us coming up with the dates in the future that the data for a 
                //given input date should be used for.

                //This is a seems like a really inefficient way of doing this... as in really really bad. 
                //There has to be a better way of doing this. Don't want to change at the moment until the
                //test coverage gets up and I know we aren't breaking anything
                var startTime = lastPredictionTime == Constant.MinDbDateTime ? Support.RoundToResolution(reducedOrdered.Last().TimeStamp, reduceResolution) : lastPredictionTime;
                var endTime = reducedOrdered.Last().TimeStamp.AddDays(7);
                for (var currentTime = startTime; currentTime.Ticks < endTime.Ticks; currentTime = currentTime.AddTicks(reduceResolution * Constant.TicksInMillisecond))
                {
                    var samples = new List<MonitorRecord<double>>();

                    //Starts going backwards in time a week at a time for the number of weeks described
                    for (var i = 0; i < WeeksHistory; ++i)
                    {
                        var timeIndex = currentTime.AddTicks(-(i + 1)*TimeSpan.FromDays(7).Ticks).Ticks;

                        MonitorRecord<double> sample;
                        if (updates.TryGetValue(timeIndex, out sample))
                            samples.Add(sample);
                    }

                    // ReduceMethodAverage the samples
                    if (samples.Count > 0)
                    {
                        long count = 0;
                        double sum = 0;
                        foreach (var sample in samples)
                        {
                            count += sample.Number;
                            sum += sample.Value * sample.Number;
                        }

                        var mean = sum/count;
                        var expectedValue = new MonitorRecordComparison<double> { TimeStamp = currentTime + halfResolution, Value = mean, Number = (int)count };

                        expectedValues.Add(expectedValue.TimeStamp.Ticks, expectedValue);
                    }
                }
            }
        }

        #region Support

        public class ProcessingInstructionData
        {
            public MonitorRecord<double> LastPrediction { get; set; }
            public DateTime LastPredictionTime { get; set; }
            public DateTime ReducedDataStartTime { get; set; }
        }

        #endregion
    }
}
