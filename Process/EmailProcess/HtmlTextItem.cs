using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogReporter.Rule;
using System.IO;

namespace LogReporter.Process.EmailProcess
{
    public class HtmlTextItem : BaseEmailBodyItem
    {
        internal const string ATTRIBUTE_SOURCE = "src";

        public HtmlTextItem(string htmlText) : base()
        {
            HtmlText = htmlText;
        }

        public HtmlTextItem(XmlNode config) : base(config)
        {
            if (config.Attributes[ATTRIBUTE_SOURCE] != null)
                HtmlSource = config.Attributes[ATTRIBUTE_SOURCE].Value;
            HtmlText = config.InnerXml;
        }

        public string HtmlText { get; set; }
        public string HtmlSource { get; set; }

        public override bool CreateContent(Dictionary<string, BaseRule> rules)
        {
            if (String.IsNullOrWhiteSpace(HtmlSource))
            {
                Content = HtmlText;
            }
            else
            {
                using (StreamReader reader = new StreamReader(HtmlSource)) {
                    Content = reader.ReadToEnd();
                }
            }
            return true;
        }
    }
}
