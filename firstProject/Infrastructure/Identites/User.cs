using Microsoft.AspNetCore.Identity;

namespace firstProject.Model
{
    public class User : IdentityUser<int>
    {
        public string fullName { get; set; } = string.Empty;
        public string Identity { get; set; } =string.Empty;
        public string? taxRecord { get; set; }
        public string? InsuranceNumber { get; set; }
        public string? license { get; set; }
        public bool isBlocked { get; set; } = false;
        public bool isActive { get; set; } = false;
        public DateTime lastLogin { get; set; }
        public override string UserName { get; set; } = Guid.NewGuid().ToString();
    }

}
