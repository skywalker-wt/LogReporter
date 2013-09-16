using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Collections.Generic;
using LogReporter.Rule;

namespace LogReporter.Process.EmailProcess
{
    public class TestEmailOutput : BaseReportProcess
    {
        public override bool Process(Dictionary<string, BaseRule> rules)
        {
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.To.Add("skwwt@163.com");
            message.From = new MailAddress("t-twu@microsoft.com");
            message.Subject = "Message Subject";
            message.Body = "Message Body";
            message.IsBodyHtml = true;

            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.microsoft.com");
            smtp.Credentials = new NetworkCredential("t-twu@microsoft.com", "!1qaz@2wsx");
            smtp.Send(message);

            return false;
        }
    }
}
