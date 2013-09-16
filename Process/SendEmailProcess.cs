using System;
using System.Xml;
using System.Net;
using System.Net.Mail;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogReporter.Rule;
using System.IO;

namespace LogReporter.Process.EmailProcess
{
    public class SendEmailProcess : BaseReportProcess
    {
        public const string TAG_FROM = "From";
        public const string TAG_TO = "To";
        public const string TAG_CC = "Cc";
        public const string TAG_SERVER = "Server";
        public const string TAG_USERNAME = "UserName";
        public const string TAG_PASSWORD = "Password";
        public const string TAG_DOMAIM = "Domain";
        public const string TAG_SUBJECT = "Subject";
        public const string TAG_BODY = "Body";
        public const string TAG_CREDENTIAL = "Credential";

        public MailAddress From { get; set; }
        public List<MailAddress> To { get; set; }
        public List<MailAddress> Cc { get; set; }
        public string Server { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public string Subject { get; set; }
        public BaseEmailBodyItem Body { get; set; }

        public SendEmailProcess( MailAddress from,
                                List<MailAddress> to,
                                List<MailAddress> cc,
                                string server,
                                string userName,
                                string password,
                                string domain,
                                string subject,
                                BaseEmailBodyItem body)
        {
            From = from;
            To = to;
            Cc = cc;
            Server = server;
            UserName = userName;
            Password = password;
            Domain = domain;
            Subject = subject;
            Body = body;
        }

        public SendEmailProcess(XmlNode config)
        {
            To = new List<MailAddress>();
            Cc = new List<MailAddress>();
            foreach (XmlNode node in config.ChildNodes)
            {
                switch (node.Name)
                {
                    case TAG_FROM:
                        CreateFrom(node);
                        break;
                    case TAG_TO:
                        CreateTo(node);
                        break;
                    case TAG_CC:
                        CreateCc(node);
                        break;
                    case TAG_SERVER:
                        CreateServer(node);
                        break;
                    case TAG_USERNAME:
                        CreateUserName(node);
                        break;
                    case TAG_PASSWORD:
                        CreatePassword(node);
                        break;
                    case TAG_DOMAIM:
                        CreateDomain(node);
                        break;
                    case TAG_SUBJECT:
                        CreateSubject(node);
                        break;
                    case TAG_BODY:
                        CreateBody(node);
                        break;
                    case TAG_CREDENTIAL:
                        CreateCredential(node);
                        break;
                }
            }
        }

        private void CreateFrom(XmlNode node)
        {
            From = new MailAddress(node.InnerText);
        }

        private void CreateTo(XmlNode node)
        {
            string[] toAddresses = node.InnerText.Split(';');
            foreach (string to in toAddresses)
                To.Add(new MailAddress(to));
        }

        private void CreateCc(XmlNode node)
        {
            string[] ccAddresses = node.InnerText.Split(';');
            foreach (string cc in ccAddresses)
                Cc.Add(new MailAddress(cc));
        }

        private void CreateServer(XmlNode node)
        {
            Server = node.InnerText;
        }

        private void CreateUserName(XmlNode node)
        {
            UserName = node.InnerText;
        }

        private void CreatePassword(XmlNode node)
        {
            Password = node.InnerText;
        }

        private void CreateDomain(XmlNode node)
        {
            Domain = node.InnerText;
        }

        private void CreateSubject(XmlNode node)
        {
            Subject = node.InnerText;
        }

        private void CreateBody(XmlNode node)
        {
            Body = new HtmlDocItem(node);
        }
        
        public override bool Process(Dictionary<string, BaseRule> rules)
        {
            return (Body.CreateContent(rules) && SendMail());
        }

        private void CreateCredential(XmlNode node)
        {
            Dictionary<string, string> accounts = new Dictionary<string, string>();
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.Name)
                {
                    case TAG_USERNAME:
                        CreateUserName(child);
                        break;
                    case TAG_PASSWORD:
                        CreatePasswordFromFile(child, accounts);
                        break;
                }
            }

            if (UserName != null)
                foreach (var accountName in accounts.Keys)
                {
                    if (accountName.Contains(UserName))
                    {
                        Password = accounts[accountName];
                    }
                }
        }

        private void CreatePasswordFromFile(XmlNode node, Dictionary<string, string> accounts)
        {
            string fileName = node.InnerText;

            using (StreamReader sr = new StreamReader(fileName))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null) {
                    string[] account = line.Split(' ');
                    accounts[account[0]] = account[1];
                }
            }
        }

        private bool SendMail() {
            MailMessage message = new MailMessage();

            message.From = From;
            foreach (MailAddress t in To)
                message.To.Add(t);
            foreach (MailAddress c in Cc)
                message.CC.Add(c);
                
            message.Subject = Subject;
            message.Body = Body.Content;
            message.IsBodyHtml = true;
            foreach (Attachment attachment in Body.Attachments)
            {
                message.Attachments.Add(attachment);
            }

            SmtpClient smtp = new SmtpClient(Server);
            smtp.Credentials = new NetworkCredential(UserName, Password, Domain);
            smtp.Send(message);
            return true;
        }

    }
}
