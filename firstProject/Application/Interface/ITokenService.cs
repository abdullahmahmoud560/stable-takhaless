namespace Application.Interface
{
    public interface ITokenService
    {
        public Task<(bool Success, string Error)> GenerateAccessToken(string Email);
        public Task<(bool Success, string Error)> GenerateActiveToken(string Email);
    }
}
