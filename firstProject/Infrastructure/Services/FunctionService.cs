using Application.Interface;
using firstProject.ApplicationDbContext;
using firstProject.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


namespace firstProject.DTO
{
    public class FunctionService:IFunctionService
    {
        private readonly DB _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        public FunctionService( DB db, IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
        }

        public async Task<string> GenerateVerifyCode(string Email, string typeOfGenerate)
        {

            var entity = await _db.Users.FirstOrDefaultAsync(x => x.Email == Email);

            if (entity == null)
                return "المستخدم غير موجود";

            var found = await _db.twoFactorVerify.Where(l => l.UserId == entity.Id && l.TypeOfGenerate == typeOfGenerate).OrderByDescending(l => l.Date).FirstOrDefaultAsync();

            if (found != null &&(DateTime.UtcNow - found.PeriodStartTime) < TimeSpan.FromHours(1) &&found.Attempts >= 3)
            {
                return "لقد تجاوزت الحد المسموح لمحاولات الإرسال. حاول بعد ساعة";
            }

            if (found != null && (DateTime.UtcNow - found.Date) < TimeSpan.FromMinutes(2))
            {
                return "انتظر دقيقتين قبل إرسال الكود مرة أخرى";
            }

            int code = RandomNumberGenerator.GetInt32(100000, 1000000); 
            string hashedCode = BCrypt.Net.BCrypt.HashPassword(code.ToString());

            if (found == null)
            {
                var twoFactor = new TwoFactorVerify
                {
                    UserId = entity.Id,
                    TypeOfGenerate = typeOfGenerate,
                    Value = hashedCode,
                    Date = DateTime.UtcNow,
                    PeriodStartTime = DateTime.UtcNow,
                    Attempts = 1,
                }
            ;
                await _db.twoFactorVerify.AddAsync(twoFactor);
            }
            else
            {
                if ((DateTime.UtcNow - found.PeriodStartTime) < TimeSpan.FromHours(1))
                {
                    found.Attempts++;
                    found.IsUsed= false;
                }
                else
                {
                    found.PeriodStartTime = DateTime.UtcNow;
                    found.Attempts = 1;
                    found.IsUsed = false;
                }

                found.Value = hashedCode;
                found.Date = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return code.ToString();
        }
        public async Task<JsonElement?> SendAPI(string ID)
        {
            try
            {
                string? token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];

                if (string.IsNullOrEmpty(token))
                {
                    return JsonDocument.Parse("{\"success\": false, \"message\": \"Missing Authorization Token\"}").RootElement;
                }

                var requestData = new { ID = ID };
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, "http://firstproject-service:9100/api/Select-Data")
                {
                    Content = jsonContent
                };

                // ✅ إرسال التوكن في الهيدر كـ Authorization Bearer (لو الـ API بيحتاجه في الهيدر)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // ولو السيرفر بيتوقعه في الكوكي:
                // request.Headers.Add("Cookie", $"token={token}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                string responseString = await response.Content.ReadAsStringAsync();
                return JsonDocument.Parse(responseString).RootElement;
            }
            catch (HttpRequestException httpEx)
            {
                return JsonDocument.Parse($"{{\"success\": false, \"message\": \"HTTP Request Error: {httpEx.Message}\"}}").RootElement;
            }
            catch (Exception ex)
            {
                return JsonDocument.Parse($"{{\"success\": false, \"message\": \"{ex.Message}\"}}").RootElement;
            }
        }

    }
}
