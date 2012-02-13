using System;
using System.Collections.Generic;
using System.Linq;

namespace ZocMonLib
{
    public class RecordReduceAggregate : IRecordReduceAggregate
    { 
        public DateTime Aggregate(DateTime lastReductionTime, ReduceLevel targetReduceLevel, IEnumerable<MonitorRecord<double>> sourceAggregationList, IDictionary<DateTime, IList<MonitorRecord<double>>> destinationAggregatedList)
        {
            //Nothing to do....
            if (!sourceAggregationList.Any()) 
                return Constant.MinDbDateTime;

            var resolutionSpan = TimeSpan.FromMilliseconds(targetReduceLevel.Resolution);
            var halfResolutionSpan = TimeSpan.FromMilliseconds((long)(targetReduceLevel.Resolution / 2));

            //We've already selected for data that's not yet been reduced, but this is needed when there is not yet any reduced data.
            var startTime = sourceAggregationList.First().TimeStamp > lastReductionTime ? Support.RoundToResolution(sourceAggregationList.First().TimeStamp, targetReduceLevel.Resolution) : lastReductionTime;
            var lastAggregated = startTime;
            var nextLimit = startTime + resolutionSpan;
            var subList = new List<MonitorRecord<double>>();

            //Start moving through the source list so that we can reduce/aggregate the list
            foreach (var update in sourceAggregationList)
            {
                //When we get to the point that the current records time is greater than 
                //the current bucket we keep incrementing the bucket untill we find a 
                //bucket that the current record fits
                while (update.TimeStamp >= nextLimit)
                {
                    if (subList.Count > 0)
                    {
                        destinationAggregatedList.Add(lastAggregated + halfResolutionSpan, subList);
                        subList = new List<MonitorRecord<double>>();
                    }
                    lastAggregated = nextLimit;
                    nextLimit += resolutionSpan;
                }
                subList.Add(update);
            }

            // Add the last list, if it has any elements
            if (subList.Count > 0) 
                destinationAggregatedList.Add(lastAggregated + halfResolutionSpan, subList); 

            return lastAggregated;
        }
    }
}