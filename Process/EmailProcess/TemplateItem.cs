using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogReporter.Rule;

namespace LogReporter.Process.EmailProcess
{
    public class TemplateItem : HtmlTextItem
    {
        internal const string ATTRIBUTE_RELATED_RULE_NAME = "rule";

        public string RelatedRuleName { get; set; }
        public LogItem LogItem { get; set; }

        public TemplateItem(XmlNode config) : base(config)
        {
            foreach (XmlAttribute att in config.Attributes)
            {
                switch (att.Name)
                {
                    case ATTRIBUTE_RELATED_RULE_NAME:
                        RelatedRuleName = att.Value;
                        break;
                }
            }
        }

        public override bool CreateContent(Dictionary<string, BaseRule> rules)
        {
            string innerText = HtmlText;

            LogItem currentItem = new LogItem();
            currentItem.Extends(BaseReportProcess.Content);
            if (RelatedRuleName != null)
            {
                currentItem.Extends(LogItem);

                BaseRule rule = rules[RelatedRuleName];
                if (rule.Count > 0)
                {
                    currentItem.Extends(rule.LogItemsInRule[0]);
                }
            }

            foreach (string fieldName in currentItem.FieldNames)
            {
                innerText = innerText.Replace(GetFieldMark(fieldName), currentItem.GetField(fieldName).ToString());
            }

            Content = innerText;
            return true;
        }

        private string GetFieldMark(string fieldName)
        {
            return "$$" + fieldName + "$$";
        }
    }
}
