using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogReporter.Rule;
using LogReporter.Process.EmailProcess;

namespace LogReporter.Process
{
    public abstract class BaseReportProcess
    {
        public const string ATT_TYPE = "type";
        public const string ATT_PRIORITY = "priority";
        public const string ATT_NAME = "name";
        public const string TYPE_SEND_EMAIL = "SendEmail";
        public const string TYPE_SAVE_TO_CSV = "SaveToCsv";
        public const string TYPE_SAVE_TO_TXT = "SaveToTxt";

        public static BaseReportProcess Create(XmlNode config)
        {
            BaseReportProcess outcome = null;

            XmlAttribute typeAtt = config.Attributes[ATT_TYPE];
            if (typeAtt == null || string.IsNullOrWhiteSpace(typeAtt.Value))
                throw new ArgumentException("The Process attribute 'type' is needed");
            string type = typeAtt.Value;

            switch (type)
            { 
                case TYPE_SEND_EMAIL:
                    outcome = new SendEmailProcess(config);
                    break;
                case TYPE_SAVE_TO_CSV:
                    outcome = new SaveToCSVProcess(config);
                    break;
                case TYPE_SAVE_TO_TXT:
                    outcome = new SaveToTextFileProcess(config);
                    break;
                default:
                    Type sourceType = Type.GetType(typeAtt.Value);
                    outcome = Activator.CreateInstance(sourceType, new object[] {config}) as BaseReportProcess;
                    break;
            }
            if (outcome != null) {
                int pri = 0;
                if (config.Attributes[ATT_PRIORITY] != null) { 
                    int.TryParse(config.Attributes[ATT_PRIORITY].Value, out pri);
                }
                outcome.Priority = pri;
                if (config.Attributes[ATT_NAME] != null)
                    outcome.Name = config.Attributes[ATT_NAME].Value;
            }
            return outcome;
        }

        public static LogItem Content { get; set; }
        public int Priority { get; set; }
        public string Name { get; set; }
        public abstract bool Process(Dictionary<string, BaseRule> rules);
    }
}
