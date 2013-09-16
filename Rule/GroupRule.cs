using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogReporter.Operator;

namespace LogReporter.Rule
{
    public class GroupRule : SingleRule
    {
        internal const string TAG_RULE = "Rule";

        public List<BaseRule> Rules { set; get; }
        

        public GroupRule(string ruleName, List<BaseRule> rules, List<RuleFilter> ruleFilters)
            : base(ruleName, ruleFilters)
        {
            Rules = rules;
        }

        public GroupRule(XmlNode config) : base(config)
        {
            Rules = new List<BaseRule>();

            foreach (XmlNode node in config.ChildNodes)
            {
                if (TAG_RULE == node.Name)
                {
                    BaseRule rule = BaseRule.Create(node);
                    if (rule != null) Rules.Add(rule);
                }
            }
        }

        public override bool MatchLogItem(LogItem logItem, Dictionary<string, IOperator> ops)
        {
            foreach (RuleFilter filter in RuleFilters)
            {
                if (!ops.ContainsKey(filter.OpName)) continue;
                IOperator op = ops[filter.OpName];
                if (!op.operate(logItem.TryGetField(filter.FieldName, ""), filter.Value))
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
            
            return false;
        }
    }
}
