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


        [Authorize(Roles =("Admin,CustomerService"))]
        [HttpGet("Get-Form")]
        public async Task<IActionResult> getForm()
        {
            if (!ModelState.IsValid)
            {
                return Ok(new ApiResponse { Message = "البيانات غير صحيحة" });
            }
            else
            {
               var listOfForm = await _dbContext.forms.ToListAsync();
                return Ok(listOfForm);
            }
        }
    }
}