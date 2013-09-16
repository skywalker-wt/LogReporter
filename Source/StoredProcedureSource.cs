using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LogReporter.Source
{
    public class StoredProcedureSourceGenerator : ISourceGenerator
    {
        internal const string ATTRIBUTE_SOURCE_TYPE = "type";
        internal const string SPSOURCE_TYPE = "StoredProcedureSource";
        public BaseSource CreateSource(XmlNode config)
        {
            XmlAttribute typeName = config.Attributes[ATTRIBUTE_SOURCE_TYPE];
            if ( typeName != null &&
                 !string.IsNullOrWhiteSpace(typeName.Value) &&
                 SPSOURCE_TYPE == typeName.Value)
                return new StoredProcedureSource(config);

            return null;
        }
    }

    public class StoredProcedureSource : SQLSource
    {
        internal const string TAG_SPNAME = "SPName";
        internal const int SqlCommandDefaultTimeout = 100;

        public string StoredProcedureName { get; set; }

        public StoredProcedureSource(XmlNode config) : base(config)
        {
            foreach (XmlNode subNode in config)
            {
                if (TAG_SPNAME == subNode.Name)
                    CreateStoredProcedureName(subNode);
            }
        }

        private void CreateStoredProcedureName(XmlNode procedureNode)
        {
            StoredProcedureName = procedureNode.InnerText;
        }

        public override IEnumerable<LogItem> GetAllLogItems()
        {
            List<LogItem> logItems = new List<LogItem>();
            
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            try
            {
                using (SqlCommand cmd = new SqlCommand(StoredProcedureName, connection))
                {
                    cmd.CommandTimeout = SqlCommandDefaultTimeout;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    SqlDataReader resultReader = cmd.ExecuteReader();

                    while (resultReader.Read())
                    {
                        LogItem logItem = new LogItem();
                        foreach (string fieldName in Fields)
                        {
                            //if (resultReader[fieldName] == null)
                            logItem.SetField(fieldName, resultReader[fieldName]);
                        }

                        logItems.Add(logItem);
                    }
                }
            }
            finally
            {
                connection.Close();
            }

            return logItems;
        }
    }
}
