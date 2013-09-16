using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogReporter.Operator;

namespace LogReporter.Rule
{
    public class AutoGroupRule : GroupRule
    {
        internal const string ATTR_GROUP_FIELD_NAME = "fieldName";

        public string GroupFieldName { get; set; }

        public AutoGroupRule(string ruleName, List<RuleFilter> ruleFilters)
            : base(ruleName, null, ruleFilters)
        {
            Rules = new List<BaseRule>();
        }

        public AutoGroupRule(XmlNode config)
            : base(config)
        {
            XmlAttribute groupFieldAtt = config.Attributes[ATTR_GROUP_FIELD_NAME];
            if (groupFieldAtt == null || string.IsNullOrWhiteSpace(groupFieldAtt.Value))
                throw new ArgumentException("The AutoGroupRule attribute 'fieldName' is needed");

            GroupFieldName = groupFieldAtt.Value;
        }

        public override bool MatchLogItem(LogItem logItem, Dictionary<string, IOperator> ops)
        {
            foreach (RuleFilter filter in RuleFilters)
            {
                if (!ops.ContainsKey(filter.OpName)) continue;
                IOperator op = ops[filter.OpName];
                if (!op.operate(logItem.GetField(filter.FieldName), filter.Value))
                    return false;
            }

            foreach (BaseRule rule in Rules)
            {
                if (rule.MatchLogItem(logItem, ops))
                {
                    AddLogItem(logItem);
                    return true;
                }
            }

            List<RuleFilter> newFilters = new List<RuleFilter>();
            newFilters.Add(new RuleFilter(GroupFieldName, "=", logItem.TryGetField(GroupFieldName, "").ToString()));
            SingleRule newRule = new SingleRule(logItem.TryGetField(GroupFieldName, "").ToString(), newFilters);
            newRule.MatchLogItem(logItem, ops);
            Rules.Add(newRule);

            return true;
        }
    }
}
