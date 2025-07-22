using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using User.ApplicationDbContext;
using User.DTO;
using User.Model;

namespace User.Controllers
{
    [Route("api/")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DB _db;
        private readonly Functions _functions;
        private const int PageSize = 10;
        private const string ErrorMessage = "حدث خطأ برجاء المحاولة فى وقت لاحق";

        public AccountController(DB db, Functions functions)
        {
            _db = db;
            _functions = functions;
        }

        #region Helper Methods

        private CultureInfo GetArabicCulture()
        {
            return new CultureInfo("ar-SA")
            {
                DateTimeFormat = { Calendar = new GregorianCalendar() },
                NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
            };
        }

        private IActionResult HandleException(Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ApiResponse { Message = ErrorMessage });
        }

        private async Task<IActionResult> ExecuteWithExceptionHandling(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception)
            {
                return HandleException(new Exception());
            }
        }

        private async Task<Dictionary<string, JsonElement>> GetUserDataAsync(IEnumerable<string> userIds)
        {
            var userData = new Dictionary<string, JsonElement>();
            
            foreach (var userId in userIds.Where(id => !string.IsNullOrEmpty(id)))
            {
                if (!userData.ContainsKey(userId))
                {
                    var response = await _functions.SendAPI(userId);
                    if (response.HasValue)
                    {
                        userData[userId] = response.Value;
                    }
                }
            }
            
            return userData;
        }

        private async Task<Dictionary<string, JsonElement>> GetBrokerAndUserDataAsync(
            IEnumerable<(string brokerId, string userId)> pairs)
        {
            var data = new Dictionary<string, JsonElement>();
            
            foreach (var (brokerId, userId) in pairs.Where(p => !string.IsNullOrEmpty(p.brokerId) && !string.IsNullOrEmpty(p.userId)))
            {
                var key = $"{brokerId}_{userId}";
                if (!data.ContainsKey(key))
                {
                    var response = await _functions.BrokerandUser(brokerId, userId);
                    data[key] = response.Value;
                }
            }
            
            return data;
        }

        private GetOrdersDTO CreateBaseOrderDTO(NewOrder order, CultureInfo culture)
        {
            return new GetOrdersDTO
            {
                Id = order.Id.ToString(),
                statuOrder = order.statuOrder,
                Location = order.Location,
                typeOrder = order.numberOfTypeOrders?.FirstOrDefault()?.typeOrder,
                Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                BrokerID = order.Accept,
                Value = order.values?.FirstOrDefault()?.Value ?? 0
            };
        }

        private async Task<PagedResult<GetOrdersDTO>> GetPagedOrdersAsync(
            string statusFilter, 
            int page, 
            Func<IQueryable<NewOrder>, IQueryable<NewOrder>> additionalIncludes = null,
            Func<List<NewOrder>, CultureInfo, Task<List<GetOrdersDTO>>> mappingFunction = null)
        {
            var culture = GetArabicCulture();
            
            IQueryable<NewOrder> baseQuery = _db.newOrders
                .AsNoTracking()
                .Include(o => o.numberOfTypeOrders)
                .Include(o => o.values)
                .Where(l => l.statuOrder == statusFilter)
                .OrderByDescending(o => o.Date);

            if (additionalIncludes != null)
            {
                baseQuery = additionalIncludes(baseQuery);
            }

            var totalCount = await baseQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            var result = await baseQuery
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var ordersDTOs = mappingFunction != null 
                ? await mappingFunction(result, culture)
                : result.Select(order => CreateBaseOrderDTO(order, culture)).ToList();

            return new PagedResult<GetOrdersDTO>
            {
                TotalPages = totalPages,
                Page = page,
                TotalUser = totalCount,
                Data = ordersDTOs
            };
        }

        private async Task CreateLogAsync(string userId, int orderId, string message, string notes = "")
        {
            var log = new LogsDTO
            {
                UserId = userId,
                NewOrderId = orderId,
                Notes = notes,
                Message = message
            };
            await _functions.Logs(log);
        }

        #endregion

        #region API Endpoints

        [Authorize(Roles = "Account,Admin,Manager")]
        [HttpGet("Get-All-Done-Accept-Orders/{Page}")]
        public async Task<IActionResult> GetAllDoneAcceptOrders(int Page)
        {
            return await ExecuteWithExceptionHandling(async () =>
            {
                var result = await GetPagedOrdersAsync(
                    "تم التنفيذ", 
                    Page,
                    mappingFunction: async (orders, culture) =>
                    {
                        var userCustomerServicePairs = orders
                            .Where(o => !string.IsNullOrEmpty(o.Accept) && !string.IsNullOrEmpty(o.AcceptCustomerService))
                            .Select(o => (o.Accept!, o.AcceptCustomerService!))
                            .Distinct()
                            .ToList();

                        var userCustomerServiceData = await GetBrokerAndUserDataAsync(userCustomerServicePairs);

                        return orders.Select(order =>
                        {
                            var dto = CreateBaseOrderDTO(order, culture);
                            var key = $"{order.Accept}_{order.AcceptCustomerService}";
                            
                            if (userCustomerServiceData.TryGetValue(key, out JsonElement response))
                            {
                                if (response.TryGetProperty("user", out JsonElement user) &&
                                    response.TryGetProperty("broker", out JsonElement customerService))
                                {
                                    if (user.TryGetProperty("fullName", out JsonElement userName) &&
                                        user.TryGetProperty("email", out JsonElement userEmail) &&
                                        customerService.TryGetProperty("fullName", out JsonElement customerServiceName) &&
                                        customerService.TryGetProperty("email", out JsonElement customerServiceEmail))
                                    {
                                        dto.Email = userEmail.ToString();
                                        dto.fullName = userName.ToString();
                                        dto.CustomerServiceEmail = customerServiceEmail.ToString();
                                        dto.CustomerServiceName = customerServiceName.ToString();
                                    }
                                }
                            }
                            
                            return dto;
                        }).ToList();
                    });

                return Ok(new
                {
                    TotalPages = result.TotalPages,
                    Page = result.Page,
                    totalUser = result.TotalUser,
                    data = result.Data
                });
            });
        }

        [Authorize(Roles = "Account,Admin")]
        [HttpPost("Change-Statu-Account")]
        public async Task<IActionResult> ChangeStatusAccount(GetID getID)
        {
            return await ExecuteWithExceptionHandling(async () =>
            {
                var userId = User.FindFirstValue("ID");
                
                if (getID.ID == 0 || getID.BrokerID != null)
                {
                    return BadRequest(new ApiResponse { Message = "بيانات غير صحيحة" });
                }

                var order = await _db.newOrders.FirstOrDefaultAsync(l => l.Id == getID.ID);
                if (order == null)
                {
                    return NotFound(new ApiResponse { Message = "الطلب غير موجود" });
                }

                order.AcceptAccount = userId;

                if (getID.statuOrder == "true")
                {
                    order.statuOrder = "تم التحويل";
                    await _db.SaveChangesAsync();
                    await CreateLogAsync(userId, getID.ID, "تم تحويل الطلب الي الطلبات التي تم تحويلها بنجاح");
                }
                else if (getID.statuOrder == "false")
                {
                    order.statuOrder = "لم يتم التحويل";
                    
                    var notes = await _db.notesAccountings.FirstOrDefaultAsync(l => l.newOrderId == getID.ID);
                    if (notes == null)
                    {
                        notes = new NotesAccounting
                        {
                            newOrderId = getID.ID,
                            Notes = getID.Notes,
                            UserID = userId,
                        };
                        await _db.notesAccountings.AddAsync(notes);
                    }
                    else
                    {
                        notes.Notes = getID.Notes;
                        notes.UserID = userId;
                    }
                    
                    await _db.SaveChangesAsync();
                    await CreateLogAsync(userId, getID.ID, "تم تحويل الطلب الي خدمة العملاء مرة أخري");
                    await CreateLogAsync(userId, getID.ID, "تم إضافة ملاحظات من قبل المحاسب", order.Notes);
                }

                return Ok();
            });
        }

        [Authorize(Roles = "Account,Admin,Manager")]
        [HttpGet("Get-All-Done-Transfer-Orders/{Page}")]
        public async Task<IActionResult> GetAllDoneTransferOrders(int Page)
        {
            return await ExecuteWithExceptionHandling(async () =>
            {
                var result = await GetPagedOrdersAsync(
                    "تم التحويل", 
                    Page,
                    mappingFunction: async (orders, culture) =>
                    {
                        var userAccountPairs = orders
                            .Where(o => !string.IsNullOrEmpty(o.Accept) && !string.IsNullOrEmpty(o.AcceptAccount))
                            .Select(o => (o.Accept!, o.AcceptAccount!))
                            .Distinct()
                            .ToList();

                        var userAccountData = await GetBrokerAndUserDataAsync(userAccountPairs);

                        return orders.Select(order =>
                        {
                            var dto = CreateBaseOrderDTO(order, culture);
                            var key = $"{order.Accept}_{order.AcceptAccount}";
                            
                            if (userAccountData.TryGetValue(key, out JsonElement response))
                            {
                                if (response.TryGetProperty("user", out JsonElement user) &&
                                    response.TryGetProperty("broker", out JsonElement account))
                                {
                                    if (user.TryGetProperty("fullName", out JsonElement userName) &&
                                        user.TryGetProperty("email", out JsonElement userEmail) &&
                                        account.TryGetProperty("fullName", out JsonElement accountName) &&
                                        account.TryGetProperty("email", out JsonElement accountEmail))
                                    {
                                        dto.Email = userEmail.GetString();
                                        dto.fullName = userName.GetString();
                                        dto.AccountEmail = accountEmail.GetString();
                                        dto.AccountName = accountName.GetString();
                                    }
                                }
                            }
                            
                            return dto;
                        }).ToList();
                    });

                return Ok(new
                {
                    TotalPages = result.TotalPages,
                    Page = result.Page,
                    totalUser = result.TotalUser,
                    data = result.Data
                });
            });
        }

        [Authorize(Roles = "Account,Admin,Manager")]
        [HttpGet("Get-All-Not-Done-Transfer-Orders/{Page}")]
        public async Task<IActionResult> GetAllNotDoneTransferOrders(int Page = 1)
        {
            return await ExecuteWithExceptionHandling(async () =>
            {
                var result = await GetPagedOrdersAsync(
                    "لم يتم التحويل", 
                    Page,
                    query => query.Include(o => o.numberOfTypeOrders),
                    async (orders, culture) =>
                    {
                        var userIds = orders.Select(o => o.UserId).Where(id => !string.IsNullOrEmpty(id)).Distinct();
                        var userData = await GetUserDataAsync(userIds);

                        return orders.Select(order =>
                        {
                            var dto = CreateBaseOrderDTO(order, culture);
                            
                            if (userData.TryGetValue(order.UserId!, out JsonElement userInfo))
                            {
                                if (userInfo.TryGetProperty("fullName", out JsonElement fullNameElement) &&
                                    userInfo.TryGetProperty("phoneNumber", out JsonElement phoneNumberElement))
                                {
                                    dto.fullName = fullNameElement.GetString() ?? "";
                                    dto.phoneNumber = phoneNumberElement.GetString() ?? "";
                                }
                            }
                            
                            return dto;
                        }).ToList();
                    });

                return Ok(new
                {
                    TotalPages = result.TotalPages,
                    Page = result.Page,
                    totalUser = result.TotalUser,
                    data = result.Data
                });
            });
        }

        [Authorize(Roles = "CustomerService,Account,Admin,Manager")]
        [HttpPost("Get-All-Informatiom-From-Broker")]
        public async Task<IActionResult> GetAllInformationFromBroker(GetID getID)
        {
            return await ExecuteWithExceptionHandling(async () =>
            {
                if (string.IsNullOrEmpty(getID.BrokerID))
                {
                    return BadRequest(new ApiResponse { Message = "معرف الوسيط مطلوب" });
                }

                var response = await _functions.SendAPI(getID.BrokerID);
                return Ok(response);
            });
        }

        [Authorize(Roles = "Account,CustomerService,Admin,Manager")]
        [HttpPost("Get-Name-File-From-CustomerService")]
        public async Task<IActionResult> GetNameFileFromCustomerService(GetNewOrderID getNewOrderID)
        {
            return await ExecuteWithExceptionHandling(async () =>
            {
                if (getNewOrderID.newOrderId == 0)
                {
                    return BadRequest(new ApiResponse { Message = "معرف الطلب مطلوب" });
                }

                var file = await _db.notesCustomerServices
                    .Where(l => l.newOrderId == getNewOrderID.newOrderId)
                    .Select(l => new
                    {
                        fileName = Path.GetFileNameWithoutExtension(l.fileName),
                        l.Notes
                    })
                    .FirstOrDefaultAsync();

                return Ok(file ?? new { fileName = "", Notes = "" });
            });
        }

        [Authorize(Roles = "Account,Admin,Manager")]
        [HttpGet("Number-Of-Operations-Account")]
        public async Task<IActionResult> NumberOfOperations()
        {
            return await ExecuteWithExceptionHandling(async () =>
            {
                var userId = User.FindFirstValue("ID");
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new ApiResponse { Message = "المستخدم غير موجود" });
                }

                var listOfDone = await _db.newOrders
                    .Where(l => l.statuOrder == "تم التحويل")
                    .CountAsync();
                    
                var listForBroker = await _db.newOrders
                    .Where(l => l.statuOrder == "تم التنفيذ")
                    .CountAsync();

                return Ok(new
                {
                    ListOfDone = listOfDone,
                    ListForBroker = listForBroker,
                });
            });
        }

        #endregion
    }

    #region Helper Classes

    public class PagedResult<T>
    {
        public int TotalPages { get; set; }
        public int Page { get; set; }
        public int TotalUser { get; set; }
        public List<T> Data { get; set; } = new List<T>();
    }

    #endregion
}