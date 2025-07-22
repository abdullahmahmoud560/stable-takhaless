namespace Admin.DTO
{
    public class ApiResponse
    {
        public string? Message { get; set; }
        public string? Data { get; set; }
        public string? Status { get; set; }

        public static ApiResponse Success(string message, string? data = null)
        {
            return new ApiResponse
            {
                Message = message,
                Data = data,
                Status = "success"
            };
        }

        public static ApiResponse Error(string message)
        {
            return new ApiResponse
            {
                Message = message,
                Status = "error"
            };
        }
    }

    public class PaginatedResponse<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<T> Data { get; set; } = new();

        public static PaginatedResponse<T> Create(int page, int pageSize, int totalCount, List<T> data)
        {
            return new PaginatedResponse<T>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Data = data
            };
        }
    }
}
