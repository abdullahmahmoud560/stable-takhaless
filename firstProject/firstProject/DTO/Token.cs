using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using firstProject.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace firstProject.DTO
{
    public class Token
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly ILogger _logger; 


        public Token(IConfiguration configuration, UserManager<User> userManager,ILogger logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _logger = logger;
        }

        public  async Task<string> GenerateToken(User user)
        {
            try
            {
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]!));
                var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                var role =await _userManager.GetRolesAsync(user);
                var roles = string.Join(", ", role);

                var claims = new List<Claim>
                {
                    new Claim("ID", user.Id.ToString()),
                    new Claim("Email", user.Email!),
                    new Claim("fullName", user.fullName!),
                    new Claim("phoneNumber", user.PhoneNumber!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("Role",roles)
                };

                var tokeOptions = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: signinCredentials
                );
                return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء توليد التوكن");
                throw new SecurityTokenException("فشل في توليد التوكن");
            }
        }
    }
}
