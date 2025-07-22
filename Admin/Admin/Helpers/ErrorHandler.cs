using Admin.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Helpers
{
    public static class ErrorHandler
    {
        public static IActionResult HandleException(Exception ex, string userMessage = "حدث خطأ، برجاء المحاولة لاحقًا")
        {
            // يمكن إضافة logging هنا
            return new ObjectResult(ApiResponse.Error(userMessage))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        public static IActionResult HandleValidationError(string message)
        {
            return new BadRequestObjectResult(ApiResponse.Error(message));
        }

        public static IActionResult HandleNotFoundError(string message = "المورد المطلوب غير موجود")
        {
            return new NotFoundObjectResult(ApiResponse.Error(message));
        }
    }
} 