namespace Admin.Configuration
{
    public class CorsConfiguration
    {
        public string PolicyName { get; set; } = "MyCors";
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public bool AllowCredentials { get; set; } = true;
    }
} 