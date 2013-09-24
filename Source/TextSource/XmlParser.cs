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

        public string Path { get; set; }
        private List<XmlField> Fields { set; get; }

        public XmlParser(XmlNode config)
        {
            Fields = new List<XmlField>();

            foreach (XmlNode fieldNode in config.ChildNodes)
            {
                
            }
        }

        public IEnumerable<LogItem> GetLogItems(string content)
        {
            XmlDocument sourceFile = new XmlDocument();
            sourceFile.LoadXml(content);

            

            return null;
        }

    }


}
