using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogReporter.Operator;

namespace LogReporter.Rule
{
    public abstract class BaseRule
    {
        internal const string ATTRIBUTE_TYPE = "type";
        internal const string TYPE_SINGLE = "single";
        internal const string TYPE_GROUP = "group";
        internal const string TYPE_AUTO_GROUP = "autogroup";
        internal const string ATTRIBUTE_NAME = "name";
        internal const string ATTRIBUTE_DESC = "desc";
        internal const string ATTRIBUTE_MAX_COUNT = "maxcount";
        internal const string ATTRIBUTE_TARGET = "target";

        public static BaseRule Create(XmlNode config)
        {
            string type = TYPE_SINGLE;

            XmlAttribute typeAtt = config.Attributes[ATTRIBUTE_TYPE];
            if (typeAtt != null && !string.IsNullOrWhiteSpace(typeAtt.Value))
                type = typeAtt.Value;

            if (TYPE_SINGLE == type)
                return new SingleRule(config);

            if (TYPE_GROUP == type)
                return new GroupRule(config);

            if (TYPE_AUTO_GROUP == type)
                return new AutoGroupRule(config);

            return null;
        }

        public BaseRule(string ruleName)
        {
            RuleName = ruleName;
            LogItemsInRule = new List<LogItem>();
            MaxContentCount = 100;
            Count = 0;
        }

        public BaseRule(XmlNode config)
        {
            LogItemsInRule = new List<LogItem>();
            MaxContentCount = 100;
            Count = 0;

            XmlAttribute ruleName = config.Attributes[ATTRIBUTE_NAME];
            if (ruleName == null || string.IsNullOrWhiteSpace(ruleName.Value))
                throw new ArgumentException("The Rule attribute 'name' is needed");

            XmlAttribute ruleDesscription = config.Attributes[ATTRIBUTE_DESC];
            if (ruleDesscription != null && !string.IsNullOrWhiteSpace(ruleDesscription.Value))
                Description = ruleDesscription.Value;

            XmlAttribute ruleTarget = config.Attributes[ATTRIBUTE_TARGET];
            if (ruleTarget != null && !string.IsNullOrWhiteSpace(ruleTarget.Value))
                Target = ruleTarget.Value;

            RuleName = ruleName.Value;

            XmlAttribute maxContentCount = config.Attributes[ATTRIBUTE_MAX_COUNT];
            if (maxContentCount != null && !string.IsNullOrWhiteSpace(maxContentCount.Value))
                MaxContentCount = int.Parse(maxContentCount.Value);
        }

        public List<LogItem> LogItemsInRule { get; set; } 
        public abstract string RuleName { get; set; }
        public string Description { get; set; }
        public int MaxContentCount { get; set; }
        public int Count { get; set; }
        public string Target { get; set; }

        public abstract bool MatchLogItem(LogItem logItem, Dictionary<string, IOperator> ops);

        public virtual void AddLogItem(LogItem logItem)
        {
            Count++;

            if (MaxContentCount == -1)
            {
                LogItemsInRule.Add(logItem);
                return;
            }

            if (LogItemsInRule.Count < MaxContentCount)
            {
                LogItemsInRule.Add(logItem);
            }
        }
    }
}
