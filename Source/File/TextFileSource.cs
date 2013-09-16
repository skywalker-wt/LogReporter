using LogReporter.Operator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace LogReporter.Source
{
    public class TextFileSourceGenerator : ISourceGenerator
    {
        internal const string ATTRIBUTE_SOURCE_TYPE = "type";
        internal const string SOURCE_TYPE = "TextFileSourceSource";
        public BaseSource CreateSource(XmlNode config)
        {
            XmlAttribute typeName = config.Attributes[ATTRIBUTE_SOURCE_TYPE];
            if ( typeName != null &&
                 !string.IsNullOrWhiteSpace(typeName.Value) &&
                 SOURCE_TYPE == typeName.Value)
                return new TextFileSource(config);

            return null;
        }
    }

    public class TextFileSource : BaseSource
    {
        internal const string ATT_SOURCE = "source";
        internal const string TAG_LOG = "log";

        public string SourceFile { set; get; }
        public LogItemPattern logPattern { set; get; }

        public TextFileSource(XmlNode config)
            : base(config)
        {
            SourceFile = null;
            logPattern = null;

            foreach (XmlAttribute attr in config.Attributes)
            {
                switch (attr.Name)
                {
                    case ATT_SOURCE:
                        SourceFile = attr.Value;
                        break;
                }
            }

            foreach (XmlNode patternNode in config.ChildNodes)
            {
                if (TAG_LOG != patternNode.Name) continue;

                logPattern = new LogItemPattern(patternNode);
                break;
            }
        }

        public override IEnumerable<LogItem> GetAllLogItems()
        {
            using (StreamReader sr = new StreamReader(SourceFile))
            {
                string allContent = sr.ReadToEnd();
                return logPattern.GetLogItems(allContent);
            }
        }
    }

    public class LogItemPattern
    {
        internal const string ATT_REGEX = "regex";
        internal const string TAG_FIELD = "field";

        private List<Field> Fields { set; get; }
        private Regex RegexPattern { set; get; }

        public LogItemPattern(XmlNode config)
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
                        string value = field.FieldValue(logStr.ToString());
                        logItem.SetField(field.Name, value);
                    }

                    yield return logItem;
                }
            }
        }

        private class Field
        {
            internal const string ATT_NAME = "name";
            internal const string ATT_PATTERN = "pattern";
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

            public string FieldValue(string logStr)
            {
                Match match = RegexPattern.Match(logStr);
                if (!match.Success) return Default;

                return match.Value;
            }

            public Field(XmlNode config)
            {
                PatternStr = string.Empty;
                Default = null;

                foreach (XmlAttribute attr in config.Attributes)
                {
                    switch (attr.Name)
                    {
                        case ATT_NAME:
                            Name = attr.Value;
                            break;
                        case ATT_PATTERN:
                            PatternStr = RegexPatterns.GetPattern(attr.Value);
                            break;
                        case ATT_DEFAULT:
                            Default = attr.Value;
                            break;
                    }
                }
            }
        }

    }
}
