using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZocMonLib
{
    public interface ITimeStamped
    {
        DateTime TimeStamp { get; set; }
    }
}
