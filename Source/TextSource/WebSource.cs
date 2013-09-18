using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace LogReporter.Source.TextSource
{
    public class WebSourceGenerator : ISourceGenerator
    {
        internal const string ATTRIBUTE_SOURCE_TYPE = "type";
        internal const string SOURCE_TYPE = "WebSource";
        public BaseSource CreateSource(XmlNode config)
        {
            XmlAttribute typeName = config.Attributes[ATTRIBUTE_SOURCE_TYPE];
            if (typeName != null &&
                 !string.IsNullOrWhiteSpace(typeName.Value) &&
                 SOURCE_TYPE == typeName.Value)
                return new WebSource(config);

            return null;
        }

    }
    public class WebSource : BaseSource
    {
        internal const string ATT_URL = "url";
        internal const string TAG_LOG = "Parser";

        public string Url { set; get; }
        public IParser Parser { set; get; }

        public WebSource(XmlNode config)
            : base(config)
        {
            Url = null;
            Parser = null;

            foreach (XmlAttribute attr in config.Attributes)
            {
                switch (attr.Name)
                {
                    case ATT_URL:
                        Url = attr.Value;
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
            using (WebClient client = new WebClient())
            {
                string content = client.DownloadString(Url);
                return Parser.GetLogItems(content);
            }
        }
    }
}
