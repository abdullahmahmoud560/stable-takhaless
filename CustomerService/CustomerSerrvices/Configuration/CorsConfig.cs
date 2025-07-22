namespace CustomerSerrvices.Configuration
{
    public class CorsConfig
    {
        public string PolicyName { get; set; } = "MyCors";
        public string[] AllowedOrigins { get; set; } = new[] { "https://test.takhleesak.com" };
        public bool AllowCredentials { get; set; } = true;
        public string[] AllowedHeaders { get; set; } = { "*" };
        public string[] AllowedMethods { get; set; } = { "*" };
    }
} 