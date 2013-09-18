using LogReporter.Operator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace LogReporter.Source.TextSource
{
    public class TextParser : IParser
    {
        internal const string ATT_REGEX = "regex";
        internal const string TAG_FIELD = "field";

        private List<Field> Fields { set; get; }
        private Regex RegexPattern { set; get; }

        public TextParser(XmlNode config)
        {
            Fields = new List<Field>();
            RegexPattern = null;

            foreach (XmlAttribute attr in config.Attributes)
            {
                switch (attr.Name)
                {
                    case ATT_REGEX:
                        RegexPattern = new Regex(attr.Value);
                        break;
                }
            }

            foreach (XmlNode fieldNode in config.ChildNodes)
            {
                if (TAG_FIELD != fieldNode.Name) continue;

                Fields.Add(new Field(fieldNode));
            }
        }

        public IEnumerable<LogItem> GetLogItems(string content)
        {
            ICollection matchs = content.Split('\n');
            if (RegexPattern != null)
                matchs = RegexPattern.Matches(content);

            foreach (var logStr in matchs)
            {
                {
                    LogItem logItem = new LogItem();

                    foreach (var field in Fields)
                    {
                        string value = field.FieldValueWithDefault(logStr.ToString());
                        logItem.SetField(field.Name, value);
                    }

                    yield return logItem;
                }
            }
        }

    }
}
