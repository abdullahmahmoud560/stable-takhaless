namespace User.Model
{
    public class SaberCertificates
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int UserId { get; set; } 
    }
}
