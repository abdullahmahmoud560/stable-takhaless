using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace User.DTO
{
    public class Middleware
    {
        private readonly HttpClient _httpClient;

        public Middleware(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task Invoke(HttpContext context)
        {
            // خذ التوكن من الكوكيز بدل الهيدر
            string token = context.Request.Cookies["token"]!;

            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsJsonAsync(new ApiResponse
                {
                    Message = "لم يتم العثور على التوكن"
                });
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await _httpClient.GetAsync("https://firstproject.takhleesak.com/api/Checker");
            string responseBody = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseBody);
            string message = doc.RootElement.GetProperty("message").GetString() ?? "رسالة غير متوفرة";

            var responseApi = new ApiResponse
            {
                Message = message,
            };

            await context.Response.WriteAsJsonAsync(responseApi);
        }
    }
}
