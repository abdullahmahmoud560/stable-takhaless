using firstProject.Model;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Security.Claims;
using static Shared.DataTransferObject;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public AuthenticationMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }

    public async Task Invoke(HttpContext context)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var Email = context.User.FindFirstValue("Email");
                
            if (string.IsNullOrEmpty(Email))
            {
                await _next(context);
                return;
            }
                
            var user = await userManager.FindByEmailAsync(Email);

            if (user == null)
            {
                var response = new ApiResponse {Message = "التوكن غير صالح"};
                await context.Response.WriteAsJsonAsync(response);
                return;
            }
            if (user.isBlocked == true || user.isActive == false)
            {
                CookieHelper.RemoveTokenCookie(context.Response);

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var response = new ApiResponse
                {
                    Message = "الحساب محظور نهائياً يرجى التواصل مع خدمة العملاء"
                };
                await context.Response.WriteAsJsonAsync(response);
                return;
            }
            await _next(context);
        }
    }

}