using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LogReporter.Source
{
    public abstract class SQLSource : BaseSource
    {
        internal const string TAG_CONNECTIONSTRING = "ConnectionString";
        internal const string TAG_FIELDS_GROUP = "Fields";
        internal const string TAG_FIELD = "Field";
        internal const string ATTRIBUTE_FIELD_NAME = "name";

        public string ConnectionString { set; get; }
        public List<string> Fields { set; get; }

        public SQLSource(string connectionString, List<string> fields) : base(connectionString)
        {
            ConnectionString = connectionString;
            Fields = fields;
        }

        public SQLSource(XmlNode config) : base(config)
        {
            Fields = new List<string>();
            ConnectionString = null;

            foreach (XmlNode sourceConfigNode in config)
            {
                if (TAG_CONNECTIONSTRING == sourceConfigNode.Name)
                    CreateConnectionString(sourceConfigNode);
                else if (TAG_FIELDS_GROUP == sourceConfigNode.Name)
                    CreateFields(sourceConfigNode);
            }
        }

        private void CreateConnectionString(XmlNode connectionNode)
        {
            ConnectionString = connectionNode.InnerText;
        }

        private void CreateFields(XmlNode fieldsNode)
        {
            foreach (XmlNode fieldNode in fieldsNode)
            {
                XmlAttribute attributeFieldName = fieldNode.Attributes[ATTRIBUTE_FIELD_NAME];
                if (attributeFieldName != null && !string.IsNullOrWhiteSpace(attributeFieldName.Value))
                    Fields.Add(attributeFieldName.InnerText);
            }
        }
    }
}
