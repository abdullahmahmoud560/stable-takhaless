using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class EmailService
{
    public async Task<string> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            // إنشاء الرسالة
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("تخليص تك", Environment.GetEnvironmentVariable("EmailConfiguration__From")));
            message.ReplyTo.Add(new MailboxAddress("تخليص تك", "support@takhleesak.com"));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            // إنشاء هيكل الرسالة مع دعم HTML
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

                // الاتصال بالخادم
                await client.ConnectAsync(smtpServer, smtpPort, useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, password);

                // إرسال البريد
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            return "✅ تم إرسال البريد الإلكتروني بنجاح.";
        }
        catch (Exception ex)
        {
            return $"❌ خطأ أثناء إرسال البريد: {ex.Message}";
        }
    }
}
