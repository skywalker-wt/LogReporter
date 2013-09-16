using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogReporter.Rule;

namespace LogReporter.Process.EmailProcess
{
    public class HtmlDocItem : GroupItem
    {
        public HtmlDocItem(List<BaseEmailBodyItem> items) : base(items)
        {  }

        public HtmlDocItem(XmlNode config) : base(config)
        {

        }

        public override bool CreateContent(Dictionary<string, BaseRule> rules)
        {
            StringBuilder contentBuilder = new StringBuilder();

            var sortedItems = Items.ToList();
            sortedItems.Sort((x,y) => x.Priority.CompareTo(y.Priority));

            foreach (BaseEmailBodyItem item in sortedItems)
            {
                item.CreateContent(rules);
            }

            foreach (BaseEmailBodyItem item in Items)
            {
                contentBuilder.Append(item.Content);
                Attachments.AddRange(item.Attachments);
            }
            Content = contentBuilder.ToString();
            return true;
        }
    }
}
