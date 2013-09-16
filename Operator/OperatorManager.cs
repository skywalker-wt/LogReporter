using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogReporter.Operator
{
    public class OperatorManager
    {
        private static OperatorManager instance = null;

        public Dictionary<string, IOperator> Operators;

        private static Dictionary<string, IOperator> GetDefaultOperator()
        {
            Dictionary<string, IOperator> defaultOperator = new Dictionary<string, IOperator>();
            EqualOp equalOp = new EqualOp();
            NotEqualOp notEqualOp = new NotEqualOp();
            ContainsOp containsOp = new ContainsOp();

            defaultOperator[equalOp.OperatorName] = equalOp;
            defaultOperator[notEqualOp.OperatorName] = notEqualOp;
            defaultOperator[containsOp.OperatorName] = containsOp;
            return defaultOperator;
        }

        private OperatorManager()
        {
            Operators = GetDefaultOperator();
        }

        public void AddOperator(IOperator op)
        {
            Operators[op.OperatorName] = op;
        }


        public static OperatorManager GetInstance()
        {
            if (instance == null)
                instance = new OperatorManager();

            return instance;
        }

        public IOperator Find(string opName)
        {
            if (opName == null)
                return null;
            if (Operators.ContainsKey(opName))
                return Operators[opName];
            IOperator op = ReflectionGenerator<IOperator>.CreateInstance(opName);
            //Operators[opName] = op; // even for failure, do not show error message again.
            return op;
        }
    }
}
