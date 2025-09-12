using CustomerSerrvices.ApplicationDbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using User.DTO;
using User.Model;

namespace User.Controllers
{
    [Route("api/")]
    [ApiController]
    public class SaberCertificationController : ControllerBase
    {
        private readonly DB _db;
        private readonly IWebHostEnvironment _env;


        public SaberCertificationController(DB db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [Authorize]
        [HttpPost("Add-Saber-Certificates")]
        public async Task<IActionResult> AddSaberCertificate([FromForm] SaberCertificationDTO saber)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("ID")?.Value;

            // 🖼 رفع الصور
            string productImageUrl = await SaveFile(saber.ProductImage, "Products");
            string crImageUrl = await SaveFile(saber.CRImage, "CR");

            // 📝 إنشاء الكيان
            var saberCertificate = new saberCertificate
            {
                HSCode = saber.HSCode,
                ComanyName = saber.ComanyName,
                ProductName = saber.ProductName,
                PhoneNumber = saber.PhoneNumber,
                Email = saber.Email,
                Description = saber.Description,
                UserId = userId!,
                ProductImage = productImageUrl,
                CRImage = crImageUrl
            };

            await _db.saberCertificates.AddAsync(saberCertificate);
            await _db.SaveChangesAsync();

            return Ok(new { message = "تم إضافة شهادة سابر بنجاح"});
        }

        private async Task<string> SaveFile(IFormFile file, string folderName)
        {
            string uploadsPath = Path.Combine(_env.WebRootPath, "Saber", folderName);

            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string fileUrl = $"{Request.Scheme}://{Request.Host}/Saber/{folderName}/{fileName}";
            return fileUrl;
        }


        [Authorize]
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

        [Authorize]
        [HttpGet("Get-Saber-CertificatesById/{pageNumber}")]
        public async Task<IActionResult> GetSaberCertificatesById(int pageNumber)
        {
            var UserId = User.FindFirstValue("ID");
            const int pageSize = 10; 

            var totalRecords = await _db.saberCertificates.CountAsync();

            var saberCertificates = await _db.saberCertificates
                .OrderBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize).Where(x => x.UserId == UserId)
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
