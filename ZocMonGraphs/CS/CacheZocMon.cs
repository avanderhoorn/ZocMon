using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZocMonLib;
using ZocUtil;

namespace Infrastructure
{
    //helper class for cache
    public class CacheZocMon
    {
        //zocmon data cache
        private static IDictionary<string, CacheZocMon> _cache = new ConcurrentDictionary<string, CacheZocMon>();

        public DateTime Newest { get; set; }
        public ZocMonGraph.BaseReduceLevels ReduceLevel { get; set; }
        public List<MonitorRecordUnion<double>> Data { get; set; }
        public TimeSpan Overlap { get; set; }
        public string Name { get; set; }
        public bool WarmedUp { get; set; }
        public DateTime LastRefreshed { get; set; }

        //TODO
        public DateTime LastAccessed { get; set; }

        #region non-static functionality

        private CacheZocMon(string tableName, ZocMonGraph.BaseReduceLevels reduceLevel)
        {
            switch (reduceLevel)
            {
                case ZocMonGraph.BaseReduceLevels.Minutely:
                    Overlap = new TimeSpan(0, -4, 0);
                    break;
                case ZocMonGraph.BaseReduceLevels.FiveMinutely:
                    Overlap = new TimeSpan(0, -10, 0);
                    break;
                case ZocMonGraph.BaseReduceLevels.Hourly:
                    Overlap = new TimeSpan(-2, 0, 0);
                    break;
                case ZocMonGraph.BaseReduceLevels.Daily:
                    Overlap = new TimeSpan(-2, 0, 0, 0);
                    break;
            }
            Name = tableName;
            Data = new List<MonitorRecordUnion<double>>();
            WarmedUp = false;
            LastRefreshed = DateTime.MinValue;
        }

        private DateTime GetLastUpdatedTime()
        {
            return Newest.Add(Overlap);
        }

        #endregion

        #region static functionality
        private static void RefreshFromDb(IEnumerable<string> tables, ZDSqlConnection conn)
        {
            var metas = new List<Tuple<string, TimeRange>>();

            try
            {
                foreach (string table in tables)
                {
                    if (!_cache.ContainsKey(table))
                    {
                        //make sure this is a real table before adding it to the cache
                        var sql = "SELECT Count(1) FROM ReduceLevel WHERE DataTableName = @table";
                        if (SqlHelper.ExecuteScalarWithConnection<int>(conn, sql, new { table } ) > 0) 
                        {
                            _cache.Add(table, new CacheZocMon(table, ZocMonGraph.GetReduceLevelFromTable(table)));    
                        }
                        else
                        {
                            continue;
                        }
                    }

                    metas.Add(new Tuple<string, TimeRange>(table, _cache[table].WarmedUp
                                       ? new TimeRange
                                       {
                                           Start = _cache[table].GetLastUpdatedTime(),
                                           End = DateTime.Now
                                       }
                                       : new TimeRange { Start = DateTime.Today.AddDays(-30), End = DateTime.Now }));
                }

                if (metas.Count > 0)
                {
                    var data = ZocMon.GetDataUnion(metas, conn, true);
                    var logString = new StringBuilder();

                    if (new Flag_ZocMonSuperLog().Enabled())
                    {
                        logString.Append("This Refresh at " + DateTime.Now + " with " + metas.Count + " tables to refresh: \n");
                    }

                    //if this is a refresh and not a warmup, log long intervals
                    if (_cache[metas[0].Item1].WarmedUp && _cache[metas[0].Item1].LastRefreshed < DateTime.Now.AddMinutes(-2))
                    {
                        SimpleLogger.logInformation("Time between ZocMon refreshes on " +
                                                    Environment.MachineName + " was " +
                                                    (DateTime.Now - _cache[metas[0].Item1].LastRefreshed).TotalMinutes +
                                                    " minutes.", to: "matthew.murchison@zocdoc.com");
                    }

                    foreach (var results in data)
                    {
                        if (results.Value.Count > 0)
                        {
                            var table = results.Value.First().Source;

                            List<MonitorRecordUnion<double>> resultsLoc = results.Value;

                            if (new Flag_ZocMonSuperLog().Enabled())
                            {
                                logString.Append("Table: " + table + "\n");
                                logString.Append("results length before removing overlap: " + _cache[table].Data.Count +
                                                 "\n");
                            }
                            //remove overlapping data 
                            _cache[table].Data.RemoveAll(x => x.TimeStamp >= resultsLoc.First().TimeStamp);

                            if (new Flag_ZocMonSuperLog().Enabled())
                            {
                                logString.Append("results length after removing overlap: " + _cache[table].Data.Count +
                                                 "\n");
                            }
                            //concat new data to old
                            _cache[table].Data = _cache[table].Data.Concat(results.Value).ToList();

                            if (new Flag_ZocMonSuperLog().Enabled())
                            {
                                logString.Append("old newest: " + _cache[table].Newest ?? "null" + "\n");
                            }
                            _cache[table].Newest = _cache[table].Data.Last().TimeStamp;

                            if (new Flag_ZocMonSuperLog().Enabled())
                            {
                                logString.Append("new newest: " + _cache[table].Newest ?? "null" + "\n");
                            }

                            //set warmed up
                            if (!_cache[table].WarmedUp)
                            {
                                if (new Flag_ZocMonSuperLog().Enabled())
                                {
                                    logString.Append("this was a warmup \n");
                                }
                                _cache[table].WarmedUp = true;
                            }
                            else
                            {
                                if (new Flag_ZocMonSuperLog().Enabled())
                                {
                                    logString.Append("this was a refresh \n");
                                }
                            }
                            _cache[table].LastRefreshed = DateTime.Now;
                        }
                    }
                    if (new Flag_ZocMonSuperLog().Enabled())
                    {
                        SimpleLogger.logInformation(logString.ToString(), to: "matthew.murchison@zocdoc.com");
                    }
                }
            }
            catch (Exception e)
            {
                SimpleLogger.logException(e, to: "matthew.murchison@zocdoc.com");
            }
        }

        //tuple represents "name of table" and "time range data is needed for"
        public static Dictionary<string, List<MonitorRecordUnion<double>>> GetData(List<Tuple<string, TimeRange>> metas, ZDSqlConnection conn)
        {
            var needingRefresh = metas.Select(x => x.Item1).Where(s => !_cache.ContainsKey(s)).ToList();
            if (needingRefresh.Count > 0)
            {
                RefreshFromDb(needingRefresh, conn);
            }

            var ret = new Dictionary<string, List<MonitorRecordUnion<double>>>();
            foreach (var meta in metas)
            {
                if (!_cache.ContainsKey(meta.Item1))
                {
                    continue;
                }

                var dataForMeta = new List<MonitorRecordUnion<double>>();
                for (int i = 0; i < _cache[meta.Item1].Data.Count; i++)
                {
                    if (_cache[meta.Item1].Data[i].TimeStamp >= meta.Item2.Start && _cache[meta.Item1].Data[i].TimeStamp <= meta.Item2.End)
                    {
                        dataForMeta.Add(_cache[meta.Item1].Data[i]);
                    }
                } 
                
                if (ret.ContainsKey(meta.Item1))
                {
                    ret[meta.Item1].AddRange(dataForMeta);
                }
                else
                {
                    ret.Add(meta.Item1, dataForMeta);
                }
            }

            return ret;
        }

        public static void RefreshAll()
        {
            using (ZDSqlConnection conn = Sql.GetLogConnection())
            {
                conn.Open();
                var needingRefresh = _cache.Values.Select(x => x.Name).ToList();
                if (needingRefresh.Count > 0)
                {
                    RefreshFromDb(needingRefresh, conn);
                }
            }
        }
        #endregion
    }
}