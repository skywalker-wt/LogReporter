using LogReporter.Rule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LogReporter.Process.BackupProcess
{
    public class TextItemConstant
    {
        public const string TYPE_TEXT = "text";
        public const string TYPE_LIST = "list";

        public const string ATT_TYPE = "type";
        public const string ATT_RULE = "rule";
        public const string ATT_ORDERBY = "orderby";
        public const string ATT_DESC = "desc";

        public const string FIELD_NAME = "Name";
        public const string FIELD_COUNT = "Count";
    }

    public class ItemFactory
    {
        public static TextItem CreateTextItem(XmlNode config)
        {
            string type = config.Attributes[TextItemConstant.ATT_TYPE].Value;

            if (TextItemConstant.TYPE_LIST.Equals(type))
                return new ListItem(config);

            return new TextItem(config);
        }
    }

    public class TextItem
    {
        public string Content { get; set; }
        public string Type { get; set; }

        public TextItem(XmlNode config)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);
            tx.Formatting = Formatting.Indented;

            config.WriteContentTo(tx);

            Content = sw.ToString();
            Type = TextItemConstant.ATT_TYPE;
        }

        public virtual string GetContent(Dictionary<string, BaseRule> rules)
        {
            return Content;
        }
    }

    public class ListItem : TextItem
    {
        public string RuleName { get; set; }
        public string OrderBy { get; set; }
        public bool Desc { get; set; }

        public ListItem(XmlNode config)
            : base(config)
        {
            Type = TextItemConstant.TYPE_LIST;
            OrderBy = null;
            Desc = false;

            RuleName = config.Attributes[TextItemConstant.ATT_RULE].Value;

            foreach (XmlAttribute att in config.Attributes)
            {
                switch (att.Name)
                {
                    case TextItemConstant.ATT_ORDERBY:
                        OrderBy = att.Value;
                        break;
                    case TextItemConstant.ATT_DESC:
                        Desc = bool.Parse(att.Value);
                        break;
                }
            }
        }

        public override string GetContent(Dictionary<string, BaseRule> rules)
        {
            IEnumerable<LogItem> logItems = Sort(GetLogItem(rules[RuleName]));

            StringBuilder newLineBuilder = new StringBuilder();
            foreach (var item in logItems)
            {
                List<string> fields = item.FieldNames;
                string newLine = Content;
                foreach (var name in fields)
                {
                    newLine = newLine.Replace("$$" + name + "$$", item.TryGetField(name, "").ToString());
                }

                newLineBuilder.AppendLine(newLine);
            }

            return newLineBuilder.ToString();
        }

        private IEnumerable<LogItem> Sort(List<LogItem> items)
        {
            var outcome = from s in items select s;
            if (OrderBy != null)
                outcome = outcome.OrderBy(item => item.TryGetField(OrderBy, ""));
            if (Desc)
                outcome = outcome.Reverse();

            return outcome;
        }

        private List<LogItem> GetLogItem(BaseRule rule)
        {
            if (rule is GroupRule)
            {
                List<LogItem> outcome = new List<LogItem>();
                foreach (BaseRule subRule in (rule as GroupRule).Rules)
                {
                    LogItem item = new LogItem();
                    item.SetField(TextItemConstant.FIELD_NAME, subRule.RuleName);
                    item.SetField(TextItemConstant.FIELD_COUNT, subRule.Count);
                    outcome.Add(item);
                }

                return outcome;
            }

            return rule.LogItemsInRule;
        }
    }
}
