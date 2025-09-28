using firstProject.ApplicationDbContext;
using firstProject.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;
using static Shared.DataTransferObject;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly DB _Context;

        public UserRepository(UserManager<User> userManager,DB Context)
        {
            _userManager = userManager;
            _Context = Context;
        }

        public async Task<(bool Success,string Error)> CreateAsync(User user, string password)
        {
             var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                return (true,string.Empty);
            }
            return (false,result.Errors.FirstOrDefault()!.ToString()!);
        }
        public async Task<IdentityResult> UpdateAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }
        public async Task<List<User>?> GetByPaging(int PageNumber, int PageSize)
        {
            return await _Context.Users.AsNoTracking().OrderBy(u => u.Id).Skip((PageNumber - 1) * PageSize).Take(PageSize).ToListAsync();
        }
        public async Task<List<User>?> GetByPagingCondition(Expression<Func<User, bool>> func, int PageNumber, int PageSize)
        {
            return await _Context.Users.AsNoTracking().Where(func).OrderBy(u => u.Id).Skip((PageNumber - 1) * PageSize).Take(PageSize).ToListAsync();
        }
        public async Task<User?> FindByEmailAsync(string Email)
        {
            return await _userManager.FindByEmailAsync(Email);
        }
        public async Task<User?> FindByIdAsync(string Id)
        {
            return await _userManager.FindByIdAsync(Id);
        }
        public async Task<IList<string>> GetRole(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }
        public async Task<(bool Success, string Error)> changeRoles(ChangeRolesDTO roles)
        {
            var user = await _userManager.FindByIdAsync(roles.ID!);
            if (user == null)
            {
                return (false,"البريد الإلكترونى الذي أدخلته غير موجود");
            }
            var oldRoles = await _userManager.GetRolesAsync(user);
            var oldRole = string.Join(", ", oldRoles);
            if (oldRole == null || oldRole == "Admin")
            {
                return (false,"لا يمكن تغيير صلاحية المسؤول");
            }
            await _userManager.RemoveFromRoleAsync(user, oldRole!);
            await _userManager.AddToRoleAsync(user!, roles.roleName!);
            return (true, "تم تغيير الصلاحية بنجاح");
        }
    }
    
}
