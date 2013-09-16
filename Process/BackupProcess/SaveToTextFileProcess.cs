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
        public const string TAG_PATH = "Path";
        public const string TAG_FILE_NAME = "FileName";
        public const string TAG_TEMPLETE = "Templete";
        public const string TAG_GROUP = "ShowGroup";

        public string RuleName { get; set; }
        public bool ShowGroup { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string Templete { get; set; }

        public SaveToTextFileProcess(XmlNode config)
        {
            ShowGroup = false;
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
                    case TAG_TEMPLETE:
                        CreateTemplete(node);
                        break;
                    case TAG_GROUP:
                        CreateShowGroup(node);
                        break;
                }
            }
        }

        private void CreateShowGroup(XmlNode node)
        {
            ShowGroup = bool.Parse(node.InnerText);
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

        private void CreateTemplete(XmlNode node)
        {
            Templete = node.InnerText;
        }

        public override bool Process(Dictionary<string, BaseRule> rules)
        {
            BaseRule inputRule = rules[RuleName];
            if (inputRule.Count < 1) return false;

            StringBuilder newLineBuilder = new StringBuilder();
            if (Templete == null)
            {
                if (ShowGroup)
                {
                    Templete = "$$Name$$, $$Count$$";
                }
                else
                {
                    LogItem first = inputRule.LogItemsInRule[0];
                    List<string> titles = new List<string>();

                    foreach (var title in first.FieldNames)
                    {
                        titles.Add(title);
                        newLineBuilder.Append("$$" + title + "$$,");
                    }

                    Templete = newLineBuilder.ToString();
                }
            }

            newLineBuilder = new StringBuilder();
            if (inputRule is GroupRule)
            {
                foreach (var rule in (inputRule as GroupRule).Rules)
                {
                    if (ShowGroup)
                    {
                        LogItem groupItem = new LogItem();
                        groupItem.SetField("Name", rule.RuleName);
                        groupItem.SetField("Count", rule.Count);

                        string content = Templete;
                        content = content.Replace("$$Name$$", groupItem.TryGetField("Name", "").ToString());
                        content = content.Replace("$$Count$$", groupItem.TryGetField("Count", "").ToString());
                        newLineBuilder.AppendLine(content);
                    }
                    else
                    {
                        foreach (var item in rule.LogItemsInRule)
                        {
                            string content = Templete;
                            foreach (string title in item.FieldNames)
                            {
                                content = content.Replace("$$" + title + "$$", item.TryGetField(title, "").ToString());
                            }

                            newLineBuilder.AppendLine(content);
                        }
                    }
                }
            }
            else
            {
                foreach (var item in inputRule.LogItemsInRule)
                {
                    string content = Templete;
                    foreach (string title in item.FieldNames)
                    {
                        content = content.Replace("$$" + title + "$$", item.TryGetField(title, "").ToString());
                    }

                    newLineBuilder.AppendLine(content);
                }
            }

            string fileName = Path + FileName + DateTime.Now.ToFileTime() + ".txt";

            TextWriter tw = new StreamWriter(fileName);
            tw.Write(newLineBuilder.ToString());
            tw.Close();

            return true;
        }
    }
}
