using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZocMonLib.Test.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            ZocMon.Settings.Initialize();

            ZocMon.Record("Test", 10, MonitorReductionType.DefaultAccumulate);
            ZocMon.Record("Test", 10, MonitorReductionType.DefaultAccumulate);
            ZocMon.Record("Test", 10, MonitorReductionType.DefaultAccumulate);
        }
    }
}
