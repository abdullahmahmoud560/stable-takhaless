using Microsoft.AspNetCore.Identity;

namespace firstProject.Model
{
    public class TwoFactorVerify : IdentityUserToken<string>
    {
        public  DateTime Date { get; set; } 
    }
}
