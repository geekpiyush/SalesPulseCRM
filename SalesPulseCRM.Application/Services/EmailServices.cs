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


        public void SendVerificationEmail(string toEmail, string link)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("dpiyush28@gmail.com", "zeopfoujocxhakbp"),
                EnableSsl = true,
            };

            var body = $@"
                <div style='font-family:Segoe UI;padding:20px'>
                    <h2 style='color:#2563eb'>Welcome to SalesPulse CRM 🚀</h2>
            
                    <p>Hi,</p>
                    <p>Thank you for registering. Please verify your email to activate your account.</p>

                    <a href='{link}' 
                       style='display:inline-block;padding:10px 20px;
                              background:#2563eb;color:white;
                              text-decoration:none;border-radius:6px;'>
                       Verify Account
                    </a>

                    <p style='margin-top:20px;color:gray;font-size:12px'>
                        This link will expire in 24 hours.
                    </p>
                </div>
            ";

            var mail = new MailMessage
            {
                From = new MailAddress("dpiyush28@gmail.com"),
                Subject = "Verify Your Account | SalesPulseCRM",
                Body = body,
                IsBodyHtml = true,
            };

            mail.To.Add(toEmail);
            smtpClient.Send(mail);
        }


        public void SendTeamAssignedEmail(string toEmail, string userName, string teamName,string managerName )
        {

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("dpiyush28@gmail.com", "zeopfoujocxhakbp"),
                EnableSsl = true,
            };


            var subject = "You have been added to a team";

            var body = $@"
                    Hi {userName}, <br>
                    You have been added to the team <b>{teamName}<b>.</br>
                    Your manager is <b>{managerName}</b>.<br><br>
                    Please login to the CRM to view details. <br> <br>
                    Regards,<br>
                    SalesPulseCRM
                      ";

            var mail = new MailMessage
            {
                From = new MailAddress("dpiyush28@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mail.To.Add(toEmail);
            smtpClient.Send(mail);
        }
    }
}
