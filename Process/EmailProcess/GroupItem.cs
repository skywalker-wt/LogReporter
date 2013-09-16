using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogReporter.Rule;

namespace LogReporter.Process.EmailProcess
{
    public class GroupItem : BaseEmailBodyItem
    {
        public const string TAG_ITEM = "Item";

        public GroupItem() : base()
        {
            Items = new List<BaseEmailBodyItem>();
        }

        public GroupItem(List<BaseEmailBodyItem> items) : base()
        {
            Items = items;  
        }

        public GroupItem(XmlNode config) : base(config)
        {
            Items = new List<BaseEmailBodyItem>();
            foreach (XmlNode node in config.ChildNodes)
            {
                BaseEmailBodyItem item = BaseEmailBodyItem.Create(node);
                if (item != null)
                    Items.Add(item);
            }
        }

        public List<BaseEmailBodyItem> Items { get; set; } 

        public override bool CreateContent(Dictionary<string, BaseRule> rules)
        {
            StringBuilder contentBuilder = new StringBuilder();
            contentBuilder.Append("<div>");

            foreach (BaseEmailBodyItem item in Items)
            {
                item.CreateContent(rules);
                contentBuilder.Append(item.Content);
                Attachments.AddRange(item.Attachments);
            }

            contentBuilder.Append("</div>");
            Content = contentBuilder.ToString();
            return true;
        }
    }
}
