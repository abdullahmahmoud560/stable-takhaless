namespace User.DTO
{
    public class GetDetailsOfOrders : NumberOfOrdersDTO
    {
        public int Id { get; set; }
        public string? Date { get; set; }
        public string? Location { get; set; }
        public string? numberOflicense { get; set; }
        public string[]? fileName { get; set; }
        public string? Notes { get; set; }
        public string? City { get; set; }
        public string? Town { get; set; }
        public string? zipCode { get; set; }
        public string[]? fileUrl { get; set; }
    }
}
