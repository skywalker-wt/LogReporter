using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LogReporter.Operator;

namespace LogReporter.Filter
{
    public class FieldFilter : BaseRowFilter
    {
        internal const string ATTRIBUTE_FIELD_NAME = "fieldName";
        internal const string ATTRIBUTE_OP = "op";
        internal const string ATTRIBUTE_VALUE = "value";

        #region Constructors
        public FieldFilter(string fieldName, string opName, string value="") : base()
        {
            FieldName = fieldName;
            OpName = opName;
            Operator = OperatorManager.GetInstance().Find(OpName);
            Value = value;
            if (string.IsNullOrWhiteSpace(FieldName))
                throw new ArgumentException("Attribute 'fieldName' is needed in Filter.");
            if (Operator == null)
                throw new ArgumentException("Operator is needed in Filter.");
        }

        public FieldFilter(XmlNode config) : base(config) {
            foreach (XmlAttribute attr in config.Attributes)
            {
                switch (attr.Name) {
                    case ATTRIBUTE_FIELD_NAME:
                        FieldName = attr.Value;
                        break;
                    case ATTRIBUTE_OP:
                        OpName = attr.Value;
                        break;
                    case ATTRIBUTE_VALUE:
                        Value = attr.Value;
                        break;
                }
            }
            Operator = OperatorManager.GetInstance().Find(OpName);
            if (string.IsNullOrWhiteSpace(FieldName))
                throw new ArgumentException("Attribute 'fieldName' is needed in Filter.");
            if (Operator == null)
                throw new ArgumentException("Operator is needed in Filter.");
        }
        #endregion

        #region Fields
        public string FieldName { get; private set; }
        public string OpName { get; private set; }
        public string Value { get; private set; }
        public IOperator Operator { get; private set; }
        #endregion

        public override bool Match(LogItem currentRow, IEnumerable<LogItem> data)
        {
            if (FieldName == null || !currentRow.HasField(FieldName))
                return false;
            object field = currentRow.GetField(FieldName);
            if (field == null || Operator == null)
                return false;
            return Operator.operate(field, Value);
        }
    }
}
