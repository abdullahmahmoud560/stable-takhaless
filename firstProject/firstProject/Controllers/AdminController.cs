using Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using static Shared.DataTransferObject;

namespace firstProject.Controllers
{
    [Route("api/")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IServiceManager _serviceManager;
        private const int pageSize = 8;
        public AdminController(IUserService userservice, IServiceManager serviceManager)
        {
            _userService = userservice;
            _serviceManager = serviceManager;

        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-User/{Page}")]
        public async Task<IActionResult> GetAllUsers(int Page)
        {
            var users = await _userService.GetByPaging(Page, pageSize, "User");
            return CreatePaginatedResponse(users, Page, pageSize);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Broker/{Page}")]
        public async Task<IActionResult> GetAllBroker(int Page)
        {
            var brokers = await _userService.GetByPaging(Page, pageSize, "Broker");
            return CreatePaginatedResponse(brokers, Page, pageSize);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-CustomerService/{Page}")]
        public async Task<IActionResult> GetAllCustomerService(int Page)
        {
            var customerService = await _userService.GetByPaging(Page, pageSize, "CustomerService");
            return CreatePaginatedResponse(customerService, Page, pageSize);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Account/{Page}")]
        public async Task<IActionResult> GetAllAccount(int Page)
        {
            var accounts = await _userService.GetByPaging(Page, pageSize, "Account");
            return CreatePaginatedResponse(accounts, Page, pageSize);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Manager/{Page}")]
        public async Task<IActionResult> GetAllManager(int Page)
        {
            var managers = await _userService.GetByPaging(Page, pageSize, "Manager");
            return CreatePaginatedResponse(managers, Page, pageSize);
        }

        [Authorize(Roles = ("Admin,Manager"))]
        [HttpPost("Blocked")]
        public async Task<IActionResult> BlockedUser(BlockedDTO blockedDTO)
        {
            blockedDTO.Email = Infrastructure.Validation.InputSanitizer.SanitizeEmail(blockedDTO.Email);
            
            var result = await _userService.Blocked(blockedDTO.Email!);
            if(!result.Success) 
                return BadRequest(new ApiResponse { Message = result.Error});
            return Ok(new ApiResponse { Message = "تم حظر المستخدم بنجاح"});
        }

        [Authorize(Roles = ("Admin,Manager"))]
        [HttpPost("Unblocked")]
        public async Task<IActionResult> UnblockedUser(BlockedDTO unBlockedDTO)
        {
            if (!string.IsNullOrEmpty(unBlockedDTO.Email))
            {
                var result = await _userService.UnBlocked(unBlockedDTO.Email);
                if(!result.Success)
                    return BadRequest(new ApiResponse {Message = result.Error});
                return Ok(new ApiResponse { Message = "تم فك حظر المستخدم بنجاح"});
            }
            return BadRequest(new ApiResponse { Message = "خطأ في البيانات المدخلة" });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Black-List/{Page}")]
        public async Task<IActionResult> BlackList(int Page)
        {
            var blackList = await _userService.GetByPagingCondition(x=>x.isBlocked == true, Page,pageSize);

            var users = new List<UserDTO>();

            foreach (var user in blackList!)
            {
                var roles = await _userService.GetRole(user.Email!);
                var role = string.Join(", ", roles);

                users.Add(new UserListDTO
                {
                    Id = user.Id,
                    fullName = user.fullName!,
                    Identity = user.Identity!,
                    PhoneNumber = user.PhoneNumber!,
                    Email = user.Email!,
                    Role = role,
                    IsBlocked = user.isBlocked,
                });
            }

            var totalCount = blackList.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return Ok(new
            {
                TotalPages = totalPages,
                Page = Page,
                totalUser = totalCount,
                data = users
            });
            
        }


        [Authorize(Roles = ("Admin"))]
        [HttpPost("Change-Roles")]
        public async Task<IActionResult> changeRoles(ChangeRolesDTO roles)
        {
            if (string.IsNullOrEmpty(roles.roleName)|| roles.ID == null)
            { return Ok(new ApiResponse { Message = "البيانات غير صحيحة" }); }

            var result = await _userService.changeRoles(roles);
            if (result.Success)
            {
                return Ok(new ApiResponse { Message = result.Error });
            }
            return Ok(new ApiResponse { Message = result.Error });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Get-Information")]
        public async Task<IActionResult> getInformation(GetInformationDTO getInformationDTO)
        {
            if (getInformationDTO.Email != null)
            {
                var user = await _userService.FindByEmailAsync(getInformationDTO.Email!);
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

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Statictis")]
        public async Task<IActionResult> statictis()
        {
            var userCount = await _userService.GetUserCountByRole("User");
            var companyCount = await _userService.GetUserCountByRole("Company");
            var brokerCount = await _userService.GetUserCountByRole("Broker");
            return Ok(userCount + companyCount + brokerCount);
            
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Get-All-Peaple-Admin/{Page}")]
        public async Task<IActionResult> GetAllPeapleAdmin(int Page)
        {
            var allUsers = await _userService.GetByPaging(Page, pageSize, "") ?? new List<UserDTO>();

            var users = allUsers.Select(u =>
            {
                string arabicRole = (u.Role ?? "") switch
                {
                    "Admin" => "مسؤول",
                    "Manager" => "مدير",
                    "User" => "عميل",
                    "Company" => "شركة",
                    "CustomerService" => "خدمة العملاء",
                    "Broker" => "مخلص",
                    "Account" => "محاسب",
                    _ => u.Role ?? ""
                };

                u.Role = arabicRole;
                return u;
            }).ToList();

            var totalUser = allUsers.Count;
            var totalPages = (int)Math.Ceiling((double)totalUser / pageSize);

            return Ok(new
            {
                TotalPages = totalPages,
                Page = Page,
                totalUser = users.Count,
                data = users
            });
        }

        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> Profile()
        {
            
            var ID = User.FindFirstValue("ID");
            if (string.IsNullOrEmpty(ID))
            {
                return BadRequest(new ApiResponse { Message = "برجاء ملئ البيانات المطلوبة" });
            }

            var data = await _userService.FindByIdAsync(ID);
            if (data != null)
            {
                List<SelectDTO> list = new List<SelectDTO>();
                var role = await _userService.GetRole(data.Email!);
                var roles = string.Join(", ", role);
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

        [Authorize(Roles = ("Admin,Manager"))]
        [HttpGet("Profile-Show-Admin/{ID}")]
        public async Task<IActionResult> profileShowAdmin(string ID)
        {
            
            if (string.IsNullOrEmpty(ID!.ToString()))
            {
                return BadRequest(new ApiResponse { Message = "برجاء ملئ البيانات المطلوبة" });
            }

            var data = await _userService.FindByIdAsync(ID.ToString()!);
            if (data == null)  return BadRequest(new ApiResponse { Message = "المستخدم غير موجود"});
            var role = await _userService.GetRole(data!.Email!);
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

                return Ok(list.FirstOrDefault());
            }
            return NotFound(new ApiResponse { Message = "بيانات المستخدم غير موجودة" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Set-Permissions")]
        public async Task<IActionResult> SetPermissions([FromBody] RolesDTO rolesDTO)
        {
            var result = await _userService.SetPermissionsAsync(rolesDTO);

            if (!result.Success)
                return BadRequest(new ApiResponse { Message = result.Message });

            return Ok(new ApiResponse { Message = result.Message, Data = result.AddedClaims });
        }

        [Authorize(Roles = "Manager,Admin")]
        [HttpGet("Get-Permissions-User/{ID}")]
        public async Task<IActionResult> GetPermissions(string ID)
        {
            var result = await _userService.GetPermissionsAsync(ID);

            if (!result.Success)
                return BadRequest(new ApiResponse { Message = result.Message });

            return Ok(new ApiResponse { Message = result.Message, Data = result.Claims });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Delete-Permissions")]
        public async Task<IActionResult> DeletePermissions([FromBody] RolesDTO rolesDTO)
        {
            var result = await _userService.DeletePermissionsAsync(rolesDTO);

            if (!result.Success)
                return BadRequest(new ApiResponse { Message = result.Message });

            return Ok(new ApiResponse { Message = result.Message, Data = result.RemainingClaims });
        }
      
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Active-Users/{Page}")]
        public async Task<IActionResult> GetActiveUsers(int Page)
        {
            var activeUsers = await _userService.GetByPagingCondition(u => u.isActive == true && u.lastLogin.AddMonths(1) >= DateTime.UtcNow, 1, pageSize);
            if (activeUsers == null)
                return Ok(new string[] { });
            var users = new List<ActiveUsersDTO>();

            foreach (var user in activeUsers)
            {
                var response = await _serviceManager.FunctionService.SendAPI(user.Id.ToString());

                if (response.HasValue &&
                    response.Value.TryGetProperty("totalOrders", out JsonElement totalOrders) &&
                    response.Value.TryGetProperty("successOrders", out JsonElement successOrders))
                {
                    users.Add(new ActiveUsersDTO
                    {
                        fullName = user.fullName,
                        Email = user.Email,
                        lastlogin = GetFormattedDate(user.lastLogin).ToString(),
                        totalOrders = totalOrders.GetInt32(),
                        SuccessOrders = successOrders.GetInt32(),
                    });
                }
            }

            var totalCount =  activeUsers.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return Ok(new
            {
                TotalPages = totalPages,
                Page = Page,
                totalUser = totalCount,
                data = users
            });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Not-Active-Users/{Page}")]
        public async Task<IActionResult> GetNotActiveUsers(int Page)
        {

            var notActiveUsers = await _userService.GetByPagingCondition(u => u.isActive == true && u.lastLogin.AddMonths(1) <= DateTime.UtcNow, 1, pageSize);
            if (notActiveUsers == null)
                return Ok(new string[] { });

            var users = new List<ActiveUsersDTO>();

            foreach (var user in notActiveUsers)
            {
                var response = await _serviceManager.FunctionService.SendAPI(user.Id.ToString());

                if (response.HasValue &&
                    response.Value.TryGetProperty("totalOrders", out JsonElement totalOrders) &&
                    response.Value.TryGetProperty("successOrders", out JsonElement successOrders))
                {
                    users.Add(new ActiveUsersDTO
                    {
                        fullName = user.fullName,
                        Email = user.Email,
                        lastlogin = GetFormattedDate(user.lastLogin).ToString(),
                        totalOrders = totalOrders.GetInt32(),
                        SuccessOrders = successOrders.GetInt32()
                    });
                }
            }

            var totalCount =  notActiveUsers.Count;


            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return Ok(new
            {
                TotalPages = totalPages,
                Page = Page,
                totalUser = totalCount,
                data = users
            });
        }



        private IActionResult CreatePaginatedResponse<T>(IEnumerable<T> data, int page, int pageSize)
        {
            var totalCount = data.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return Ok(new
            {
                TotalPages = totalPages,
                Page = page,
                Data = data,
                TotalUser = totalCount
            });
        }
        private IActionResult GetFormattedDate(DateTime dateTime)
        {
            CultureInfo culture = new CultureInfo("ar-SA")
            {
                DateTimeFormat = { Calendar = new GregorianCalendar() },
                NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
            };

            string today = dateTime.ToString("dddd, dd MMMM yyyy", culture);
            return new JsonResult(new { date = today });
        }


    }
}
