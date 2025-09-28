using Application.Interface;
using AutoMapper;
using firstProject.ApplicationDbContext;
using firstProject.Model;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;
using static Shared.DataTransferObject;

namespace Application.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMapper _mapper;
        private readonly DB _db;
        public UserService(IUserRepository userRepository,UserManager<User> userManager ,SignInManager<User> signInManager,IMapper mapper,DB db)
        {
            
            _userRepository = userRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _db = db;   
        }

        public async Task<(bool Success,string Error)> changeRoles(ChangeRolesDTO roles)
        {
          return await _userRepository.changeRoles(roles);
        }
        public async Task<(bool Success,string Error)> CreateAsync(BrokerDTO userdto, string password)
        {
            var user = _mapper.Map<User>(userdto);
            var result =  await _userRepository.CreateAsync(user, password);
            if (result.Success)
            {
                await _userManager.AddToRoleAsync(user, userdto.Role!);
                return (true,string.Empty);
            }
            return (false,result.Error);
        }
        public async Task<User?> FindByEmailAsync(string Email)
        {
          return await _userRepository.FindByEmailAsync(Email);
        }
        public async Task<User?> FindByIdAsync(string Id)
        {
          return await _userRepository.FindByIdAsync(Id);
        }
        public async Task<List<UserDTO>> GetByPaging(int PageNumber, int PageSize, string? targetRole)
        {
            var users = await _userRepository.GetByPaging(PageNumber, PageSize);
            var usersWithRoles = new List<UserDTO>();

            foreach (var user in users!)
            {
                var roles = await _userRepository.GetRole(user);

                if (string.IsNullOrWhiteSpace(targetRole) || roles.Contains(targetRole))
                {
                    usersWithRoles.Add(new UserListDTO
                    {
                        Id = user.Id,
                        fullName = user.fullName!,
                        Identity = user.Identity!,
                        PhoneNumber = user.PhoneNumber!,
                        Email = user.Email!,
                        Role = roles.FirstOrDefault() ?? "None",
                        IsBlocked = user.isBlocked,
                    });
                }
            }

            return usersWithRoles;
        }
        public async Task<List<User>?> GetByPagingCondition(Expression<Func<User, bool>> func, int PageNumber, int PageSize)
        {
            return await _userRepository.GetByPagingCondition(func, PageNumber, PageSize);
        }
        public  async Task<IList<string>> GetRole(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
                return new List<string>();
            return await _userRepository.GetRole(user);
        }
        public async Task<IdentityResult> UpdateAsync(User user)
        {
            return await _userRepository.UpdateAsync(user);
        }
        public async Task<(bool Success, string Error)> LoginUser(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email!);
            if (user == null)
            {
                return (false, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return (false, "تم قفل الحساب مؤقتًا بسبب المحاولات الفاشلة، برجاء المحاولة بعد 15 دقيقة");
            }

            if (user.isBlocked == true)
            {
                return (false, "تم حظر الحساب من قبل الإدارة");
            }

            if (user.isActive == false)
            {
                return (false, "الحساب غير مفعل");
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password!, lockoutOnFailure: true);

            if (!signInResult.Succeeded)
            {
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return (false, "تم حظر الحساب مؤقتًا بسبب تجاوز الحد الأقصى للمحاولات. برجاء المحاولة بعد 15 دقيقة");
                }

                int maxAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;
                int failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
                int attemptsLeft = maxAttempts - failedAttempts;

                return (false, $"البريد الإلكتروني أو كلمة المرور غير صحيحة. يتبقى لديك {attemptsLeft} محاولات قبل إيقاف الحساب.");
            }


            var roles = await _userManager.GetRolesAsync(user);
            var rolesString = string.Join(", ", roles);

            user.lastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return (true, rolesString);
        }
        public async Task<(bool Success, string Error)> CheckIsFoundEmailOrPhoneOrIdentity(string Email, string Phone,string Identity)
        {
            var emailExiting= await _userManager.FindByEmailAsync(Email);
            var PhoneExiting= await _db.Users.AnyAsync(x=>x.PhoneNumber == Phone);
            var IdentityExiting= await _db.Users.AnyAsync(x=>x.Identity == Identity);
            if (emailExiting != null)
                return (false, "البريد الإلكتروني مستخدم بالفعل");
            if (PhoneExiting)
                return (false, "رقم الهاتف مستخدم بالفعل");
            if (IdentityExiting)
                return (false, "رقم الهوية الوطنية مستخدم بالفعل");
            return (true,string.Empty); 
        }
        public async Task<(bool Success, string Error)> CheckIsFoundtaxRecordOrInsuranceNumber(string InsuranceNumber, string taxRecord,string license)
        {
            var InsuranceNumberExiting = await _db.Users.AnyAsync(x=>x.InsuranceNumber == InsuranceNumber);
            var existtaxRecord = await _db.Users.AnyAsync(x => x.taxRecord == taxRecord);
            if (InsuranceNumberExiting)
                return (false, "الرقم الضريبي مستخدم بالفعل");
            if (existtaxRecord)
                return (false, "السجل التجاري مستخدم بالفعل");
            if (!string.IsNullOrEmpty(license))
            {
                var exitinglicense = await _db.Users.AnyAsync(x => x.license == license);
                if(existtaxRecord)
                    return (false, "رخصة المخلص مستخدمة بالفعل");
                return (true, string.Empty);
            }
            return (true,string.Empty);
        }
        public async Task<(bool Success, string Error)> ResetPassword(ResetPasswordDTO reset, string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
                return (false, "المستخدم غير موجود");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, resetToken, reset.newPassword!);
            if (!result.Succeeded)
            {
                await _userManager.AccessFailedAsync(user);
                return (false, "فشل تغيير كلمة المرور");
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            return (true, string.Empty);
        }
        public async Task<(bool Success, string Error)> VerifyCode(VerifyCode verifyCode,string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
                return (false, "المستخدم غير موجود");

            var time = await _db.twoFactorVerify.Where(l => l.UserId == user.Id && l.TypeOfGenerate == verifyCode.typeOfGenerate).FirstOrDefaultAsync();

            if (time == null)
                return (false, "رمز التحقق غير موجود");

            bool isCodeValid = BCrypt.Net.BCrypt.Verify(verifyCode.Code, time.Value);
            bool isNotExpired = (DateTime.UtcNow - time.Date) <= TimeSpan.FromMinutes(5);
            bool isUserd = time.IsUsed;
            if (!isCodeValid)
                return (false, "رمز التحقق غير صحيح");

            if (!isNotExpired)
                return (false, "رمز التحقق منتهي الصلاحية");

            if (isUserd)
                return (false, "رمز التحقق مستخدم من قبل");

            time.IsUsed = true;
            await _db.SaveChangesAsync();
            return (true,user.Email!);
        }
        public async Task<(bool Success, string Error)> ActiveEmail(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null) return (false, "المستخدم غير موجود");
            user.isActive = true;
            await _userManager.UpdateAsync(user);
            return (true,string.Empty);
        }
        public async Task<(bool Success, string Error)>UnBlocked(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null) return (false, "المستخدم غير موجود");
            var role = await _userManager.GetRolesAsync(user!);
            var roles = string.Join(", ", role);
            if (user is not null && user.isBlocked == true)
            {
                user.isBlocked = false;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return (true,string.Empty);
                return (false,string.Empty);
            }
            return (false,string.Empty);
        }
        public async Task<(bool Success, string Error)>Blocked(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null) return (false, "المستخدم غير موجود");
            var role = await _userManager.GetRolesAsync(user!);
            var roles = string.Join(", ", role);
            if (user.isBlocked == false)
            {
                if (roles != "Admin" && roles != null)
                {
                    user.isBlocked = true;
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                        return(true,string.Empty);
                }
                else
                {
                    return (false,"لا يمكن حظر المسؤول");
                }
            }
            return (false,"المستخدم محظور بالفعل");
        }
        public async Task<(bool Success, string Message, IEnumerable<string>? AddedClaims)> SetPermissionsAsync(RolesDTO rolesDTO)
        {
            var user = await _userManager.FindByIdAsync(rolesDTO.ID!);
            if (user is null)
                return (false, "المستخدم غير موجود", null);

            var existingClaims = await _userManager.GetClaimsAsync(user);
            var distinctPermissions = rolesDTO.NameOfPermissions!.Distinct();
            var addedClaims = new List<string>();

            foreach (var role in distinctPermissions)
            {
                if (!existingClaims.Any(c => c.Type == role && c.Value == "true"))
                {
                    var result = await _userManager.AddClaimAsync(user, new Claim(role, "true"));
                    if (result.Succeeded)
                        addedClaims.Add(role);
                }
            }

            return (true, "تمت إضافة الصلاحيات", addedClaims);
        }
        public async Task<(bool Success, string Message, IEnumerable<string>? Claims)> GetPermissionsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return (false, "برجاء ملئ البيانات المطلوبة", null);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return (false, "المستخدم غير موجود", null);

            var claims = await _userManager.GetClaimsAsync(user);
            return (true, "تم جلب الصلاحيات", claims.Select(c => c.Type));
        }
        public async Task<(bool Success, string Message, IEnumerable<string>? RemainingClaims)> DeletePermissionsAsync(RolesDTO rolesDTO)
        {
            if (string.IsNullOrEmpty(rolesDTO.ID))
                return (false, "برجاء ملئ البيانات المطلوبة", null);

            var user = await _userManager.FindByIdAsync(rolesDTO.ID);
            if (user is null)
                return (false, "المستخدم غير موجود", null);

            var claims = await _userManager.GetClaimsAsync(user);

            foreach (var permission in rolesDTO.NameOfPermissions!)
            {
                var claimToRemove = claims.FirstOrDefault(c => c.Type == permission && c.Value == "true");
                if (claimToRemove != null)
                {
                    var result = await _userManager.RemoveClaimAsync(user, claimToRemove);
                    if (result.Succeeded)
                    {
                        var remainingClaims = await _userManager.GetClaimsAsync(user);
                        return (true, "تم حذف الصلاحيات", remainingClaims.Select(c => c.Type));
                    }
                }
            }

            return (false, "لم يتم حذف أي صلاحيات", null);
        }
        public async Task<int> GetUserCountByRole(string role)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            return usersInRole.Count;
        }
    }
}
