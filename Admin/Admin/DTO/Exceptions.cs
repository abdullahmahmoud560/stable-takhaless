namespace Admin.DTO
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public ApiException(string message, int statusCode = 500) : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string userId) : base($"User with ID {userId} not found") { }
    }

    public class TokenNotFoundException : Exception
    {
        public TokenNotFoundException() : base("Authentication token not found") { }
    }
} 