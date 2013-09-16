using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LogReporter.Filter
{
    public abstract class BaseRowFilter : IRowFilter
    {
        #region Constructors
        public BaseRowFilter()
        {
        }

        public BaseRowFilter(XmlNode config)
        {
        }
        #endregion

        #region Generators
        public static List<IRowFilterGenerator> Generators = new List<IRowFilterGenerator> { new DefaultRowFilterGenerator(), new ReflectionFilterGenerator() };
        public static IRowFilter Create(XmlNode config)
        {
            foreach (var generator in Generators)
            {
                try
                {
                    var filter = generator.CreateFilter(config);
                    if (filter != null)
                        return filter;
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
            }
            return null;
        }
        #endregion

        public abstract bool Match(LogItem currentRow, IEnumerable<LogItem> data);
    }

    public interface IRowFilterGenerator
    {
        IRowFilter CreateFilter(XmlNode config);
    }

    class DefaultRowFilterGenerator : IRowFilterGenerator
    {
        internal const string ATTRIBUTE_TYPE = "type";
        internal const string TYPE_FIELD_FILTER = "field";
        
        public IRowFilter CreateFilter(XmlNode config)
        {
            string type = TYPE_FIELD_FILTER;
            if ((config.Attributes[ATTRIBUTE_TYPE] != null) && (!string.IsNullOrWhiteSpace(config.Attributes[ATTRIBUTE_TYPE].Value)))
                type = config.Attributes[ATTRIBUTE_TYPE].Value;
                 switch (type) {
                     case TYPE_FIELD_FILTER:
                        return new FieldFilter(config);
                 }
            return null;
        }
    }
}
