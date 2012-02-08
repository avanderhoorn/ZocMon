using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using ZocMonLib;
using ZocUtil;

namespace Reducer
{
    class Program
    {
        private bool cont = true;
        private int sleepTime = 60 * 1000;

        public void Loop()
        {
            Stopwatch sw = new Stopwatch();
            ZocLogger.Log("Reducing");
            sw.Start();
            string errorReturn = ZocMon.ReduceAll();
            sw.Stop();
            if (!"".Equals(errorReturn))
            {
                ZocLogger.Log("Reducing Failed for: \"" + errorReturn + "\"");
            }
            ZocLogger.Log("Done Reducing: " + sw.ElapsedMilliseconds + " ms");
        }

        static void Main(string[] args)
        {
            try
            {

                Config.Debug = false;

                ZocLogger.LogConfig logConfig = new ZocLogger.LogConfig()
                {
                    LogFilePrefix = "Reducer",
                    To = "matthew.murchison@zocdoc.com",
                    ThrowInDebug = false,
                    LogFileAndDelegate = true,
                    Info = ZocLogger.MailLog
                };

                Config.Initialize(Config.Testing.CreateTestConnection, logConfig);
                ZocLogger.Log("Done Initializing");

                Program program = new Program();
                program.Loop();
            }
            catch (Exception e)
            {
                ZocLogger.Log("Reducer failed", e);
            }
        }
    }
}
