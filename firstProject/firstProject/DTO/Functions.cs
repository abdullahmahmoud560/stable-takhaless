using firstProject.ApplicationDbContext;
using firstProject.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace firstProject.DTO
{
    public class Functions
    {
        private readonly UserManager<User> _userManager;
        private readonly DB _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        public Functions(UserManager<User> userManager, DB db, IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
            _userManager = userManager;
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
        }

        public async Task<string> GenerateVerifyCode(User user,string typeOfGenerate)
        {
            var verifyCode = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            var hashedCode = BCrypt.Net.BCrypt.HashPassword(verifyCode);
            var found =await _db.twoFactorVerify.Where(l => l.UserId == user.Id).FirstOrDefaultAsync();
            if (found == null)
            {
                var twoFactor = new TwoFactorVerify
                {
                    UserId = user.Id,
                    LoginProvider = "Default",
                    Name = typeOfGenerate,
                    Value = hashedCode,
                    Date = DateTime.UtcNow
                };
                await _db.twoFactorVerify.AddAsync(twoFactor);
            }
            else
            {
                _db.twoFactorVerify.Remove(found);
                var twoFactor = new TwoFactorVerify
                {
                    UserId = user!.Id,
                    LoginProvider = "Default",
                    Name = typeOfGenerate,
                    Value = hashedCode,
                    Date = DateTime.UtcNow
                };
                await _db.twoFactorVerify.AddAsync(twoFactor);
            }
            await _db.SaveChangesAsync();
            return verifyCode;
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

                using var request = new HttpRequestMessage(HttpMethod.Post, "https://Admin.takhleesak.com/api/Select-Data")
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
