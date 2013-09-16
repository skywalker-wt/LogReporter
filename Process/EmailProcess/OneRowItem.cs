using LogReporter.Rule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LogReporter.Process.EmailProcess
{
    public class OneRowItem : GroupItem
    {
        public OneRowItem() : base()
        { }

        public OneRowItem(List<BaseEmailBodyItem> items) : base()
        { }

        public OneRowItem(XmlNode config)
            : base(config)
        { }

        public override bool CreateContent(Dictionary<string, BaseRule> rules)
        {
            StringBuilder contentBuilder = new StringBuilder();
            contentBuilder.Append("<table><tr>");

            foreach (BaseEmailBodyItem item in Items)
            {
                contentBuilder.Append("<td>");
                item.CreateContent(rules);
                contentBuilder.Append(item.Content);
                Attachments.AddRange(item.Attachments);
                contentBuilder.Append("</td>");
            }

            contentBuilder.Append("</tr></table>");
            Content = contentBuilder.ToString();
            return true;
        }
    }
}
