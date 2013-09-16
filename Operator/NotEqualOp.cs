using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogReporter.Operator
{
    public class NotEqualOp : IOperator
    {
        public bool operate(object input, string criterion)
        {
            return input.ToString() != criterion;
        }

        public string OperatorName
        {
            get
            {
                return "!=";
            }
        }
    }
}
