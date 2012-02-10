using System.Configuration;
using System.Globalization;
using System.IO;
using System.Xml; 

namespace ZocMonLib
{
    /// <summary>
    /// NOTE: Not currently been setup, when i get a chance I'll implement.
    /// </summary>
    public class ExternalConfiguration : ConfigurationSection, IExternalConfiguration
    { 
        [ConfigurationProperty("enabled", DefaultValue = "false", IsRequired = false)]
        public bool Enabled
        {
            set { this["enabled"] = value; }
            get
            {
                bool result; //false which matches the default above
                bool.TryParse(this["enabled"].ToString(), out result);
                return result;
            }
        }

        [ConfigurationProperty("loggingEnabled", DefaultValue = "false", IsRequired = false)]
        public bool LoggingEnabled
        {
            set { this["loggingEnabled"] = value; }
            get
            {
                bool result; //false which matches the default above
                bool.TryParse(this["loggingEnabled"].ToString(), out result);
                return result;
            }
        }

        public override string ToString()
        {
            var result = "";
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                using (var xmlWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented, Indentation = 4, IndentChar = ' ' })
                    this.SerializeToXmlElement(xmlWriter, "zocmon");
                result = stringWriter.ToString();
            }
            return result;
        }
    }
}
