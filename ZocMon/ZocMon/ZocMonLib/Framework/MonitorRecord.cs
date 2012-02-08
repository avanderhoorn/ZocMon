using System;

namespace ZocMonLib
{
    /// <summary>
    /// Record that captures the record of occurance of a given monitor info.
    /// </summary>
    /// <typeparam name="T">Data type that is being collected</typeparam>
    public class MonitorRecord<T> : IComparable<MonitorRecord<T>>, ITimeStamped
    {
        public MonitorRecord()
        {
        }

        public MonitorRecord(T value)
            : this(DateTime.Now, value)
        {
        }

        public MonitorRecord(DateTime time, T value)
            : this(time, value, 1)
        {
        }

        public MonitorRecord(DateTime time, T value, int number)
        {
            this.TimeStamp = time;
            this.Value = value;
            this.Number = number;
        }

        /// <summary>
        /// Time that the event occured
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Value that we are storeing against this event
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// The number of records that merged into this record
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Tracks the total sum of all the values collected so far 
        /// </summary>
        public double IntervalSum { get; set; }

        public double IntervalSumOfSquares { get; set; }

        public int CompareTo(MonitorRecord<T> other)
        {
            return TimeStamp.CompareTo(other.TimeStamp);
        }

        public override string ToString()
        {
            return string.Format("TimeStamp: {0}, Value: {1}, Number: {2}, IntervalSum: {3}, IntervalSumOfSquares: {4}", TimeStamp, Value, Number, IntervalSum, IntervalSumOfSquares);
        }
    }
}