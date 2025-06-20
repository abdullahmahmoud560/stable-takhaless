using System.ComponentModel.DataAnnotations;

namespace User.Model
{
    public class NumberOfTypeOrder
    {
        public int Id { get; set; }
        [Required]
        [RegularExpression(@"^[\p{L}\s]+$")]
        public string? typeOrder { get; set; }
        [RegularExpression(@"^[0-9\u0660-\u0669]+$")]
        public int? Number { get; set; }
        [RegularExpression(@"^-?[0-9\u0660-\u0669]+([.,][0-9\u0660-\u0669]+)?$")]
        public float? Weight { get; set; }

        public string? Size { get; set; }
       // public NewOrder? newOrder { get; set; }
        [Required]

        public int? newOrderId { get; set; }

    }
}
