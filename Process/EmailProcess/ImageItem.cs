using LogReporter.Rule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LogReporter.Process.EmailProcess
{
    public class ImageItem : BaseEmailBodyItem
    {
        const string ATTR_URL = "url";

        public string URL
        {
            get;
            set;
        }

        public ImageItem(XmlNode config)
        {
            foreach (XmlAttribute att in config.Attributes)
            {
                switch (att.Name)
                {
                    case ATTR_URL:
                        URL = att.Value;
                        break;
                }
            }
        }

        public override bool CreateContent(Dictionary<string, BaseRule> rules)
        {
            Attachment attachment = GetAttachment();
            if (attachment == null) return false;

            this.Attachments.Add(attachment);
            Content = string.Format("<img src='cid:{0}'/>", attachment.Name);
            return true;
        }

        protected Attachment GetAttachment()
        {
            string url = URL;
            if (url == null) return null;

            LogItem currentItem = new LogItem();
            currentItem.Extends(BaseReportProcess.Content);
            foreach (string fieldName in currentItem.FieldNames)
            {
                url = url.Replace(GetFieldMark(fieldName), currentItem.GetField(fieldName).ToString());
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            return new Attachment(response.GetResponseStream(), string.Format("{0}.png", Guid.NewGuid().ToString()));
        }

        private string GetFieldMark(string fieldName)
        {
            return "$$" + fieldName + "$$";
        }
    }
}
