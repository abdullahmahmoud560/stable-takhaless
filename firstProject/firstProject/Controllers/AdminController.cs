using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using firstProject.DTO;
using firstProject.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AdminController> _logger;
        private readonly IDataProtector _protector;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient httpClient1;

        public AdminController(UserManager<User> userManager, ILogger<AdminController> logger, IDataProtectionProvider protector,HttpClient httpClient,IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _logger = logger;
            _protector = protector.CreateProtector("OrderIdProtector");
            httpClient1 = httpClient;
            _httpContextAccessor = httpContextAccessor;

        }

        //جلب جميع المستخدمين
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-User")]
        public async Task<IActionResult> getAllUsers()
        {
            try
            {
                var allUsers = await _userManager.Users.ToListAsync();
                List<UserDTO> users = new List<UserDTO>();
                if (allUsers is not null)
                {

                    for (int i = 0; i < allUsers.Count; i++)
                    {
                        var roles = await _userManager.GetRolesAsync(allUsers[i]);
                        var role = string.Join(", ", roles);
                        if (role != null)
                        {
                            if (role == "User" || role == "Company")
                            {
                                users.Add(new UserDTO
                                {
                                    Id = allUsers[i].Id,
                                    fullName = allUsers[i].fullName,
                                    Identity = allUsers[i].Identity,
                                    phoneNumber = allUsers[i].PhoneNumber,
                                    Email = allUsers[i].Email,
                                    Role = role,
                                    IsBlocked = allUsers[i].isBlocked
                                });
                            }
                        }
                    }
                    return Ok(users);
                }
                return Ok(new string[] { });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق"});
            }
        }

        //جلب جميع المخلصين
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Broker")]
        public async Task<IActionResult> getAllBroker()
        {

            try
            {
                var allUsers = await _userManager.Users.ToListAsync();
                List<UserDTO> users = new List<UserDTO>();
                if (allUsers is not null)
                {

                    for (int i = 0; i < allUsers.Count; i++)
                    {
                        var roles = await _userManager.GetRolesAsync(allUsers[i]);
                        var role = string.Join(", ", roles);

                        if (role != null)
                        {
                            if (role == "Broker")
                            {
                                users.Add(new UserDTO
                                {
                                    Id = allUsers[i].Id,
                                    fullName = allUsers[i].fullName,
                                    Identity = allUsers[i].Identity,
                                    phoneNumber = allUsers[i].PhoneNumber,
                                    Email = allUsers[i].Email,
                                    Role = role,
                                    IsBlocked = allUsers[i].isBlocked
                                });
                            }
                        }
                    }
                    return Ok(users);
                }
                return Ok(new string[] { });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //جلب جميع خدمة العملاء
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-CustomerService")]
        public async Task<IActionResult> getAllCustomerService()
        {

            try
            {
                var allUsers = await _userManager.Users.ToListAsync();
                List<UserDTO> users = new List<UserDTO>();
                if (allUsers is not null)
                {

                    for (int i = 0; i < allUsers.Count; i++)
                    {
                        var roles = await _userManager.GetRolesAsync(allUsers[i]);
                        var role = string.Join(", ", roles);

                        if (role != null)
                        {
                            if (role == "CustomerService")
                            {
                                users.Add(new UserDTO
                                {
                                    Id = allUsers[i].Id,
                                    fullName = allUsers[i].fullName,
                                    Identity = allUsers[i].Identity,
                                    phoneNumber = allUsers[i].PhoneNumber,
                                    Email = allUsers[i].Email,
                                    Role = role,
                                    IsBlocked = allUsers[i].isBlocked
                                });
                            }
                        }
                    }
                    return Ok(users);
                }
                return Ok(new string[] { });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //جلب جميع المحاسبين
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Account")]
        public async Task<IActionResult> getAllAccount()
        {

            try
            {
                var allUsers = await _userManager.Users.ToListAsync();
                List<UserDTO> users = new List<UserDTO>();
                if (allUsers is not null)
                {

                    for (int i = 0; i < allUsers.Count; i++)
                    {
                        var roles = await _userManager.GetRolesAsync(allUsers[i]);
                        var role = string.Join(", ", roles);

                        if (role != null)
                        {
                            if (role == "Account")
                            {
                                users.Add(new UserDTO
                                {
                                    Id = allUsers[i].Id,
                                    fullName = allUsers[i].fullName,
                                    Identity = allUsers[i].Identity,
                                    phoneNumber = allUsers[i].PhoneNumber,
                                    Email = allUsers[i].Email,
                                    Role = role,
                                    IsBlocked = allUsers[i].isBlocked
                                });
                            }
                        }
                    }
                    return Ok(users);
                }
                return Ok(new string[] { });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //جلب جميع المديرين
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Manager")]
        public async Task<IActionResult> getAllManager()
        {

            try
            {
                var allUsers = await _userManager.Users.ToListAsync();
                List<UserDTO> users = new List<UserDTO>();
                if (allUsers is not null)
                {

                    for (int i = 0; i < allUsers.Count; i++)
                    {
                        var roles = await _userManager.GetRolesAsync(allUsers[i]);
                        var role = string.Join(", ", roles);

                        if (role != null)
                        {
                            if (role == "Manager")
                            {
                                users.Add(new UserDTO
                                {
                                    Id = allUsers[i].Id,
                                    fullName = allUsers[i].fullName,
                                    Identity = allUsers[i].Identity,
                                    phoneNumber = allUsers[i].PhoneNumber,
                                    Email = allUsers[i].Email,
                                    Role = role,
                                    IsBlocked = allUsers[i].isBlocked
                                });
                            }
                        }
                    }
                    return Ok(users);
                }
                return Ok(new string[] { });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //قائمة المحظورين
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Black-List")]
        public async Task<IActionResult> balckList()
        {
            try
            {
                var blackList = await _userManager.Users.Where(l => l.isBlocked == true).ToListAsync();
                List<UserDTO> users = new List<UserDTO>();
                if (blackList is not null)
                {

                    for (int i = 0; i < blackList.Count; i++)
                    {
                        var roles = await _userManager.GetRolesAsync(blackList[i]);
                        var role = string.Join(", ", roles);
                        if (role != null)
                        {
                            users.Add(new UserDTO
                            {
                                Id = blackList[i].Id,
                                fullName = blackList[i].fullName,
                                Identity = blackList[i].Identity,
                                phoneNumber = blackList[i].PhoneNumber,
                                Email = blackList[i].Email,
                                Role = role,
                                IsBlocked = blackList[i].isBlocked
                            });
                        }
                    }
                    return Ok(users);
                }
                return Ok(new string[] { });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
                return Ok(new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق" });
            }
        }

        // جلب جميع المستخدمين للصلاحيات
        [Authorize(Roles = "Admin")]
        [HttpGet("Get-All-Peaple-Admin")]
        public async Task<IActionResult> getAllPeapleAdmin()
        {
            try
            {
                var allUsers = await _userManager.Users.ToListAsync();
                List<UserDTO> users = new List<UserDTO>();
                if (allUsers is not null)
                {

                    for (int i = 0; i < allUsers.Count; i++)
                    {
                        var roles = await _userManager.GetRolesAsync(allUsers[i]);
                        var role = string.Join(", ", roles);
                        if (role.Equals("Admin"))
                        {
                            role= "مسؤول";
                        }
                        else if(role.Equals("Manager"))
                        {
                            role = "مدير";
                        }
                        else if (role.Equals("User"))
                        {
                            role = "عميل";
                        }
                        else if (role.Equals("Company"))
                        {
                            role = "شركة";
                        }
                        else if (role.Equals("CustomerService"))
                        {
                            role = "خدمة العملاء";
                        }
                        else if (role.Equals("Broker"))
                        {
                            role = "مخلص";
                        }
                        else if (role.Equals("Account"))
                        {
                            role = "محاسب";
                        }   
                        if (role != null)
                        {
                            users.Add(new UserDTO
                            {
                                Id = allUsers[i].Id,
                                fullName = allUsers[i].fullName,
                                Identity = allUsers[i].Identity,
                                phoneNumber = allUsers[i].PhoneNumber,
                                Email = allUsers[i].Email,
                                Role = role,
                                IsBlocked = allUsers[i].isBlocked
                            });
                        }
                    }
                    return Ok(users);
                }
                return Ok(new string[] { });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //عرض الصفحة الشخصية للمسؤول
        [Authorize(Roles =("Admin,Manager"))]
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
                var  role = await _userManager.GetRolesAsync(data!);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Active-Users")]
        public async Task<IActionResult> getActiveUsers()
        {
            try
            {
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                var Active = await _userManager.Users.Where(l => l.isActive == true && l.lastLogin!.Value.AddMonths(1) >= DateTime.UtcNow).ToListAsync();
                List<ActiveUsersDTO> users = new List<ActiveUsersDTO>();
                foreach (var user in Active)
                {
                    var Response = await new SendApis(httpClient1, _httpContextAccessor).SendAPI(user.Id);
                    if (Response.HasValue && Response.Value.TryGetProperty("totalOrders", out JsonElement totalOrders) && Response.Value.TryGetProperty("successOrders", out JsonElement successOrders))
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
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق "});
            }
        }
        
        
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Not-Active-Users")]
        public async Task<IActionResult> getNotActiveUsers()
        {
            try
            {
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                var Active = await _userManager.Users.Where(l => l.isActive == true && l.lastLogin!.Value.AddMonths(1) <= DateTime.UtcNow).ToListAsync();
                List<ActiveUsersDTO> users = new List<ActiveUsersDTO>();
                foreach (var user in Active)
                {
                    var Response = await new SendApis(httpClient1, _httpContextAccessor).SendAPI(user.Id);
                    if (Response.HasValue && Response.Value.TryGetProperty("totalOrders", out JsonElement totalOrders) && Response.Value.TryGetProperty("successOrders", out JsonElement successOrders))
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
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ اثناء تنفيذ العملية");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق "});
            }
        }
    }
}
