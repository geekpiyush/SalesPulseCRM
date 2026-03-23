using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SalesPulseCRM.Application.Services
{
    public class EmailServices
    {
       public void SendEmail(string toEmail,string link)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("dpiyush28@gmail.com", "zeopfoujocxhakbp"),
                EnableSsl = true,

            };
            var mail = new MailMessage
            {
                From = new MailAddress("dpiyush28@gmail.com"),
                Subject = "Reset Password | SalesPulseCRM",
                Body = $"Click here to reset password: {link}",
                IsBodyHtml = true,
            };
            mail.To.Add(toEmail);
            smtpClient.Send(mail);
        }
    }
}
