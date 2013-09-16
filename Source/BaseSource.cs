using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LogReporter.Source
{
    public abstract class BaseSource
    {
        public const string TAG_NAME = "name";

        public abstract IEnumerable<LogItem> GetAllLogItems();

        #region Fields
        public String Name { get; private set; }
        #endregion

        #region Constructors
        public BaseSource(string name) {
            Name = name;
            if (Name == null)
                throw new ArgumentException("Attribute 'name' must be specified in Source.");
        }

        public BaseSource(XmlNode config) {
            foreach (XmlAttribute attr in config.Attributes)
            {
                switch (attr.Name)
                {
                    case TAG_NAME:
                        Name = attr.Value;
                        break;
                }
            }
            if (Name == null)
                throw new ArgumentException("Attribute 'name' must be specified in Source.");
        }
        #endregion

        #region Generator
        public static List<ISourceGenerator> Generators = new List<ISourceGenerator>{new StoredProcedureSourceGenerator(), new ReflectionSourceGenerator(), new TextFileSourceGenerator()};

        public static BaseSource Create(XmlNode config)
        {
            foreach (var generator in Generators)
            {
                try
                {
                    var source = generator.CreateSource(config);
                    if (source != null)
                        return source;
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
            }
            return null;
        }
        #endregion
    }

    public interface ISourceGenerator
    {
        BaseSource CreateSource(XmlNode config);
    }


}
