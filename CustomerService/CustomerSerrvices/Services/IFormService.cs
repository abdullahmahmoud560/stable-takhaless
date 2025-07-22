using CustomerSerrvices.DTO;
using CustomerSerrvices.Models;

namespace CustomerSerrvices.Services
{
    public interface IFormService
    {
        Task<ApiResponse> SubmitFormAsync(FormDTO formDTO);
        Task<object> GetFormsAsync(int page, int pageSize = 10);
    }
} 