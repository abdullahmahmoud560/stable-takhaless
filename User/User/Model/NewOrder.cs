using System.ComponentModel.DataAnnotations;

namespace User.Model
{
    public class NewOrder
    {
        public int Id { get; set; }
        [Required]
        public string? Location { get; set; }
        [Required]
        public DateTime? Date { get; set; }
        [Required]
        public string? UserId { get; set; }
        public ICollection<UploadFile>? uploadFiles { get; set; }
        public ICollection<NumberOfTypeOrder>? numberOfTypeOrders { get; set; }
        public ICollection<Values>? values { get; set; }
        public ICollection<NotesCustomerService>? NotesCustomerServices  { get; set; }
        public ICollection<NotesAccounting>? notesAccountings  { get; set; }
        [Required]
        public string? statuOrder { get; set; }
        [Required]
        public string? numberOfLicense { get; set; }
        public string? Accept { get; set; }
        public string? AcceptCustomerService { get; set; }

        public string? AcceptAccount { get; set; }
        public string? Notes { get; set; }
        public string? City { get; set; }
        public string? Town { get; set; }
        public string? zipCode { get; set; }
        public string? step1 { get; set; }
        public string? step2 { get; set; }
        public string? step3 { get; set; }
        public string? JopID { get; set; }
    }
}
