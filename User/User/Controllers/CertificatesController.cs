using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User.ApplicationDbContext;
using User.DTO;
using User.Model;

namespace User.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificatesController : ControllerBase
    {
        private readonly DB _db;

        public CertificatesController(DB db)
        {
            _db = db;
        }
        [Authorize(Roles ="User")]
        [HttpPost("Add-Saber-Certificates")]
        public async Task<IActionResult> AddSaberCertificates(SaberCertificatesDTO certificates)
        {
            var UserId = int.Parse(User.FindFirst("ID")!.Value);
            if (string.IsNullOrWhiteSpace(certificates.Subject) || string.IsNullOrWhiteSpace(certificates.Description))
            {
                return Ok("الرجاء ملء جميع الحقول");
            }
            SaberCertificates saberCertificates = new SaberCertificates
            {
                Subject = certificates.Subject,
                Description = certificates.Description,
                UserId = UserId
            };
            await _db.saberCertificates.AddAsync(saberCertificates);
            await _db.SaveChangesAsync();
            return Ok(new { message = "تم إضافة شهادة سابر بنجاح" });
        }

        [Authorize(Roles = "Saber")]
        [HttpGet("Get-Saber-Certificates")]
        public async Task<IActionResult> GetSaberCertificates()
        {
            var UserId = int.Parse(User.FindFirst("ID")!.Value);
            var certificates = await _db.saberCertificates
                .Where(c => c.UserId == UserId)
                .Select(c => new 
                {
                    Id = c.Id,
                    Subject = c.Subject,
                    Description = c.Description,
                    UserId = c.UserId
                }).ToListAsync();
            return Ok(certificates);
        }


    }
}
