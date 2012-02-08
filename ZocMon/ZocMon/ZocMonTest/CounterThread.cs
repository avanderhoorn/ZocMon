using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using ZocMonLib;
using ZocMonLib.Plumbing;

namespace ZocMonTest
{
    public class CounterThread
    {
        public readonly string TimeSpanFormat = @"hh\:mm\:ss\.fff";

        public String ConfigName { get; set; }
        public PerformanceCounter Counter { get; set; }
        public int Resolution { get; set; }
        public TimeSpan FlushInterval { get; set; }
        public TimeSpan ReduceInterval { get; set; }
        public MonitorReductionType MonitorReductionType { get; set; }

        private volatile bool cont = true;
        private Thread me;

        public void Start()
        {
            me = Thread.CurrentThread;
            DateTime now = DateTime.Now;
            DateTime nextFlush = now + FlushInterval;
            DateTime nextReduce = now + ReduceInterval;
            while (cont)
            {
                try
                {
                    ZocMon.Record(ConfigName, Counter.NextValue(), MonitorReductionType);
                    Thread.Sleep(Resolution);
                    if (DateTime.Now > nextFlush)
                    {
                        Flush();
                        nextFlush += FlushInterval;
                    }
                    if (DateTime.Now > nextReduce)
                    {
                        Reduce();
                        nextReduce += ReduceInterval;
                    }
                }
                catch (ThreadInterruptedException e)
                {
//                    SystemLogger.Log(e.Message);
//                    SystemLogger.Log(e.StackTrace);
                    // ignore, and continue
                }
            }
            ZocMon.Flush(ConfigName);
        }

        public void Flush()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SystemLogger.Log("Flushing " + ConfigName);
            ZocMon.Flush(ConfigName);
            sw.Stop();
            SystemLogger.Log("Flushing " + ConfigName + " done in " + sw.Elapsed.ToString(TimeSpanFormat));
        }
        
        public void Reduce()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SystemLogger.Log("Reducing " + ConfigName);
            string errorReturn = ZocMon.Reduce(ConfigName);
            sw.Stop();
            if (!"".Equals(errorReturn))
            {
                SystemLogger.Log("Reducing Failed for: \"" + errorReturn + "\"");
            }
            SystemLogger.Log("Reducing" + ConfigName + " done in " + sw.Elapsed.ToString(TimeSpanFormat));
        }

        public Thread Stop()
        {
            cont = false;
            try
            {
//                Thread.CurrentThread.Interrupt();
                if (me != null) me.Interrupt();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return me;
        }

    }
}
