using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Utilities;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using User.Model;

namespace User.DTO
{
    public class Functions
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiUrl = "https://firstproject.takhleesak.com/api/";
        public Functions(HttpClient client, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = client;
            _httpContextAccessor = httpContextAccessor;
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
                Console.WriteLine(_apiUrl + "Select-Data");
                using var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl + "Select-Data")
                {
                    Content = jsonContent
                };

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
        public async Task<JsonElement?> BrokerandUser(string ID, string BrokerID)
        {
            try
            {
                // ✅ الحصول على التوكن من الكوكيز
                string? token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];

                if (string.IsNullOrEmpty(token))
                {
                    return JsonDocument.Parse("{\"success\": false, \"message\": \"Missing Authorization Token\"}").RootElement;
                }

                // ✅ إعداد HttpClientHandler لتمكين إرسال الكوكيز مع الطلب
                var handler = new HttpClientHandler
                {
                    UseCookies = true, // ✅ تفعيل استخدام الكوكيز
                    CookieContainer = new CookieContainer() // ✅ إنشاء حاوية كوكيز
                };

                using var client = new HttpClient(handler);

                // ✅ ضبط التوكن في الهيدرز
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // ✅ إضافة الكوكيز يدويًا في الطلب
                Uri apiUri = new Uri(_apiUrl + "Select-Broker-User");
                handler.CookieContainer.Add(apiUri, new Cookie("token", token));

                var requestData = new { ID = ID, BrokerID = BrokerID };
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUri, jsonContent);

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

        public async Task<JsonElement?> Admin()
        {
            try
            {
                string? token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];

                if (string.IsNullOrEmpty(token))
                {
                    return JsonDocument.Parse("{\"success\": false, \"message\": \"Missing Authorization Token\"}").RootElement;
                }

                // إضافة التوكن إلى ملفات تعريف الارتباط الخاصة بالطلب
                _httpClient.DefaultRequestHeaders.Add("Cookie", $"token={token}");

                HttpResponseMessage response = await _httpClient.GetAsync(_apiUrl + "Statictis");

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

        public async Task<JsonElement?> GetAllBroker()
        {
            try
            {
                // ✅ الحصول على التوكن من الكوكي
                string? token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];

                if (string.IsNullOrEmpty(token))
                {
                    return JsonDocument.Parse("{\"success\": false, \"message\": \"Missing token in cookie\"}").RootElement;
                }

                // ✅ إرسال التوكن في الهيدر
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // ✅ طلب الـ API
                HttpResponseMessage response = await _httpClient.GetAsync(_apiUrl + "Get-Broker");
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

        public async Task<JsonElement?> Logs(LogsDTO logsDTO)
        {
            try
            {
                string? token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJJRCI6IjE0MDZkYTIzLTA5MjgtNGYwNC04ODA5LTU2NWRlNDFlZmYyNCIsIkVtYWlsIjoieS5hYmJnaGFnaEBnbWFpbC5jb20iLCJmdWxsTmFtZSI6ImFiZHVsbGFoIG1haG1vdWQiLCJwaG9uZU51bWJlciI6IisyMTA2OTEzODkyMyIsImp0aSI6ImQ4YzRhYzAzLTM5YTEtNDBiYy1iNDA2LTIyMmI2ZTM0MzdkNiIsIlJvbGUiOiJVc2VyIiwiZXhwIjoxNzUxODAwNDI2LCJpc3MiOiJodHRwczovL2ZpcnN0cHJvamVjdC50YWtobGVlc2FrLmNvbSIsImF1ZCI6Imh0dHBzOi8vZmlyc3Rwcm9qZWN0LnRha2hsZWVzYWsuY29tIn0.dFQtxR7o0bKVRFrIB8t4gBX6OXXh2lIPpa4u37MSC3E";

                if (string.IsNullOrEmpty(token))
                {
                    return JsonDocument.Parse("{\"success\": false, \"message\": \"Missing Authorization Token\"}").RootElement;
                }

                var requestData = new
                {
                    message = logsDTO.Message,
                    newOrderId = logsDTO.NewOrderId,
                    userId = logsDTO.UserId,
                    notes = logsDTO.Notes
                };
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
                Console.WriteLine(_apiUrl + "Select-Data");
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://admin.takhleesak.com/api/Add-Logs")
                {
                    Content = jsonContent
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

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