using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogReporter;
using LogReporter.Rule;
using LogReporter.Operator;

namespace LogReporter.Process.EmailProcess
{
	public class TableColumnItem : GroupItem
	{
		internal const string ATTRIBUTE_TITLE = "title";
        internal const string ATTRIBUTE_FIELD = "field";
        internal const string ATTRIBUTE_STYLE = "filteredStyle";
        internal const string ATTRIBUTE_FILTER = "filter";
        internal const string TAG_RULE = "Rule";
		internal const string PARENT_RULE = "parentRule";

		#region Constructors
		public TableColumnItem(string title, string fieldName, List<BaseRule> rules, string filter = null, string style = null)
		{
			Title = title;
			FieldName = fieldName;
			Rules = rules;
			Filter = filter;
            Style = style;
		}

		public TableColumnItem(XmlNode config)
			: base(config)
		{
			Attrs = new List<KeyValuePair<string, string>>();
			Rules = new List<BaseRule>();

			foreach (XmlAttribute att in config.Attributes)
			{
				switch (att.Name)
				{
					case ATTRIBUTE_TITLE:
						Title = att.Value;
						break;
                    case ATTRIBUTE_FIELD:
                        FieldName = att.Value;
                        break;
                    case ATTRIBUTE_STYLE:
                        Style = att.Value;
                        break;
                    case ATTRIBUTE_FILTER:
                        Filter = att.Value;
                        break;
					default:
						Attrs.Add(new KeyValuePair<string, string>(att.Name, att.Value));
						break;
				}
			}

			foreach (XmlNode node in config.ChildNodes)
			{
				switch (node.Name)
				{
					case TAG_RULE:
						Rules.Add(BaseRule.Create(node));
						break;
				}
			}
		}
		#endregion

		#region Fields
		public string Title { get; private set; }
		public string FieldName { get; private set; }
		public string Filter { get; private set; }
		public string Style { get; private set; }
		public List<BaseRule> Rules { get; private set; }
		public List<KeyValuePair<string, string>> Attrs { get; private set; }
		#endregion

		public string GetColumnContent(BaseRule parentRule, LogItem logItem, List<string> activeFilters)
		{
			StringBuilder contentBuilder = new StringBuilder();
			contentBuilder.Append("<td");
            var customedAttrs = Attrs.ToList();
            if (!string.IsNullOrWhiteSpace(Filter) && !string.IsNullOrWhiteSpace(Style) && activeFilters.Contains(Filter)) {
                string value = Style;
                for (int i = 0; i < customedAttrs.Count; i++)
                {
                    var attr = customedAttrs[i];
                    if (attr.Key == "class")
                        value = value + " " + attr.Value;
                    customedAttrs.Remove(attr);
                }
                customedAttrs.Add(new KeyValuePair<string, string>("class", value));
            }
            foreach (KeyValuePair<string, string> attr in customedAttrs)
			{
				contentBuilder.Append(" " + attr.Key + "='" + attr.Value + "'");
			}
			contentBuilder.Append(">");

			Dictionary<string, BaseRule> rules = new Dictionary<string, BaseRule>();
			if (parentRule != null)
			{
				rules.Add(PARENT_RULE, parentRule);

				foreach (BaseRule rule in Rules)
				{
					ClearRule(rule);
					foreach (LogItem item in parentRule.LogItemsInRule)
					{
                        rule.MatchLogItem(item, OperatorManager.GetInstance().Operators);
					}

					rules.Add(rule.RuleName, rule);
				}
			}

			if (Items != null && Items.Count > 0)
			{
				foreach (BaseEmailBodyItem item in Items)
				{
					if (item is TemplateItem)
					{
						((TemplateItem)item).LogItem = logItem;
					}
					item.CreateContent(rules);
                    Attachments.AddRange(item.Attachments);
					contentBuilder.Append(item.Content);
				}
			}
			else
			{
                if (logItem.HasField(FieldName))
                {
                    object value = logItem.GetField(FieldName);
                    if (value != null)
                    {
                        contentBuilder.Append(value.ToString());
                    }
                }
			}

			contentBuilder.Append("</td>");
			return contentBuilder.ToString();
		}

		private void ClearRule(BaseRule rule)
		{
			rule.LogItemsInRule.Clear();
            rule.Count = 0;

			if (rule is GroupRule)
			{
				GroupRule groupRule = (GroupRule)rule;
				foreach (BaseRule childRule in groupRule.Rules)
					ClearRule(childRule);
			}
		}
	}
}
