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
            if (string.IsNullOrEmpty(formDTO.fullName) ||
               string.IsNullOrWhiteSpace(formDTO.Message) ||
               string.IsNullOrEmpty(formDTO.Email) ||
               string.IsNullOrEmpty(formDTO.phoneNumber)) { return BadRequest("برجاء إدجاء جميع الحقول المطلوبة"); }

            formDTO.fullName = StripHtml(formDTO.fullName!);
            formDTO.Message = StripHtml(formDTO.Message!);
            formDTO.Email = StripHtml(formDTO.Email!);
            formDTO.phoneNumber = StripHtml(formDTO.phoneNumber!);

            var result = await _formService.SubmitFormAsync(formDTO);
            return Ok(result);
        }

        private string StripHtml(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
        }


        [Authorize(Roles = "Admin,CustomerService")]
        [HttpGet("Get-Form/{page}")]
        public async Task<IActionResult> GetForms(int page)
        {

            const int pageSize = 10;
            var result = await _formService.GetFormsAsync(page, pageSize);
            return Ok(result);
        }
    }
}