using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZocMonLib
{
    public class MonitorRecordComparison<T> : IComparable<MonitorRecord<T>>, ITimeStamped
    {
        public MonitorRecordComparison()
        {
        }

        public MonitorRecordComparison(T value) : this(DateTime.Now, value)
        {
        }

        public MonitorRecordComparison(DateTime time, T value) : this(time, value, 1)
        {
        }

        public MonitorRecordComparison(DateTime time, T value, int number)
        {
            this.TimeStamp = time;
            this.Value = value;
            this.Number = number;
        }

        public DateTime TimeStamp { get; set; }

        public T Value { get; set; }

        public int Number { get; set; }

        public int CompareTo(MonitorRecord<T> other)
        {
            return TimeStamp.CompareTo(other.TimeStamp);
        }
    }
}