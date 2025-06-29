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
        private readonly UserManager<User> _userManager;


        public Token(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public  async Task<string> GenerateToken(User user)
        {
            try
            {
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__SecretKey")!));
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
                    issuer: Environment.GetEnvironmentVariable("JWT__Issuer"),
                    audience: Environment.GetEnvironmentVariable("JWT__Audience"),
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: signinCredentials
                );
                return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            }
            catch (Exception)
            {
                throw new SecurityTokenException("فشل في توليد التوكن");
            }
        }
    }
}
