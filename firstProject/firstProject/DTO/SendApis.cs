using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

public class SendApis
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SendApis(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<JsonElement?> SendAPI(string brokerId)
    {
        try
        {
            // ✅ التحقق من صحة المدخلات
            if (string.IsNullOrEmpty(brokerId))
            {
                return JsonDocument.Parse("{\"success\": false, \"message\": \"BrokerID مفقود\"}").RootElement;
            }

            // ✅ استخراج التوكن من الكوكيز
            string? token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];
            if (string.IsNullOrEmpty(token))
            {
                return JsonDocument.Parse("{\"success\": false, \"message\": \"Missing Authorization Token\"}").RootElement;
            }

            // ✅ إعداد بيانات الطلب
            var requestData = new { BrokerID = brokerId };
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://user.runasp.net/api/Get-Count-Of-Orders-From-Active-Users")
            {
                Content = jsonContent
            };

            // ✅ إضافة التوكن للمصادقة
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // ✅ إرسال الطلب
            var response = await _httpClient.SendAsync(request);

            // ✅ التحقق من حالة الاستجابة
            if (!response.IsSuccessStatusCode)
            {
                var errorResponseString = await response.Content.ReadAsStringAsync();
                return JsonDocument.Parse($"{{\"success\": false, \"message\": \"API Error: {errorResponseString}\"}}").RootElement;
            }

            // ✅ قراءة الاستجابة الناجحة
            var successResponseString = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(successResponseString).RootElement;
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