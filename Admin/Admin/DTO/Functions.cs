using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace Admin.DTO
{
    public class Functions
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Functions(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = client;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<JsonElement?> SendAPI(string ID)
        {
            try
            {
                // ✅ قراءة التوكن من الكوكي بدل الهيدر
                string? token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];

                if (string.IsNullOrEmpty(token))
                {
                    return JsonDocument.Parse("{\"success\": false, \"message\": \"Missing Token in Cookies\"}").RootElement;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var requestData = new { ID = ID };
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("http://firstproject-service:9100/api/Select-Data", jsonContent);

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
