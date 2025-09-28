using System.Text.Json;

namespace Application.Interface
{
    public interface IFunctionService
    {
        public Task<string> GenerateVerifyCode(string Email, string typeOfGenerate);
        Task<JsonElement?> SendAPI(string ID);
    }
}
