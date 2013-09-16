using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using LogReporter.Operator;
using LogReporter.Process;
using LogReporter.Rule;
using LogReporter.Source;

namespace LogReporter
{
    public class Reporter
    {
        internal const string TAG_REPORTER = "Reporter";
        internal const string TAG_SOURCE = "Source";
        internal const string TAG_PROCESS = "Process";
        internal const string TAG_RULE = "Rule";
        internal const string ATTRIBUTE_TYPE = "type";

        #region Fields
        public Dictionary<string, BaseSource> Sources { get; private set; }
        public List<BaseReportProcess> Processes { get; private set; }
        public Dictionary<string, BaseRule> Rules { get; private set; }
        public Dictionary<string, BaseRule> FirstClassRules { get; private set; }
        public LogItem Content { get; set; }

        #endregion

        public Reporter(Dictionary<string, BaseSource> sources, IEnumerable<BaseReportProcess> processes, Dictionary<string, BaseRule> rules)
        {
            this.Sources = sources;
            this.Processes = processes.ToList();
            this.Rules = rules;

            Content = new LogItem();
        }

        public Reporter(XmlNode config)
        {
            if (TAG_REPORTER != config.Name)
            {
                throw new ArgumentException(string.Format("The config xmlNode's name is not '{0}', config = '{1}'", TAG_REPORTER, config.OuterXml));
            }

            Sources = new Dictionary<string, BaseSource>();
            Processes = new List<BaseReportProcess>();
            Rules = new Dictionary<string, BaseRule>();
            FirstClassRules = new Dictionary<string, BaseRule>();
            Content = new LogItem();

            XmlNodeList xmlNodes = config.ChildNodes;
            foreach (XmlNode node in xmlNodes)
            {
                switch (node.Name)
                {
                    case TAG_SOURCE:
                        BaseSource source = BaseSource.Create(node);
                        if (source != null)
                            Sources[source.Name] = source;
                        break;
                    case TAG_RULE:
                        BaseRule rule = BaseRule.Create(node);
                        if (rule != null)
                        {
                            FirstClassRules[rule.RuleName] = rule;
                            AddRule(rule);
                        }
                        break;
                    case TAG_PROCESS:
                        BaseReportProcess process = BaseReportProcess.Create(node);
                        if (process != null)
                            Processes.Add(process);
                        break;
                }
            }
        }

        private void AddRule(BaseRule rule)
        {
            if (rule is GroupRule)
                foreach (BaseRule subRule in (rule as GroupRule).Rules)
                    AddRule(subRule);

            Rules[rule.RuleName] = rule;
        }
        
        #region Runtime

        public void Start()
        {
            run();
        }

        protected void run()
        {
            // Load all source
            Dictionary<string, IEnumerable<LogItem>> sourceDict = new Dictionary<string, IEnumerable<LogItem>>();
            foreach (var name in Sources.Keys)
            {
                if (Sources[name] != null) {
                    var items = Sources[name].GetAllLogItems();
                    if (items == null) {
                        Console.WriteLine("GetAllLogItems failed for Source {0}.", name);
                    }
                    else {
                        sourceDict[name] = items;
                    }

                    if (Sources[name] is IExtendResult)
                        Content.Extends((Sources[name] as IExtendResult).GetExtendResult());
                }
            }

            // Process data using rule

            foreach (var source in sourceDict) {
                foreach (var logItem in source.Value)
                {
                    foreach (BaseRule rule in FirstClassRules.Values)
                    {
                        rule.MatchLogItem(logItem, OperatorManager.GetInstance().Operators);
                    }
                }
            }

            // post process
            BaseReportProcess.Content = Content;
            Processes.Sort((x, y) => x.Priority.CompareTo(y.Priority));
            foreach (BaseReportProcess process in Processes)
            {
                try
                {
                    process.Process(Rules);
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }

        public void Stop()
        {
            
        }

        #endregion
    }
}
