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

        public AccountController(DB db, Functions functions)
        {
            _db = db;
            _functions = functions;
        }

        [Authorize(Roles = "Account,Admin,Manager")]
        [HttpGet("Get-All-Done-Accept-Orders/{Page}")]
        public async Task<IActionResult> GetAllDoneAcceptOrders(int Page)
        {
            try
            {
                const int PageSize = 10;

                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                // ✅ تحسين: استخدام Include و AsNoTracking
                var baseQuery = _db.newOrders
                    .AsNoTracking()
                    .Include(o => o.numberOfTypeOrders)
                    .Include(o => o.values)
                    .Where(l => l.statuOrder == "تم التنفيذ")
                    .OrderByDescending(o => o.Date);
                var totalCount = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                var result = await baseQuery
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                // ✅ تحسين: تجميع جميع الـ UserIDs و CustomerServiceIDs مرة واحدة
                var userCustomerServicePairs = result
                    .Where(o => !string.IsNullOrEmpty(o.Accept) && !string.IsNullOrEmpty(o.AcceptCustomerService))
                    .Select(o => new { o.Accept, o.AcceptCustomerService })
                    .Distinct()
                    .ToList();

                var userCustomerServiceData = new Dictionary<string, JsonElement>();

                foreach (var pair in userCustomerServicePairs)
                {
                    var key = $"{pair.Accept}_{pair.AcceptCustomerService}";
                    if (!userCustomerServiceData.ContainsKey(key))
                    {
                        var Response = await _functions.BrokerandUser(pair.Accept!, pair.AcceptCustomerService!);
                        userCustomerServiceData[key] = Response.Value;
                    }
                }

                // ✅ تحسين: استخدام Select بدلاً من foreach
                var ordersDTOs = result.Select(order =>
                {
                    var key = $"{order.Accept}_{order.AcceptCustomerService}";
                    var Response = userCustomerServiceData.GetValueOrDefault(key);

                    if (Response.TryGetProperty("user", out JsonElement User) &&
                        Response.TryGetProperty("broker", out JsonElement CustomerService))
                    {
                        if (User.TryGetProperty("fullName", out JsonElement UserName) &&
                            User.TryGetProperty("email", out JsonElement UserEmail) &&
                            CustomerService.TryGetProperty("fullName", out JsonElement CustomerServiceName) &&
                            CustomerService.TryGetProperty("email", out JsonElement CustomerServiceEmail))
                        {
                            return new GetOrdersDTO
                            {
                                Id = order.Id.ToString(),
                                statuOrder = order.statuOrder,
                                Location = order.Location,
                                typeOrder = order.numberOfTypeOrders?.FirstOrDefault()?.typeOrder,
                                Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                                Email = UserEmail.ToString(),
                                fullName = UserName.ToString(),
                                CustomerServiceEmail = CustomerServiceEmail.ToString(),
                                CustomerServiceName = CustomerServiceName.ToString(),
                                BrokerID = order.Accept,
                                Value = order.values?.FirstOrDefault()?.Value ?? 0
                            };
                        }
                    }

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
                }).ToList();

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = totalCount,
                    data = ordersDTOs
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }


        [Authorize(Roles = "Account,Admin")]
        [HttpPost("Change-Statu-Account")]
        public async Task<IActionResult> chageStatueAccount(GetID getID)
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                if (getID.ID != 0 && getID.BrokerID == null && getID.statuOrder == "true")
                {
                    var order = await _db.newOrders.FirstOrDefaultAsync(l => l.Id == getID.ID);
                    order!.statuOrder = "تم التحويل";
                    order.AcceptAccount = ID;
                    await _db.SaveChangesAsync();
                    var Logs = new LogsDTO
                    {
                        UserId = ID,
                        NewOrderId = getID.ID,
                        Notes = string.Empty,
                        Message = "تم تحويل الطلب الي الطلبات التي تم تحويلها بنجاح"
                    };
                    await _functions.Logs(Logs);
                }
                else if (getID.ID != 0 && getID.BrokerID == null && getID.statuOrder == "false")
                {
                    var order = await _db.newOrders.FirstOrDefaultAsync(l => l.Id == getID.ID);
                    order!.statuOrder = "لم يتم التحويل";
                    order.AcceptAccount = ID;
                    var notes = await _db.notesAccountings.FirstOrDefaultAsync(l => l.newOrderId == getID.ID);
                    if (notes == null)
                    {
                        notes = new NotesAccounting
                        {
                            newOrderId = getID.ID,
                            Notes = getID.Notes,
                            UserID = ID,
                        };
                        await _db.notesAccountings.AddAsync(notes);
                    }
                    else
                    {
                        notes.Notes = getID.Notes;
                        notes.UserID = ID;
                    }
                    await _db.SaveChangesAsync();
                    var Logs = new LogsDTO
                    {
                        UserId = ID,
                        NewOrderId = getID.ID,
                        Notes = string.Empty,
                        Message = "تم تحويل الطلب الي خدمة العملاء مرة أخري"
                    };
                    await _functions.Logs(Logs);
                    var Log = new LogsDTO
                    {
                        UserId = ID,
                        NewOrderId = getID.ID,
                        Notes = order.Notes,
                        Message = "تم إضافة ملاحظات من قبل المحاسب"
                    };
                    await _functions.Logs(Log);
                }
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "Account,Admin,Manager")]
        [HttpGet("Get-All-Done-Transfer-Orders/{Page}")]
        public async Task<IActionResult> GetAllDoneTransferOrders(int Page)
        {
            try
            {
                const int PageSize = 10;

                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                // ✅ تحسين: استخدام Include و AsNoTracking
                var baseQuery = _db.newOrders
                    .AsNoTracking()
                    .Include(o => o.numberOfTypeOrders)
                    .Include(o => o.values)
                    .Where(l => l.statuOrder == "تم التحويل")
                    .OrderByDescending(o => o.Date);

                var totalCount = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                var result = await baseQuery
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                // ✅ تحسين: تجميع جميع الـ UserIDs و AccountIDs مرة واحدة
                var userAccountPairs = result
                    .Where(o => !string.IsNullOrEmpty(o.Accept) && !string.IsNullOrEmpty(o.AcceptAccount))
                    .Select(o => new { o.Accept, o.AcceptAccount })
                    .Distinct()
                    .ToList();

                var userAccountData = new Dictionary<string, JsonElement>();

                foreach (var pair in userAccountPairs)
                {
                    var key = $"{pair.Accept}_{pair.AcceptAccount}";
                    if (!userAccountData.ContainsKey(key))
                    {
                        var Response = await _functions.BrokerandUser(pair.Accept!, pair.AcceptAccount!);
                        userAccountData[key] = Response.Value;
                    }
                }

                // ✅ تحسين: استخدام Select بدلاً من foreach
                var ordersDTOs = result.Select(order =>
                {
                    var key = $"{order.Accept}_{order.AcceptAccount}";
                    var Response = userAccountData.GetValueOrDefault(key);

                    if (Response.TryGetProperty("user", out JsonElement User) &&
                        Response.TryGetProperty("broker", out JsonElement Account))
                    {
                        if (User.TryGetProperty("fullName", out JsonElement UserName) &&
                            User.TryGetProperty("email", out JsonElement UserEmail) &&
                            Account.TryGetProperty("fullName", out JsonElement AccountName) &&
                            Account.TryGetProperty("email", out JsonElement AccountEmail))
                        {
                            return new GetOrdersDTO
                            {
                                Id = order.Id.ToString(),
                                statuOrder = order.statuOrder,
                                Location = order.Location,
                                typeOrder = order.numberOfTypeOrders?.FirstOrDefault()?.typeOrder,
                                Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                                Email = UserEmail.GetString(),
                                fullName = UserName.GetString(),
                                AccountEmail = AccountEmail.GetString(),
                                AccountName = AccountName.GetString(),
                                BrokerID = order.Accept,
                                Value = order.values?.FirstOrDefault()?.Value ?? 0
                            };
                        }
                    }

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
                }).ToList();

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = totalCount,
                    data = ordersDTOs
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }


        [Authorize(Roles = "Account,Admin,Manager")]
        [HttpGet("Get-All-Not-Done-Transfer-Orders/{Page}")]
        public async Task<IActionResult> GetAllNotDoneTransferOrders(int Page = 1)
        {
            try
            {
                const int PageSize = 10;

                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                // ✅ تحسين: استخدام Include و AsNoTracking
                var baseQuery = _db.newOrders
                    .AsNoTracking()
                    .Include(o => o.numberOfTypeOrders)
                    .Where(l => l.statuOrder == "لم يتم التحويل")
                    .OrderByDescending(o => o.Date);

                var totalCount = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                var result = await baseQuery
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                // ✅ تحسين: تجميع جميع الـ UserIDs مرة واحدة
                var userIds = result.Select(o => o.UserId).Distinct().ToList();
                var userData = new Dictionary<string, (string fullName, string phoneNumber)>();

                foreach (var userId in userIds)
                {
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var Response = await _functions.SendAPI(userId);
                        if (Response.HasValue &&
                            Response.Value.TryGetProperty("fullName", out JsonElement fullNameElement) &&
                            Response.Value.TryGetProperty("phoneNumber", out JsonElement phoneNumberElement))
                        {
                            userData[userId] = (fullNameElement.GetString() ?? "", phoneNumberElement.GetString() ?? "");
                        }
                    }
                }

                // ✅ تحسين: استخدام Select بدلاً من foreach
                var ordersDTOs = result.Select(order =>
                {
                    var userInfo = userData.GetValueOrDefault(order.UserId!, ("", ""));
                    return new GetOrdersDTO
                    {
                        Id = order.Id.ToString(),
                        statuOrder = order.statuOrder,
                        Location = order.Location,
                        typeOrder = order.numberOfTypeOrders?.FirstOrDefault()?.typeOrder,
                        Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                        fullName = userInfo.Item1,
                        phoneNumber = userInfo.Item2,
                        BrokerID = order.Accept
                    };
                }).ToList();

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = totalCount,
                    data = ordersDTOs
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }


        [Authorize(Roles = "CustomerService,Account,Admin,Manager")]
        [HttpPost("Get-All-Informatiom-From-Broker")]
        public async Task<IActionResult> getAllInformationFromBroker(GetID getID)
        {
            try
            {
                if (getID.BrokerID != null)
                {
                    var Response = await _functions.SendAPI(getID.BrokerID!);
                    return Ok(Response);
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }


        [Authorize(Roles = "Account,CustomerService,Admin,Manager")]
        [HttpPost("Get-Name-File-From-CustomerService")]
        public async Task<IActionResult> getNameFileFromCustomerService(GetNewOrderID getNewOrderID)
        {
            try
            {
                if (getNewOrderID.newOrderId != 0)
                {
                    var file = await _db.notesCustomerServices.Where(l => l.newOrderId == getNewOrderID.newOrderId).Select(l => new
                    {

                        fileName = Path.GetFileNameWithoutExtension(l.fileName),
                        l.Notes
                    })
                    .ToListAsync();
                    if (file.Any())
                    {
                        return Ok(file[0]);
                    }
                }
                return Ok(new string[] { });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "Account,Admin,Manager")]
        [HttpGet("Number-Of-Operations-Account")]
        public async Task<IActionResult> Numberofoperations()
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "المستخدم غير موجود" });
                }
                var ListOfDone = await _db.newOrders.Where(l => l.statuOrder == "تم التحويل").CountAsync();
                var ListForBroker = await _db.newOrders.Where(l => l.statuOrder == "تم التنفيذ" || l.statuOrder == "تم التنفيذ").CountAsync();
                return Ok(new
                {
                    ListOfDone = ListOfDone,
                    ListForBroker = ListForBroker,
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

    }
}