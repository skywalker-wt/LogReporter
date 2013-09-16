using LogReporter.Process.BackupProcess;
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
    public class SaveToTextFileProcess : BaseReportProcess
    {
        public const string TAG_RULE = "Rule";
        public const string TAG_CONTENT = "Content";
        public const string TAG_ITEM = "Item";
        public const string TAG_PATH = "Path";
        public const string TAG_FILE_NAME = "FileName";

        public string RuleName { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public List<TextItem> TextItems { get; set; }

        public SaveToTextFileProcess(XmlNode config)
        {
            TextItems = new List<TextItem>();
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
                    case TAG_CONTENT:
                        CreateContent(node);
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

        private void CreateContent(XmlNode config)
        {
            foreach (XmlNode node in config.ChildNodes)
            {
                if (TAG_ITEM.Equals(node.Name))
                {
                    TextItem newItem = ItemFactory.CreateTextItem(node);
                    if (newItem != null) TextItems.Add(newItem);
                }
            }
        }

        public override bool Process(Dictionary<string, BaseRule> rules)
        {
            StringBuilder newLineBuilder = new StringBuilder();

            foreach (TextItem item in TextItems)
            {
                newLineBuilder.AppendLine(item.GetContent(rules));
            }

            string fileName = Path + FileName + DateTime.Now.ToFileTime() + ".txt";

            TextWriter tw = new StreamWriter(fileName);
            tw.Write(newLineBuilder.ToString());
            tw.Close();

            return true;
        }
    }
}
