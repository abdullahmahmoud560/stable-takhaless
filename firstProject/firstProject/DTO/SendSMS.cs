using Microsoft.EntityFrameworkCore;
using Vonage.Messaging;
using Vonage.Request;

namespace firstProject.DTO
{
    public class SendSMS
    {
        public static async Task <bool> sendSMS(SMSDTO smsDTO)
        {
            try
            {
                if (smsDTO != null)
                {
                    var credentials = Credentials.FromApiKeyAndSecret("2a120f7f", "5veCtWnt0qqeDVgE");

                    var client = new SmsClient(credentials);

                    var response = await client.SendAnSmsAsync(new SendSmsRequest
                    {
                        To = smsDTO.To,
                        From = "Takhleesak",
                        Text = smsDTO.Text
                    });
                    if (response != null)
                    {
                        return true;
                    }
                }
                else
                { 
                    return false;
                }
            }

            catch (Exception)
            {
                return false;
            }
            return false;
        }
    }
}
