using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LogReporter.Source.TextSource
{
    public class XmlParser : IParser
    {
        internal const string ATT_PATH = "path";
        internal const string TAG_FIELD = "field";

        public string Path { get; set; }
        private List<XmlField> Fields { set; get; }

        public XmlParser(XmlNode config)
        {
            Path = config.Attributes[ATT_PATH].Value;
            Fields = new List<XmlField>();

            foreach (XmlNode fieldNode in config.ChildNodes)
            {
                if (TAG_FIELD != fieldNode.Name) continue;

                Fields.Add(new XmlField(fieldNode));
            }
        }

        public IEnumerable<LogItem> GetLogItems(string content)
        {
            HtmlDocument sourceFile = new HtmlDocument();
            sourceFile.LoadHtml(content);
            HtmlNode rootNode = sourceFile.DocumentNode;

            HtmlNodeCollection elements = rootNode.SelectNodes(Path);
            if (elements != null)
            {
                foreach (HtmlNode node in elements)
                {
                    LogItem logItem = new LogItem();
                    foreach (XmlField field in Fields)
                    {
                        logItem.SetField(field.Name, field.FieldValueWithDefault(node));
                    }

                    yield return logItem;
                }
            }
        }

    }


}
