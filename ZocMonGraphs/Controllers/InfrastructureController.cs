using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;
using Csr.Model;
using Infrastructure;
using Services;
using ZocDoc.Mvc;
using ZocDoc.Mvc.ActionFilters;
using ZocDoc.Mvc.ViewModels;
using ZocMonLib;
using ZocUtil;
using ZocUtil.Extensions;
using ZocUtil.CollectionExtensions;


namespace Controllers
{
    [ZDRequireHttps]
    [SessionState(SessionStateBehavior.Disabled)]
    public class InfrastructureController : ZDController
    {
        protected List<MonitorConfig> ConfigOptionValues = Config.MonitorConfigs.Values.ToList();
        private ZocMonGraph zocMonGraph;

        public InfrastructureController()
        {
            SecurityCheck();
        }


        // begin Pages
        public ActionResult ZocMonGraph()
        {
            if (!CacheUser.IsInCsrRole())
            {
                throw new UnauthorizedAccessException();
            }
            try
            {
                return View(GetGraphSelectionViewModel());
            }
            catch (Exception e)
            {
                SimpleLogger.logException(e, to: "matthew.murchison@zocdoc.com");
                throw;
            }
        }

        [HttpPost]
        public ActionResult ZocMonGraph(FormCollection form)
        {
            if (!CacheUser.IsInCsrRole())
            {
                throw new UnauthorizedAccessException();
            }
            try
            {
                if (Request.Params["password"] == null || Util.GetAppSetting("zocmonGraphPassword") != Request.Params["password"] ||
                    (!CacheTrustedIp.RequestIsFromTrustedIp() && !CacheUser.UserIsOnLocalhost()))
                {
                    if (form.Get("formAction") != null && form.Get("formAction") == "delete")
                    {
                        Infrastructure.ZocMonGraph.RemoveSavedGroup(int.Parse(form.Get("savedGroup")));
                    }
                    else if (form.Get("formAction") != null && form.Get("formAction") == "saveNew")
                    {
                        Infrastructure.ZocMonGraph.AddSavedGroup(new SavedGroup
                                                                     {
                                                                         Name = form.Get("name"),
                                                                         Query =
                                                                             Infrastructure.ZocMonGraph.
                                                                             FormCollectionToQueryString(form)
                                                                     });
                    }
                    else if (form.Get("formAction") != null && form.Get("formAction") == "saveEdits")
                    {
                        Infrastructure.ZocMonGraph.UpdateSavedGroup(new SavedGroup
                                                                        {
                                                                            Id = int.Parse(form.Get("savedGroup")),
                                                                            Name = form.Get("name"),
                                                                            Query =
                                                                                Infrastructure.ZocMonGraph.
                                                                                FormCollectionToQueryString(form)
                                                                        });
                    }
                }
                else
                {
                    throw new UnauthorizedAccessException("Unauthorized Access.");
                }

                return View(GetGraphSelectionViewModel(form));
            }
            catch (Exception e)
            {
                SimpleLogger.logException(e, to: "matthew.murchison@zocdoc.com");
                throw;
            }
        }

        public ActionResult Graphs()
        {
            try
            {
                var model = new GraphsWrapperViewModel();

                model.UrlBase = Util.isDebugMachine() ? "" : "https://csr.zocdoc.com";
                model.GraphParams = Request.QueryString.ToString();

                return View(model);
            }
            catch (Exception e)
            {
                SimpleLogger.logException(e, to: "matthew.murchison@zocdoc.com");
                throw;
            }
        }

        public ActionResult GraphsFrame()
        {
            try
            {
                var model = new GraphsViewModel();
                
                model.GraphsList = GetGraphs(Request.Params["savedGroup"] != null && Request.Params["savedGroup"] != ""
                                                 ? HttpUtility.ParseQueryString(
                                                     Infrastructure.ZocMonGraph.GetSavedGroup(
                                                         int.Parse(Request.Params["savedGroup"])).Query)
                                                 : Request.QueryString);

                return View(model);
            }
            catch (Exception e)
            {
                SimpleLogger.logException(e, to: "matthew.murchison@zocdoc.com");
                throw;
            }
        }

        
        public ActionResult ConversionRates()
        {
            try
            {
                GraphsViewModel model = new GraphsViewModel();

                var graph = new ZocMonGraph(Infrastructure.ZocMonGraph.GraphType.ConversionRates);
                graph.SetConversionRatesData();

                model.GraphsList.Add(new GraphViewModel
                                         {
                                             TickFormatter = graph.GetTickFormatter(),
                                             MinYAxis = graph.GetMinYAxis(),
                                             MaxYAxis = graph.GetMaxYAxis(),
                                             MinXAxis = graph.GetMinXAxis(),
                                             MaxXAxis = graph.GetMaxXAxis(),
                                             AlertStatus = graph.GetAlertStatus().ToString(),
                                             LabeledData = graph.GetConversionRatesData(0),
                                             Events = graph.GetMarkerJsString(),
                                             Labels = graph.GetLabelJsString(),
                                             Index = 0,
                                             Width = graph.DisplayWidth,
                                             Height = graph.DisplayHeight,
                                             Title = "Conversion Rates: Last Day",
                                             Ticks = graph.GetTicks(),
                                             SubTitle = "",
                                             TitleSize = graph.DisplayTitleSize,
                                             Type = Infrastructure.ZocMonGraph.GraphType.Line,
                                             IsRatio = false,
                                             DataSources =
                                                 Infrastructure.ZocMonGraph.ListOfStringsToJsArrayString(
                                                     graph.DataSources)
                                         });
                model.GraphsList.Add(new GraphViewModel
                                         {
                                             TickFormatter = graph.GetTickFormatter(),
                                             MinYAxis = graph.GetMinYAxis(),
                                             MaxYAxis = graph.GetMaxYAxis(),
                                             MinXAxis = graph.GetMinXAxis(),
                                             MaxXAxis = graph.GetMaxXAxis(),
                                             AlertStatus = graph.GetAlertStatus().ToString(),
                                             LabeledData = graph.GetConversionRatesData(1),
                                             Events = graph.GetMarkerJsString(),
                                             Labels = graph.GetLabelJsString(),
                                             Index = 1,
                                             Width = graph.DisplayWidth,
                                             Height = graph.DisplayHeight,
                                             Title = "Conversion Rates: Last Week",
                                             Ticks = graph.GetTicks(),
                                             SubTitle = "",
                                             TitleSize = graph.DisplayTitleSize,
                                             Type = Infrastructure.ZocMonGraph.GraphType.Line,
                                             IsRatio = false,
                                             DataSources =
                                                 Infrastructure.ZocMonGraph.ListOfStringsToJsArrayString(
                                                     graph.DataSources)
                                         });
                model.GraphsList.Add(new GraphViewModel
                                         {
                                             TickFormatter = graph.GetTickFormatter(),
                                             MinYAxis = "-100",
                                             MaxYAxis = "100",
                                             MinXAxis = graph.GetMinXAxis(),
                                             MaxXAxis = graph.GetMaxXAxis(),
                                             AlertStatus = graph.GetAlertStatus().ToString(),
                                             LabeledData = graph.GetConversionRatesData(2),
                                             Events = graph.GetMarkerJsString(),
                                             Labels = graph.GetLabelJsString(),
                                             Index = 2,
                                             Width = graph.DisplayWidth,
                                             Height = 400,
                                             Title = "Conversion Rate Ratios",
                                             Ticks = graph.GetTicks(),
                                             SubTitle = "",
                                             TitleSize = graph.DisplayTitleSize,
                                             Type = Infrastructure.ZocMonGraph.GraphType.Ratio,
                                             IsRatio = true,
                                             DataSources =
                                                 Infrastructure.ZocMonGraph.ListOfStringsToJsArrayString(
                                                     graph.DataSources)
                                         });

                return View(model);
            }
            catch (Exception e)
            {
                SimpleLogger.logException(e, to: "matthew.murchison@zocdoc.com");
                throw;
            }
        }

#region Helpers

        private void SecurityCheck()
        {
            if (!CacheTrustedIp.RequestIsFromTrustedIp() &&
                !CacheUser.UserIsOnLocalhost() &&
                !CacheUser.IsInCsrRole())
            {
                throw new UnauthorizedAccessException("Unauthorized Access.");
            }
        }

        private GraphSelectionViewModel GetGraphSelectionViewModel(FormCollection form = null)
        {
            var model = new GraphSelectionViewModel();
            model.PageTitle = "ZocMon Graph Selection";
            ConfigOptionValues.Sort((x, y) => string.Compare(x.Name, y.Name));
            model.ConfigOptionValues = ConfigOptionValues;
            model.MonitorReduceLevelSeparator = Infrastructure.ZocMonGraph.MonitorReduceLevelSeparator;
            model.SavedGroupsSection = new SavedGroupsViewModel
                                           {
                                               SavedGroupsDropdown =
                                                   Infrastructure.ZocMonGraph.SavedGroupsToSelectListItems(
                                                       Infrastructure.ZocMonGraph.GetSavedGroups().OrderBy(
                                                           group => group.Name))
                                           };

            if (form != null && form.Get("formAction") != null && form.Get("formAction") == "edit")
            {
                SavedGroup group = Infrastructure.ZocMonGraph.GetSavedGroup(int.Parse(form.Get("savedGroup")));
                model.EditGroupJson = "{ query: '" + group.Query + "', id: '" + group.Id + "', name: '" + group.Name +
                                      "' }";
            }
            return model;
        }

        private List<GraphViewModel> GetGraphs(NameValueCollection values)
        {
            var graphsList = new List<GraphViewModel>();
            if (values.Get("graphCount") != null)
            {
                var graphCount = int.Parse(values.Get("graphCount"));

                for (var i = 0; i < graphCount; i++)
                {
                    bool isMonthReview = values.Get("isMonthReview") != null;
                    string reduceLevelNameParams = values.Get("ReduceLevel" + i);
                    if (reduceLevelNameParams != null || isMonthReview)
                    {

                        if (isMonthReview)
                        {
                            //each graph gets a fresh instance of ZocMonGraph to work with
                            //in this case reduceLevelNameParams is equal to the data source 
                            //(without reduce level portion of string)
                            zocMonGraph = new ZocMonGraph(reduceLevelNameParams);
                        }
                        else
                        {
                            #region prep line and ratio graphs

                            ZocMonGraph.GraphType type = (ZocMonGraph.GraphType)
                                                         Enum.Parse(typeof (ZocMonGraph.GraphType),
                                                                    values.Get("graphType" + i), true);

                            #region parse date ranges

                            var isExplicitTimeRange = type != Infrastructure.ZocMonGraph.GraphType.Ratio &&
                                                      values.Get("HistoryRange" + i) != null &&
                                                      "0".Equals(values.Get("HistoryRange" + i)) &&
                                                      values.Get("fromDate" + i) != null &&
                                                      values.Get("fromTime" + i) != null &&
                                                      values.Get("toDate" + i) != null &&
                                                      values.Get("toTime" + i) != null;

                            DateTime from = new DateTime();
                            DateTime to = new DateTime();

                            if (isExplicitTimeRange)
                            {
                                if (
                                    !DateTime.TryParse(
                                        values.Get("fromDate" + i) + " " + values.Get("fromTime" + i),
                                        out from))
                                {
                                    throw new Exception("Failed to parse date time string \"" +
                                                        values.Get("fromDate" + i) +
                                                        " " + values.Get("fromTime" + i) + "\"");
                                }
                                if (
                                    !DateTime.TryParse(values.Get("toDate" + i) + " " + values.Get("toTime" + i),
                                                       out to))
                                {
                                    throw new Exception("Failed to parse date time string \"" +
                                                        values.Get("toDate" + i) +
                                                        " " + values.Get("toTime" + i) + "\"");
                                }
                            }
                            else
                            {
                                from = Infrastructure.ZocMonGraph.MinDate;
                                to = DateTime.MaxValue;
                            }

                            #endregion

                            #region parse history range

                            ZocMonGraph.HistoryRangeOptions historyRange;

                            if (
                                !Infrastructure.ZocMonGraph.HistoryRangeOptions.TryParse(
                                    values.Get("HistoryRange" + i),
                                    out historyRange))
                            {
                                SimpleLogger.logException("Failed to convert \"" + values.Get("HistoryRange" + i) +
                                                          "\" to enum value in HistoryRangeOptions");
                                throw new Exception("Back up and select a history range...");
                            }

                            #endregion

                            //if showEvents is in the url it overrides the bool that has been saved
                            bool showEvents = true;
                            if (Request.Params.Get("showevents" + i) != null)
                            {
                                showEvents = Request.Params.Get("showEvents" + i) == "on" ||
                                             Request.Params.Get("showEvents" + i) == "true"
                                                 ? true
                                                 : false;
                            }
                            else
                            {
                                if (values.Get("showevents" + i) != null)
                                {
                                    showEvents = values.Get("showevents" + i) == "on" ? true : false;
                                }
                            }


                            //allow for ratio weeks url hack
                            string weeks = "";
                            if (Request.Params.Get("weeks" + i) != null)
                            {
                                weeks = Request.Params.Get("weeks" + i);
                            }

                            //each graph gets a fresh instance of ZocMonGraph to work with
                            zocMonGraph = new ZocMonGraph(graphCount, values.Get("titleBox" + i),
                                                          type,
                                                          values.Get("showYAxis" + i) == "on" ? true : false,
                                                          values.Get("zeroMinYAxis" + i) == "on" ? true : false,
                                                          showEvents,
                                                          reduceLevelNameParams, false, isExplicitTimeRange, from,
                                                          to,
                                                          historyRange, weeks);

                            #endregion
                        }


                        graphsList.Add(new GraphViewModel
                                           {
                                               TickFormatter = zocMonGraph.GetTickFormatter(),
                                               MinYAxis = zocMonGraph.GetMinYAxis(),
                                               MaxYAxis = zocMonGraph.GetMaxYAxis(),
                                               MinXAxis = zocMonGraph.GetMinXAxis(),
                                               MaxXAxis = zocMonGraph.GetMaxXAxis(),
                                               AlertStatus = zocMonGraph.GetAlertStatus().ToString(),
                                               LabeledData = zocMonGraph.GetData(),
                                               Events = zocMonGraph.GetMarkerJsString(),
                                               ShowEvents = zocMonGraph.ShowEvents,
                                               Labels = zocMonGraph.GetLabelJsString(),
                                               Index = i,
                                               Width = zocMonGraph.DisplayWidth,
                                               Height = zocMonGraph.DisplayHeight,
                                               Title = zocMonGraph.GraphTitle,
                                               Ticks = zocMonGraph.GetTicks(),
                                               SubTitle = zocMonGraph.GraphSubTitle,
                                               TitleSize = zocMonGraph.DisplayTitleSize,
                                               Type = zocMonGraph.Type,
                                               IsRatio =
                                                   zocMonGraph.Type == Infrastructure.ZocMonGraph.GraphType.Ratio,
                                               DataSources =
                                                   Infrastructure.ZocMonGraph.ListOfStringsToJsArrayString(
                                                       zocMonGraph.DataSources)
                                           });
                    }
                    else
                    {
                        SimpleLogger.logException(new Exception("ReduceLevel" + i + " can't be null"), null,
                                                  to: "matthew.murchison@zocdoc.com");
                    }
                }
            }
            return graphsList;
        }

        /// <summary>
        /// access rules: plain access allowed on prod network, otherwise https+password required
        /// throws on failure
        /// </summary>
        public static void CheckAccess(string password)
        {
            var local = CacheUser.UserIsOnLocalhost() || CacheUser.UserIsOnProductionNetwork();
            if (local)
            {
                //ok, on local network, no further checks needed
                return;
            }

            var isHttps = System.Web.HttpContext.Current.Request.Url.Scheme.ToLower() == "https";

            if (CacheTrustedIp.RequestIsFromTrustedIp())
            {
                if (Util.useSSL() && !isHttps)
                {
                    //not on local network, https required if calling from the office or home if the machine is set to use https
                    throw new UnauthorizedAccessException("https required");
                }
            }
            else
            {
                if (!isHttps)
                {
                    //accessible outside of trusted ips if https and has password
                    throw new UnauthorizedAccessException("https required for remote");
                }
            }

            //if not on lan, password is required
            var pwd = System.Web.HttpContext.Current.Request["password"];
            if (pwd != password)
            {
                throw new UnauthorizedAccessException("not authenticated");
            }

        }
#endregion

        public ActionResult AutoRefreshPage(string path, string queryString, int refreshRateInSeconds, string title)
        {
            var model = new AutoRefreshViewModel
                            {
                                Path = path,
                                QueryString = queryString,
                                RefreshRate = refreshRateInSeconds,
                                Title = title
                            };

            return View("AutoRefreshPage", model);
        }

        public ActionResult AutoRefreshFrame(AutoRefreshViewModel model)
        {
            return View("AutoRefreshFrame", model);
        }



        public ActionResult NetPromoterScore()
        {
            return AutoRefreshPage("/remote/infrastructure/NetPromoterScoreContent", Request.QueryString.ToString(), 60,
                                   "Net Promoter Score");
        }

        private static double _lastNetPromoterScore = -1;
        public ActionResult NetPromoterScoreContent()
        {
            var fb = SqlHelper.CreateList<FeedBack>(Sql.GetConnection,
                                                 @" select NetPromoterScore from Feedback f
                                                inner join ProfessionalRating pr ON pr.ID = f.ProfessionalRatingId
                                                where NetPromoterScore is not NULL
                                                and pr.CreationDate > getdate() - 7 ");


            var netPromoterScore = FeedBack.GetNetPromoterScore(fb);
            var model = new NetPromoterModel{NetPromoterScore = netPromoterScore, OldNetPromoterScore = _lastNetPromoterScore};
            _lastNetPromoterScore = netPromoterScore;

            return View(model);
        }
        public class NetPromoterModel
        {
            public double NetPromoterScore { get; set; }
            public double OldNetPromoterScore { get; set; }

            public double Delta
            {
                get
                {
                    if (OldNetPromoterScore < 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return NetPromoterScore - OldNetPromoterScore;
                    }
                }
            }
        }

        public ActionResult AvailableAppointments()
        {
            return AutoRefreshPage("/remote/infrastructure/AvailableAppointmentsContent", Request.QueryString.ToString(), 60 * 15,
                                   "Available Appointments");
        }

        public ActionResult AvailableAppointmentsContent()
        {
            var availableMinutes = SqlHelper.ExecuteScalar<long>(Sql.GetConnection,
                                                 @" SELECT sum(Duration) from Timeslot where RequestID is NULL and StartTime > getdate() and StartTime < getdate() + 7 ");

            var availableAppts = availableMinutes / 15; // assume a 15-minute appt duration

            return View(availableAppts);
        }


        public ActionResult OnboardingServiceLevels()
        {
            return AutoRefreshPage("/remote/infrastructure/OnboardingServiceLevelsContent", Request.QueryString.ToString(), 60 * 15,
                                   "Onboarding Service Levels");
        }

        public ActionResult OnboardingServiceLevelsContent()
        {
            var tickets = SqlHelper.CreateList<OpsQueueActivation>(Sql.GetConnection,
                                                                   "select * from opsQueueActivation where createdDate > getdate() - 14");

            var activationNeeded = 0;
            var activatedInTime = 0;
            var trainingNeeded = 0;
            var trainedInTime = 0;

            var shouldBeActivatedCutoff = new TimeSpan(1, 0, 0, 0);
            var shouldBeTrainedCutoff = new TimeSpan(7, 0, 0, 0);

            foreach (var ticket in tickets)
            {
                if (TimeZoneMath.BusinessDayDiff(ticket.CreatedDate, DateTime.Now) >= shouldBeActivatedCutoff)
                {
                    activationNeeded++;
                    if (ticket.ActivationCompleted != null)
                    {
                        if (TimeZoneMath.BusinessDayDiff(ticket.CreatedDate, ticket.ActivationCompleted.Value) <= shouldBeActivatedCutoff)
                        {
                            activatedInTime++;
                        }

                        if (DateTime.Now - ticket.ActivationCompleted.Value >= shouldBeTrainedCutoff)
                        {
                            trainingNeeded++;
                            if (ticket.TrainingCompleted.HasValue && ticket.TrainingCompleted.Value - ticket.ActivationCompleted.Value <= shouldBeTrainedCutoff)
                            {
                                trainedInTime++;
                            }
                        }
                    }
                }
            }

            return
                View(new OnboardingServiceLevelsModel
                         {
                             ActivationInTimeRatio = activatedInTime/(double) activationNeeded,
                             TrainingInTimeRatio = trainedInTime/(double) trainingNeeded
                         });
        }
        public class OnboardingServiceLevelsModel
        {
            public double ActivationInTimeRatio { get; set; }
            public double TrainingInTimeRatio { get; set; }
        }


        public ActionResult EmailQueueResponseTime()
        {
            return AutoRefreshPage("/remote/infrastructure/EmailQueueResponseTimeContent", Request.QueryString.ToString(), 60 * 1,
                                   "Email Queue Response Time");
        }
       
        public ActionResult EmailQueueResponseTimeContent()
        {
            var emails = CacheEmail.GetAllEmails()
                .Where(e => e.ResponseDate.HasValue && e.ResponseDate > DateTime.Now.AddDays(-1));

            var avgResponseTime = emails.Any()
                                      ? emails.Average(e => e.ResponseTime.Value.TotalMinutes)
                                      : 0;
            return View(avgResponseTime);
        }

        public class ZoComLogModel
        {
            public int? Entries { get; set; }
        }
        public ActionResult ZoComLog(ZoComLogModel m)
        {
            return View(m);
    }


    }

    public class ZoComLog
    {
        public long Id { get; set; }
        public string TaskUrl { get; set; }
        public DateTime? Ran { get; set; }
        public DateTime? Finished { get; set; }
        public string ResultCode { get; set; }
        public string ResponseText { get; set; }

        public static IEnumerable<ZoComLog> GetLatestTasks(int? items)
        {
            var topN = items.HasValue ? "top " + items.Value.ToString(CultureInfo.InvariantCulture) : String.Empty;
            return SqlHelper.CreateList<ZoComLog>(Sql.GetLogConnection, "select " + topN + " * from ZocomLog order by Finished desc");
        }
    }

    [SessionStateAttribute(SessionStateBehavior.Disabled)]
    public class SqlController : ZDController 
    {
        public SqlController()
        { InfrastructureController.CheckAccess("Dumble123Derp"); }

        public ActionResult FrequencyViewer()
        {
            return View("");
        }

        public ActionResult FrequencyData(string sort = "time", string format="html", bool clear = false)
        {
            if(clear)
            {
                QueryFrequencyLogger.ResetCache();
            }

            //sorts: time (total time), average (avg time per call), calls (number of times called)
            var model = new SqlFrequencyModel
                    {
                        HoursAlive = (DateTime.Now - QueryFrequencyLogger.CollectionStart).TotalHours,
                        Queries = QueryFrequencyLogger.GetTopQueries(sort)
                    };

            long TotalMilliseconds = 0;
            long TotalCalls = 0;
            foreach(var q in model.Queries)
            {
                //aggregate them by server
                TotalMilliseconds += q.TotalMilliseconds;
                TotalCalls += q.TotalCalls;
            }
            var serverTotal = new QueryFrequencyLogger.QueryLog
            {
                Stack = Environment.MachineName + " totals",
                TotalCalls = TotalCalls,
                TotalMilliseconds = TotalMilliseconds
            };
            model.Total = serverTotal;

            if(format == "json")
            {
                model.Queries = model.Queries.Take(99).ToList();//limit
                return Json(model, JsonRequestBehavior.AllowGet);
            }

            return View(model);
        }

        public ActionResult FrequencyAggregate(string sort = "time", bool clear = false)
        {
            var clr = clear ? "&clear=true" : "";
            var remoteUrl = "/remote/infrastructure/Sql/FrequencyData?sort=" + sort + "&format=json" + clr;
            var data = ServerAggregate.HitAllServers(remoteUrl);

            var q =
                (from d in data
                 let model =
                     !d.Error ? Newtonsoft.Json.JsonConvert.DeserializeObject<SqlFrequencyModel>(d.Response) : null
                 select new {d, model})
                 .ToList();

            var ttl = q.Where(x => !x.d.Error).Select(x => x.model).ToList();
            var ttlLog = new QueryFrequencyLogger.QueryLog{Stack = "grand totals all servers"};
            double hoursAlive = 0;
            double percentageSum = 0;
            if(ttl.Count > 0)
            {
                ttlLog.TotalCalls = ttl.Sum(x => x.Total.TotalCalls);
                ttlLog.TotalMilliseconds = ttl.Sum(x => x.Total.TotalMilliseconds);
                hoursAlive = ttl.Sum(x => x.HoursAlive);
                percentageSum = ttl.Sum(x => x.Total.GetPercentTotalTime(x.HoursAlive));
            }

            return
                View(new SqlAggregateModel
                         {
                             Servers = q.ToDictionary(x => x.d, x => x.model),
                             Total = ttlLog,
                             TotalHoursAlive = hoursAlive,
                             SumOfPercentages = percentageSum
                         });
            //return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Sessions(bool ipOnly = false)
        {
            var sessions = ZDSqlServerSessionState.SourceSessions();

            if (sessions == null)
            {
                Response.Write("failed to parse");
                return null;
            }

            var seq = ipOnly
                          ? sessions.TaggedSessionsByIpAndUserAgent.Select(x => x.Item1) // just ip address
                          : sessions.TaggedSessionsByIpAndUserAgent.Select(x => x.ToString()); //ip address + user agent
            
            var grouped = seq.ToHashBag().OrderByDescending(x => x.Value).Select(x => new { count = x.Value, x.Key }).ToList();
            
            var counts = new { totalSessionRecords = sessions.TotalSessions, deserializedSessions = sessions.ParsedSessions, tagged = sessions.TaggedSessionsByIpAndUserAgent.Count, timeToDeserializeInMs = sessions.MsToParse, groups = grouped.Count };
            var tab = grouped.ToHtmlTable();

            Response.Write(counts);
            Response.Write(tab);
            return null;

        }


    }
}

namespace ZocDoc.Mvc.ViewModels
{
    public class SqlAggregateModel
    {
        public Dictionary<ServerAggregate, SqlFrequencyModel> Servers;
        public QueryFrequencyLogger.QueryLog Total;
        public double TotalHoursAlive;
        public double SumOfPercentages;
    }

    public class SqlFrequencyModel
    {
        public List<QueryFrequencyLogger.QueryLog> Queries { get; set; }
        public QueryFrequencyLogger.QueryLog Total { get; set; }
        public double HoursAlive { get; set; }
        public string Error { get; set; }
    }


    public class GraphSelectionViewModel : PageViewModel
    {
        public GraphSelectionViewModel()
        {
            HistoryRange = new ZocMonGraph.HistoryRangeOptions().ToSelectListItems().ToList();
            HistoryRange.Insert(0, new SelectListItem
                    {
                        Value = "0",
                        Text = "Select...",
                        Selected = true
                    });
            
            ConfigOptionValues = new List<MonitorConfig>();
            MonitorReduceLevelSeparator = new char();
            SavedGroupsSection = new SavedGroupsViewModel();
            EditGroupJson = "null";
        }

        public List<MonitorConfig> ConfigOptionValues { get; set; }

        public List<SelectListItem> HistoryRange { get; set; }

        public char MonitorReduceLevelSeparator;

        public SavedGroupsViewModel SavedGroupsSection { get; set; }

        public string EditGroupJson { get; set; }

    }

    public class SavedGroupsViewModel : PageViewModel
    {
        public SavedGroupsViewModel()
        {
            SavedGroupsDropdown = new List<SelectListItem>();
        }

        public List<SelectListItem> SavedGroupsDropdown { get; set; }

    }

    public class GraphsViewModel : PageViewModel
    {
        public GraphsViewModel()
        {
            GraphsList = new List<GraphViewModel>();
        }
        
        public List<GraphViewModel> GraphsList { get; set; }
    }

    public class GraphsWrapperViewModel : PageViewModel
    {
        public GraphsWrapperViewModel()
        {
            GraphParams = string.Empty;
            UrlBase = string.Empty;
        }

        public string GraphParams { get; set; }
        public string UrlBase { get; set; }
    }

    public class GraphViewModel : PageViewModel
    {
        public GraphViewModel ()
        {
            TickFormatter = string.Empty;
            MinYAxis = string.Empty;
            MaxYAxis = string.Empty;
            MinXAxis = string.Empty;
            MaxXAxis = string.Empty;
            AlertStatus = string.Empty;
            LabeledData = string.Empty;
            Index = 0;
            Width = 900;
            Height = 600;
            Title = string.Empty;
            SubTitle = string.Empty;
            TitleSize = 36;
            Events = string.Empty;
            Labels = string.Empty;
            IsRatio = false;
            Type = ZocMonGraph.GraphType.Line;
            Ticks = string.Empty;
            DataSources = string.Empty;
            ShowEvents = true;
        }

        public string TickFormatter { get; set; }

        public string MinYAxis { get; set; }

        public string MaxYAxis { get; set; }

        public string MinXAxis { get; set; }

        public string MaxXAxis { get; set; }

        public string AlertStatus { get; set; }

        public string LabeledData { get; set; }

        public int Index { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string Title { get; set; }

        public string SubTitle { get; set; }

        public int TitleSize { get; set; }

        public string Events { get; set; }

        public string Labels { get; set; }

        public ZocMonGraph.GraphType Type { get; set; }

        public string Ticks { get; set; }

        public string DataSources { get; set; }

        public bool IsRatio { get; set; }

        public bool ShowEvents { get; set; }

    }

    public class AutoRefreshViewModel : PageViewModel
    {
        public AutoRefreshViewModel ()
        {
            Path = String.Empty;
            QueryString = String.Empty;
            RefreshRate = 60;
            Title = String.Empty;
        }

        public string Path { get; set; }
        public string QueryString { get; set; }
        public int RefreshRate { get; set; }
        public string Title { get; set; }
    }

}