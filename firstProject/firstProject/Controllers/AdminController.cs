using firstProject.DTO;
using firstProject.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient httpClient1;

        public AdminController(UserManager<User> userManager, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            httpClient1 = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        //جلب جميع المستخدمين
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-User/{Page}")]
        public async Task<IActionResult> GetAllUsers(int Page)
        {
            try
            {
                const int pageSize = 8;

                // Get all users with pagination
                var allUsers = await _userManager.Users
                    .Skip((Page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var usersWithRoles = new List<UserDTO>();

                // Process users and get their roles
                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var role = string.Join(", ", roles);

                    if (role.Contains("User") || role.Contains("Company"))
                    {
                        usersWithRoles.Add(new UserDTO
                        {
                            Id = user.Id,
                            fullName = user.fullName,
                            Identity = user.Identity,
                            phoneNumber = user.PhoneNumber,
                            Email = user.Email,
                            Role = role,
                            IsBlocked = user.isBlocked
                        });
                    }
                }

                // Get total count efficiently
                var totalUser = await _userManager.Users.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalUser / pageSize);

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    data = usersWithRoles,
                    totalUser = usersWithRoles.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق" });
            }
        }

        //جلب جميع المخلصين
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Broker/{Page}")]
        public async Task<IActionResult> GetAllBroker(int Page)
        {
            try
            {
                const int PageSize = 8;

                // Get all users with pagination
                var allUsers = await _userManager.Users
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                var brokers = new List<UserDTO>();

                // Process users and get their roles
                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var role = string.Join(", ", roles);

                    if (role.Contains("Broker"))
                    {
                        brokers.Add(new UserDTO
                        {
                            Id = user.Id,
                            fullName = user.fullName,
                            Identity = user.Identity,
                            phoneNumber = user.PhoneNumber,
                            Email = user.Email,
                            Role = role,
                            IsBlocked = user.isBlocked
                        });
                    }
                }

                // Get total count efficiently
                var totalUser = await _userManager.Users.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalUser / PageSize);

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = brokers.Count,
                    data = brokers
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }


        //جلب جميع خدمة العملاء
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-CustomerService/{Page}")]
        public async Task<IActionResult> GetAllCustomerService(int Page)
        {
            try
            {
                const int PageSize = 8;

                // Get all users with pagination
                var allUsers = await _userManager.Users
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                var customerServices = new List<UserDTO>();

                // Process users and get their roles
                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var role = string.Join(", ", roles);

                    if (role.Contains("CustomerService"))
                    {
                        customerServices.Add(new UserDTO
                        {
                            Id = user.Id,
                            fullName = user.fullName,
                            Identity = user.Identity,
                            phoneNumber = user.PhoneNumber,
                            Email = user.Email,
                            Role = role,
                            IsBlocked = user.isBlocked
                        });
                    }
                }

                // Get total count efficiently
                var totalUser = await _userManager.Users.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalUser / PageSize);

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = customerServices.Count,
                    data = customerServices
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //جلب جميع المحاسبين
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Account/{Page}")]
        public async Task<IActionResult> GetAllAccount(int Page)
        {
            try
            {
                const int PageSize = 8;

                // Get all users with pagination
                var allUsers = await _userManager.Users
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                var accounts = new List<UserDTO>();

                // Process users and get their roles
                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var role = string.Join(", ", roles);

                    if (role.Contains("Account"))
                    {
                        accounts.Add(new UserDTO
                        {
                            Id = user.Id,
                            fullName = user.fullName,
                            Identity = user.Identity,
                            phoneNumber = user.PhoneNumber,
                            Email = user.Email,
                            Role = role,
                            IsBlocked = user.isBlocked
                        });
                    }
                }

                // Get total count efficiently
                var totalUser = await _userManager.Users.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalUser / PageSize);

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = accounts.Count,
                    data = accounts
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //جلب جميع المديرين
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Manager/{Page}")]
        public async Task<IActionResult> GetAllManager(int Page)
        {
            try
            {
                const int PageSize = 8;

                // Get all users with pagination
                var allUsers = await _userManager.Users
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                var managers = new List<UserDTO>();

                // Process users and get their roles
                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var role = string.Join(", ", roles);

                    if (role.Contains("Manager"))
                    {
                        managers.Add(new UserDTO
                        {
                            Id = user.Id,
                            fullName = user.fullName,
                            Identity = user.Identity,
                            phoneNumber = user.PhoneNumber,
                            Email = user.Email,
                            Role = role,
                            IsBlocked = user.isBlocked
                        });
                    }
                }

                // Get total count efficiently
                var totalUser = await _userManager.Users.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalUser / PageSize);

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = managers.Count,
                    data = managers
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        // عمل حظر
        [Authorize(Roles = ("Admin,Manager"))]
        [HttpPost("Blocked")]
        public async Task<IActionResult> BlockedUser(BlockedDTO blockedDTO)
        {
            try
            {
                if (blockedDTO.Email is not null)
                {
                    var user = await _userManager.FindByEmailAsync(blockedDTO.Email);
                    var role = await _userManager.GetRolesAsync(user!);
                    var roles = string.Join(", ", role);
                    if (user is not null && user.isBlocked == false)
                    {
                        if (roles != "Admin" && roles != null)
                        {
                            user.isBlocked = true;
                            var result = await _userManager.UpdateAsync(user);
                            if (result.Succeeded)
                                return Ok(new ApiResponse { Message = "تم حظر المستخدم بنجاح" });
                        }
                        else
                        {
                            return Ok(new ApiResponse { Message = "لا يمكن حظر المسؤول" });
                        }
                    }
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        // فك الحظر
        [Authorize(Roles = ("Admin,Manager"))]
        [HttpPost("Unblocked")]
        public async Task<IActionResult> UnblockedUser(BlockedDTO unBlockedDTO)
        {
            try
            {
                if (unBlockedDTO.Email is not null)
                {
                    var user = await _userManager.FindByEmailAsync(unBlockedDTO.Email);
                    var role = await _userManager.GetRolesAsync(user!);
                    var roles = string.Join(", ", role);
                    if (user is not null && user.isBlocked == true)
                    {
                        user.isBlocked = false;
                        var result = await _userManager.UpdateAsync(user);
                        if (result.Succeeded)
                            return Ok(new ApiResponse { Message = "تم فك حظر المستخدم بنجاح" });
                    }
                }
                return BadRequest(new ApiResponse { Message = "خطأ في البيانات المدخلة" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //قائمة المحظورين
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Black-List/{Page}")]
        public async Task<IActionResult> BlackList(int Page)
        {
            try
            {
                const int PageSize = 8;

                // Get blocked users with pagination
                var blackList = await _userManager.Users
                    .Where(u => u.isBlocked == true)
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                var users = new List<UserDTO>();

                // Process users and get their roles
                foreach (var user in blackList)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var role = string.Join(", ", roles);

                    users.Add(new UserDTO
                    {
                        Id = user.Id,
                        fullName = user.fullName,
                        Identity = user.Identity,
                        phoneNumber = user.PhoneNumber,
                        Email = user.Email,
                        Role = role,
                        IsBlocked = user.isBlocked
                    });
                }

                // Get total count efficiently
                var totalCount = await _userManager.Users
                    .Where(u => u.isBlocked == true)
                    .CountAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = totalCount,
                    data = users
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }


        //تغيير صلاحية الطلب 
        [Authorize(Roles = ("Admin"))]
        [HttpPost("Change-Roles")]
        public async Task<IActionResult> changeRoles(ChageRolesDTO roles)
        {
            try
            {
                if (roles.ID == null || roles.roleName == null)
                { return Ok(new ApiResponse { Message = "البيانات غير صحيحة" }); }

                else
                {
                    var user = await _userManager.FindByIdAsync(roles.ID!);
                    if (user == null)
                    {
                        return Ok(new ApiResponse { Message = "البريد الإلكترونى الذي أدخلته غير موجود" });
                    }
                    var oldRoles = await _userManager.GetRolesAsync(user);
                    var oldRole = string.Join(", ", oldRoles);
                    if (oldRole == null || oldRole == "Admin")
                    {
                        return Ok(new ApiResponse { Message = "لا يمكن تغيير صلاحية المسؤول" });
                    }
                    await _userManager.RemoveFromRoleAsync(user, oldRole!);
                    await _userManager.AddToRoleAsync(user!, roles.roleName!);
                    return Ok(new ApiResponse { Message = "تم تغيير الصلاحية بنجاح" });
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //جلب المعلومات
        [Authorize(Roles = "Admin")]
        [HttpPost("Get-Information")]
        public async Task<IActionResult> getInformation(GetInformationDTO getInformationDTO)
        {
            try
            {
                if (getInformationDTO.Email != null)
                {
                    var user = await _userManager.FindByEmailAsync(getInformationDTO.Email!);
                    if (user != null)
                    {
                        return Ok(new { user.Email, user.fullName });
                    }
                    else
                    {
                        return Ok(new ApiResponse { Message = "لا توجد بيانات لهذا الايميل" });
                    }
                }
                return Ok(new ApiResponse { Message = "" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //الاحصائيات
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Statictis")]
        public async Task<IActionResult> statictis()
        {
            try
            {
                var allUsers = await _userManager.GetUsersInRoleAsync("User");
                var allCompanies = await _userManager.GetUsersInRoleAsync("Company");
                var allBrokers = await _userManager.GetUsersInRoleAsync("Broker");
                return Ok(allUsers.Count + allCompanies.Count + allBrokers.Count);
            }
            catch (Exception)
            {
                return Ok(new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق" });
            }
        }

        // جلب جميع المستخدمين للصلاحيات
        [Authorize(Roles = "Admin")]
        [HttpGet("Get-All-Peaple-Admin/{Page}")]
        public async Task<IActionResult> GetAllPeapleAdmin(int Page)
        {
            try
            {
                const int PageSize = 8;

                // Get all users with pagination
                var allUsers = await _userManager.Users
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                var users = new List<UserDTO>();

                // Process users and get their roles
                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var role = string.Join(", ", roles);

                    string arabicRole = role switch
                    {
                        "Admin" => "مسؤول",
                        "Manager" => "مدير",
                        "User" => "عميل",
                        "Company" => "شركة",
                        "CustomerService" => "خدمة العملاء",
                        "Broker" => "مخلص",
                        "Account" => "محاسب",
                        _ => role
                    };

                    users.Add(new UserDTO
                    {
                        Id = user.Id,
                        fullName = user.fullName,
                        Identity = user.Identity,
                        phoneNumber = user.PhoneNumber,
                        Email = user.Email,
                        Role = arabicRole,
                        IsBlocked = user.isBlocked
                    });
                }

                // Get total count efficiently
                var totalUser = await _userManager.Users.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalUser / PageSize);

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = users.Count,
                    data = users
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //عرض الصفحة الشخصية
        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "برجاء ملئ البيانات المطلوبة" });
                }

                var data = await _userManager.FindByIdAsync(ID);
                if (data != null)
                {
                    List<SelectDTO> list = new List<SelectDTO>();
                    list.Add(new SelectDTO
                    {
                        Id = data!.Id,
                        fullName = data.fullName,
                        Email = data.Email,
                        PhoneNumber = data.PhoneNumber,
                        taxRecord = data.taxRecord,
                        InsuranceNumber = data.InsuranceNumber,
                        license = data.license,
                        Identity = data.Identity,
                        Role = _userManager.GetRolesAsync(data).Result.FirstOrDefault(),
                    });

                    return Ok(list[0]);
                }
                return NotFound(new ApiResponse { Message = "بيانات المستخدم غير موجودة" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //عرض الصفحة الشخصية للمسؤول
        [Authorize(Roles = ("Admin,Manager"))]
        [HttpGet("Profile-Show-Admin/{ID}")]
        public async Task<IActionResult> profileShowAdmin(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID!.ToString()))
                {
                    return BadRequest(new ApiResponse { Message = "برجاء ملئ البيانات المطلوبة" });
                }

                var data = await _userManager.FindByIdAsync(ID.ToString()!);
                var role = await _userManager.GetRolesAsync(data!);
                var roles = string.Join(", ", role);
                if (data != null)
                {
                    List<SelectDTO> list = new List<SelectDTO>();
                    list.Add(new SelectDTO
                    {
                        Id = data!.Id,
                        fullName = data.fullName,
                        Email = data.Email,
                        PhoneNumber = data.PhoneNumber,
                        taxRecord = data.taxRecord,
                        InsuranceNumber = data.InsuranceNumber,
                        license = data.license,
                        Identity = data.Identity,
                        Role = roles,
                    });

                    return Ok(list[0]);
                }
                return NotFound(new ApiResponse { Message = "بيانات المستخدم غير موجودة" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        // التخصوصات للمدير
        [Authorize(Roles = "Admin")]
        [HttpPost("Set-Permissions")]
        public async Task<IActionResult> SetPermissions(RolesDTO rolesDTO)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(rolesDTO.ID!);
                if (user is null)
                {
                    return BadRequest(new ApiResponse { Message = "المستخدم غير موجود" });
                }

                var existingClaims = await _userManager.GetClaimsAsync(user);

                var distinctPermissions = rolesDTO.NameOfPermissions!.Distinct();
                var addClaims = new List<string>();

                foreach (var role in distinctPermissions)
                {
                    if (!existingClaims.Any(c => c.Type == role && c.Value == "true"))
                    {
                        var result = await _userManager.AddClaimAsync(user, new Claim(role, "true"));
                        if (result.Succeeded)
                            addClaims.Add(role);
                    }
                }
                return Ok(addClaims);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //جلب الصلاحيات
        [Authorize(Roles = "Manager,Admin")]
        [HttpGet("Get-Permissions/{ID}")]
        public async Task<IActionResult> GetPermissions(string ID)
        {
            try
            {
                if (!string.IsNullOrEmpty(ID!.ToString()))
                {
                    var TokenUser = await _userManager.FindByIdAsync(ID.ToString()!);
                    var claims = await _userManager.GetClaimsAsync(TokenUser!);
                    var selectClaims = claims.Select(c => c.Type).ToArray();
                    return Ok(selectClaims);
                }
                return BadRequest(new ApiResponse { Message = "برجاء ملئ البيانات المطلوبة" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //حذف الصلاحيات
        [Authorize(Roles = "Admin")]
        [HttpPost("Delete-Permissions")]
        public async Task<IActionResult> DeletePermissions(RolesDTO rolesDTO)
        {
            try
            {
                if (!string.IsNullOrEmpty(rolesDTO.ID))
                {
                    var user = await _userManager.FindByIdAsync(rolesDTO.ID);
                    if (user is not null)
                    {
                        var claims = await _userManager.GetClaimsAsync(user);

                        foreach (var permission in rolesDTO.NameOfPermissions!)
                        {
                            var claimToRemove = claims.FirstOrDefault(c => c.Type == permission && c.Value == "true");
                            if (claimToRemove != null)
                            {
                                var result = await _userManager.RemoveClaimAsync(user, claimToRemove);
                                if (result.Succeeded)
                                {
                                    var results = await _userManager.GetClaimsAsync(user);
                                    return Ok(results.Select(t => t.Type));
                                }
                            }
                        }
                    }
                }
                return Ok(new string[] { });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("Get-Permissions-User")]
        public async Task<IActionResult> GetPermissionsUser()
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                if (!string.IsNullOrEmpty(ID!.ToString()))
                {
                    var TokenUser = await _userManager.FindByIdAsync(ID.ToString()!);
                    var claims = await _userManager.GetClaimsAsync(TokenUser!);
                    var selectClaims = claims.Select(c => new { c.Type }).ToList();
                    return Ok(selectClaims);
                }
                return BadRequest(new ApiResponse { Message = "برجاء ملئ البيانات المطلوبة" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Active-Users/{Page}")]
        public async Task<IActionResult> GetActiveUsers(int Page)
        {
            try
            {
                const int PageSize = 8;

                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                // Get active users with pagination
                var activeUsers = await _userManager.Users
                    .Where(u => u.isActive == true && u.lastLogin!.Value.AddMonths(1) >= DateTime.UtcNow)
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                var users = new List<ActiveUsersDTO>();

                // Process users and get their data
                foreach (var user in activeUsers)
                {
                    var response = await new SendApis(httpClient1, _httpContextAccessor).SendAPI(user.Id);

                    if (response.HasValue &&
                        response.Value.TryGetProperty("totalOrders", out JsonElement totalOrders) &&
                        response.Value.TryGetProperty("successOrders", out JsonElement successOrders))
                    {
                        users.Add(new ActiveUsersDTO
                        {
                            fullName = user.fullName,
                            Email = user.Email,
                            lastlogin = user.lastLogin!.Value.ToString("dddd, dd MMMM yyyy", culture),
                            totalOrders = totalOrders.GetInt32(),
                            SuccessOrders = successOrders.GetInt32(),
                        });
                    }
                }

                // Get total count efficiently
                var totalCount = await _userManager.Users
                    .Where(u => u.isActive == true && u.lastLogin!.Value.AddMonths(1) >= DateTime.UtcNow)
                    .CountAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = totalCount,
                    data = users
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Not-Active-Users/{Page}")]
        public async Task<IActionResult> GetNotActiveUsers(int Page)
        {
            try
            {
                const int PageSize = 8;

                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                // Get not active users with pagination
                var notActiveUsers = await _userManager.Users
                    .Where(u => u.isActive == true && u.lastLogin!.Value.AddMonths(1) <= DateTime.UtcNow)
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                var users = new List<ActiveUsersDTO>();

                // Process users and get their data
                foreach (var user in notActiveUsers)
                {
                    var response = await new SendApis(httpClient1, _httpContextAccessor).SendAPI(user.Id);

                    if (response.HasValue &&
                        response.Value.TryGetProperty("totalOrders", out JsonElement totalOrders) &&
                        response.Value.TryGetProperty("successOrders", out JsonElement successOrders))
                    {
                        users.Add(new ActiveUsersDTO
                        {
                            fullName = user.fullName,
                            Email = user.Email,
                            lastlogin = user.lastLogin!.Value.ToString("dddd, dd MMMM yyyy", culture),
                            totalOrders = totalOrders.GetInt32(),
                            SuccessOrders = successOrders.GetInt32()
                        });
                    }
                }

                // Get total count efficiently
                var totalCount = await _userManager.Users
                    .Where(u => u.isActive == true && u.lastLogin!.Value.AddMonths(1) <= DateTime.UtcNow)
                    .CountAsync();


                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = totalCount,
                    data = users
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

    }
}
