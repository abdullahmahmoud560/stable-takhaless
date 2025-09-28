using firstProject.Model;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using static Shared.DataTransferObject;

namespace Application.Interface
{
    public interface IUserService
    {
        Task<(bool Success,string Error)> CreateAsync(BrokerDTO user, string password);
        Task<List<UserDTO>> GetByPaging(int PageNumber, int PageSize, string targetRole);
        Task<List<User>?> GetByPagingCondition(Expression<Func<User, bool>> func, int PageNumber, int PageSize);
        Task<User?> FindByEmailAsync(string Email);
        Task<User?> FindByIdAsync(string Id);
        Task<IList<string>> GetRole(string Email);
        Task<(bool Success, string Error)> changeRoles(ChangeRolesDTO roles);
        Task<IdentityResult> UpdateAsync(User user);
        Task<(bool Success, string Error)> LoginUser(LoginDTO loginDTO);
        Task<(bool Success, string Error)> CheckIsFoundEmailOrPhoneOrIdentity(string Email,string Phone,string Identity);
        Task<(bool Success, string Error)> CheckIsFoundtaxRecordOrInsuranceNumber(string InsuranceNumber, string taxRecord,string license);
        Task<(bool Success, string Error)> ResetPassword(ResetPasswordDTO reset, string Id);
        Task<(bool Success, string Error)> VerifyCode(VerifyCode verifyCode, string Email);
        Task<(bool Success, string Error)> ActiveEmail(string Email);
        Task<(bool Success, string Error)> Blocked(string Email);
        Task<(bool Success, string Error)> UnBlocked(string Email);
        Task<(bool Success, string Message, IEnumerable<string>? AddedClaims)> SetPermissionsAsync(RolesDTO rolesDTO);
        Task<(bool Success, string Message, IEnumerable<string>? Claims)> GetPermissionsAsync(string userId);
        Task<(bool Success, string Message, IEnumerable<string>? RemainingClaims)> DeletePermissionsAsync(RolesDTO rolesDTO);
        Task<int> GetUserCountByRole(string role);
    }
}
