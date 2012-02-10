using System;
using System.Configuration;
using System.IO;
using ZocMonLib;

namespace ZocMonLib
{
    public class RecordReduceStatusSourceProviderFile : RecordReduceStatusSourceProvider
    {
        private readonly ISystemLogger _logger;
        private readonly string _reducingStatusTxt = ConfigurationManager.AppSettings["isReducingFilePath"];

        public RecordReduceStatusSourceProviderFile(ISettings settings)
        {
            _logger = settings.LoggerProvider.CreateLogger(typeof(RecordReduceStatusSourceProviderFile));
        }

        public override string ReadValue()
        {
            var status = SeedValue();
            try
            {
                if (File.Exists(_reducingStatusTxt))
                {
                    using (TextReader reader = new StreamReader(_reducingStatusTxt))
                        status = reader.ReadLine();
                }
            }
            catch (Exception e)
            {
                _logger.Fatal("Something went wrong Reading from " + _reducingStatusTxt, e);
            }
            return status;
        }

        public override void WriteValue(string value)
        {
            try
            {
                if (!File.Exists(_reducingStatusTxt))
                    File.Create(_reducingStatusTxt).Dispose();

                using (TextWriter writer = new StreamWriter(_reducingStatusTxt))
                    writer.WriteLine(value);
            }
            catch (Exception e)
            { 
                _logger.Fatal("Something went wrong Reading from " + _reducingStatusTxt, e);
            }
        }
    }
}