using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Emails
{
    public interface IEmailAppService
    {
        Task<bool> SendEmailAsync(string EmailAddress, string Subject, string Body, string AttachmentFileName);
    }
}
