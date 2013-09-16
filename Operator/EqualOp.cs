using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogReporter.Operator
{
    public class EqualOp : IOperator
    {
        public bool operate(object input, string criterion)
        {
            if (input == null && criterion == null) return true;
            if (input == null || criterion == null) return false;
            return  criterion.Equals(input.ToString());
        }

        public string OperatorName
        {
            get
            {
                return "=";
            }
        }
    }
}
