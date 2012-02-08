using System;

namespace ZocMonLib
{
    public class StorageCommandsSql
    {
        public const string DataTableCreateFormat = 
            @"CREATE TABLE [{0}] (" +
            "  [TimeStamp] [datetime] NOT NULL," +
            "  [Value] [float] NOT NULL," +
            "  [Number] [int] NOT NULL DEFAULT 1," +
            "  [IntervalSum] [float] NOT NULL," +
            "  [IntervalSumOfSquares] [float] NOT NULL," +
            "  CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED " +
            "  ( [TimeStamp] ASC )) " +
            "  UPDATE ReduceLevel " +
            "  SET DataTableName = '{0}'" +
            "  WHERE MonitorConfigName = '{1}' and Resolution = {2}";

        public const string ComparisonTableCreateFormat = 
            @" CREATE TABLE [{0}] (" +
            "  [TimeStamp] [datetime] NOT NULL," +
            "  [Value] [float] NOT NULL," +
            "  [Number] [int] NOT NULL DEFAULT 1," +
            "  CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED " +
            "  ( [TimeStamp] ASC ))";


        public const string ReduceLevelSql = @"select * from ReduceLevel";
        public const string MonitorConfigSql = @"select * from MonitorConfig";
        public const string GetExistingTables = "SELECT name FROM sys.objects WHERE type in (N'U')";


        public const string Comma = ", ";

        public const string UpdateReduceLevelTableNameFormat =
            @"update ReduceLevel set DataTableName = '{0}' where MonitorConfigName = '{1}' and Resolution = {2}";

        public const string TableVerifyFormat =
            @"SELECT count(1) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U')";

        public const string DataParameters = "TimeStamp, Value, Number, IntervalSum, IntervalSumOfSquares";
        public const string DataValueFormats = "{0}, {1}, {2}, {3}, {4}";

        public const string ComparisonParameters = "TimeStamp, Value, Number";
        public const string ComparisonValueFormats = "{0}, {1}, {2}";

        public const string RangeTableCreateFormat =
            @" IF (SELECT count(1) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U')) = 0 " +
            "  BEGIN" +
            "  CREATE TABLE [{0}] (" +
            "  [TimeStamp] [datetime] NOT NULL," +
            "  [High] [float]," +
            "  [Mid] [float]," +
            "  [Low] [float], " +
            "  CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED " +
            "  ( [TimeStamp] ASC ))" +
            "  END ";

        public const string RangeParameters = "TimeStamp, High, Mid, Low";
        public const string RangeValueFormats = "{0}, {1}, {2}, {3}";

        public const string TimeStampParameterName = "TimeStamp";
        public const string TimeStampParameter = "@" + TimeStampParameterName;
        public const string ValueParameter = "@Value";
        public const string NumberParameter = "@Number";
        public const string IntervalSumParameter = "@IntervalSum";
        public const string IntervalSumOfSquaresParameter = "@IntervalSumOfSquares";
        public const string RangeHighParameterName = "High";
        public const string RangeHighParameter = "@" + RangeHighParameterName;
        public const string RangeMidParameterName = "Mid";
        public const string RangeMidParameter = "@" + RangeMidParameterName;
        public const string RangeLowParameterName = "Low";
        public const string RangeLowParameter = "@" + RangeLowParameterName;

        public static readonly string InsertFormat =
            String.Format("insert into [{{0}}] (" + DataParameters + ") values (" + DataValueFormats + ")",
                TimeStampParameter, ValueParameter, NumberParameter, IntervalSumParameter, IntervalSumOfSquaresParameter);

        public static readonly string InsertComparisonFormat =
            String.Format("insert into [{{0}}] (" + ComparisonParameters + ") values (" + ComparisonValueFormats + ")",
                TimeStampParameter, ValueParameter, NumberParameter);

        public static readonly string InsertRangeFormat =
            String.Format("insert into [{{0}}] (" + RangeParameters + ") values (" + RangeValueFormats + ")",
                TimeStampParameter, RangeHighParameter, RangeMidParameter, RangeLowParameter);

        public static readonly string UpdateFormat =
            String.Format("update [{{0}}] set Value = {1}, Number = {2}, IntervalSum = {3}, IntervalSumOfSquares = {4} where TimeStamp = {0}",
                TimeStampParameter, ValueParameter, NumberParameter, IntervalSumParameter, IntervalSumOfSquaresParameter);

        public const string LoadDataFormat = "select " + DataParameters + " from [{0}] {1} order by TimeStamp";
        public const string LoadRangeFormat = "select " + RangeParameters + " from [{0}] {1} order by TimeStamp";
        public const string LoadComparisonFormat = "select " + ComparisonParameters + " from [{0}] {1} order by TimeStamp";
        public const string LoadDataWhereFormat = "  where TimeStamp > '{0}'";
        public const string LoadDataWhereEqualFormat = "  where TimeStamp = '{0}'";
        public const string LoadDataWhereRange = @"  where TimeStamp > '{0}' and TimeStamp < '{1}'";

        public const string LoadLastUpdateFormat =
            "select " + DataParameters + " from [{0}] where TimeStamp = (select MAX(t2.TimeStamp) from [{0}] t2)";

        public const string LoadLastComparisonFormat =
            "select " + ComparisonParameters + " from [{0}] where TimeStamp = (select MAX(t2.TimeStamp) from [{0}] t2)";

        public const string LoadLastRangeFormat =
            "select " + RangeParameters + " from [{0}] where TimeStamp = (select MAX(t2.TimeStamp) from [{0}] t2)";

        public const string LoadMaxReducedFormat =
            "select MAX(t2.TimeStamp) from [{0}] t2";

        public const string DeleteDataFormat =
            "delete [{0}] from [{0}] t1 where t1.TimeStamp < '{1}'";

        public const string LoadDataTimeSeriesFormat =
            @"select TimeStamp, Value from [{0}] where TimeStamp > @TimeStamp order by TimeStamp";

        public const string LoadMonitorConfigFormat = "select * from MonitorConfig m where m.Name = '{0}'";

        public const string LoadReduceLevelsForMonitorFormat = "select * from ReduceLevel r where r.MonitorConfigName = '{0}'";

        public const string MonitorConfigInsert =
            @"IF (select count(1) from Monitorconfig where Name = @name) = '0'
            INSERT INTO zocdoc_logs.dbo.MonitorConfig
            (Name, MonitorReductionType) VALUES
            (@name, @monitorReductionType)";

        public const string ReduceLevelInsert =
            @"IF (select count(1) from ReduceLevel where MonitorConfigName = @name AND Resolution = @res) = '0'
            INSERT into zocdoc_logs.dbo.ReduceLevel
            (MonitorConfigName, Resolution, HistoryLength, AggregationClassName)
            VALUES (@name, @res, @historyLength, @aggregationClassName)";
    }
}

