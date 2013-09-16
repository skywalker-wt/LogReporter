using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogReporter.Rule
{
    public class RuleFilter
    {
        public RuleFilter(string fieldName, string opName, string value)
        {
            FieldName = fieldName;
            OpName = opName;
            Value = value;
        }

        public string FieldName { get; set; }
        public string OpName { get; set; }
        public string Value { get; set; }
    }
}
