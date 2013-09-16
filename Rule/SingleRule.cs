using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogReporter.Operator;

namespace LogReporter.Rule
{
    public class SingleRule : BaseRule
    {
        internal const string ATTRIBUTE_FIELD_NAME = "fieldName";
        internal const string ATTRIBUTE_OP = "op";
        internal const string ATTRIBUTE_VALUE = "value";
        internal const string TAG_FILTER = "Filter";

        public SingleRule(string ruleName, List<RuleFilter> ruleFilters) : base(ruleName)
        {
            RuleFilters = ruleFilters;
            RuleName = ruleName;
        }

        public SingleRule(XmlNode config)
            : base(config)
        {
            RuleFilters = new List<RuleFilter>();

            foreach (XmlNode filterNode in config.ChildNodes)
            {
                if (TAG_FILTER == filterNode.Name)
                {
                    RuleFilter filter = CreateRuleFilter(filterNode);
                    if (filter != null)
                        RuleFilters.Add(filter);
                }
            }
        }

        private RuleFilter CreateRuleFilter(XmlNode filterNode)
        {
            XmlAttribute fieldNameAtt = filterNode.Attributes[ATTRIBUTE_FIELD_NAME];
            if (fieldNameAtt == null || string.IsNullOrWhiteSpace(fieldNameAtt.Value))
                throw new ArgumentException("The Filter attribute 'fieldName' is needed");
            string fieldName = fieldNameAtt.Value;


            XmlAttribute opNameAtt = filterNode.Attributes[ATTRIBUTE_OP];
            if (opNameAtt == null || string.IsNullOrWhiteSpace(opNameAtt.Value))
                throw new ArgumentException("The Filter attribute 'op' is needed");
            string opName = opNameAtt.Value;

            // Could be Empty, default value is ""
            string value = "";
            XmlAttribute valueAtt = filterNode.Attributes[ATTRIBUTE_VALUE];
            if (fieldName != null && !string.IsNullOrWhiteSpace(valueAtt.Value))
                value = valueAtt.Value;

            RuleFilter filter = new RuleFilter(fieldName, opName, value);
            return filter;
        }

        public override bool MatchLogItem(LogItem logItem, Dictionary<string, IOperator> ops)
        {
            foreach (RuleFilter filter in RuleFilters)
            {
                if (!ops.ContainsKey(filter.OpName)) continue;
                IOperator op = ops[filter.OpName];
                if (!op.operate(logItem.TryGetField(filter.FieldName, null), filter.Value))
                    return false;
            }

            AddLogItem(logItem);
            return true;
        }

        public override string RuleName { get; set; }
        public List<RuleFilter> RuleFilters { get; set; }
        public string Description { get; set; }
    }
}
