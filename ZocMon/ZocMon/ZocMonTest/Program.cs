using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using ZocMonLib;
using ZocMonLib.Plumbing; 

namespace ZocMonTest
{
    public class Program
    {
        private readonly IDictionary<string, CounterThread> counters = new Dictionary<string, CounterThread>();

        public Program()
        {
//            string configName = "ReallySuper2LongCPUMonitorConfigNameThatExceedesTheMaximumMonitorConfigNameLength";
            string configName = "CPU";
            counters.Add(configName, new CounterThread()
            {
                ConfigName = configName,
                Counter = new PerformanceCounter("Processor", "% Processor Time", "_Total"),
                Resolution = 500,
                FlushInterval = TimeSpan.FromMinutes(10),
                ReduceInterval = TimeSpan.FromHours(1),
                MonitorReductionType = MonitorReductionType.DefaultAverage
            });
            configName = "RAM";
            counters.Add(configName, new CounterThread()
            {
                ConfigName = configName,
                Counter = new PerformanceCounter("Memory", "Available MBytes"),
                Resolution = 1000,
                FlushInterval = TimeSpan.FromMinutes(10),
                ReduceInterval = TimeSpan.FromHours(1),
                MonitorReductionType = MonitorReductionType.DefaultAverage
            });
//            configName = "HTTPGET";
//            counters.Add(configName, new CounterThread()
//            {
//                ConfigName = configName,
//                Counter = new PerformanceCounter("HTTP Service Url Groups", "GetRequests"),
//                Resolution = 500
//            });
//            configName = "Network-BytesTotal2";
//            counters.Add(configName, new CounterThread()
//            {
//                ConfigName = configName,
//                Counter = new PerformanceCounter("Network Interface", "Bytes Total/sec", FindNetworkName()),
//                Resolution = 2000,
//                FlushInterval = TimeSpan.FromMinutes(10),
//                ReduceInterval = TimeSpan.FromHours(1),
//                MonitorReductionType = MonitorReductionType.DefaultAccumulate
//            });

//            try
//            {

            SystemLogger.LogConfig logConfig = new SystemLogger.LogConfig()
            {
                //LogFilePrefix = "ZocMon",
                To = "karsten.braaten@zocdoc.com",   // WTF?  No aliases?
                ThrowInDebug = false,
                LogFileAndDelegate = true,
                //Info = SystemLogger.MailLog
            };

            Config.Initialize(Config.Testing.CreateTestConnection, logConfig);
//            Config.MonitorConfigs.Clear();
//            }
//            catch (Exception e)
//            {
//                SystemLogger.Log(e.Message);
//                SystemLogger.Log(e.StackTrace);
//                throw;
//            }
        }

        private string FindNetworkName()
        {
            string ret = "";
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in interfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up && "Local Area Connection".Equals(networkInterface.Name))
                {
                    ret = networkInterface.Description;
                    break;
                }
            }
            return ret;
        }

        public IDictionary<string, CounterThread> startMonitors()
        {
            foreach (KeyValuePair<string, CounterThread> performanceCounter in counters)
            {
                Thread thread = new Thread(performanceCounter.Value.Start);
                thread.IsBackground = true;
                SystemLogger.Log("Starting monitor " + performanceCounter.Key);
                thread.Start();
            }
            return counters;
        }

        /// <summary>
        /// List all available performance counters.
        /// </summary>
        private void List()
        {
            PerformanceCounterCategory[] categories = PerformanceCounterCategory.GetCategories();
            for (int i = 0; i < categories.Length; i++)
            {
                Console.Out.WriteLine("{0}. Name=\"{1}\" Help=\"{2}\"", i, categories[i].CategoryName, categories[i].CategoryHelp);
                string[] instanceNames = categories[i].GetInstanceNames();
                for (int j = 0; j < instanceNames.Length; j++)
                {
                    try
                    {
                        PerformanceCounter[] counters = categories[i].GetCounters(instanceNames[j]);
                        for (int k = 0; k < counters.Length; k++)
                        {
                            Console.Out.WriteLine("{0} ({1} {2}). CategoryName = \"{3}\"; CounterName = \"{4}\"; InstanceName = \"{5}\"", i, j, k,
                                                  counters[k].CategoryName,
                                                  counters[k].CounterName, counters[k].InstanceName);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Out.WriteLine("Ignore: " + e.Message);
                    }
                }
            }
        }

        const string names = @"MonitorConfigName




239808-web5-CPU
239808-web5-CPU
239808-web5-CPU
239808-web5-CPU
239808-web5-RAM
239808-web5-RAM
239808-web5-RAM
239808-web5-RAM
239808_web5_CPU
239808_web5_CPU
239808_web5_CPU
239808_web5_CPU
239808_web5_RAM
239808_web5_RAM
239808_web5_RAM
239808_web5_RAM
269770-WEB6-CPU
269770-WEB6-CPU
269770-WEB6-CPU
269770-WEB6-CPU
269770-WEB6-RAM
269770-WEB6-RAM
269770-WEB6-RAM
269770-WEB6-RAM
269770_WEB6_CPU
269770_WEB6_CPU
269770_WEB6_CPU
269770_WEB6_CPU
269770_WEB6_RAM
269770_WEB6_RAM
269770_WEB6_RAM
269770_WEB6_RAM
269800-web7-CPU
269800-web7-CPU
269800-web7-CPU
269800-web7-CPU
269800-web7-RAM
269800-web7-RAM
269800-web7-RAM
269800-web7-RAM
269800_web7_CPU
269800_web7_CPU
269800_web7_CPU
269800_web7_CPU
269800_web7_RAM
269800_web7_RAM
269800_web7_RAM
269800_web7_RAM
305313-web8-CPU
305313-web8-CPU
305313-web8-CPU
305313-web8-CPU
305313-web8-RAM
305313-web8-RAM
305313-web8-RAM
305313-web8-RAM
305313_web8_CPU
305313_web8_CPU
305313_web8_CPU
305313_web8_CPU
305313_web8_RAM
305313_web8_RAM
305313_web8_RAM
305313_web8_RAM
305319-web9-CPU
305319-web9-CPU
305319-web9-CPU
305319-web9-CPU
305319-web9-RAM
305319-web9-RAM
305319-web9-RAM
305319-web9-RAM
305319_web9_CPU
305319_web9_CPU
305319_web9_CPU
305319_web9_CPU
305319_web9_RAM
305319_web9_RAM
305319_web9_RAM
305319_web9_RAM
350230-DB1-CPU
350230-DB1-CPU
350230-DB1-CPU
350230-DB1-CPU
350230-DB1-RAM
350230-DB1-RAM
350230-DB1-RAM
350230-DB1-RAM
350230_DB1_CPU
350230_DB1_CPU
350230_DB1_CPU
350230_DB1_CPU
350230_DB1_RAM
350230_DB1_RAM
350230_DB1_RAM
350230_DB1_RAM
AlerterCheckin
AlerterCheckin
AlerterCheckin
AlerterCheckin
Appt Booked-Browser-chrome
Appt Booked-Browser-chrome
Appt Booked-Browser-chrome
Appt Booked-Browser-chrome
Appt Booked-Browser-firefox
Appt Booked-Browser-firefox
Appt Booked-Browser-firefox
Appt Booked-Browser-firefox
Appt Booked-Browser-firefox3
Appt Booked-Browser-firefox3
Appt Booked-Browser-firefox3
Appt Booked-Browser-firefox3
Appt Booked-Browser-firefox4
Appt Booked-Browser-firefox4
Appt Booked-Browser-firefox4
Appt Booked-Browser-firefox4
Appt Booked-Browser-ie6
Appt Booked-Browser-ie6
Appt Booked-Browser-ie6
Appt Booked-Browser-ie6
Appt Booked-Browser-ie7
Appt Booked-Browser-ie7
Appt Booked-Browser-ie7
Appt Booked-Browser-ie7
Appt Booked-Browser-ie8
Appt Booked-Browser-ie8
Appt Booked-Browser-ie8
Appt Booked-Browser-ie8
Appt Booked-Browser-iphone
Appt Booked-Browser-iphone
Appt Booked-Browser-iphone
Appt Booked-Browser-iphone
Appt Booked-Browser-safari
Appt Booked-Browser-safari
Appt Booked-Browser-safari
Appt Booked-Browser-safari
Appt Booked-Browser-unknown
Appt Booked-Browser-unknown
Appt Booked-Browser-unknown
Appt Booked-Browser-unknown
Appt Booked-Market-Atlanta, GA
Appt Booked-Market-Atlanta, GA
Appt Booked-Market-Atlanta, GA
Appt Booked-Market-Atlanta, GA
Appt Booked-Market-Chicago, IL
Appt Booked-Market-Chicago, IL
Appt Booked-Market-Chicago, IL
Appt Booked-Market-Chicago, IL
Appt Booked-Market-Dallas, TX
Appt Booked-Market-Dallas, TX
Appt Booked-Market-Dallas, TX
Appt Booked-Market-Dallas, TX
Appt Booked-Market-Houston, TX
Appt Booked-Market-Houston, TX
Appt Booked-Market-Houston, TX
Appt Booked-Market-Houston, TX
Appt Booked-Market-Los Angeles, CA
Appt Booked-Market-Los Angeles, CA
Appt Booked-Market-Los Angeles, CA
Appt Booked-Market-Los Angeles, CA
Appt Booked-Market-New York, NY
Appt Booked-Market-New York, NY
Appt Booked-Market-New York, NY
Appt Booked-Market-New York, NY
Appt Booked-Market-Philadelphia, PA
Appt Booked-Market-Philadelphia, PA
Appt Booked-Market-Philadelphia, PA
Appt Booked-Market-Philadelphia, PA
Appt Booked-Market-Phoenix, AZ
Appt Booked-Market-Phoenix, AZ
Appt Booked-Market-Phoenix, AZ
Appt Booked-Market-Phoenix, AZ
Appt Booked-Market-San Francisco, CA
Appt Booked-Market-San Francisco, CA
Appt Booked-Market-San Francisco, CA
Appt Booked-Market-San Francisco, CA
Appt Booked-Market-Washington, DC
Appt Booked-Market-Washington, DC
Appt Booked-Market-Washington, DC
Appt Booked-Market-Washington, DC
Appt Booked-Referrer-Hospital
Appt Booked-Referrer-Hospital
Appt Booked-Referrer-Hospital
Appt Booked-Referrer-Hospital
Appt Booked-Referrer-IphoneApp
Appt Booked-Referrer-IphoneApp
Appt Booked-Referrer-IphoneApp
Appt Booked-Referrer-IphoneApp
Appt Booked-Referrer-Web
Appt Booked-Referrer-Web
Appt Booked-Referrer-Web
Appt Booked-Referrer-Web
Appt Booked-Referrer-Widget
Appt Booked-Referrer-Widget
Appt Booked-Referrer-Widget
Appt Booked-Referrer-Widget
Appt Booked-Synchronous
Appt Booked-Synchronous
Appt Booked-Synchronous
Appt Booked-Synchronous
Appt_Booked_Browser_chrome
Appt_Booked_Browser_chrome
Appt_Booked_Browser_chrome
Appt_Booked_Browser_chrome
Appt_Booked_Browser_firefox
Appt_Booked_Browser_firefox
Appt_Booked_Browser_firefox
Appt_Booked_Browser_firefox
Appt_Booked_Browser_firefox3
Appt_Booked_Browser_firefox3
Appt_Booked_Browser_firefox3
Appt_Booked_Browser_firefox3
Appt_Booked_Browser_firefox4
Appt_Booked_Browser_firefox4
Appt_Booked_Browser_firefox4
Appt_Booked_Browser_firefox4
Appt_Booked_Browser_ie6
Appt_Booked_Browser_ie6
Appt_Booked_Browser_ie6
Appt_Booked_Browser_ie6
Appt_Booked_Browser_ie7
Appt_Booked_Browser_ie7
Appt_Booked_Browser_ie7
Appt_Booked_Browser_ie7
Appt_Booked_Browser_ie8
Appt_Booked_Browser_ie8
Appt_Booked_Browser_ie8
Appt_Booked_Browser_ie8
Appt_Booked_Browser_iphone
Appt_Booked_Browser_iphone
Appt_Booked_Browser_iphone
Appt_Booked_Browser_iphone
Appt_Booked_Browser_safari
Appt_Booked_Browser_safari
Appt_Booked_Browser_safari
Appt_Booked_Browser_safari
Appt_Booked_Browser_unknown
Appt_Booked_Browser_unknown
Appt_Booked_Browser_unknown
Appt_Booked_Browser_unknown
Appt_Booked_Market_Atlanta__GA
Appt_Booked_Market_Atlanta__GA
Appt_Booked_Market_Atlanta__GA
Appt_Booked_Market_Atlanta__GA
Appt_Booked_Market_Chicago__IL
Appt_Booked_Market_Chicago__IL
Appt_Booked_Market_Chicago__IL
Appt_Booked_Market_Chicago__IL
Appt_Booked_Market_Dallas__TX
Appt_Booked_Market_Dallas__TX
Appt_Booked_Market_Dallas__TX
Appt_Booked_Market_Dallas__TX
Appt_Booked_Market_Houston__TX
Appt_Booked_Market_Houston__TX
Appt_Booked_Market_Houston__TX
Appt_Booked_Market_Houston__TX
Appt_Booked_Market_Los_Angeles__CA
Appt_Booked_Market_Los_Angeles__CA
Appt_Booked_Market_Los_Angeles__CA
Appt_Booked_Market_Los_Angeles__CA
Appt_Booked_Market_New_York__NY
Appt_Booked_Market_New_York__NY
Appt_Booked_Market_New_York__NY
Appt_Booked_Market_New_York__NY
Appt_Booked_Market_Philadelphia__PA
Appt_Booked_Market_Philadelphia__PA
Appt_Booked_Market_Philadelphia__PA
Appt_Booked_Market_Philadelphia__PA
Appt_Booked_Market_San_Francisco__CA
Appt_Booked_Market_San_Francisco__CA
Appt_Booked_Market_San_Francisco__CA
Appt_Booked_Market_San_Francisco__CA
Appt_Booked_Market_Washington__DC
Appt_Booked_Market_Washington__DC
Appt_Booked_Market_Washington__DC
Appt_Booked_Market_Washington__DC
Appt_Booked_Referrer_Hospital
Appt_Booked_Referrer_Hospital
Appt_Booked_Referrer_Hospital
Appt_Booked_Referrer_Hospital
Appt_Booked_Referrer_IphoneApp
Appt_Booked_Referrer_IphoneApp
Appt_Booked_Referrer_IphoneApp
Appt_Booked_Referrer_IphoneApp
Appt_Booked_Referrer_Web
Appt_Booked_Referrer_Web
Appt_Booked_Referrer_Web
Appt_Booked_Referrer_Web
Appt_Booked_Referrer_Widget
Appt_Booked_Referrer_Widget
Appt_Booked_Referrer_Widget
Appt_Booked_Referrer_Widget
Appt_Booked_Server_305313_WEB8
Appt_Booked_Server_305313_WEB8
Appt_Booked_Server_305313_WEB8
Appt_Booked_Server_305313_WEB8
Appt_Booked_Server_305319_WEB9
Appt_Booked_Server_305319_WEB9
Appt_Booked_Server_305319_WEB9
Appt_Booked_Server_305319_WEB9
Appt_Booked_Synchronous
Appt_Booked_Synchronous
Appt_Booked_Synchronous
Appt_Booked_Synchronous
CacheProfessionalCacheMiss
CacheProfessionalCacheMiss
CacheProfessionalCacheMiss
CacheProfessionalCacheMiss
CalendarAddAvailability
CalendarAddAvailability
CalendarAddAvailability
CalendarAddAvailability
CalendarDeleteAvailability
CalendarDeleteAvailability
CalendarDeleteAvailability
CalendarDeleteAvailability
CalendarSaveHolidayInfo
CalendarSaveHolidayInfo
CalendarSaveHolidayInfo
CalendarSaveHolidayInfo
CalendarUpdateApptStatus
CalendarUpdateApptStatus
CalendarUpdateApptStatus
CalendarUpdateApptStatus
CalendarUpdateAvailability
CalendarUpdateAvailability
CalendarUpdateAvailability
CalendarUpdateAvailability
default_aspx
default_aspx
default_aspx
default_aspx
Failed_Login_Attempt
Failed_Login_Attempt
Failed_Login_Attempt
Failed_Login_Attempt
FeedbackReceived
FeedbackReceived
FeedbackReceived
FeedbackReceived
getInitialData_iphone
getInitialData_iphone
getInitialData_iphone
getInitialData_iphone
Hospital Appt Booked
Hospital Appt Booked
Hospital Appt Booked
Hospital Appt Booked
IphoneApp Appt Booked
IphoneApp Appt Booked
IphoneApp Appt Booked
IphoneApp Appt Booked
JobApplicants
JobApplicants
JobApplicants
JobApplicants
New_SQL_Connection_crawl
New_SQL_Connection_crawl
New_SQL_Connection_crawl
New_SQL_Connection_crawl
New_SQL_Connection_replicated_crawl
New_SQL_Connection_replicated_crawl
New_SQL_Connection_replicated_crawl
New_SQL_Connection_replicated_crawl
New_SQL_Connection_replicated_zocdoc
New_SQL_Connection_replicated_zocdoc
New_SQL_Connection_replicated_zocdoc
New_SQL_Connection_replicated_zocdoc
New_SQL_Connection_reporting
New_SQL_Connection_reporting
New_SQL_Connection_reporting
New_SQL_Connection_reporting
New_SQL_Connection_secondary
New_SQL_Connection_secondary
New_SQL_Connection_secondary
New_SQL_Connection_secondary
New_SQL_Connection_zocdoc
New_SQL_Connection_zocdoc
New_SQL_Connection_zocdoc
New_SQL_Connection_zocdoc
packages_search_search_aspx
packages_search_search_aspx
packages_search_search_aspx
packages_search_search_aspx
pagespeed__default_aspx__305313-WEB8
pagespeed__default_aspx__305313-WEB8
pagespeed__default_aspx__305313-WEB8
pagespeed__default_aspx__305313-WEB8
pagespeed__default_aspx__305313_WEB8
pagespeed__default_aspx__305313_WEB8
pagespeed__default_aspx__305313_WEB8
pagespeed__default_aspx__305313_WEB8
pagespeed__default_aspx__305319-WEB9
pagespeed__default_aspx__305319-WEB9
pagespeed__default_aspx__305319-WEB9
pagespeed__default_aspx__305319-WEB9
pagespeed__default_aspx__305319_WEB9
pagespeed__default_aspx__305319_WEB9
pagespeed__default_aspx__305319_WEB9
pagespeed__default_aspx__305319_WEB9
pagespeed__default_aspx__356126-WEB10
pagespeed__default_aspx__356126-WEB10
pagespeed__default_aspx__356126-WEB10
pagespeed__default_aspx__356126-WEB10
pagespeed__practice3_aspx__305313-WEB8
pagespeed__practice3_aspx__305313-WEB8
pagespeed__practice3_aspx__305313-WEB8
pagespeed__practice3_aspx__305313-WEB8
pagespeed__practice3_aspx__305313_WEB8
pagespeed__practice3_aspx__305313_WEB8
pagespeed__practice3_aspx__305313_WEB8
pagespeed__practice3_aspx__305313_WEB8
pagespeed__practice3_aspx__305319-WEB9
pagespeed__practice3_aspx__305319-WEB9
pagespeed__practice3_aspx__305319-WEB9
pagespeed__practice3_aspx__305319-WEB9
pagespeed__practice3_aspx__305319_WEB9
pagespeed__practice3_aspx__305319_WEB9
pagespeed__practice3_aspx__305319_WEB9
pagespeed__practice3_aspx__305319_WEB9
pagespeed__profile_aspx__305313-WEB8
pagespeed__profile_aspx__305313-WEB8
pagespeed__profile_aspx__305313-WEB8
pagespeed__profile_aspx__305313-WEB8
pagespeed__profile_aspx__305313_WEB8
pagespeed__profile_aspx__305313_WEB8
pagespeed__profile_aspx__305313_WEB8
pagespeed__profile_aspx__305313_WEB8
pagespeed__profile_aspx__305319-WEB9
pagespeed__profile_aspx__305319-WEB9
pagespeed__profile_aspx__305319-WEB9
pagespeed__profile_aspx__305319-WEB9
pagespeed__profile_aspx__305319_WEB9
pagespeed__profile_aspx__305319_WEB9
pagespeed__profile_aspx__305319_WEB9
pagespeed__profile_aspx__305319_WEB9
pagespeed__profile_aspx__356126-WEB10
pagespeed__profile_aspx__356126-WEB10
pagespeed__profile_aspx__356126-WEB10
pagespeed__profile_aspx__356126-WEB10
pagespeed__remote_alerter_aspx__305313-WEB8
pagespeed__remote_alerter_aspx__305313-WEB8
pagespeed__remote_alerter_aspx__305313-WEB8
pagespeed__remote_alerter_aspx__305313-WEB8
pagespeed__remote_alerter_aspx__305313_WEB8
pagespeed__remote_alerter_aspx__305313_WEB8
pagespeed__remote_alerter_aspx__305313_WEB8
pagespeed__remote_alerter_aspx__305313_WEB8
pagespeed__remote_alerter_aspx__305319-WEB9
pagespeed__remote_alerter_aspx__305319-WEB9
pagespeed__remote_alerter_aspx__305319-WEB9
pagespeed__remote_alerter_aspx__305319-WEB9
pagespeed__remote_alerter_aspx__305319_WEB9
pagespeed__remote_alerter_aspx__305319_WEB9
pagespeed__remote_alerter_aspx__305319_WEB9
pagespeed__remote_alerter_aspx__305319_WEB9
pagespeed__remote_schedule2_js_aspx__305313-WEB8
pagespeed__remote_schedule2_js_aspx__305313-WEB8
pagespeed__remote_schedule2_js_aspx__305313-WEB8
pagespeed__remote_schedule2_js_aspx__305313-WEB8
pagespeed__remote_schedule2_js_aspx__305313_WEB8
pagespeed__remote_schedule2_js_aspx__305313_WEB8
pagespeed__remote_schedule2_js_aspx__305313_WEB8
pagespeed__remote_schedule2_js_aspx__305313_WEB8
pagespeed__remote_schedule2_js_aspx__305319-WEB9
pagespeed__remote_schedule2_js_aspx__305319-WEB9
pagespeed__remote_schedule2_js_aspx__305319-WEB9
pagespeed__remote_schedule2_js_aspx__305319-WEB9
pagespeed__remote_schedule2_js_aspx__305319_WEB9
pagespeed__remote_schedule2_js_aspx__305319_WEB9
pagespeed__remote_schedule2_js_aspx__305319_WEB9
pagespeed__remote_schedule2_js_aspx__305319_WEB9
pagespeed__remote_schedule2_js_aspx__356126-WEB10
pagespeed__remote_schedule2_js_aspx__356126-WEB10
pagespeed__remote_schedule2_js_aspx__356126-WEB10
pagespeed__remote_schedule2_js_aspx__356126-WEB10
pagespeed__sanction_aspx__305313-WEB8
pagespeed__sanction_aspx__305313-WEB8
pagespeed__sanction_aspx__305313-WEB8
pagespeed__sanction_aspx__305313-WEB8
pagespeed__sanction_aspx__305313_WEB8
pagespeed__sanction_aspx__305313_WEB8
pagespeed__sanction_aspx__305313_WEB8
pagespeed__sanction_aspx__305313_WEB8
pagespeed__sanction_aspx__305319-WEB9
pagespeed__sanction_aspx__305319-WEB9
pagespeed__sanction_aspx__305319-WEB9
pagespeed__sanction_aspx__305319-WEB9
pagespeed__sanction_aspx__305319_WEB9
pagespeed__sanction_aspx__305319_WEB9
pagespeed__sanction_aspx__305319_WEB9
pagespeed__sanction_aspx__305319_WEB9
pagespeed__sanction_aspx__356126-WEB10
pagespeed__sanction_aspx__356126-WEB10
pagespeed__sanction_aspx__356126-WEB10
pagespeed__sanction_aspx__356126-WEB10
pagespeed__search_aspx__305313-WEB8
pagespeed__search_aspx__305313-WEB8
pagespeed__search_aspx__305313-WEB8
pagespeed__search_aspx__305313-WEB8
pagespeed__search_aspx__305313_WEB8
pagespeed__search_aspx__305313_WEB8
pagespeed__search_aspx__305313_WEB8
pagespeed__search_aspx__305313_WEB8
pagespeed__search_aspx__305319-WEB9
pagespeed__search_aspx__305319-WEB9
pagespeed__search_aspx__305319-WEB9
pagespeed__search_aspx__305319-WEB9
pagespeed__search_aspx__305319_WEB9
pagespeed__search_aspx__305319_WEB9
pagespeed__search_aspx__305319_WEB9
pagespeed__search_aspx__305319_WEB9
pagespeed__search_sl_aspx__305313-WEB8
pagespeed__search_sl_aspx__305313-WEB8
pagespeed__search_sl_aspx__305313-WEB8
pagespeed__search_sl_aspx__305313-WEB8
pagespeed__search_sl_aspx__305313_WEB8
pagespeed__search_sl_aspx__305313_WEB8
pagespeed__search_sl_aspx__305313_WEB8
pagespeed__search_sl_aspx__305313_WEB8
pagespeed__search_sl_aspx__305319-WEB9
pagespeed__search_sl_aspx__305319-WEB9
pagespeed__search_sl_aspx__305319-WEB9
pagespeed__search_sl_aspx__305319-WEB9
pagespeed__search_sl_aspx__305319_WEB9
pagespeed__search_sl_aspx__305319_WEB9
pagespeed__search_sl_aspx__305319_WEB9
pagespeed__search_sl_aspx__305319_WEB9
pagespeed__search_sl_aspx__356126-WEB10
pagespeed__search_sl_aspx__356126-WEB10
pagespeed__search_sl_aspx__356126-WEB10
pagespeed__search_sl_aspx__356126-WEB10
practice3_aspx
practice3_aspx
practice3_aspx
practice3_aspx
profile_aspx
profile_aspx
profile_aspx
profile_aspx
remote_alerter_aspx
remote_alerter_aspx
remote_alerter_aspx
remote_alerter_aspx
remote_schedule2_js_aspx
remote_schedule2_js_aspx
remote_schedule2_js_aspx
remote_schedule2_js_aspx
sanction_aspx
sanction_aspx
sanction_aspx
sanction_aspx
search_aspx
search_aspx
search_aspx
search_aspx
search_iphone
search_iphone
search_iphone
search_iphone
search_sl_aspx
search_sl_aspx
search_sl_aspx
search_sl_aspx
SEM_Traffic
SEM_Traffic
SEM_Traffic
SEM_Traffic
Synchronous Appt Booked
Synchronous Appt Booked
Synchronous Appt Booked
Synchronous Appt Booked
Testing
Testing
Testing
Testing
Testing2
Testing2
Testing2
Testing2
View CSR homepage
View CSR homepage
View CSR homepage
View CSR homepage
View_CSR_homepage
View_CSR_homepage
View_CSR_homepage
View_CSR_homepage
Web Appt Booked
Web Appt Booked
Web Appt Booked
Web Appt Booked
WebToLeads
WebToLeads
WebToLeads
WebToLeads
Widget Appt Booked
Widget Appt Booked
Widget Appt Booked
Widget Appt Booked
";


        private static void Main(string[] args)
        {
//            char[] charArray = names.ToCharArray();
//            StringBuilder sb = new StringBuilder();
//            for (int i = 0; i < charArray.Length; ++i)
//            {
//                if (!char.IsLetterOrDigit(charArray[i]) && !'_'.Equals(charArray[i]))
//                {
//                    sb.Append("\"" + charArray[i] + "\"");
//                }
//            }
//
//            Console.WriteLine("Chars:");
//            Console.WriteLine(sb.ToString());
//            Console.ReadLine();
//            
//            Environment.Exit(0);


//            string sql =
//                @"
//            INSERT INTO zocdoc_logs.dbo.MonitorConfig
//              (Name, PredictionSourceResolution, PredictionReferenceResolution, PredictionClassName, MonitorReductionType) VALUES
//              ('pagespeed__packages_search_search_aspx__305319_WEB9', 0, 60 * 60 * 1000, 'ZocMonLib.PredictionMethods.WeeklyByHour', 'DefaultAverage')
//";
//
//            using (IDbConnection conn = Config.Testing.CreateTestConnection())
//            {
//                conn.Open();
//                try
//                {
//                    SqlCommand command = new SqlCommand(sql, conn);
//                    command.ExecuteNonQuery();
//                }
//                catch (Exception e)
//                {
//                    SystemLogger.Log("Failed", e);
//                }
//                finally
//                {
//                    conn.Close();
//                }
//            }
//
//            SystemLogger.Log("Succeeded");
//
//            Environment.Exit(0);

            /**************************************************/


//            const string sqlFormat =
//                @"
//SELECT d.[TimeStamp], count(*) as count
//  into #tmp1
//  from [{0}] d
//  group BY d.[TimeStamp]
//  having count(*) > 1
//DELETE [{0}]
//  from [{0}] d
//  inner JOIN #tmp1 t1 on t1.TimeStamp = d.TimeStamp
//drop table #tmp1
//go
//";
//
//            string[] tables = new[]
//                                  {
//                                      "239808_web5_CPU",
//                                      "New_SQL_Connection_crawl",
//                                      "269800_web7_CPU",
//                                      "pagespeed__sanction_aspx__305319_WEB9",
//                                      "269770_WEB6_CPU",
//                                      "305319_web9_CPU",
//                                      "269800_web7_RAM",
//                                      "Appt_Booked_Server_305319_WEB9",
//                                      "pagespeed__profile_aspx__305313_WEB8",
//                                      "pagespeed__default_aspx__305319_WEB9",
//                                      "pagespeed__remote_schedule2_js_aspx__305313_WEB8",
//                                      "HitCount_Page_packages_search_search_aspx",
//                                      "HitCount_Page_profile_aspx",
//                                      "HitCount_Page_remote_schedule2_js_aspx",
//                                      "350230_DB1_CPU",
//                                      "Appt_Booked_Market_New_York__NY",
//                                      "pagespeed__practice3_aspx__305313_WEB8",
//                                      "pagespeed__search_aspx__356126_WEB10",
//                                      "HitCount_Page_default_aspx",
//                                      "pagespeed__remote_alerter_aspx__305319_WEB9",
//                                      "239808_web5_RAM",
//                                      "CacheProfessionalCacheMiss",
//                                      "269770_WEB6_RAM",
//                                      "pagespeed__remote_schedule2_js_aspx__305319_WEB9",
//                                      "305319_web9_RAM",
//                                      "pagespeed__search_sl_aspx__356126_WEB10",
//                                      "356126_WEB10_CPU",
//                                      "305313_web8_CPU",
//                                      "pagespeed__default_aspx__356126_WEB10",
//                                      "pagespeed__sanction_aspx__305313_WEB8",
//                                      "356126_WEB10_RAM",
//                                      "350230_DB1_RAM",
//                                  };
//
//            string[] tables = new[]
//                                  {
//                                      "pagespeed__packages_search_search_aspx__305313_WEB8",
//                                      "269770_WEB6_RAM",
//                                      "305319_web9_RAM",
//                                      "356126_WEB10_CPU",
//                                      "pagespeed__search_sl_aspx__305313_WEB8",
//                                      "305313_web8_CPU",
//                                      "pagespeed__search_sl_aspx__305319_WEB9",
//                                      "HitCount_Page_remote_alerter_aspx",
//                                      "pagespeed__remote_alerter_aspx__356126_WEB10",
//                                      "pagespeed__sanction_aspx__305313_WEB8",
//                                      "356126_WEB10_RAM",
//                                      "350230_DB1_RAM"
//                                  };
//
//            using (StreamWriter outfile = new StreamWriter(@"C:\usr\sqlFile4.sql"))
//            {
//                foreach (string table in tables)
//                {
//                    string outSql = string.Format(sqlFormat, table + "FiveMinutelyData");
//                    Console.WriteLine(outSql);
//                    outfile.Write(outSql);
//                    outfile.Write(Environment.NewLine);
//                }
//            }
//
//            Console.ReadKey();
//
//            Environment.Exit(0);


            /**************************************************/

            Program program = new Program();

            if (args.Length > 0 && "List".Equals(args[0]))
            {
                program.List();
                //                Console.ReadKey();
                return;
            }

            IDictionary<string, CounterThread> counterThreads = program.startMonitors();

            ConsoleKeyInfo keyInfo;
            do
            {
                keyInfo = Console.ReadKey();
                SystemLogger.Log("");
                switch (keyInfo.KeyChar)
                {
                    case 'f':
                        foreach (CounterThread counterThread in counterThreads.Values)
                        {
                            counterThread.Flush();
                        }
                        SystemLogger.Log("Finished Flushing");
                        break;
                    case 'r':
                        foreach (CounterThread counterThread in counterThreads.Values)
                        {
                            counterThread.Reduce();
                        }
                        SystemLogger.Log("Finished Reducing");
                        break;
                    case 'a':
                        ZocMon.ReduceAll(false);
                        break;
                }

            } while (keyInfo.KeyChar != 'e');

            IList<Thread> threads = new List<Thread>();
            foreach (KeyValuePair<string, CounterThread> counterThread in counterThreads)
            {
                SystemLogger.Log("Stopping Monitor " + counterThread.Key);
                Thread t = counterThread.Value.Stop();
                if (t != null) threads.Add(t);
            }

            foreach (Thread thread in threads)
            {
                try
                {
                    thread.Join();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

        }
    }
}