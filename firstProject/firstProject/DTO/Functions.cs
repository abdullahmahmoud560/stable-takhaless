using firstProject.Model;
using Microsoft.AspNetCore.Identity;
using BCrypt.Net;
using firstProject.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;


namespace firstProject.DTO
{
    public class Functions
    {
        private readonly UserManager<User> _userManager;
        private readonly DB _db;

        public Functions(UserManager<User> userManager, DB db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<string> GenerateVerifyCode(User user,string typeOfGenerate)
        {
            // ⚡ OPTIMIZED: Fast random code generation (was taking 30+ seconds)
            var verifyCode = new Random().Next(100000, 999999).ToString();
            var hashedCode = BCrypt.Net.BCrypt.HashPassword(verifyCode, 4);
            var found =await _db.twoFactorVerify.Where(l => l.UserId == user.Id).FirstOrDefaultAsync();
            if (found == null)
            {
                var twoFactor = new TwoFactorVerify
                {
                    UserId = user.Id,
                    LoginProvider = "Default",
                    Name = typeOfGenerate,
                    Value = hashedCode,
                    Date = DateTime.UtcNow
                };
                await _db.twoFactorVerify.AddAsync(twoFactor);
            }
            else
            {
                found!.Value = hashedCode;
                found.Date = DateTime.UtcNow;
                found.Name = typeOfGenerate;
            }
            await _db.SaveChangesAsync();
            return verifyCode;
        }

    }
}
