using System.ComponentModel.DataAnnotations;

namespace User.Model
{
    public class NotesCustomerService
    {
        public int Id { get; set; }
        public string? Notes { get; set; }
        public string? fileName { get; set; }
        public Byte[]? fileData { get; set; }
        public string? ContentType { get; set; }
        public NewOrder? newOrder { get; set; }
        public int? newOrderId { get; set; }
    }
}
