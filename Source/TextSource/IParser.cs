using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LogReporter.Source.TextSource
{
    public interface IParser
    {
        IEnumerable<LogItem> GetLogItems(string content);
    }

    public class ParserCreater
    {
        public const string TYPE_JSON = "json";
        public const string TYPE_XML = "xml";
        public const string TYPE_TXT = "txt";

        public const string ATT_TYPE = "type";

        public static IParser Create(XmlNode config)
        {
            string type = config.Attributes[ATT_TYPE].Value;

            switch (type)
            {
                case TYPE_JSON:
                    return new JsonParser(config);
                case TYPE_TXT:
                    return new TextParser(config);
            }

            return null;
        }
    }
}
