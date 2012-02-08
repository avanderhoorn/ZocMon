using System;
using System.Collections.Generic;
using System.Data;
using ZocMonLib.Extensibility;
using ZocMonLib;

namespace ZocMonLib
{
    public class RecordCompare : IRecordCompare
    {
        private readonly ISystemLogger _logger;
        private readonly IStorageCommands _storageCommands;
        private readonly ISettings _settings;

        public RecordCompare(IStorageCommands storageCommands, ISettings settings)
        {
            _logger = settings.LoggerProvider.CreateLogger();
            _storageCommands = storageCommands;
            _settings = settings;
        }

        public void CalculateComparisons(string configName, MonitorInfo monitorInfo, ReduceLevel reduceLevel, IDbConnection conn)
        {
            try
            {
                if (monitorInfo.MonitorConfig.ComparisonCalculator != null)
                {
                    var comparisons = new SortedDictionary<long, MonitorRecordComparison<double>>();
                    monitorInfo.MonitorConfig.ComparisonCalculator.CalculateExpectedValues(configName, reduceLevel, comparisons, conn);

                    var tableName = Support.MakeReducedName(configName, reduceLevel.Resolution);
                    _storageCommands.Insert(tableName, comparisons.Values, conn, null);
                }
            }
            catch (Exception e)
            {
                _logger.Fatal("Exception swallowed: ", e);
                if (_settings.Debug)
                    throw;
            }
        }
    }
}