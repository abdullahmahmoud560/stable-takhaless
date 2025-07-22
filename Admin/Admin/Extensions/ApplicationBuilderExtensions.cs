namespace Admin.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app, string corsPolicyName)
        {
            // تفعيل Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            // إجبار التحويل لـ HTTPS
            app.UseHttpsRedirection();

            // تفعيل CORS
            app.UseCors(corsPolicyName);

            // إضافة Authentication
            app.UseAuthentication();

            // إضافة Authorization
            app.UseAuthorization();

            return app;
        }
    }
} 