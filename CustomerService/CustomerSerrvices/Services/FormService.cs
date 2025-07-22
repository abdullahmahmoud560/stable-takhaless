using CustomerSerrvices.ApplicationDbContext;
using CustomerSerrvices.DTO;
using CustomerSerrvices.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerSerrvices.Services
{
    public class FormService : IFormService
    {
        private readonly DB _dbContext;

        public FormService(DB dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse> SubmitFormAsync(FormDTO formDTO)
        {
            try
            {
                var form = new Form
                {
                    Email = formDTO.Email,
                    fullName = formDTO.fullName,
                    Message = formDTO.Message,
                    phoneNumber = formDTO.phoneNumber,
                    createdAt = DateTime.UtcNow
                };

                _dbContext.forms.Add(form);
                await _dbContext.SaveChangesAsync();

                return new ApiResponse 
                { 
                    Message = "تم إرسال الطلب بنجاح",
                    State = "Success"
                };
            }
            catch (Exception)
            {
                return new ApiResponse 
                { 
                    Message = "حدث خطأ أثناء حفظ النموذج",
                    State = "Error"
                };
            }
        }

        public async Task<object> GetFormsAsync(int page, int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                throw new ArgumentException("معلمات الصفحة غير صحيحة");
            }

            var totalCount = await _dbContext.forms.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var forms = await _dbContext.forms
                .OrderByDescending(f => f.createdAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Data = forms
            };
        }
    }
} 