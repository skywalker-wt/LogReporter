using System;
using System.Reflection;
using System.Xml;
using LogReporter.Filter;
using LogReporter.Process.EmailProcess;
using LogReporter.Rule;
using LogReporter.Source;

namespace LogReporter
{
    class ReflectionGenerator<T> where T : class
    {
        public const string ATTRIBUTE_TYPE = "type";

        public static T CreateInstance(string typeName, object[] param = null)
        {
            if (typeName == null)
                return null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName);
                if (type != null)
                    return Activator.CreateInstance(type, param) as T;
            }
            Console.WriteLine("Cannot find type {0}.", typeName);
            return null;
        }

        public static T CreateInstanceFromXml(string typeName, XmlNode config)
        {
            if (typeName == null || config == null)
                return null;
            return CreateInstance(typeName, new object[] { config });
        }

        public static T CreateInstanceFromXml(XmlNode config)
        {
            if (config == null || config.Attributes[ATTRIBUTE_TYPE] == null)
                return null;
            string typeName = config.Attributes[ATTRIBUTE_TYPE].Value;
            return CreateInstanceFromXml(typeName, config);
        }
    }

    class ReflectionItemGenerator : IItemGenerator
    {
        public BaseEmailBodyItem CreateBaseEmailBodyItem(XmlNode config)
        {
            return ReflectionGenerator<BaseEmailBodyItem>.CreateInstanceFromXml(config);
        }
    }

    class ReflectionSourceGenerator : ISourceGenerator
    {
        public BaseSource CreateSource(XmlNode config)
        {
            return ReflectionGenerator<BaseSource>.CreateInstanceFromXml(config);
        }
    }

    class ReflectionFilterGenerator : IRowFilterGenerator
    {
        public IRowFilter CreateFilter(XmlNode config)
        {
            return ReflectionGenerator<IRowFilter>.CreateInstanceFromXml(config);
        }
    }

}
