using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;

namespace LogReporter.Source.TextSource
{
    public class JsonParser : IParser
    {
        internal const string TAG_FIELD = "field";

        private List<Field> Fields { set; get; }

        public JsonParser(XmlNode config)
        {
            Fields = new List<Field>();

            foreach (XmlNode fieldNode in config.ChildNodes)
            {
                if (TAG_FIELD != fieldNode.Name) continue;

                Fields.Add(new Field(fieldNode));
            }
        }
        

        public IEnumerable<LogItem> GetLogItems(string content)
        {
            var jss = new JavaScriptSerializer();
            object obj = jss.Deserialize<object>(content);
            if (obj == null) return new List<LogItem>();
            return Dfs(obj);
        }

        IEnumerable<LogItem> Dfs(object o)
        {
            if (o is Dictionary<string, object>)
            {
                LogItem item = GetItemFromJson(o as Dictionary<string, object>);
                if (item != null)
                    yield return item;

                foreach (var child in (o as Dictionary<string, object>).Values)
                {
                    IEnumerable<LogItem> subItems = Dfs(child);
                    foreach (var subItem in subItems)
                        yield return subItem;
                }
            }
            else if (o is object[])
            {
                foreach (var child in (o as object[]))
                {
                    IEnumerable<LogItem> subItems = Dfs(child);
                    foreach (var subItem in subItems)
                        yield return subItem;
                }
            }
        }

        LogItem GetItemFromJson(Dictionary<string, object> item)
        {
            LogItem outcome = new LogItem();
            foreach (Field field in Fields)
            {
                if (!item.ContainsKey(field.Name)) return null;

                object value = item[field.Name];
                if (value == null) return null;
                if (value is object[]) return null;
                if (value is Dictionary<string, object>) return null;

                string valueStr = field.FieldValue(value.ToString());
                if (valueStr == null) return null;

                outcome.SetField(field.Name, valueStr);
            }

            return outcome;
        }
    }
}
