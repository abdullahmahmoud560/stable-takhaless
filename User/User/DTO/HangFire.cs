using System.Globalization;
using User.ApplicationDbContext;

namespace User.DTO
{
    public class HangFire
    {
        private readonly EmailService _emailService;
        private readonly DB _db;
        private Functions _functions;
        public HangFire(EmailService emailService, Functions functions,DB db ,Functions functions1)
        {
            _emailService = emailService;
            _db = db;
            _functions = functions;
        }

        public async Task<string> sendEmail(string Email, int NewOrderId, DateTime endDate)
        {
            CultureInfo culture = new CultureInfo("ar-SA")
            {
                DateTimeFormat = { Calendar = new GregorianCalendar() },
                NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
            };
            var body = string.Format(@"
                <!DOCTYPE html>
                <html lang=""ar"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>تنبيه بخصوص طلبك - تخليص تك</title>
                </head>
                <body style=""font-family: Arial, sans-serif; color: #333; text-align: center; padding: 20px;"">
                    <div style=""max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; padding: 20px; background-color: #f9f9f9;"">
                        <h2 style=""color: #ff0000;"">تنبيه هام</h2>
                        <p style=""font-size: 16px;"">مرحبًا،</p>
                        <p style=""font-size: 16px;"">نود إعلامك بأن موعد انتهاء طلبك رقم <strong>{0}</strong> قد اقترب.</p>
                        <p style=""font-size: 16px;"">يرجى مراجعة طلبك واتخاذ الإجراء اللازم قبل تاريخ الانتهاء.</p>
                        <p style=""display: inline-block; padding: 12px 25px; background-color: #ff0000; color: white; text-decoration: none; font-size: 22px; border-radius: 5px; font-weight: bold; margin-top: 10px;"">
                        {1}
                        </p>
                        <p style=""font-size: 16px; margin-top:20px;"">لأي استفسار يمكنك التواصل معنا عبر الدعم الفني.</p>
                        <hr style=""margin:30px 0;""/>
                        <p style=""font-size: 14px; color: #777;"">
                            مع تحيات<br/>
                            فريق <strong>Takhleesak</strong><br/>
                            <a href=""https://takhleesak.com"" target=""_blank"">takhleesak.com</a><br/>
                            للدعم الفني: <a href=""mailto:support@takhleesak.com"">support@takhleesak.com</a>
                        </p>
                    </div>
                </body>
                </html>", NewOrderId, endDate.ToString("dddd, dd MMMM yyyy", culture));

            await _emailService.SendEmailAsync(Email, "تنبيه: اقترب موعد انتهاء طلبك", body);
            return null!;
        }
        public async Task<string> sendEmilToBroker(string Email, int NewOrderId , DateTime endDate)
        {
            CultureInfo culture = new CultureInfo("ar-SA")
            {
                DateTimeFormat = { Calendar = new GregorianCalendar() },
                NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
            };
            var body = string.Format(@"
                    <!DOCTYPE html>
                    <html lang=""ar"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport""content=""width=device-width, initial-scale=1.0"">
                        <title>تم قبول طلبك - Takhleesak</title>
                    </head>
                    <body style=""font-family: Arial, sans-serif; color: #333; text-align: center; padding: 20px;"">
                        <div style=""max-width: 600px; margin: auto; border: 1px solid #ddd; border-radius: 10px; padding: 20px; background-color: #f9f9f9;"">
                            <h2 style=""color: #007bff;"">تم قبول طلبك</h2>
                            <p style=""font-size: 16px;"">مرحبًا،</p>
                            <p style=""font-size: 16px;"">نود إعلامك بأن طلبك رقم <strong>{0}</strong> تم قبوله من قبل العميل.</p>
                            <p style=""display: inline-block; padding: 12px 25px; background-color: #007bff; color: white; text-decoration: none; font-size: 22px; border-radius: 5px; font-weight: bold; margin-top: 10px;"">
                            {1}
                            </p>
                            <p style=""font-size: 16px; margin-top:20px;"">لأي استفسار يمكنك التواصل معنا عبر الدعم الفني.</p>
                            <hr style=""margin:30px 0;""/>
                            <p style=""font-size: 14px; color: #777;"">
                                مع تحيات<br/>
                                فريق <strong>Takhleesak</strong><br/>
                                <a href=""https://takhleesak.com"" target=""_blank"">takhleesak.com</a><br/>
                                للدعم الفني: <a href=""mailto:support@takhleesak.com"">support@takhleesak.com</a>
                            </p>
                        </div>
                    </body>
                    </html>", NewOrderId , endDate.ToString("dddd, dd MMMM yyyy", culture));

            await _emailService.SendEmailAsync(Email, $"🚀 طلبك رقم {NewOrderId} تم قبوله من قبل العميل", body);
            return null!;
        }
    }
}

