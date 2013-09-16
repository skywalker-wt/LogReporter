using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogReporter.Filter
{
    public interface IRowFilter
    {
        bool Match(LogItem currentRow, IEnumerable<LogItem> data);
    }
}
