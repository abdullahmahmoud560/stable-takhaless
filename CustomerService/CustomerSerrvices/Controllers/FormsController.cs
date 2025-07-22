using CustomerSerrvices.DTO;
using CustomerSerrvices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSerrvices.Controllers
{
    [Route("api/")]
    [ApiController]
    public class FormsController : ControllerBase
    {
        private readonly IFormService _formService;

        public FormsController(IFormService formService)
        {
            _formService = formService;
        }

        [HttpPost("Form")]
        public async Task<IActionResult> SubmitForm(FormDTO formDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse 
                { 
                    Message = "البيانات غير صحيحة",
                    State = "Error"
                });
            }

            var result = await _formService.SubmitFormAsync(formDTO);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,CustomerService")]
        [HttpGet("Get-Form/{page}")]
        public async Task<IActionResult> GetForms(int page)
        {
            try
            {
                const int pageSize = 10;
                var result = await _formService.GetFormsAsync(page, pageSize);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse 
                { 
                    Message = ex.Message,
                    State = "Error"
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiResponse 
                    { 
                        Message = "حدث خطأ، برجاء المحاولة لاحقًا",
                        State = "Error"
                    });
            }
        }
    }
}