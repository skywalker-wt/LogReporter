using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace LogReporter.Source
{
    class XmlSource : BaseSource
    {
        internal const string TAG_SOURCEURI = "SourceURI";
        internal const string TAG_XPATHHEADER = "XpathHeader";
        internal const string TAG_XPATHDATA = "XpathData";
        internal const string TAG_XPATHVALUE = "XpathValue";
        internal const string TAG_AUTH = "Auth";
        internal const string ATTRIBUTE_DOMAIN = "domain";
        internal const string ATTRIBUTE_USER = "user";
        internal const string ATTRIBUTE_PASSWORD = "password";

        internal const string xpathTableRow = @"{0}";
        internal const string xpathTableColumn = @"{0}/text()";
        const string xpathTableDataText = @"td/text()";

        public String SourceURI { get; private set; }
        public String XpathHeader { get; private set; }
        public String XpathData { get; private set; }
        public String XpathValue { get; private set; }

        public bool UseAuth { get; private set; }
        private NetworkCredential credential { get; set; }
        

        private List<Dictionary<String, Object>> Data;
        public List<String> Header { get; private set; }
        public int Rows { get { return Data.Count; } }
        public int Columns { get { return Header.Count; } }

        public XmlSource(String name, String sourceURI, String xpathHeader, String xpathData, String xpathValue) : base(name)
        {
            SourceURI = sourceURI;
            XpathHeader = xpathHeader;
            XpathData = xpathData;
            XpathValue = xpathValue;
            Data = new List<Dictionary<string, object>>();
            Header = new List<String>();
            UseAuth = false;
        }

        public XmlSource(XmlNode config)
            : base(config)
        {
            Data = new List<Dictionary<string, object>>();
            Header = new List<String>();
            UseAuth = false;
            foreach (XmlNode node in config.ChildNodes)
            {
                switch (node.Name) {
                    case TAG_SOURCEURI:
                        SourceURI = node.InnerText;
                        break;
                    case TAG_XPATHHEADER:
                        XpathHeader = node.InnerText;
                        break;
                    case TAG_XPATHDATA:
                        XpathData = node.InnerText;
                        break;
                    case TAG_XPATHVALUE:
                        XpathValue = node.InnerText;
                        break;
                    case TAG_AUTH:
                        CreateAuthToken(node);
                        break;
                }
            }
        }

        private void CreateAuthToken(XmlNode node)
        {
            String Domain = "";
            String Password = "";
            String User = "";
            foreach (XmlAttribute attr in node.Attributes)
            {
                switch (attr.Name)
                {
                    case ATTRIBUTE_DOMAIN:
                        Domain = attr.Value;
                        break;
                    case ATTRIBUTE_PASSWORD:
                        Password = attr.Value;
                        break;
                    case ATTRIBUTE_USER:
                        User = attr.Value;
                        break;
                }
            }
            credential = new NetworkCredential(User, Password, Domain);
            UseAuth = (credential != null);
        }

        private XmlNamespaceManager LoadContents(XmlDocument document)
        {
            int retry = 3;
            while (retry > 0)
            {
                try
                {
                    WebRequest myWebRequest = WebRequest.Create(SourceURI);
                    if (UseAuth)
                        myWebRequest.Credentials = credential;

                    using (WebResponse myWebResponse = myWebRequest.GetResponse())
                    {
                        using (Stream stream = myWebResponse.GetResponseStream())
                        {
                            // read in all the source
                            StreamReader streamReader = new StreamReader(stream);
                            string source = streamReader.ReadToEnd();
                            streamReader.Close();
                            // use reader 
                            StringReader stringReader1 = new StringReader(source);
                            document.Load(stringReader1);
                            stringReader1.Close();
                            StringReader stringReader2 = new StringReader(source);
                            XPathNavigator sourceNav = new XPathDocument(stringReader2).CreateNavigator();
                            stringReader2.Close();

                            sourceNav.MoveToFollowing(XPathNodeType.Element);
                            IDictionary<String, String> namespaces = sourceNav.GetNamespacesInScope(XmlNamespaceScope.All);
                            XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
                            foreach (string key in namespaces.Keys)
                            {
                                nsmgr.AddNamespace(key, namespaces[key]);
                            }
                            return nsmgr;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Retry {0} time(s).", retry);
                }
                retry--;
            }
            return null;
        }

        public override IEnumerable<LogItem> GetAllLogItems()
        {
            XmlDocument sourceDoc = new XmlDocument();
            var nsmgr = LoadContents(sourceDoc);
            if (nsmgr == null) // fail to load document
                return null;

            // add namespaces to resolve XML correctly
            
            var tableHeaderNode = sourceDoc.SelectSingleNode(XpathHeader, nsmgr);
            if (tableHeaderNode == null)
            {
                Console.WriteLine("ERROR: 'header' not found.");
                return null;
            }
            var items = new List<LogItem>();

            var tableHeaderTextNodes = tableHeaderNode.SelectNodes(XpathValue, nsmgr);
            if (tableHeaderTextNodes == null)
                return items;

            foreach (XmlNode headerNode in tableHeaderTextNodes)
            {
                Header.Add(headerNode.InnerText.Trim());
            }

            int headerCount = Header.Count;
            var tableDataNodes = sourceDoc.SelectNodes(XpathData, nsmgr);
            if (tableDataNodes == null)
                return items;

            foreach (XmlNode tableDataNode in tableDataNodes)
            {
                var tableDataTextNodes = tableDataNode.SelectNodes(XpathValue, nsmgr);
                if (tableDataTextNodes == null) // skip this line
                    continue;
                int count = 0;
                var item = new LogItem();
                foreach (XmlNode tableDataTextNode in tableDataTextNodes)
                {
                    if (count >= headerCount)
                        break;
                    item.SetField(Header[count], tableDataTextNode.InnerText.Trim());
                    count++;
                }
                items.Add(item);
            }
            return items;
        }
    }
}
