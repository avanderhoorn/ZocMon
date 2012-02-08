using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ZocMonLib;
using ZocUtil;
using ZocUtil.CollectionExtensions;

namespace Infrastructure
{
    /// <summary>
    /// This is a class for making ZocMon Graphs
    /// Trying to clean up the InfrastructureController by moving some stuff here
    /// </summary>
    public class ZocMonGraph
    {
        #region public constants
        public const int MaxDataPointsPerLine = 2000; // roughly 3 months of hourly data
        public const string Prefix = "[ ";
        public const string Suffix = " ]";
        public const string Comma = ", ";
        public const char MonitorReduceLevelSeparator = '/';
        public static readonly string CommaSuffix = Suffix + Comma;
        public const string DataSectionFormat = "{{ label: \"{0}\", data: [ {1} ] }}";
        public const string RatioDataSectionFormat = "{{ label: \"{0}\", data: [ {1} ] }}";
        public const string ColoredRatioDataSectionFormat = "{{ label: \"{0}\", data: [ {1} ], color: '{2}' }}";
        public static DateTime MinDate = DateTime.Parse("1/1/1753 12:00:00 AM");
        #endregion

        #region private constants
        private int[] RatioDates = { -7, -14, -21, -28 };
        private List<int> DefaultWeeks = new List<int>{ 0, 1, 2 };
        private const string LoadDataWhereToPresent = @"  where TimeStamp > '{0}'";
        private const string LoadDataWhereRange = @"  where TimeStamp > '{0}' and TimeStamp < '{1}'";
        private static readonly TimeSpan EpochInTimeZone = new TimeSpan(DateTime.Parse("1/1/1970").Ticks);
        #endregion

        #region dynamic variables

        public string GraphTitle { get; set; }
        public string GraphSubTitle { get; set; }
        public GraphType Type { get; set; }
        public bool ShowYAxis { get; set; }
        public bool ZeroMinYAxis { get; set; }
        public string AllTablesString { get; set; }
        public List<string> DataSources { get; set; }
        public List<Tuple<string, TimeRange>>  DataSourceMetas { get; set; }
        public List<string> DataSourceLabels { get; set; }
        public int IndexOnPage { get; set; }
        public int DisplayWidth { get; set; }
        public int DisplayHeight { get; set; }
        public int DisplayTitleSize { get; set; }
        public List<TimeRange> TimeRanges { get; set; }
        public bool TimeRangeIsDynamic { get; set; }
        public bool IsExplicitTimeRange { get; set; }
        public bool IsMonthReview { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public HistoryRangeOptions HistoryRange { get; set; }
        public int GraphCount { get; set; }
        public List<double> RatioGraphValues { get; set; }
        public List<GraphEvent> Events =  new List<GraphEvent>();
        public bool ShowEvents { get; set; }
        public List<int> Weeks { get; set; }
        public List<String> ConversionRatesData { get; set; }

        //feature flag
        public bool UseCache;
        
        #endregion

        #region constructors
        //regular ratio or line graph constructor
        public ZocMonGraph(int graphCount, string graphTitle, GraphType type, bool showYAxis, bool zeroMinYAxis, bool showEvents, string allTablesString, bool isMonthReview,
            bool isExplicitTimeRange, DateTime from, DateTime to, HistoryRangeOptions historyRange, string weeks)
        {
            UseCache = new Flag_ZocMonCache().Enabled();
            GraphCount = graphCount;
            GraphTitle = graphTitle;
            Type = type;
            ShowYAxis = showYAxis;
            ZeroMinYAxis = zeroMinYAxis;
            ShowEvents = type == GraphType.Ratio ? false : showEvents;
            AllTablesString = allTablesString;
            IsMonthReview = isMonthReview;

            if (string.IsNullOrEmpty(weeks))
            {
                Weeks = DefaultWeeks;
            }
            else
            {
                Weeks = new List<int>();
                foreach (char c in weeks)
                {
                    int week;
                    if (int.TryParse(c.ToString(), out week) && week >= 1 && week <= 4)
                    {
                        week--;
                        Weeks.Add(week);
                    }
                    else
                    {
                        Weeks = DefaultWeeks;
                        break;
                    }
                }
            }


            //this parameters is meaningless at the moment, but could be useful in the future
            TimeRangeIsDynamic = isMonthReview;
            
            IsExplicitTimeRange = isExplicitTimeRange;
            From = from;
            To = to;
            TimeRanges = new List<TimeRange>();
            HistoryRange = historyRange;

            Process();
        }

        //month review constructor
        public ZocMonGraph(string dataSource)
        {
            UseCache = new Flag_ZocMonCache().Enabled();
            Type = GraphType.MonthReview;
            TimeRangeIsDynamic = true;
            IsExplicitTimeRange = false;
            ShowYAxis = true;
            ShowEvents = false;
            ZeroMinYAxis = false;
            DataSources = new List<string> { 
                dataSource + "HourlyData"
            };
            SetDataSourceLabels(); 
            TimeRanges = new List<TimeRange>
                             {
                                new TimeRange
                                {
                                    Start = DateTime.Today.AddDays(-28),
                                    End = DateTime.Now
                                }
            };
            SetDisplay();
            SetMetas();
        }

        //other types constructor
        public ZocMonGraph(GraphType graphType)
        {
            UseCache = new Flag_ZocMonCache().Enabled();
            //one off conversionrates page
            if (graphType == GraphType.ConversionRates)
            {
                Type = GraphType.ConversionRates;
                TimeRangeIsDynamic = false;
                IsExplicitTimeRange = false;
                ShowYAxis = true;
                ZeroMinYAxis = false;
                DataSources = new List<string> { 
                    "Appt_Booked_Server_305313_WEB8FiveMinutelyData",
                    "Appt_Booked_Server_305319_WEB9FiveMinutelyData", 
                    "Appt_Booked_Server_356126_WEB10FiveMinutelyData",
                    "Appt_Booked_Server_305313_WEB8HourlyData",
                    "Appt_Booked_Server_305319_WEB9HourlyData", 
                    "Appt_Booked_Server_356126_WEB10HourlyData",
                    "HitCount_DynamicPageRealUser_Machine_305313_WEB8FiveMinutelyData",
                    "HitCount_DynamicPageRealUser_Machine_305319_WEB9FiveMinutelyData", 
                    "HitCount_DynamicPageRealUser_Machine_356126_WEB10FiveMinutelyData",
                    "HitCount_DynamicPageRealUser_Machine_305313_WEB8HourlyData",
                    "HitCount_DynamicPageRealUser_Machine_305319_WEB9HourlyData", 
                    "HitCount_DynamicPageRealUser_Machine_356126_WEB10HourlyData"
                };
                var lastMonth = HistoryRangeToTimeRange(HistoryRangeOptions.LastMonth);
                var lastWeek = HistoryRangeToTimeRange(HistoryRangeOptions.LastWeek);
                TimeRanges = new List<TimeRange> {
                                     lastMonth,
                                     lastMonth,
                                     lastMonth,
                                     lastWeek,
                                     lastWeek,
                                     lastWeek,
                                     lastMonth,
                                     lastMonth,
                                     lastMonth,
                                     lastWeek,
                                     lastWeek,
                                     lastWeek
                                 };
                DataSourceLabels = new List<string>
                                       {
                                            "WEB8",
                                            "WEB9",
                                            "WEB10"
                                       };
                SetConversionRatesEvents();
                SetDisplay();
                SetMetas();
            }
        }

        #endregion

        #region internal processing behavior
        private void Process()
        {
            SetDataSources();
            SetDataSourceLabels();
            SetTimeRanges();
            if (Type == GraphType.Line)
            {
                SetEvents();
            }
            SetDisplay();
            SetMetas();
        }

        private void SetDataSources()
        {
            if (Type != GraphType.MonthReview && Type != GraphType.ConversionRates)
            {
                DataSources = new List<string>();

                string[] encodedReduceLevels = AllTablesString.Split(',');
                string[] monitorReduceLevel;

                foreach (string encodedReduceLevel in encodedReduceLevels)
                {
                    monitorReduceLevel = encodedReduceLevel.Split(new[] {MonitorReduceLevelSeparator}, 2);
                    if (monitorReduceLevel.Length == 2)
                    {
                        DataSources.Add(monitorReduceLevel[1]);
                    }
                    else
                    {
                        SimpleLogger.logException("Failed to parse: \"" + encodedReduceLevel + "\"");
                    }
                }
            }
        }

        private void SetDataSourceLabels()
        {
            DataSourceLabels = RemoveCommonSuffix(RemoveCommonPrefix(DataSources.ToList()));
            for (var i = 0; i < DataSourceLabels.Count; i ++){
                DataSourceLabels[i] = DataSourceLabels[i].Replace('_', ' ').Trim();
            }
        } 

        private void SetTimeRanges()
        {
            if (!TimeRangeIsDynamic)
            {
                //if value is 0 it is an explicit time range
                //OVERRIDING THIS WITH MAXVALUE FOR CACHING
                if (IsExplicitTimeRange)
                {
                    TimeRanges.Add(new TimeRange
                                       {
                                           Start = From,
                                           End = DateTime.Now,
                                           WhereClause = string.Format(LoadDataWhereToPresent, From)
                                       });
                }
                else if (Type == GraphType.Ratio)
                {
                    TimeRanges.Add(new TimeRange
                        {
                            Start = DateTime.Today.AddDays(-28),
                            End = DateTime.Now,
                        });
                }
                else
                {
                    TimeRanges.Add(HistoryRangeToTimeRange(HistoryRange));
                }
            }
            else
            {
                if (Type == GraphType.MonthReview || Type == GraphType.Ratio)
                {
                    TimeRanges = new List<TimeRange>
                    {
                        new TimeRange
                        {
                            Start = DateTime.Today.AddDays(-28),
                            End = DateTime.Now,
                            WhereClause = string.Format(LoadDataWhereToPresent,DateTime.Today.AddDays(-28))
                        }
                    };
                }
            }
        }

        //this will replace set datasources and set timeranges when we move completely from non-cached to cached zocmon
        //and its no longer feature-flagged
        private void SetMetas()
        {
            DataSourceMetas = new List<Tuple<string, TimeRange>>();
            
            //sanity check
            bool oneTable = DataSources.Count == 1;
            bool oneTimeRange = TimeRanges.Count == 1;
            if ((!oneTable && !oneTimeRange && DataSources.Count != TimeRanges.Count) ||
                (DataSources.Count == 0 || TimeRanges.Count == 0))
            {
                throw (new Exception("Number of tables and timeranges doesn't make sense with tables = " + DataSources.Count +
                                     " and timeranges = " + TimeRanges.Count));
            }

            if (oneTable && !oneTimeRange)
            {
                foreach (var range in TimeRanges)
                {
                    DataSourceMetas.Add(new Tuple<string, TimeRange>(DataSources[0], range));
                }
            }
            else if (!oneTable && oneTimeRange)
            {
                foreach (var table in DataSources)
                {
                    DataSourceMetas.Add(new Tuple<string, TimeRange>(table, TimeRanges[0]));
                } 
            }
            else
            {
                for (var i = 0; i < DataSources.Count; i++)
                {
                    DataSourceMetas.Add(new Tuple<string, TimeRange>(DataSources[i], TimeRanges[i]));
                }
            }
        }

        private void SetDisplay()
        {
            DisplayWidth = 900;
            DisplayHeight = 600;
            DisplayTitleSize = 36;

            if (Type != GraphType.Ratio)
            {
                if (GraphCount > 6)
                {
                    DisplayWidth = 300;
                    DisplayHeight = 200;
                    DisplayTitleSize = 20;
                }
                else if (GraphCount > 2)
                {
                    DisplayWidth = 450;
                    DisplayHeight = 300;
                    DisplayTitleSize = 26;
                }
            }
            else
            {
                DisplayHeight = 600;
            }
        }

        private void SetEvents()
        {
            Events = IsExplicitTimeRange
                         ? CacheGraphEvents.GetEvents(From, To).ToList()
                         : CacheGraphEvents.GetEvents(From).ToList();
        }

        private void SetConversionRatesEvents()
        {
            Events = CacheGraphEvents.GetEvents(DateTime.Today.AddDays(-7)).ToList();
        }
        #endregion

        #region Enums
        public enum BaseReduceLevels
        {
            Minutely,
            FiveMinutely,
            Hourly,
            Daily
        }

        public enum HistoryRangeOptions
        {
            LastHour,
            LastFourHours,
            LastEightHours,
            LastDay,
            LastWeek,
            LastMonth,
            LastWeekToday
        }

        public enum MonitorReductionType
        {
            DefaultAverage,
            DefaultAccumulate
        }

        public enum GraphEventType
        {
            Add,
            Remove
        }

        public enum GraphType
        {
            Line,
            Ratio,
            MonthReview,
            ConversionRates
        }
        #endregion

        #region Big Functions: GetLineGraphData and GetRatioData
        public string GetData()
        {
            if (Type == GraphType.Ratio)
            {
                return GetRatioData();
            }
            if (Type == GraphType.MonthReview)
            {
                return GetMonthReviewData();
            }
            return GetLineGraphData();
        }
        
        private string GetLineGraphData()
        {
            Dictionary<string, List<MonitorRecordUnion<double>>> dataDict;

            using (var conn = Sql.GetLogConnection().Open())
            {
                dataDict = UseCache
                               ? CacheZocMon.GetData(DataSourceMetas, conn)
                               : ZocMon.GetDataUnion(DataSourceMetas, conn);
            }

            //build js array string
            return BuildJsArrayString(dataDict.Values.ToList());
        }

        private string GetMonthReviewData()
        {
            Dictionary<string, List<MonitorRecordUnion<double>>> dataDict;

            using (var conn = Sql.GetLogConnection().Open())
            {
                dataDict = UseCache
                                   ? CacheZocMon.GetData(DataSourceMetas, conn)
                                   : ZocMon.GetDataUnion(DataSourceMetas, conn);
            }
            var allRecords = dataDict.Values.First();
            var dataList = new List<List<MonitorRecordUnion<double>>>
                                {
                                    allRecords.Where(x => x.TimeStamp >= DateTime.Now.AddDays(-28) && x.TimeStamp < DateTime.Now.AddDays(-21)).ToList(),
                                    allRecords.Where(x => x.TimeStamp >= DateTime.Now.AddDays(-21) && x.TimeStamp < DateTime.Now.AddDays(-14)).ToList(),
                                    allRecords.Where(x => x.TimeStamp >= DateTime.Now.AddDays(-14) && x.TimeStamp < DateTime.Now.AddDays(-7)).ToList(),
                                    allRecords.Where(x => x.TimeStamp >= DateTime.Now.AddDays(-7) && x.TimeStamp < DateTime.Now.AddHours(-1)).ToList()  
                                };
            return BuildJsArrayString(dataList);
        }
        
        private string GetRatioData()
        {
            const string dataPointFormat = "[{0}, '{1}']";
            IList<string> dataStrings = new List<string>();

            RatioGraphValues = new List<double>();
            Dictionary<string, List<MonitorRecordUnion<double>>> dataSets;

            using (var conn = Sql.GetLogConnection().Open())
            {
                dataSets = UseCache
                    ? CacheZocMon.GetData(DataSourceMetas, conn)
                    : ZocMon.GetDataUnion(DataSourceMetas, conn, true);
            }
            for (var i = 0; i < DataSources.Count; i++)
            {
                var lookupByDate = dataSets[DataSources[i]].ToLookup(rec => rec.TimeStamp.Date);
                var reductionType = GetReductionType(DataSources[i]);
                double avg = 0;
                double today = 0;

                switch (reductionType)
                {
                    case MonitorReductionType.DefaultAccumulate:
                        avg = GetAccumulateAvg(lookupByDate);
                        today = GetAccumulateToday(lookupByDate);
                        break;
                    case MonitorReductionType.DefaultAverage:
                        avg = GetAverageAvg(lookupByDate);
                        today = GetAverageToday(lookupByDate);
                        break;
                }

                var ratio = 100 * ((avg != 0) ? today / avg : 1) - 100;
                RatioGraphValues.Add(ratio);
                if (ratio < -25)
                {
                    dataStrings.Add(string.Format(ColoredRatioDataSectionFormat, DataSourceLabels[i] + "<br/>today = " + today + "<br/>avg = " + avg.ToString("f2"),
                                                string.Format(dataPointFormat,
                                                            i + .5,
                                                            ratio), "#ff0000"));
                }
                else if (ratio > 25)
                {
                    dataStrings.Add(string.Format(ColoredRatioDataSectionFormat, DataSourceLabels[i] + "<br/>today = " + today + "<br/>avg = " + avg.ToString("f2"),
                                                string.Format(dataPointFormat,
                                                            i + .5,
                                                            ratio), "#00ff00"));
                }
                else
                {
                    dataStrings.Add(string.Format(RatioDataSectionFormat, DataSourceLabels[i] + "<br/>today = " + today + "<br/>avg = " + avg.ToString("f2"),
                                                    string.Format(dataPointFormat,
                                                                i + .5,
                                                                ratio)));
                }
            }

            string ret;
            if (dataStrings.Count > 0)
            {
                ret = "[" +  dataStrings.StringJoin(",") + "]";
            }
            else
            {
                ret = "[ [ ] ] ";
            }
            return ret;
        }

        public void SetConversionRatesData()
                
        {
            if (Type == GraphType.ConversionRates)
            {
                Dictionary<string, List<MonitorRecordUnion<double>>> dataDict;

                using (var conn = Sql.GetLogConnection().Open())
                {
                    dataDict = UseCache
                                   ? CacheZocMon.GetData(DataSourceMetas, conn)
                                   : ZocMon.GetDataUnion(DataSourceMetas, conn, true);
                }

                //set the data for the 3 graphs
                ConversionRatesData = new List<string>();
                SetConversionRatesLastDay(new List<List<MonitorRecordUnion<double>>>
                                               {
                                                   dataDict[DataSources[0]].Where( x => x.TimeStamp > DateTime.Now.AddDays(-7)).ToList(), 
                                                   dataDict[DataSources[1]].Where( x => x.TimeStamp > DateTime.Now.AddDays(-7)).ToList(), 
                                                   dataDict[DataSources[2]].Where( x => x.TimeStamp > DateTime.Now.AddDays(-7)).ToList(),
                                                   dataDict[DataSources[6]].Where( x => x.TimeStamp > DateTime.Now.AddDays(-7)).ToList(), 
                                                   dataDict[DataSources[7]].Where( x => x.TimeStamp > DateTime.Now.AddDays(-7)).ToList(), 
                                                   dataDict[DataSources[8]].Where( x => x.TimeStamp > DateTime.Now.AddDays(-7)).ToList()
                                               });

                SetConversionRatesLastWeek(new List<List<MonitorRecordUnion<double>>>
                                               {
                                                   dataDict[DataSources[3]], 
                                                   dataDict[DataSources[4]], 
                                                   dataDict[DataSources[5]], 
                                                   dataDict[DataSources[9]], 
                                                   dataDict[DataSources[10]], 
                                                   dataDict[DataSources[11]]
                                               });

                SetConversionRatesRatio(new  List<List<MonitorRecordUnion<double>>>
                                               {
                                                   dataDict[DataSources[0]],
                                                   dataDict[DataSources[1]],
                                                   dataDict[DataSources[2]],
                                                   dataDict[DataSources[6]],
                                                   dataDict[DataSources[7]],
                                                   dataDict[DataSources[8]]
                                               });
            }
            else
            {
                ConversionRatesData = new List<string> {"[[]]", "[[]]", "[[]]"};
            }
        }

        public void SetConversionRatesLastDay(List<List<MonitorRecordUnion<double>>> data)
        {
            var calculatedData = new List<List<MonitorRecordUnion<double>>>();
            var sumInterval = new TimeSpan(0, 30, 0);
            var halfInterval = new TimeSpan(0, 15, 0); ;
            DateTime tempDateTime;

            for (var i = 0; i < 3; i++)
            {
                if (data[i + 3] == null)
                {
                    throw (new Exception("Data isn't complete for SetConversionRatesLastDay"));
                }
                tempDateTime = DateTime.Now.AddDays(-1);
                calculatedData.Add(new List<MonitorRecordUnion<double>>());

                while (tempDateTime < DateTime.Now)
                {
                    DateTime time = tempDateTime;


                    double tempApptBooked = data[i].Where(
                        x => x.TimeStamp >= time && x.TimeStamp < time + sumInterval).Sum(x => x.Value);

                    double tempHitCount = data[i + 3].Where(
                        x => x.TimeStamp >= time && x.TimeStamp < time + sumInterval).Sum(x => x.Value);

                    if (tempHitCount != 0)
                    {
                        calculatedData[i].Add(new MonitorRecordUnion<double>
                        {
                            Value = tempApptBooked / tempHitCount,
                            TimeStamp = tempDateTime.Add(halfInterval)
                        });
                    }

                    //increment time
                    tempDateTime = tempDateTime.Add(sumInterval);
                }
            }

            ConversionRatesData.Add(BuildJsArrayString(calculatedData));
        }

        public void SetConversionRatesLastWeek(List<List<MonitorRecordUnion<double>>> data)
        {
            var calculatedData = new List<List<MonitorRecordUnion<double>>>();
            var sumInterval = new TimeSpan(4, 0, 0);
            var halfInterval = new TimeSpan(2, 0, 0); ;
            DateTime tempDateTime;

            for (var i = 0; i < 3; i++)
            {
                if (data[i + 3] == null)
                {
                    throw (new Exception("Data isn't complete for SetConversionRatesLastWeek"));
                }

                calculatedData.Add(new List<MonitorRecordUnion<double>>());

                tempDateTime = DateTime.Now.AddDays(-7);
                
                while (tempDateTime < DateTime.Now)
                {
                    DateTime time = tempDateTime;


                    double tempApptBooked = data[i].Where(
                        x => x.TimeStamp >= time && x.TimeStamp < time.Add(sumInterval)).Sum(x => x.Value);

                    double tempHitCount = data[i + 3].Where(
                        x => x.TimeStamp >= time && x.TimeStamp < time + sumInterval).Sum( x => x.Value);

                    if (tempHitCount != 0)
                    {
                        calculatedData[i].Add(new MonitorRecordUnion<double>
                        {
                            Value = tempApptBooked / tempHitCount,
                            TimeStamp = tempDateTime.Add(halfInterval)
                        });    
                    }

                    //increment time
                    tempDateTime = tempDateTime.Add(sumInterval);
                }
            }

            ConversionRatesData.Add(BuildJsArrayString(calculatedData));
        }

        public void SetConversionRatesRatio(List<List<MonitorRecordUnion<double>>> data)
        {
            const string dataPointFormat = "[{0}, '{1}']";
            IList<string> dataStrings = new List<string>();
            RatioGraphValues = new List<double>();

            for (var i = 0; i < 3; i++)
            {
                if (data[i + 3] == null)
                {
                    throw (new Exception("Data isn't complete for SetConversionRatesLastWeek"));
                }

                var apptLookup = data[i].ToLookup(rec => rec.TimeStamp.Date);
                var hitCountLookup = data[i + 3].ToLookup(rec => rec.TimeStamp.Date);
                
                var avgMe = new List<double>();
                double tempAppt = 0;
                double tempHitCount = 0;
                foreach (var dateNum in DefaultWeeks)
                {
                    tempAppt = 0;
                    tempHitCount = 0;
                    if (apptLookup[DateTime.Today.AddDays(RatioDates[dateNum])].Any())
                    {
                        tempAppt = apptLookup[DateTime.Today.AddDays(RatioDates[dateNum])]
                            .Sum(r => (r.TimeStamp.TimeOfDay < DateTime.Now.TimeOfDay)
                                ? r.Value
                                : 0);
                    }
                    if (hitCountLookup[DateTime.Today.AddDays(RatioDates[dateNum])].Any())
                    {
                        tempHitCount = hitCountLookup[DateTime.Today.AddDays(RatioDates[dateNum])]
                            .Sum(r => (r.TimeStamp.TimeOfDay < DateTime.Now.TimeOfDay)
                                          ? r.Value
                                          : 0);
                    }

                    if (tempHitCount != 0)
                    {
                        avgMe.Add(tempAppt / tempHitCount);
                    }
                }
                var avgConversionRate = !avgMe.Any() ? 0 : avgMe.Average();

                tempAppt = apptLookup[DateTime.Today].Sum(r => (r.TimeStamp.TimeOfDay < DateTime.Now.TimeOfDay)
                                ? r.Value
                                : 0);
                tempHitCount = hitCountLookup[DateTime.Today].Sum(r => (r.TimeStamp.TimeOfDay < DateTime.Now.TimeOfDay)
                                ? r.Value
                                : 0);

                double todayConversionRate;
                double ratio;
                if (tempHitCount != 0)
                {
                    todayConversionRate = tempAppt / tempHitCount;
                    ratio = 100 * ((avgConversionRate != 0) ? todayConversionRate / avgConversionRate : 1) - 100;
                }
                else
                {
                    todayConversionRate = 0;
                    ratio = 0;
                }

                RatioGraphValues.Add(ratio);

                if (ratio < -25)
                {
                    dataStrings.Add(string.Format(ColoredRatioDataSectionFormat, DataSourceLabels[i] + "<br/>today = " + todayConversionRate + "<br/>avg = " + avgConversionRate.ToString("f2"),
                                                string.Format(dataPointFormat,
                                                            i + .5,
                                                            ratio), "#ff0000"));
                }
                else if (ratio > 25)
                {
                    dataStrings.Add(string.Format(ColoredRatioDataSectionFormat, DataSourceLabels[i] + "<br/>today = " + todayConversionRate + "<br/>avg = " + avgConversionRate.ToString("f2"),
                                                string.Format(dataPointFormat,
                                                            i + .5,
                                                            ratio), "#00ff00"));
                }
                else
                {
                    dataStrings.Add(string.Format(RatioDataSectionFormat, DataSourceLabels[i] + "<br/>today = " + todayConversionRate + "<br/>avg = " + avgConversionRate.ToString("f2"),
                                                    string.Format(dataPointFormat,
                                                                i + .5,
                                                                ratio)));
                }
            }

            string ret;
            if (dataStrings.Count > 0)
            {
                ret = "[" + dataStrings.Aggregate((i, j) => i + "," + j) + "]";
            }
            else
            {
                ret = "[ [ ] ] ";
            }

            ConversionRatesData.Add(ret);
        }

        public string GetConversionRatesData(int i)
        {
            if (i > -1 && i < 3)
            {
                return ConversionRatesData[i];
            }
            return "[[]]";
        }


        #endregion  

        #region Functions for Saved Groups 
        public static SavedGroup GetSavedGroup(int id)
        {
            var groups = SqlHelper.CreateList<SavedGroup>(Sql.GetLogConnection, "SELECT * FROM savedgraphs with (nolock) WHERE id=@id", new { id });

            if (groups.Count > 1)
            {
                throw new Exception("Impossible: More than one SavedGroup with Id " + id + " in Database");
            }
            if (groups.Count == 0)
            {
                throw new Exception("No SavedGroup with Id " + id);
            }

            return groups.First();
        }

        public static void UpdateSavedGroup(SavedGroup group)
        {
            SqlHelper.UpdateRecord<SavedGroup>(Sql.GetLogConnection, group, "savedgraphs");
        }

        public static IEnumerable<SavedGroup> GetSavedGroups()
        {
            return SqlHelper.CreateList<SavedGroup>(Sql.GetLogConnection, "SELECT * FROM savedgraphs");
        }

        public static void AddSavedGroup(SavedGroup group)
        {
            SqlHelper.InsertRecord(Sql.GetLogConnection, group, "savedgraphs");
        }

        //returns true if successful
        public static bool RemoveSavedGroup(int id)
        {
            try
            {
                string sql = "DELETE FROM savedgraphs WHERE id=@id";
                SqlHelper.ExecuteNonQuery(Sql.GetLogConnection, sql, new { id });
            }
            catch (Exception e)
            {
                SimpleLogger.logException(e, to: "matthew.murchison@zocdoc.com");
                return false;
            }
            return true;
        }
        #endregion

        #region Converter Functions 
        public static string FormCollectionToQueryString(FormCollection collection)
        {
            var queryString = "";
            foreach (var key in collection.AllKeys)
            {
                queryString += "&" + key + "=" + collection[key];
            }
            return queryString;
        }

        public string GetMarkerJsString()
        {
            if (!Events.Any())
            {
                return "[]";
            }

            StringBuilder sb = new StringBuilder("[");

            for (int i = 0; i < Events.Count; i++)
            {
                GraphEvent graphEvent = Events[i];
                sb.Append(@"{
                    color: '#000',
                    lineWidth: 1,
                    xaxis: {
                        from: ");
                sb.Append(TimeZoneAdjusted(graphEvent.Event_time)/Config.TICKS_IN_MILLISECOND);
                sb.Append(@",
                        to: ");
                sb.Append(TimeZoneAdjusted(graphEvent.Event_time)/Config.TICKS_IN_MILLISECOND);
                sb.Append(@"
                    }
                }");
                if(i < Events.Count - 1)
                {
                    sb.Append(",");
            }
            }
            sb.Append("]");
            return sb.ToString();            
        }

        public string GetLabelJsString()
        {
            if (Events.Any())
            {
                var str = Events.Aggregate("[",
                                           (current, graphEvent) =>
                                           current +
                                           (@"{
                    text: 'w" + (graphEvent.Event_server != null ? graphEvent.Event_server.Substring(3) : "Unknown") +
                                            ((GraphEventType)
                                             Enum.ToObject(typeof(GraphEventType), graphEvent.Event_type)).ToString().
                                                Substring(0, 1) + @"',
                    x: " +
                                            TimeZoneAdjusted(graphEvent.Event_time) / Config.TICKS_IN_MILLISECOND +
                                            @",
                    y: " + GetMinYAxis() +
                                            @"
                },"));
                return str.Substring(0, str.Length - 1) + "]";
            }
            return "[]";
        }

        public static string ListOfStringsToJsArrayString(List<string> strings)
        {
            var divider = ", ";
            var final = strings.Aggregate("[", (current, str) => current + ("'" + str + "'" + divider));
            if (strings.Count > 0)
            {
                final = final.Substring(0, final.Length - divider.Length);
            }

            return final + "]";
        }

        public static TimeRange HistoryRangeToTimeRange(HistoryRangeOptions historyRangeOptions)
        {
            TimeRange ret;
            DateTime start = MinDate;
            DateTime end = DateTime.MaxValue;
            switch (historyRangeOptions)
            {
                case HistoryRangeOptions.LastHour:
                    start = DateTime.Now.AddHours(-1);
                    break;
                case HistoryRangeOptions.LastFourHours:
                    start = DateTime.Now.AddHours(-4);
                    break;
                case HistoryRangeOptions.LastEightHours:
                    start = DateTime.Now.AddHours(-8);
                    break;
                case HistoryRangeOptions.LastDay:
                    start = DateTime.Now.AddDays(-1);
                    break;
                case HistoryRangeOptions.LastWeek:
                    start = DateTime.Now.AddDays(-7);
                    break;
                case HistoryRangeOptions.LastMonth:
                    start = DateTime.Now.AddDays(-30);
                    break;
                case HistoryRangeOptions.LastWeekToday:
                    start = DateTime.Now.AddDays(-8);
                    end = DateTime.Now.AddDays(-7);
                    break;
                default:
                    SimpleLogger.logException("Unknown history range option " + historyRangeOptions);
                    break;
            }

            ret = new TimeRange
            {
                Start = start,
                End = end,
                WhereClause = DateTime.MaxValue.Equals(end) ? string.Format(LoadDataWhereToPresent, start) : string.Format(LoadDataWhereRange, start, end),
            };
            return ret;
        }
        
        public static List<SelectListItem> SavedGroupsToSelectListItems(IEnumerable<SavedGroup> groups)
        {
            var selectListItems = new List<SelectListItem>
                                      {
                                          new SelectListItem
                                              {
                                                  Value = "0",
                                                  Text = "Select...",
                                                  Selected = true
                                              }
                                      };
            foreach (var group in groups)
            {
                selectListItems.Add(new SelectListItem
                {
                    Value = group.Id.ToString(),
                    Text = group.Name,
                    Selected = false
                });
            }
            return selectListItems;
        }

        public static List<SelectListItem> MonitorConfigsToSelectListItems(List<MonitorConfigRecord> records)
        {
            var selectListItems = new List<SelectListItem>
                                      {
                                          new SelectListItem
                                              {
                                                  Value = "0",
                                                  Text = "Select...",
                                                  Selected = true
                                              }
                                      };
            selectListItems.AddRange(records.Select(t => new SelectListItem
                                                                {
                                                                    Value = t.Name, Text = t.Name, Selected = false
                                                                }));
            return selectListItems;
        }

        #endregion

        #region  X and Y Axis Functions 
        public static long TimeZoneAdjusted(DateTime dateTime)
        {
            return dateTime.Subtract(EpochInTimeZone).Ticks;
        }

        public string GetTickFormatter()
        {
            const string nullTickFormatter = "function (v) { return \"\"; }";
            const string showTickFormatter = @"function(v){
                    if (v < 2 && v > -2 && v != 0){
                        return v.toFixed(5);
                    }
                    return v;
            }";
            return ShowYAxis ? showTickFormatter : nullTickFormatter;
        }

        public string GetMinYAxis()
        {
            const string defaultMin = "null";
            return (Type == GraphType.Ratio) ? "-100" : (ZeroMinYAxis ? "0" : defaultMin);
        }

        public string GetMaxYAxis()
        {
            const string defaultMax = "null";
            return (Type == GraphType.Ratio) ? "100" : defaultMax;
        }

        public string GetMinXAxis()
        {
            const string defaultMin = "null";
            return (Type == GraphType.Ratio) ? ".49" : defaultMin;
        }

        public string GetMaxXAxis()
        {
            const string defaultMax = "null";
            return (Type == GraphType.Ratio) ? (DataSources.Count() + .49).ToString() : defaultMax;
        }

        public bool GetAlertStatus()
        {
            return true;
        }

        #endregion

        #region Ratio Helpers
        private double GetAccumulateAvg(ILookup<DateTime, MonitorRecordUnion<double>> values)
        {
            var avgMe = new List<double>();

            foreach (var dateNum in Weeks)
            {
                if (values[DateTime.Today.AddDays(RatioDates[dateNum])].Any())
                {
                    avgMe.Add(values[DateTime.Today.AddDays(RatioDates[dateNum])]
                        .Sum(r => (r.TimeStamp.TimeOfDay < DateTime.Now.TimeOfDay)
                            ? r.Value
                            : 0));
                }
            }

            return !avgMe.Any() ? 0 : avgMe.Average();
        }

        private double GetAccumulateToday(ILookup<DateTime, MonitorRecordUnion<double>> values)
        {
            return values[DateTime.Today].Sum(r => (r.TimeStamp.TimeOfDay < DateTime.Now.TimeOfDay)
                        ? r.Value
                        : 0);
        }

        private double GetAverageAvg(ILookup<DateTime, MonitorRecordUnion<double>> values)
        {
            var avgMe = new List<double>();

            foreach (var dateNum in Weeks)
            {
                int totalMeasurements = 0;
                double valueSum = 0;
                if (values[DateTime.Today.AddDays(RatioDates[dateNum])].Any())
                {
                    foreach (var record in values[DateTime.Today.AddDays(RatioDates[dateNum])])
                    {
                        if (record.TimeStamp.Hour == DateTime.Now.Hour)
                        {
                            totalMeasurements += record.Number;
                            valueSum += record.Value * record.Number;
                        }
                    }
                    if (totalMeasurements > 0)
                    {
                        avgMe.Add(valueSum / totalMeasurements);
                    }
                }
            }
            return !avgMe.Any() ? 0 : avgMe.Average();
        }

        private double GetAverageToday(ILookup<DateTime, MonitorRecordUnion<double>> values)
        {
            int totalMeasurements = 0;
            double valueSum = 0;
            if (values[DateTime.Today].Any())
            {
                foreach (var record in values[DateTime.Today])
                {
                    if (record.TimeStamp.Hour == DateTime.Now.Hour)
                    {
                        totalMeasurements += record.Number;
                        valueSum += record.Value * record.Number;
                    }
                }
            }
            return (totalMeasurements > 0) ? valueSum / totalMeasurements : 0;
        }

        public string GetTicks()
        {
            string tickFormat = "[{0}, '{1}'],";
            string ret = "[";

            if (DataSources.Any() && (Type == GraphType.Ratio || Type == GraphType.ConversionRates))
            {
                ret = DataSourceLabels.Aggregate(ret, (current, reduceTableName) => current + string.Format(tickFormat, DataSourceLabels.IndexOf(reduceTableName) + 1, GetReduceTableLabel(reduceTableName, DataSourceLabels.IndexOf(reduceTableName))));
                return ret.Substring(0, ret.Length - 1) + "]";
            }
            return "[]";
        }

        private string GetReduceTableLabel(string reduceTableName, int index)
        {
            var label = reduceTableName;

            if (RatioGraphValues[index] < -25)
            {
                label = "<span style=\"color: red;\">" + label + "</span>";
            }
            else if (RatioGraphValues[index] > 25)
            {
                label = "<span style=\"color: green;\">" + label + "</span>";
            }
            return label;
        }
        #endregion

        #region General Helpers

        private static Dictionary<string, MonitorReductionType> _monitorReductionTypes = new Dictionary<string, MonitorReductionType>();

        private static MonitorReductionType GetReductionType(string reduceTableName)
        {
            if (_monitorReductionTypes.ContainsKey(reduceTableName))
            {
                return _monitorReductionTypes[reduceTableName];
            }

            const string sql = @"SELECT MonitorReductionType from MonitorConfig mc
                      inner join ReduceLevel rl ON rl.MonitorConfigName = mc.Name
                      where rl.DataTableName=@reduceTableName";

            List<MonitorReductionTypeRecord> result;

            using (var conn = Sql.GetLogConnection().Open())
            {
                result = SqlHelper.CreateListWithConnection<MonitorReductionTypeRecord>(conn, sql,
                    new { reduceTableName });
            }

            var reductionType = (result.Any())
                ? ((MonitorReductionType)Enum.Parse(typeof(MonitorReductionType), result.First().MonitorReductionType))
                : MonitorReductionType.DefaultAccumulate;

            _monitorReductionTypes[reduceTableName] = reductionType;
            
            return reductionType;
        }

        private List<string> RemoveCommonPrefix(List<string> reduceTableNames)
        {
            if (!reduceTableNames.Any()) return reduceTableNames;

            string tester = reduceTableNames[0];
            List<string> newNames = new List<string>();
            int mark = 0;
            foreach (char c in tester)
            {
                if (reduceTableNames.Any(s => s.Length <= mark || s[mark] != c))
                {
                    break;
                }
                mark++;
            }

            if (mark > 0)
            {
                GraphSubTitle += tester.Substring(0, mark).Replace('_', ' ').Trim();
                newNames.AddRange(reduceTableNames.Select(s => s.Substring(mark, s.Length - mark)));
                return newNames;
            }
            return reduceTableNames;
        }

        private List<string> RemoveCommonSuffix(List<string> reduceTableNames)
        {
            if (!reduceTableNames.Any()) return reduceTableNames;

            string tester = new string(reduceTableNames[0].Reverse().ToArray());
            List<string> revReduceTableNames = reduceTableNames.Select(s => new string(s.Reverse().ToArray())).ToList();
            List<string> newNames = new List<string>();
            int mark = 0;
            foreach (char c in tester)
            {
                if (revReduceTableNames.Any(s => s.Length <= mark || s[mark] != c))
                {
                    break;
                }
                mark++;
            }

            if (mark > 0)
            {
                GraphSubTitle += " - " + new string(tester.Substring(0, mark).Reverse().ToArray()).Replace('_', ' ').Trim();
                newNames.AddRange(
                    revReduceTableNames.Select(s => new string(s.Substring(mark, s.Length - mark).Reverse().ToArray())));
                return newNames;
            }
            return reduceTableNames;
        }
        public static BaseReduceLevels GetReduceLevelFromTable(string tableName)
        {
            if (tableName.Contains("DailyData"))
            {
                return BaseReduceLevels.Daily;
            }
            if (tableName.Contains("HourlyData"))
            {
                return BaseReduceLevels.Hourly;
            }
            if (tableName.Contains("FiveMinutelyData"))
            {
                return BaseReduceLevels.FiveMinutely;
            }
            return BaseReduceLevels.Minutely;
        }

        public static TimeRange GetTimeRange(DateTime start, DateTime end)
        {
            return new TimeRange
                       {
                           Start = start,
                           End = end,
                           WhereClause = end == DateTime.Now || end == DateTime.MaxValue 
                            ? string.Format(LoadDataWhereToPresent, start) 
                            : string.Format(LoadDataWhereRange, start, end)
                       };
        }

        private string BuildJsArrayString(List<List<MonitorRecordUnion<double>>> dataSets)
        {
            //build js array string
            StringBuilder dataSb = new StringBuilder("[");
            if (dataSets.Count > 0)
            {
                for (var i = 0; i < dataSets.Count; i++)
                {
                    if (dataSets[i].Count > MaxDataPointsPerLine)
                    {
                        throw new DataException("Too many data points for \"" + DataSources[i] + "\" (" +
                                                dataSets[i].Count + " of " + MaxDataPointsPerLine + " allowed)");
                    }

                    if (dataSets[i].Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int j = 0; j < dataSets[i].Count; j++)
                        {
                            var ds = dataSets[i][j];
                            sb.Append(Prefix);
                            sb.Append(TimeZoneAdjusted(ds.TimeStamp)/Config.TICKS_IN_MILLISECOND);
                            sb.Append(Comma);
                            sb.Append(ds.Value);
                            sb.Append(Suffix);
                            if (j < dataSets[i].Count - 1)
                            {
                                sb.Append(Comma);
                            }
                        }

                        string labelStr = "";
                        
                        if (Type != GraphType.MonthReview)
                        {
                            labelStr = DataSourceLabels[i];
                        }
                        string dataSection = string.Format(DataSectionFormat, labelStr, sb.ToString());
                        dataSb.Append(dataSection);
                        if (i < dataSets.Count - 1)
                        {
                            dataSb.Append(Comma);
                        }
                    }
                }
            }
            if (dataSb.Length <= 1)
            {
                return "[ [ ] ]";
            }
            dataSb.Append("]");
            return dataSb.ToString();
        }
        #endregion
    }

    #region Sql Structs
    public class SavedGroup
    {
        public long Id { get; set; }
        public String Name { get; set; }
        public String Query { get; set; }
    }

    public class GraphEvent
    {
        public long Id { get; set; }
        public Int32 Event_type { get; set; }
        public DateTime Event_time { get; set; }
        public String Event_server { get; set; }
    }

    public class MonitorReductionTypeRecord
    {
        public String MonitorReductionType { get; set; }

    }

    public class MonitorConfigRecord
    {
        public String Name { get; set; }
        public long PredictionSourceResolution { get; set; }
        public long PredictionReferenceResolution { get; set; }
        public String PredictionClassName { get; set; }
        public String MonitorReductionType { get; set; }

    }

    #endregion

    public static class HtmlExtensions
    {
        public static IEnumerable<SelectListItem> ToSelectListItems<TEnum>(this TEnum enumObj)
        {
            return from TEnum e in Enum.GetValues(typeof(TEnum))
                   let name = e.ToString()
                   select new SelectListItem
                   {
                       Value = name,
                       Text = name,
                       Selected = false
                   };
        }
    }

}