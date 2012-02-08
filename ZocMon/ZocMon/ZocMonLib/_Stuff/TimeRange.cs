using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZocMonLib
{
    public class TimeRange
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string WhereClause { get; set; }
    }
}
