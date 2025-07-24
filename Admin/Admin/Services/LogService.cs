using Admin.DTO;

namespace Admin.Services
{
    public class LogService : ILogService
    {
        private readonly Functions _functions;

        public LogService(Functions functions)
        {
            _functions = functions ?? throw new ArgumentNullException(nameof(functions));
        }

        public async Task<ApiResult<(string fullName, string email)>> GetUserInfoAsync(string userId)
        {
            try
            {
                var result = await _functions.SendAPI(userId);
                if (!result.IsSuccess)
                {
                    return ApiResult<(string fullName, string email)>.Failure(result.ErrorMessage ?? "فشل في جلب معلومات المستخدم");
                }

                var userData = result.Data;
                var fullName = userData.GetProperty("fullName").GetString() ?? string.Empty;
                var email = userData.GetProperty("email").GetString() ?? string.Empty;

                return ApiResult<(string fullName, string email)>.Success((fullName, email));
            }
            catch (Exception ex)
            {
                return ApiResult<(string fullName, string email)>.Failure($"خطأ في جلب معلومات المستخدم: {ex.Message}");
            }
        }
    }
} 