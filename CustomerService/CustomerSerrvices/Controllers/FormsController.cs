using CustomerSerrvices.ApplicationDbContext;
using CustomerSerrvices.DTO;
using CustomerSerrvices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerSerrvices.Controllers
{
    [Route("api/")]
    [ApiController]

    public class FormsController : ControllerBase
    {
        private readonly DB _dbContext;
        public FormsController(DB dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("Form")]
        public async Task<IActionResult> SubmitFrom(FormDTO formDTO)
        {

            if (!ModelState.IsValid)
            {
                return Ok(new ApiResponse { Message = "البيانات غير صحيحة" });
            }
            else
            {
                Form form = new Form
                {
                    Email = formDTO.Email,
                    fullName = formDTO.fullName,
                    Message = formDTO.Message,
                    phoneNumber = formDTO.phoneNumber,
                    createdAt = DateTime.Now,
                };
                _dbContext.Add(form);
                await _dbContext.SaveChangesAsync();
                return Ok(new ApiResponse { Message = "تم إرسال الطلب بنجاح" });
            }
        }


        [Authorize(Roles = ("Admin,CustomerService"))]
        [HttpGet("Get-Form/{Page}")]
        public async Task<IActionResult> getForm(int page)
        {
            try
            {
                const int pageSize = 10;
                if (page <= 0 || pageSize <= 0)
                {
                    return BadRequest(new ApiResponse { Message = "معلمات الصفحة غير صحيحة" });
                }

                var totalCount = await _dbContext.forms.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var forms = await _dbContext.forms
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    Data = forms
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ، برجاء المحاولة لاحقًا" });
            }
        }

    }
}