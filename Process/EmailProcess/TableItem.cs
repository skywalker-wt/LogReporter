using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogReporter;
using LogReporter.Rule;
using LogReporter.Operator;
using LogReporter.Filter;


namespace LogReporter.Process.EmailProcess
{
	public class TableItem : BaseEmailBodyItem
	{
		internal const string ATTRIBUTE_RELATED_RULE_NAME = "rule";
		internal const string ATTRIBUTE_SIMPLE = "simple";
		internal const string ATTRIBUTE_MAX_LINES = "maxline";
		internal const string TAG_HEAD_TEXT = "HeadText";
		internal const string TAG_COLUMN = "Column";
		internal const string TAG_STYLE_FILTER = "StyleFilter";
		internal const string ATTRIBUTE_FILTER_STYLE = "style";
        internal const string ATTRIBUTE_FILTER_NAME = "name";

		public string RelatedRuleName { get; private set; }
		public string HeadText { get; private set; }
		public List<TableColumnItem> Columns { get; private set; }
		public List<KeyValuePair<string, string>> Attrs { get; private set; }
		public Dictionary<string, IRowFilter> StyleFilters { get; private set; }
        public Dictionary<string, string> Styles { get; private set; }
		public bool IsSimple { get; private set; }
		public int MaxLines { get; private set; }

		public TableItem(XmlNode config)
			: base(config)
		{
			IsSimple = false;
			MaxLines = int.MaxValue;
			Attrs = new List<KeyValuePair<string, string>>();
			foreach (XmlAttribute att in config.Attributes)
			{
				switch (att.Name)
				{
					case ATTRIBUTE_RELATED_RULE_NAME:
						RelatedRuleName = att.Value;
						break;
					case ATTRIBUTE_SIMPLE:
						IsSimple = bool.Parse(att.Value);
						break;
					case ATTRIBUTE_MAX_LINES:
						MaxLines = int.Parse(att.Value);
						break;
					default:
						Attrs.Add(new KeyValuePair<string, string>(att.Name, att.Value));
						break;
				}
			}

			Columns = new List<TableColumnItem>();
			StyleFilters = new Dictionary<string, IRowFilter>();
            Styles = new Dictionary<string, string>();
			foreach (XmlNode node in config.ChildNodes)
			{
				switch (node.Name)
				{
					case TAG_HEAD_TEXT:
						CreateHeadText(node);
						break;
					case TAG_COLUMN:
						CreateColumn(node);
						break;
					case TAG_STYLE_FILTER:
						CreateStyleFilter(node);
						break;
				}
			}

		}

		private void CreateHeadText(XmlNode node)
		{
			HeadText = node.InnerText;
		}

		private void CreateColumn(XmlNode node)
		{
			TableColumnItem column = new TableColumnItem(node);
			Columns.Add(column);
		}

		private void CreateStyleFilter(XmlNode node)
		{
			string style = null;
            string name = null;
			foreach (XmlAttribute attr in node.Attributes)
				switch (attr.Name)
				{
					case ATTRIBUTE_FILTER_STYLE:
						style = attr.Value;
						break;
                    case ATTRIBUTE_FILTER_NAME:
                        name = attr.Value;
                        break;
				}
			if (string.IsNullOrWhiteSpace(style) || string.IsNullOrWhiteSpace(name))
				return;
			IRowFilter filter = BaseRowFilter.Create(node);
			if (filter != null)
			{
				StyleFilters[name] = filter;
                Styles[name] = style;
			}
		}

		public override bool CreateContent(Dictionary<string, BaseRule> rules)
		{
			BaseRule relatedRule = rules[RelatedRuleName];
			StringBuilder contentBuilder = new StringBuilder();

			contentBuilder.AppendLine("<br/>");
			contentBuilder.AppendLine("<table");

			foreach (KeyValuePair<string, string> attr in Attrs)
			{
				contentBuilder.Append(" " + attr.Key + "='" + attr.Value + "'");
			}
			contentBuilder.Append(">");
			contentBuilder.AppendLine();

			if (!IsSimple)
			{
				contentBuilder.AppendLine("<caption><h2>" + HeadText + "</h2></caption>");

				contentBuilder.AppendLine("<thead>");
				foreach (TableColumnItem column in Columns)
				{
					contentBuilder.Append("<th>");

					contentBuilder.AppendLine(column.Title);
					contentBuilder.AppendLine("</th>");
				}
				contentBuilder.AppendLine("</thead>");
			}

			int lineNum = 0;
			foreach (LogItem item in rules[RelatedRuleName].LogItemsInRule)
			{
				++lineNum;
				if (lineNum > MaxLines)
					break;

				if (Columns.Count == 0)
				{
					foreach (string fieldName in item.FieldNames)
					{
						Columns.Add(new TableColumnItem(fieldName, fieldName, new List<BaseRule>()));
					}
				}

				var activeFilters = (from pair in StyleFilters
						 where (pair.Value != null && pair.Value.Match(item, relatedRule.LogItemsInRule))
						 select pair.Key).ToList();

                var activeStyles = activeFilters.Select(x => Styles[x]).ToList();

                if (activeStyles.Count > 0)
					contentBuilder.AppendLine(string.Format("<tr class='{0}'>", string.Join(" ", activeStyles)));
				else
					contentBuilder.AppendLine("<tr>");
				foreach (TableColumnItem column in Columns)
				{
					contentBuilder.AppendLine(column.GetColumnContent(null, item, activeFilters));
                    Attachments.AddRange(column.Attachments);
				}
				contentBuilder.AppendLine("</tr>");
			}

			contentBuilder.AppendLine("</table>");
			Content = contentBuilder.ToString();
			return true;
		}
	}
}
