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
                if (pattern == null)
                    pattern = new Regex(PatternStr);

                return pattern;
            }
        }

        public string FieldValueWithDefault(string logStr)
        {
            Match match = RegexPattern.Match(logStr);
            if (!match.Success) return Default;

            return match.Value;
        }

        public string FieldValue(string logStr)
        {
            Match match = RegexPattern.Match(logStr);
            if (!match.Success) return null;

            return match.Value;
        }

        public Field(XmlNode config)
        {
            string strFieldPattern = config.InnerText.Trim();
            if (strFieldPattern.Length == 0) PatternStr = ".*";
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
        internal const string ATT_ATT_NAME = "attr";
        internal const string ATT_XML = "isXml";

        public string Path { get; set; }
        public string AttrName { get; set; }
        public bool IsXml { get; set; }

        public XmlField(XmlNode config) :base(config)
        {
            AttrName = null;
            Path = null;
            IsXml = false;

            foreach (XmlAttribute attr in config.Attributes)
            {
                switch (attr.Name)
                {
                    case ATT_PATH:
                        Path = attr.Value;
                        break;
                    case ATT_ATT_NAME:
                        AttrName = attr.Value;
                        break;
                    case ATT_XML:
                        IsXml = bool.Parse(attr.Value);
                        break;
                }
            }
        }

        public string FieldValueWithDefault(XmlNode element)
        {
            if (Path != null)
                element = element.SelectSingleNode(Path);

            string content = "";
            if (AttrName == null)
            {
                if (IsXml) content = element.InnerXml;
                else content = element.InnerText;

                if (content == null || content.Length == 0)
                    content = Default;
            }
            //else
            //{
            //    if (element.Attributes)
            //}

            return "";
        }
    }
}
