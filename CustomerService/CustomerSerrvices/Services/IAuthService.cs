namespace CustomerSerrvices.Services
{
    public interface IAuthService
    {
        string GenerateJwtToken(string userId, string role);
        bool ValidateToken(string token);
    }
} 