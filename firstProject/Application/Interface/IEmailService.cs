namespace Application.Interface
{
    public interface IEmailService
    {
        public Task<(bool Success,string Error)> SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
