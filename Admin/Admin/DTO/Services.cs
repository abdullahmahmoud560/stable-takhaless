using System.Text.Json;

namespace Admin.DTO
{
    public interface ILogService
    {
        Task<ApiResult<(string fullName, string email)>> GetUserInfoAsync(string userId);
        Task<PaginatedResponse<LogsDTO>> GetPaginatedLogsAsync(int newOrderId, int page, int pageSize);
    }

    public class LogService : ILogService
    {
        private readonly Functions _functions;

        public LogService(Functions functions)
        {
            _functions = functions ?? throw new ArgumentNullException(nameof(functions));
        }

        public async Task<ApiResult<(string fullName, string email)>> GetUserInfoAsync(string userId)
        {
            if (!Helpers.Validation.IsValidId(userId))
            {
                return ApiResult<(string, string)>.Failure("Invalid user ID");
            }

            var response = await _functions.SendAPI(userId);

            if (!response.IsSuccess)
            {
                return ApiResult<(string, string)>.Failure(response.ErrorMessage ?? "Failed to get user info");
            }

            if (response.Data.TryGetProperty("fullName", out JsonElement fullName) &&
                response.Data.TryGetProperty("email", out JsonElement email))
            {
                return ApiResult<(string, string)>.Success((fullName.GetString() ?? "", email.GetString() ?? ""));
            }

            return ApiResult<(string, string)>.Failure("Invalid user data format");
        }

        public async Task<PaginatedResponse<LogsDTO>> GetPaginatedLogsAsync(int newOrderId, int page, int pageSize)
        {
            // This method would be implemented in the controller since it needs access to the database context
            throw new NotImplementedException("This method should be implemented in the controller");
        }
    }
} 