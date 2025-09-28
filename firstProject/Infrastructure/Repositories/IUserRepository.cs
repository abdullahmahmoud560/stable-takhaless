using firstProject.Model;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using static Shared.DataTransferObject;

namespace Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<(bool Success,string Error)> CreateAsync(User user, string password);
        Task<IdentityResult> UpdateAsync(User user);
        Task<List<User>?> GetByPaging(int PageNumber,int PageSize);
        Task<List<User>?> GetByPagingCondition(Expression<Func<User, bool>> func, int PageNumber,int PageSize);
        Task<IList<string>> GetRole(User user);
        Task<User?> FindByEmailAsync(string Email);
        Task<User?> FindByIdAsync(string Id);
        Task<(bool Success, string Error)> changeRoles(ChangeRolesDTO roles);
    }
}
