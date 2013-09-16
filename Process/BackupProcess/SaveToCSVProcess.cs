using LogReporter.Rule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LogReporter.Process
{
    public class SaveToCSVProcess : BaseReportProcess
    {
        public const string TAG_RULE = "Rule";
        public const string TAG_PATH = "Path";
        public const string TAG_FILE_NAME = "FileName";

        public string RuleName { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }

        public SaveToCSVProcess(XmlNode config)
        {
            foreach (XmlNode node in config.ChildNodes)
            {
                switch (node.Name)
                {
                    case TAG_RULE:
                        CreateRuleName(node);
                        break;
                    case TAG_PATH:
                        CreatePath(node);
                        break;
                    case TAG_FILE_NAME:
                        CreateFileName(node);
                        break;
                }
            }
        }

        private void CreateRuleName(XmlNode node)
        {
            RuleName = node.InnerText;
        }

        private void CreatePath(XmlNode node)
        {
            Path = node.InnerText;
        }

        private void CreateFileName(XmlNode node)
        {
            FileName = node.InnerText;
        }

        public override bool Process(Dictionary<string, BaseRule> rules)
        {
            BaseRule inputRule = rules[RuleName];

            if (inputRule.Count < 1) return false;

            LogItem first = inputRule.LogItemsInRule[0];
            List<string> titles = new List<string>();

            StringBuilder newLineBuilder = new StringBuilder();
            foreach (var title in first.FieldNames)
            {
                titles.Add(title);
                newLineBuilder.Append(title + ",");
            }
            newLineBuilder.AppendLine();

            foreach (var item in inputRule.LogItemsInRule)
            {
                foreach (string title in titles)
                {
                    newLineBuilder.Append( "\"" + item.TryGetField(title, "") + "\",");
                }

                newLineBuilder.AppendLine();
            }

            string fileName = Path + FileName + DateTime.Now.ToFileTime() + ".csv";

            TextWriter tw = new StreamWriter(fileName);
            tw.Write(newLineBuilder.ToString());
            tw.Close();

            return true;
        }
    }
}
