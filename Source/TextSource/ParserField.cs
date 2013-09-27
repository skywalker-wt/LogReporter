using HtmlAgilityPack;
using LogReporter.Operator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace LogReporter.Source.TextSource
{
    public class Field
    {
        internal const string ATT_NAME = "name";
        internal const string ATT_DEFAULT = "default";

        public string Name { get; set; }
        public string PatternStr { get; set; }
        public string Default { get; set; }

        private Regex pattern = null;
        public Regex RegexPattern
        {
            get
            {
                if (PatternStr == null) return null;

                if (pattern == null)
                    pattern = new Regex(PatternStr);
                return pattern;
            }
        }

        public string FieldValueWithDefault(string logStr)
        {
            if (RegexPattern == null) return logStr;

            Match match = RegexPattern.Match(logStr);
            if (!match.Success) return Default;

            return match.Value;
        }

        public string FieldValue(string logStr)
        {
            if (RegexPattern == null) return logStr;
            Match match = RegexPattern.Match(logStr);
            if (!match.Success) return null;

            return match.Value;
        }

        public Field(XmlNode config)
        {
            string strFieldPattern = config.InnerText.Trim();
            if (strFieldPattern.Length == 0) PatternStr = null;
            else PatternStr = RegexPatterns.GetPattern(strFieldPattern);

            Default = "";

            foreach (XmlAttribute attr in config.Attributes)
            {
                switch (attr.Name)
                {
                    case ATT_NAME:
                        Name = attr.Value;
                        break;
                    case ATT_DEFAULT:
                        Default = attr.Value;
                        break;
                }
            }
        }
    }

    public class XmlField : Field
    {
        internal const string ATT_PATH = "path";
        internal const string ATT_XML = "isXml";
        internal const string ATT_ATTR = "attr";

        public string Path { get; set; }
        public string Attribute { get; set; }
        public bool IsXml { get; set; }

        public XmlField(XmlNode config) :base(config)
        {
            Path = null;
            IsXml = true;
            Attribute = null;

            foreach (XmlAttribute attr in config.Attributes)
            {
                switch (attr.Name)
                {
                    case ATT_PATH:
                        Path = attr.Value;
                        break;
                    case ATT_ATTR:
                        Attribute = attr.Value;
                        break;
                    case ATT_XML:
                        IsXml = bool.Parse(attr.Value);
                        break;
                }
            }
        }

        public string FieldValueWithDefault(HtmlNode element)
        {
            if (Path != null)
                element = element.SelectSingleNode(Path);

            string content = "";

            if (Attribute != null)
            {
                content = element.Attributes[Attribute].Value;
            }
            else
            {
                if (IsXml)
                    content = element.InnerHtml;
                else
                    content = element.InnerText;
            }

            if (string.IsNullOrEmpty(content))
                return Default;

            return FieldValueWithDefault(content);
        }
    }
}
