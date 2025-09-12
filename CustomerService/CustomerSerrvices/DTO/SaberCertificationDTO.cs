namespace User.DTO
{
    public class SaberCertificationDTO
    {
        public string ComanyName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public IFormFile ProductImage { get; set; } = null!;
        public IFormFile CRImage { get; set; } = null!;
        public string? HSCode { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
