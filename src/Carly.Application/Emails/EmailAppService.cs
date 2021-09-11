using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace Carly.Emails
{
    public class EmailAppService : IEmailAppService
    {
        private readonly ISettingManager _settingManager;

        public EmailAppService(ISettingManager settingManager)
        {
            _settingManager = settingManager;
        }

        public async Task<bool> SendEmailAsync(string EmailAddress, string Subject, string Body, string AttachmentFileName)
        {
            IReadOnlyList<ISettingValue> ts = await _settingManager.GetAllSettingValuesAsync(SettingScopes.All);
            if (ts.Count > 0)
            {
                MimeMessage email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_settingManager.GetSettingValue("Abp.Net.Mail.DefaultFromAddress")));
                email.Bcc.Add(MailboxAddress.Parse(_settingManager.GetSettingValue("Abp.Net.Mail.DefaultFromAddress")));
                //foreach(string EmailAddr in EmailAddress)
                //{
                email.To.Add(MailboxAddress.Parse(EmailAddress));
                //}
                email.Subject = Subject;
                //email.Body = new TextPart(TextFormat.Html) { Text = Body + EmailFooter };                
                var builder = new BodyBuilder();
                builder.HtmlBody = Body;
                //builder.TextBody = Body.Replace(@"<br/>", Environment.NewLine);
                if (!Equals(AttachmentFileName, "")) { builder.Attachments.Add(AttachmentFileName); }
                email.Body = builder.ToMessageBody();

                try
                {
                    SmtpClient smtpClient = new SmtpClient();
                    smtpClient.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                    smtpClient.Connect(_settingManager.GetSettingValue("Abp.Net.Mail.Smtp.Host"), Convert.ToInt32(_settingManager.GetSettingValue("Abp.Net.Mail.Smtp.Port")), MailKit.Security.SecureSocketOptions.Auto);
                    smtpClient.Authenticate(_settingManager.GetSettingValue("Abp.Net.Mail.Smtp.UserName"), _settingManager.GetSettingValue("Abp.Net.Mail.Smtp.Password"));
                    smtpClient.Send(email);
                    smtpClient.Disconnect(true); 
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Email : " + ex);
                }
            }
            return true;
        }
    }
}
