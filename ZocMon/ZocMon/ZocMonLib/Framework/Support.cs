using System;
using System.Data;
using System.Text;

namespace ZocMonLib
{
    public class Support
    {
        public const string DataTableSuffix = @"Data";
        public const string ComparisonTableSuffix = @"Comparison";

        /// <summary>
        /// Validates whether a proposed name is ok to be used and parses out invalid 
        /// characters.
        /// </summary>
        /// <param name="configName">Proposed name of an item to be stored</param>
        /// <returns>Valid config name</returns>
        public static string ValidateConfigName(string configName)
        {
            configName.ThrowIfNull("configName");

            if (configName.Length > Constant.MaxConfigNameLength)
                throw new DataException("Invalid montior configuration name: \"" + configName + "\"; length " + configName.Length + " > " + Constant.MaxConfigNameLength);

            var charArray = configName.ToCharArray();
            var result = new StringBuilder();
            for (var i = 0; i < configName.Length; ++i)
                result.Append(char.IsLetterOrDigit(charArray[i]) ? charArray[i] : '_');

            return result.ToString();
        }

        /// <summary>
        /// Define data table names based on the monitor configuration and the reduction resolution.
        /// </summary>
        /// <param name="configName"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static string MakeReducedName(string configName, long resolution)
        {
            return configName + ResolutionToName(resolution) + DataTableSuffix;
        }

        public static string MakeComparisonName(string configName, long resolution)
        {
            return configName + ResolutionToName(resolution) + ComparisonTableSuffix;
        }

        /// <summary>
        /// Round the input DateTime to the largest value smaller than the given DateTime, that divides evenly by the resolution.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static DateTime RoundToResolution(DateTime input, long resolution)
        {
            return new DateTime((input.Ticks / (Constant.TicksInMillisecond * resolution)) * Constant.TicksInMillisecond * resolution);
        }


        /// <summary>
        /// Try to derive a name from the given resolution (for use in table names and elsewhere).
        /// </summary>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static string ResolutionToName(long resolution)
        {
            string ret;
            switch (resolution / 1000)
            {
                case 0:
                    ret = "Primary";
                    break;
                case 1:
                    ret = "Secondly";
                    break;
                case 60:
                    ret = "Minutely";
                    break;
                case 5 * 60:
                    ret = "FiveMinutely";
                    break;
                case 10 * 60:
                    ret = "TenMinutely";
                    break;
                case 20 * 60:
                    ret = "HalfHourly";
                    break;
                case 60 * 60:
                    ret = "Hourly";
                    break;
                case 24 * 60 * 60:
                    ret = "Daily";
                    break;
                case 7 * 24 * 60 * 60:
                    ret = "Weekly";
                    break;
                case 28 * 24 * 60 * 60:
                    ret = "Lunarly";
                    break;
                default:
                    ret = resolution.ToString();
                    break;
            }
            return ret;
        }
    }
}