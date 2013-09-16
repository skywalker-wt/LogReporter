using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogReporter;
using LogReporter.Rule;

namespace LogReporter.Process.EmailProcess
{
    public class ClassifyTableItem : TableItem
    {
        internal const string ATTRIBUTE_SHOW_EMPTY_ROW = "showEmptyRow";
        internal const string COL_NAME = "name";
        internal const string COL_COUNT = "count";
        internal const string COL_DESC = "description";
        internal const string COL_PERCENT = "percent";

        public ClassifyTableItem(XmlNode config)
            : base(config)
        {
            
        }

        public override bool CreateContent(Dictionary<string, BaseRule> rules)
        {
            if (!rules.ContainsKey(RelatedRuleName))
            {
                Console.WriteLine("Specified rule {0} not found.", RelatedRuleName);
                return false;
            }
            BaseRule relatedRule = rules[RelatedRuleName];
            if (!(relatedRule is GroupRule)) {
                Console.WriteLine("ClassifyTableItem's rule should be a GroupRule.");
                return false;
            }

            GroupRule groupRule = (GroupRule)relatedRule;

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

                contentBuilder.AppendLine("<tr>");
                foreach (TableColumnItem column in Columns)
                {
                    contentBuilder.Append("<th>");
                    contentBuilder.AppendLine(column.Title);
                    contentBuilder.AppendLine("</th>");
                }
                contentBuilder.AppendLine("</tr>");
            }

            int totalCount = groupRule.Rules.Sum(r => r.Count);

            foreach (BaseRule rule in groupRule.Rules)
            {
                if (rule.Count == 0)
                {
                    bool showEmpty = false;
                    foreach (KeyValuePair<string, string> pair in Attrs)
                    {
                        if (ATTRIBUTE_SHOW_EMPTY_ROW.Equals(pair.Key))
                            showEmpty = bool.Parse(pair.Value);
                    }

                    if (!showEmpty) continue;
                }

                LogItem item = new LogItem();
                item.SetField(COL_NAME, rule.RuleName);
                item.SetField(COL_COUNT, rule.Count);
                item.SetField(COL_DESC, rule.Description);
                item.SetField(COL_PERCENT, String.Format( "{0:N3}% ",(double)rule.Count / (double)totalCount * 100));

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
                    contentBuilder.AppendLine(column.GetColumnContent(rule, item, activeFilters));
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
