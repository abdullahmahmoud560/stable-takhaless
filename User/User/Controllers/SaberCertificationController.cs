using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User.ApplicationDbContext;
using User.DTO;
using User.Model;

namespace User.Controllers
{
    [Route("api/")]
    [ApiController]
    public class SaberCertificationController : ControllerBase
    {
        private readonly DB _db;

        public SaberCertificationController(DB db)
        {
            _db = db;
        }

        [Authorize(Roles ="User")]
        [HttpPost("Add-Saber-Certificates")]
        public async Task<IActionResult> AddSaberCertificate(SaberCertificationDTO saber)
        {
            if (string.IsNullOrEmpty(saber.Subject) || string.IsNullOrEmpty(saber.Description))
            {
                return BadRequest("بجاء ملئ الحقول المطلوبة");
            }
            var UserId = User.FindFirst("ID")?.Value;
            saberCertificate saberCertificates = new saberCertificate
            {
                Subject = saber.Subject,
                Description = saber.Description,
                UserId = UserId!
            };
            await _db.saberCertificates.AddAsync(saberCertificates);
            await _db.SaveChangesAsync();
            return Ok(new { message = "تم اضافة شهادة سابر بنجاح" });
        }

        [Authorize(Roles ="Saber, Admin")]
        [HttpGet("Get-Saber-Certificates/{pageNumber}")]
        public async Task<IActionResult> GetSaberCertificates(int pageNumber)
        {
            const int pageSize = 10; 

            var totalRecords = await _db.saberCertificates.CountAsync();

            var saberCertificates = await _db.saberCertificates
                .OrderBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = saberCertificates
            });
        }



    }
}
