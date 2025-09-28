using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services
{
    public static class CookieHelper
    {
        
        public static void SetTokenCookie(HttpResponse response, string token, int expiresInMinutes = 30)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes),
                Domain = ".runasp.net",
            };

            response.Cookies.Append("token", token, cookieOptions);
        }

        public static void SetTokenCookieInDays(HttpResponse response, string token, int expiresInDays = 14)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(expiresInDays),
                Domain = ".runasp.net",
            };

            response.Cookies.Append("token", token, cookieOptions);
        }

        public static void RemoveTokenCookie(HttpResponse response)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1),
                Domain = ".runasp.net",
            };

            response.Cookies.Append("token", "", cookieOptions);
        }
    }
}
