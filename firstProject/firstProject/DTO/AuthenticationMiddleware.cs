using System.Net;
using System.Security.Claims;
using firstProject.DTO;
using firstProject.Model;
using Microsoft.AspNetCore.Identity;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            try
            {
                var ID = context.User.FindFirstValue("ID");
                var user = await userManager.FindByIdAsync(ID!);
                
                if (user!.isBlocked == true && user.isActive == true)
                {
                    // احذف التوكن من الكوكيز
                    context.Response.Cookies.Append("token", "", new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(-1),
                        Domain = ".runasp.net",
                        Secure = true,
                        HttpOnly = true,
                        SameSite = SameSiteMode.None
                    });

                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var response = new ApiResponse
                    {
                        Message = "الحساب محظور نهائياً يرجى التواصل مع خدمة العملاء"
                    };
                    _logger.LogWarning("الحساب محظور: {UserId}", ID);
                    await context.Response.WriteAsJsonAsync(response);
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.Cookies.Delete("token");

                _logger.LogError(ex, "حدث خطأ أثناء عملية المصادقة");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var response = new ApiResponse
                {
                    Message = "حدث خطأ أثناء عملية المصادقة. يرجى المحاولة لاحقًا"+ex.Message,
                };
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }

}