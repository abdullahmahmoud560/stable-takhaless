using Application.Interface;
using firstProject.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace firstProject.DTO
{
    public class TokenService:ITokenService
    {
        private readonly UserManager<User> _userManager;

        public TokenService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public  async Task<(bool Success,string Error)> GenerateAccessToken(string Email)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__SecretKey")!));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var mappedUser = await _userManager.FindByEmailAsync(Email);
            if (mappedUser == null)
                return (false,"المستخدم غير موجود");
            var role =await _userManager.GetRolesAsync(mappedUser);
            var roles = string.Join(", ", role);

            var claims = new List<Claim>
            {
                new Claim("ID", mappedUser.Id!.ToString()),
                new Claim("Email", mappedUser.Email!),
                new Claim("fullName", mappedUser.fullName!),
                new Claim("phoneNumber", mappedUser.PhoneNumber!),
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
            return (true,new JwtSecurityTokenHandler().WriteToken(tokeOptions));
        }
        public async Task<(bool Success, string Error)> GenerateActiveToken(string Email)
        {
 
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__SecretKey")!));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var mappedUser = await _userManager.FindByEmailAsync(Email);
            if (mappedUser == null) 
                return (false,"المستخدم غير موجود");
            var role = await _userManager.GetRolesAsync(mappedUser);
            var roles = string.Join(", ", role);

            var claims = new List<Claim>
            {
                new Claim("ID", mappedUser.Id.ToString()),
            };

            var tokeOptions = new JwtSecurityToken(
                issuer: Environment.GetEnvironmentVariable("JWT__Issuer"),
                audience: Environment.GetEnvironmentVariable("JWT__Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: signinCredentials
            );
            return (true,new JwtSecurityTokenHandler().WriteToken(tokeOptions));
            
        }
    }
}
