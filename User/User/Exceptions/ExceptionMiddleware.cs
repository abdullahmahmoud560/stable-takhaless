using System.Net;
using System.Text.Json;

namespace firstProject.Exceptions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment()
                    ? new ErrorDetails
                    {
                        Status = 500,
                        Message = ex.Message,
                        Details = ex.StackTrace
                    }
                    : new ErrorDetails
                    {
                        Status = 500,
                        Message = "حدث خطأ غير متوقع، يرجى المحاولة لاحقًا."
                    };

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                var json = JsonSerializer.Serialize(response, options);

                await httpContext.Response.WriteAsync(json);
            }
        }
        public class ErrorDetails
        {
            public int Status { get; set; }
            public string Message { get; set; }
            public string? Details { get; set; }
        }
    }
}
