using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using LCMIS.Server.Model;

namespace College_ERP.Models.MailService
{
    public class MailService
    {
        string senderEmail = ConfigurationManager.AppSettings["SenderEmail"];
        string password = ConfigurationManager.AppSettings["SenderPassword"].Replace(" ", "");

        public CommonMessage SendEmail(string subject, string body, string receiverEmail)
        {
            try
            {
                string mailbody = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <style>
        body {{
            margin: 0;
            padding: 0;
            background-color: #eef2f7;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }}
        .email-wrapper {{
            max-width: 600px;
            margin: 40px auto;
            background: #ffffff;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }}
        .email-header {{
            background-color: #003366;
            color: white;
            text-align: center;
            padding: 20px 40px;
        }}
        .email-header h1 {{
            margin: 0;
            font-size: 24px;
            letter-spacing: 1px;
        }}
        .email-body {{
            padding: 30px 40px;
            color: #555555;
            font-size: 16px;
            line-height: 1.8;
        }}
        .email-body p {{
            margin-bottom: 20px;
        }}
        .email-footer {{
            background-color: #f5f5f5;
            padding: 20px 40px;
            text-align: center;
            font-size: 12px;
            color: #888888;
        }}
        a.button {{
            display: inline-block;
            margin-top: 20px;
            padding: 10px 20px;
            background-color: #0066cc;
            color: white;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class=""email-wrapper"">
        <div class=""email-header"">
            <h1>College ERP</h1>
        </div>
        <div class=""email-body"">
            <p>Dear {receiverEmail.Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries)[0]},</p>

            <p>We hope this message finds you well.</p>

            <p>
              {body}.<br/>
                Kindly review the provided information carefully and take any necessary actions as applicable.
            </p>


            <p>Thank you for your attention and cooperation.</p>

            <p>Best regards,<br>
            <strong>College ERP</strong><br>
            </p>
        </div>
        <div class=""email-footer"">
            This is an automatically generated email. Please do not reply to this message.
        </div>
    </div>
</body>
</html>
";
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(senderEmail, password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail),
                    Subject = subject,
                    Body = mailbody,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(receiverEmail);

                smtpClient.Send(mailMessage);
                return new CommonMessage { status = true, message = "mail send successfully" };
            }
            catch (Exception ex)
            {
                return new CommonMessage
                {
                    status = false,
                    message = ex.ToString()
                };
            }

        }
    }
}