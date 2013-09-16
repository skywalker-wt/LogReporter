using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogReporter.Operator
{
    public interface IOperator
    {
        bool operate(object input, string criterion);

        string OperatorName { get; }
    }
}
