using Application.Interface;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class EmailService:IEmailService
{
    public async  Task<(bool Success,string Error)> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("تخليص تك", Environment.GetEnvironmentVariable("EmailConfiguration__From")));
            message.ReplyTo.Add(new MailboxAddress("دعم تخليص تك", "support@gmail.com"));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                string smtpServer = Environment.GetEnvironmentVariable("EmailConfiguration__SmtpServer")!;
                int smtpPort = int.TryParse(Environment.GetEnvironmentVariable("EmailConfiguration__Port"), out int port) ? port : 587;
                string username = Environment.GetEnvironmentVariable("EmailConfiguration__Username")!;
                string password = Environment.GetEnvironmentVariable("EmailConfiguration__Password")!;
                bool useSsl = bool.TryParse(Environment.GetEnvironmentVariable("EmailConfiguration__UseSSL"), out bool ssl) && ssl;

                await client.ConnectAsync(smtpServer, smtpPort, useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            return (true, "تم إرسال البريد الإلكتروني بنجاح");
        }
        catch (Exception)
        {
            return (false, "حدث خطأ اثناء توليد الكود");
        }
    }
}
