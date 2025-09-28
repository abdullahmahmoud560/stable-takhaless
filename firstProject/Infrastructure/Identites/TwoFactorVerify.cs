using System.ComponentModel.DataAnnotations.Schema;

namespace firstProject.Model
{
    public class TwoFactorVerify
    {
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string TypeOfGenerate { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime PeriodStartTime { get; set; }
        public int Attempts { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}
