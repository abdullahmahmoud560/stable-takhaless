namespace User.DTO
{
    public class NotesFromCustomerServiceDTO
    {
        public int? newOrderId {  get; set; }
        public string? Notes { get; set; }
        public IFormFile? formFile {  get; set; }  
    }
}
