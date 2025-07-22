using Admin.DTO;

namespace Admin.Services
{
    public interface ILogService
    {
        Task<ApiResult<(string fullName, string email)>> GetUserInfoAsync(string userId);
    }
} 