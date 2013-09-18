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

namespace LogReporter.Source.TextSource
{
    public class TextFileSourceGenerator : ISourceGenerator
    {
        internal const string ATTRIBUTE_SOURCE_TYPE = "type";
        internal const string SOURCE_TYPE = "FileSource";
        public BaseSource CreateSource(XmlNode config)
        {
            XmlAttribute typeName = config.Attributes[ATTRIBUTE_SOURCE_TYPE];
            if ( typeName != null &&
                 !string.IsNullOrWhiteSpace(typeName.Value) &&
                 SOURCE_TYPE == typeName.Value)
                return new FileSource(config);

            return null;
        }
    }

    public class FileSource : BaseSource
    {
        internal const string ATT_SOURCE = "source";
        internal const string TAG_LOG = "Parser";

        public string SourceFile { set; get; }
        public IParser Parser { set; get; }

        public FileSource(XmlNode config)
            : base(config)
        {
            SourceFile = null;
            Parser = null;

            foreach (XmlAttribute attr in config.Attributes)
            {
                switch (attr.Name)
                {
                    case ATT_SOURCE:
                        SourceFile = attr.Value;
                        break;
                }
            }

            foreach (XmlNode node in config.ChildNodes)
            {
                if (TAG_LOG != node.Name) continue;

                Parser = ParserCreater.Create(node);
                break;
            }
        }

        public override IEnumerable<LogItem> GetAllLogItems()
        {
            using (StreamReader sr = new StreamReader(SourceFile))
            {
                string allContent = sr.ReadToEnd();
                return Parser.GetLogItems(allContent);
            }
        }
    }
}
