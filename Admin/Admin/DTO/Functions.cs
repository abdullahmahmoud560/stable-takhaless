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
            _httpClient = client ?? throw new ArgumentNullException(nameof(client));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<ApiResult<JsonElement>> SendAPI(string id)
        {
            try
            {
                var token = GetTokenFromCookies();
                if (!token.IsSuccess)
                {
                    return ApiResult<JsonElement>.Failure(token.ErrorMessage ?? "Unknown error");
                }

                SetAuthorizationHeader(token.Data!);

                var requestData = new { ID = id };
                var jsonContent = CreateJsonContent(requestData);

                var response = await _httpClient.PostAsync($"{Helpers.Constants.API_BASE_URL}/Select-Data", jsonContent);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                var jsonElement = JsonDocument.Parse(responseString).RootElement;

                return ApiResult<JsonElement>.Success(jsonElement);
            }
            catch (HttpRequestException httpEx)
            {
                return ApiResult<JsonElement>.Failure($"HTTP Request Error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                return ApiResult<JsonElement>.Failure(ex.Message);
            }
        }

        private ApiResult<string> GetTokenFromCookies()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies[Helpers.Constants.TOKEN_COOKIE_NAME];
            
            if (string.IsNullOrEmpty(token))
            {
                return ApiResult<string>.Failure("Missing Token in Cookies");
            }

            return ApiResult<string>.Success(token);
        }

        private void SetAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private StringContent CreateJsonContent<T>(T data)
        {
            var jsonString = JsonSerializer.Serialize(data);
            return new StringContent(jsonString, Encoding.UTF8, "application/json");
        }
    }

    public class ApiResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? ErrorMessage { get; private set; }

        private ApiResult(bool isSuccess, T? data, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
        }

        public static ApiResult<T> Success(T data) => new(true, data, null);
        public static ApiResult<T> Failure(string errorMessage) => new(false, default, errorMessage);
    }
}
