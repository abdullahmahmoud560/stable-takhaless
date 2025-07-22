namespace CustomerSerrvices.DTO
{
    public class ApiResponse<T>
    {
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string State { get; set; } = "Success";
        public bool IsSuccess => State == "Success";

        public static ApiResponse<T> Success(T data, string message = "تمت العملية بنجاح")
        {
            return new ApiResponse<T>
            {
                Data = data,
                Message = message,
                State = "Success"
            };
        }

        public static ApiResponse<T> Error(string message, T? data = default)
        {
            return new ApiResponse<T>
            {
                Data = data,
                Message = message,
                State = "Error"
            };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Success(string message = "تمت العملية بنجاح")
        {
            return new ApiResponse
            {
                Message = message,
                State = "Success"
            };
        }

        public static ApiResponse Error(string message)
        {
            return new ApiResponse
            {
                Message = message,
                State = "Error"
            };
        }
    }
}
