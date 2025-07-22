using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace User.DTO
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

        // 🧩 دالة مساعدة لاستخراج التوكن من الكوكيز
        private string? GetTokenFromCookies()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies["token"];
        }

        // 🧩 دالة موحدة لإرسال POST
        private async Task<JsonElement?> SendPostRequestAsync(string url, object data, string? token, bool useCookieHeader = false)
        {
            if (string.IsNullOrEmpty(token))
            {
                return JsonDocument.Parse("{\"success\": false, \"message\": \"Missing Authorization Token\"}").RootElement;
            }

            var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = jsonContent
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (useCookieHeader)
            {
                request.Headers.Add("Cookie", $"token={token}");
            }

            try
            {
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

        // 🧩 دالة موحدة لإرسال GET
        private async Task<JsonElement?> SendGetRequestAsync(string url, string? token, bool useCookieHeader = false)
        {
            if (string.IsNullOrEmpty(token))
            {
                return JsonDocument.Parse("{\"success\": false, \"message\": \"Missing Authorization Token\"}").RootElement;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (useCookieHeader)
            {
                request.Headers.Add("Cookie", $"token={token}");
            }

            try
            {
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

        // 📌 SendAPI
        public async Task<JsonElement?> SendAPI(string ID)
        {
            string? token = GetTokenFromCookies();
            var requestData = new { ID = ID };
            return await SendPostRequestAsync("http://firstproject-service:9100/api/Select-Data", requestData, token);
        }

        // 📌 BrokerandUser
        public async Task<JsonElement?> BrokerandUser(string ID, string BrokerID)
        {
            string? token = GetTokenFromCookies();
            if (string.IsNullOrEmpty(token))
            {
                return JsonDocument.Parse("{\"success\": false, \"message\": \"Missing Authorization Token\"}").RootElement;
            }

            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            using var client = new HttpClient(handler);
            Uri apiUri = new Uri("http://firstproject-service:9100/api/Select-Broker-User");

            handler.CookieContainer.Add(apiUri, new Cookie("token", token));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestData = new { ID = ID, BrokerID = BrokerID };
            var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

            try
            {
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

        // 📌 Admin
        public async Task<JsonElement?> Admin()
        {
            string? token = GetTokenFromCookies();
            return await SendGetRequestAsync("http://firstproject-service:9100/api/Statictis", token, useCookieHeader: true);
        }

        // 📌 GetAllBroker
        public async Task<JsonElement?> GetAllBroker(int Page)
        {
            string? token = GetTokenFromCookies();
            return await SendGetRequestAsync($"http://firstproject-service:9100/api/Get-Broker/{Page}", token);
        }

        // 📌 Logs
        public async Task<JsonElement?> Logs(LogsDTO logsDTO)
        {
            string? token = GetTokenFromCookies();

            var requestData = new
            {
                message = logsDTO.Message,
                newOrderId = logsDTO.NewOrderId,
                userId = logsDTO.UserId,
                notes = logsDTO.Notes
            };

            return await SendPostRequestAsync("http://admin-service:80/api/Add-Logs", requestData, token);
        }
    }
}
