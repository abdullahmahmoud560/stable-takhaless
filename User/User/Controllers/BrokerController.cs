using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using User.ApplicationDbContext;
using User.DTO;
using User.Model;

namespace User.Controllers
{
    [Route("api/")]
    [ApiController]
    public class BrokerController : ControllerBase
    {
        private readonly DB _db;
        private readonly Functions _functions;
        private readonly HangFire _hangfire;
        private readonly IDataProtector _protector;

        private static readonly CultureInfo ArabicCulture = new("ar-SA")
        {
            DateTimeFormat = { Calendar = new GregorianCalendar() },
            NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
        };

        public BrokerController(DB db, Functions functions, HangFire hangfire, IDataProtectionProvider provider)
        {
            _db = db;
            _functions = functions;
            _hangfire = hangfire;
            _protector = provider.CreateProtector("OrderIdProtector");
        }

        //جميع الطلبات المتاحة اللي هي قيد الإنتظار
        [Authorize(Roles = "Broker,Admin,Manager")]
        [HttpGet("Get-All-Orders/{Page}")]
        public async Task<IActionResult> GetOrders(int Page)
        {
            try
            {
                const int PageSize = 10;
                var culture = ArabicCulture; // ✅ الحل لتفادي التحذير

                var query = _db.newOrders
                    .AsNoTracking()
                    .Where(order => order.Date.HasValue &&
                                    order.Date.Value.AddDays(7) > DateTime.Now &&
                                    order.statuOrder == "قيد الإنتظار")
                    .OrderByDescending(order => order.Date);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                if (totalCount == 0)
                {
                    return NotFound(new ApiResponse { Message = "لا توجد طلبات قيد الإنتظار" });
                }

                var ordersList = await query
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .Select(order => new GetOrdersDTO
                    {
                        Id = order.Id.ToString(),
                        Location = order.Location,
                        Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                    })
                    .ToListAsync();

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = totalCount,
                    data = ordersList
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق"});
            }
        }


        // عرض جميع التفاصيل لعرض معين
        [Authorize(Roles = "Broker,User,Admin,Manager,Company")]
        [HttpGet("Get-Details/{OrderId}")]
        public async Task<IActionResult> getDetails(string OrderId)
        {
            try
            {
                int NewOrderId = int.Parse(_protector.Unprotect(OrderId));

                if (NewOrderId == 0)
                {
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف الطلب أو أنه غير صالح" });
                }

                // ✅ تحسين: استخدام Include لجلب جميع البيانات في استعلام واحد
                var order = await _db.newOrders
                    .AsNoTracking()
                    .Include(o => o.numberOfTypeOrders)
                    .Include(o => o.uploadFiles)
                    .FirstOrDefaultAsync(o => o.Id == NewOrderId);

                if (order == null)
                {
                    return NotFound(new ApiResponse { Message = "الطلب غير موجود" });
                }

                List<GetDetailsOfOrders> details = new List<GetDetailsOfOrders>();

                if (!order.uploadFiles?.Any() == true) { return Ok(details); }

                var fileNames = order.uploadFiles?.Select(f => new { f.fileName, f.fileUrl }).ToArray() ?? Array.Empty<object>();
                var detailsOfOrder = order.numberOfTypeOrders?.ToList() ?? new List<NumberOfTypeOrder>();

                if (detailsOfOrder.Any())
                {
                    var fileNameArray = fileNames.Cast<dynamic>().Select(g => (string)g.fileName).ToArray();
                    var fileUrlArray = fileNames.Cast<dynamic>().Select(g => (string)g.fileUrl).ToArray();

                    details.Add(new GetDetailsOfOrders
                    {
                        Id = order.Id,
                        Location = order.Location,
                        Date = order.Date?.ToString("dddd, dd MMMM yyyy", ArabicCulture),
                        Notes = order.Notes,
                        numberOflicense = order.numberOfLicense,
                        typeOrder = detailsOfOrder[0].typeOrder,
                        Weight = detailsOfOrder[0].Weight,
                        Number = detailsOfOrder[0].Number,
                        Size = detailsOfOrder[0].Size,
                        fileName = fileNameArray,
                        City = order.City,
                        Town = order.Town,
                        zipCode = order.zipCode,
                        fileUrl = fileUrlArray
                    });

                    for (int i = 1; i < detailsOfOrder.Count; i++)
                    {
                        details.Add(new GetDetailsOfOrders
                        {
                            Id = order.Id,
                            Location = order.Location,
                            Date = order.Date?.ToString("dddd, dd MMMM yyyy", ArabicCulture),
                            Notes = order.Notes,
                            numberOflicense = order.numberOfLicense,
                            typeOrder = detailsOfOrder[i].typeOrder,
                            Weight = detailsOfOrder[i].Weight,
                            Number = detailsOfOrder[i].Number,
                            Size = detailsOfOrder[i].Size,
                        });
                    }
                }

                return Ok(details);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        // يضيف قيمة جديدة
        [Authorize(Roles = "Broker")]
        [HttpPost("Apply-Order")]
        public async Task<IActionResult> applyOrder(GetValue getValue)
        {
            try
            {
                var ID = User.FindFirst("ID")?.Value;
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
                }
                if (getValue.Value != 0 && getValue.newOrderId != null)
                {
                    var appy = new Values
                    {
                        Value = getValue.Value,
                        BrokerID = ID,
                        newOrderId = getValue.newOrderId,
                    };
                    _db.Add(appy);
                    var result = await _db.SaveChangesAsync();
                    if (result > 0)
                    {
                        // ✅ تحسين: جلب Count في استعلام واحد
                        var valueData = await _db.values
                            .Where(l => l.newOrderId == getValue.newOrderId)
                            .Select(v => new { v.BrokerID, v.JopID })
                            .ToListAsync();
                        var Count = await _db.newOrders.Where(l => l.Accept == ID && l.statuOrder == "تم التحويل").CountAsync();
                        if (!valueData.Any())
                        {
                            return Ok(new string[] { });
                        }
                        var getValues = new GetValue
                        {
                            BrokerID = getValue.BrokerID,
                            Count = Count,
                            Value = getValue.Value,
                        };
                        var Logs = new LogsDTO
                        {
                            UserId = ID,
                            NewOrderId = getValue.newOrderId,
                            Message = "تم تقديم عرض جديد",
                            Notes = string.Empty,
                        };
                        await _functions.Logs(Logs);
                        var Jop = await _db.values.Where(l => l.newOrderId == getValue.newOrderId && l.BrokerID == ID).FirstOrDefaultAsync();
                        if (Jop != null && Jop.JopID == null)
                        {
                            var Response = await _functions.SendAPI(ID);
                            if (Response.HasValue && Response.Value.TryGetProperty("email", out JsonElement Email))
                            {
                                var Date = _db.newOrders.FirstOrDefault(l => l.Id == getValue.newOrderId);
                                var difference = (Date!.Date!.Value.AddDays(7) - DateTime.Now).TotalDays;
                                var JopID = BackgroundJob.Schedule(() => _hangfire.sendEmail(Email.ToString(), getValue.newOrderId!.Value, DateTime.Now.AddDays(difference)), DateTime.Now.AddDays(difference - 1));
                                Jop.JopID = JopID;
                                await _db.SaveChangesAsync();
                            }
                        }
                        return Ok(getValues);
                    }
                }
                return BadRequest(new ApiResponse { Message = "لم يتم تقديم العرض حاول مرة أخري" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        // جميع القيم بتاعت الطلب
        [Authorize(Roles = "Broker,User,Admin,Manager,Company")]
        [HttpGet("Get-all-Values/{OrderId}/{Page}")]
        public async Task<IActionResult> getAllValues(string OrderId, int Page)
        {
            try
            {
                const int PageSize = 8;

                int NewOrderId = int.Parse(_protector.Unprotect(OrderId.ToString()));
                if (NewOrderId == 0)
                {
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف الطلب" });
                }

                // ✅ تحسين: استخدام AsNoTracking و Projection
                var valuesQuery = _db.values
                    .AsNoTracking()
                    .Where(l => l.newOrderId == NewOrderId);

                var totalCount = await valuesQuery.CountAsync();
                if (totalCount == 0)
                {
                    return Ok(new
                    {
                        TotalPages = 0,
                        Page = Page,
                        totalUser = 0,
                        data = new List<GetValue>()
                    });
                }

                // ✅ تحسين: جلب البيانات مع Pagination
                var result = await valuesQuery
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                // ✅ تحسين: جلب جميع الـ BrokerIDs مرة واحدة
                var brokerIds = result.Select(v => v.BrokerID).Distinct().ToList();
                var brokerCounts = await _db.newOrders
                    .AsNoTracking()
                    .Where(o => brokerIds.Contains(o.Accept!) && o.statuOrder == "تم التحويل")
                    .GroupBy(o => o.Accept)
                    .Select(g => new { BrokerID = g.Key, Count = g.Count() })
                    .ToListAsync();

                var values = result.Select(order =>
                {
                    var count = brokerCounts.FirstOrDefault(b => b.BrokerID == order.BrokerID)?.Count ?? 0;
                    return new GetValue
                    {
                        BrokerID = order.BrokerID,
                        Value = order.Value!.Value,
                        Count = count
                    };
                }).ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = totalCount,
                    data = values
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        // الطلبات القائمة 
        [Authorize(Roles = "Broker,Admin,Manager")]
        [HttpGet("Current-Offers/{Page}")]
        public async Task<IActionResult> CurrentOffers(int Page)
        {
            var ID = User.FindFirst("ID")?.Value;
            var Role = User.FindFirstValue("Role");
            const int PageSize = 10;

            try
            {
                // ✅ تحسين: استخدام AsNoTracking و Projection مباشرة
                var query = _db.values
                    .AsNoTracking()
                    .Where(v => v.BrokerID == ID)
                    .GroupBy(v => v.newOrderId)
                    .Select(g => g.First().newOrderId)
                    .Join(_db.newOrders,
                        newOrderId => newOrderId,
                        order => order.Id,
                        (newOrderId, order) => new
                        {
                            order.Date,
                            order.Location,
                            order.statuOrder,
                            order.Id
                        })
                    .Where(order => order.statuOrder == "قيد الإنتظار")
                    .OrderByDescending(order => order.Date);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                var result = await query
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                var getOrders = result.Select(order => new GetOrdersDTO
                {
                    Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", ArabicCulture),
                    Location = order.Location,
                    statuOrder = "في إنتظار العميل لقبول عرضك",
                    Id = order.Id.ToString()
                }).ToList();

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = totalCount,
                    data = getOrders
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        // الطلبات الجارية
        [Authorize(Roles = "Broker,User,Admin,Manager,Company")]
        [HttpGet("Order-Requests/{Page}")]
        public async Task<IActionResult> OrderRequests(int Page)
        {
            try
            {
                const int PageSize = 10;
                var ID = User.FindFirst("ID")?.Value;
                var Role = User.FindFirst("Role")?.Value;

                if (string.IsNullOrEmpty(ID))
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });

                if (string.IsNullOrEmpty(Role))
                    return BadRequest(new ApiResponse { Message = "لا توجد أدوار لهذا المستخدم" });

                if (Role == "Broker")
                {
                    // ✅ تحسين: استخدام Include و AsNoTracking
                    var query = _db.newOrders
                        .AsNoTracking()
                        .Include(o => o.numberOfTypeOrders)
                        .Where(l => l.Accept == ID && (l.statuOrder != "قيد الإنتظار" && l.statuOrder != "تم التحويل"))
                        .OrderByDescending(l => l.Date);

                    var totalCount = await query.CountAsync();
                    var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                    var result = await query
                        .Skip((Page - 1) * PageSize)
                        .Take(PageSize)
                        .ToListAsync();

                    var ordersDTOs = result.Select(order => new GetOrdersDTO
                    {
                        Id = order.Id.ToString(),
                        statuOrder = order.statuOrder,
                        Location = order.Location,
                        typeOrder = order.numberOfTypeOrders?.FirstOrDefault()?.typeOrder,
                        Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", ArabicCulture),
                        Notes = order.Notes,
                    }).ToList();

                    return Ok(new
                    {
                        TotalPages = totalPages,
                        Page,
                        totalUser = totalCount,
                        data = ordersDTOs
                    });
                }
                else if (Role == "User" || Role == "Company")
                {
                    // ✅ تحسين: استخدام Include و AsNoTracking
                    var query = _db.newOrders
                        .AsNoTracking()
                        .Include(o => o.numberOfTypeOrders)
                        .Where(l => l.UserId == ID && (l.statuOrder != "تم التحويل" && l.statuOrder != "قيد الإنتظار") && l.Accept != null)
                        .OrderByDescending(l => l.Date);

                    var totalCount = await query.CountAsync();
                    var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                    var result = await query
                        .Skip((Page - 1) * PageSize)
                        .Take(PageSize)
                        .ToListAsync();

                    var ordersDTOs = result.Select(order => new GetOrdersDTO
                    {
                        Id = order.Id.ToString(),
                        statuOrder = "في إنتظار المخلص لتنفيذ طلبك",
                        Location = order.Location,
                        typeOrder = order.numberOfTypeOrders?.FirstOrDefault()?.typeOrder,
                        Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", ArabicCulture),
                    }).ToList();

                    return Ok(new
                    {
                        TotalPages = totalPages,
                        Page,
                        totalOrders = totalCount,
                        data = ordersDTOs
                    });
                }
                else if (Role == "Admin" || Role == "Manager")
                {
                    // ✅ تحسين: استخدام Include و AsNoTracking
                    var query = _db.newOrders
                        .AsNoTracking()
                        .Include(o => o.numberOfTypeOrders)
                        .Where(l => (l.statuOrder != "تم التحويل" && l.statuOrder != "قيد الإنتظار") && l.Accept != null)
                        .OrderByDescending(l => l.Date);

                    var totalCount = await query.CountAsync();
                    var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                    var result = await query
                        .Skip((Page - 1) * PageSize)
                        .Take(PageSize)
                        .ToListAsync();

                    // ✅ تحسين: جلب جميع الـ UserIDs و BrokerIDs دفعة واحدة (Batch)
                    var userBrokerPairs = result.Select(o => (o.UserId!, o.Accept!)).Distinct().ToList();
                    var userBrokerData = new Dictionary<string, JsonElement>();

                    if (userBrokerPairs.Any())
                    {
                        var userBrokerInfoList = await Task.WhenAll(userBrokerPairs.Select(async pair =>
                        {
                            var key = $"{pair.Item1}_{pair.Item2}";
                            var Response = await _functions.BrokerandUser(pair.Item1, pair.Item2);
                            return (key, Response.Value);
                        }));
                        userBrokerData = userBrokerInfoList.ToDictionary(x => x.key, x => x.Value);
                    }

                    var ordersDTOs = result.Select(order =>
                    {
                        var key = $"{order.UserId}_{order.Accept}";
                        var response = userBrokerData[key];

                        if (response.TryGetProperty("user", out JsonElement user) &&
                            response.TryGetProperty("broker", out JsonElement broker))
                        {
                            if (user.TryGetProperty("fullName", out JsonElement userName) &&
                                user.TryGetProperty("email", out JsonElement userEmail) &&
                                broker.TryGetProperty("fullName", out JsonElement brokerName) &&
                                broker.TryGetProperty("email", out JsonElement brokerEmail))
                            {
                                return new GetOrdersDTO
                                {
                                    Id = order.Id.ToString(),
                                    statuOrder = "فى إنتظار المخلص لتنفيذ طلب العميل",
                                    Location = order.Location,
                                    typeOrder = order.numberOfTypeOrders?.FirstOrDefault()?.typeOrder,
                                    Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", ArabicCulture),
                                    fullName = userName.ToString(),
                                    Email = userEmail.ToString(),
                                    BrokerName = brokerName.ToString(),
                                    BrokerEmail = brokerEmail.ToString(),
                                };
                            }
                        }
                        return new GetOrdersDTO
                        {
                            Id = order.Id.ToString(),
                            statuOrder = "فى إنتظار المخلص لتنفيذ طلب العميل",
                            Location = order.Location,
                            typeOrder = order.numberOfTypeOrders?.FirstOrDefault()?.typeOrder,
                            Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", ArabicCulture),
                        };
                    }).ToList();

                    return Ok(new
                    {
                        TotalPages = totalPages,
                        Page,
                        totalUser = totalCount,
                        data = ordersDTOs
                    });
                }

                return BadRequest(new ApiResponse { Message = "دور غير صالح" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "Broker")]
        [HttpPost("Change-Statu-Broker")]
        public async Task<IActionResult> chageStatueBroker(GetID getID)
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم" });
                }

                if (getID.ID <= 0)
                {
                    return BadRequest(new ApiResponse { Message = "معرف الطلب غير صحيح" });
                }

                if (getID.BrokerID == null && getID.statuOrder == "true")
                {
                    // ✅ تحسين: استخدام ExecuteUpdateAsync بدلاً من جلب الكائن وتحديثه
                    var order = await _db.newOrders
                        .AsNoTracking()
                        .Where(l => l.Id == getID.ID && l.step1 != null && l.step2 != null && l.step3 != null)
                        .Select(o => new { o.JopID, o.statuOrder })
                        .FirstOrDefaultAsync();

                    if (order == null)
                    {
                        return BadRequest(new ApiResponse { Message = "استكمل مراحل الطلب في الطلبات الجاريةٍ" });
                    }

                    var values = await _db.values
                        .AsNoTracking()
                        .Where(l => l.newOrderId == getID.ID)
                        .Select(v => new { v.JopID })
                        .FirstOrDefaultAsync();

                    // ✅ تحسين: استخدام ExecuteUpdateAsync للتحديث
                    await _db.newOrders
                        .Where(l => l.Id == getID.ID)
                        .ExecuteUpdateAsync(s => s.SetProperty(o => o.statuOrder, "منفذ"));

                    if (order.JopID != null) BackgroundJob.Delete(order.JopID);
                    if (values?.JopID != null) BackgroundJob.Delete(values.JopID);

                    var Logs = new LogsDTO
                    {
                        UserId = ID,
                        NewOrderId = getID.ID,
                        Message = "تم تنفيذ الطلب من قبل المخلص",
                        Notes = string.Empty,
                    };
                    await _functions.Logs(Logs);
                    return Ok();
                }
                else if (getID.BrokerID == null && getID.statuOrder == "false")
                {
                    // ✅ تحسين: استخدام ExecuteUpdateAsync للتحديث
                    var affectedRows = await _db.newOrders
                        .Where(l => l.Id == getID.ID)
                        .ExecuteUpdateAsync(s => s.SetProperty(o => o.statuOrder, "ملغى"));

                    if (affectedRows == 0)
                    {
                        return NotFound(new ApiResponse { Message = "الطلب غير موجود" });
                    }

                    var Logs = new LogsDTO
                    {
                        UserId = ID,
                        NewOrderId = getID.ID,
                        Message = "تم إلغاء الطلب من قبل المخلص",
                        Notes = string.Empty,
                    };
                    await _functions.Logs(Logs);
                    return Ok();
                }
                return BadRequest(new ApiResponse { Message = "هنا خطأ في البيانات المدخلة" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق ", Data = ex.Message });
            }
        }

        // الطلبات المحولة من خدمة العملاء
        [Authorize(Roles = "Broker,Admin,Manager")]
        [HttpGet("Order-Transfer-From-CustomerService/{Page}")]
        public async Task<IActionResult> orderTransferFromCustomerService(int Page)
        {
            try
            {
                const int PageSize = 10;
                var ID = User.FindFirst("ID")?.Value;
                var Role = User.FindFirstValue("Role");

                if (string.IsNullOrEmpty(ID))
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });

                // ✅ تحسين: استخدام Include و AsNoTracking
                var baseQuery = _db.newOrders
                    .AsNoTracking()
                    .Include(o => o.numberOfTypeOrders)
                    .Where(l => l.statuOrder == "محولة" || l.statuOrder == "لم يتم التنفيذ");

                if (Role == "Broker")
                    baseQuery = baseQuery.Where(l => l.Accept == ID);

                int totalCount = await baseQuery.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                var result = await baseQuery
                    .OrderByDescending(o => o.Date)
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                // ✅ تحسين: جلب جميع الـ UserIDs و BrokerIDs دفعة واحدة (Batch)
                var userBrokerData = new Dictionary<string, JsonElement>();
                if (Role != "Broker")
                {
                    var uniquePairs = result
                        .Where(o => !string.IsNullOrEmpty(o.AcceptCustomerService) && !string.IsNullOrEmpty(o.Accept))
                        .Select(o => (o.AcceptCustomerService!, o.Accept!))
                        .Distinct()
                        .ToList();

                    if (uniquePairs.Any())
                    {
                        var userBrokerInfoList = await Task.WhenAll(uniquePairs.Select(async pair =>
                        {
                            var key = $"{pair.Item1}_{pair.Item2}";
                            var response = await _functions.BrokerandUser(pair.Item1, pair.Item2);
                            return (key, response.Value);
                        }));
                        userBrokerData = userBrokerInfoList.ToDictionary(x => x.key, x => x.Value);
                    }
                }

                var ordersDTOs = result.Select(order =>
                {
                    var dto = new GetOrdersDTO
                    {
                        Id = order.Id.ToString(),
                        statuOrder = order.statuOrder,
                        Location = order.Location,
                        typeOrder = order.numberOfTypeOrders?.FirstOrDefault()?.typeOrder,
                        Date = order.Date?.ToString("dddd, dd MMMM yyyy", ArabicCulture),
                        Notes = order.Notes
                    };

                    if (Role != "Broker" && !string.IsNullOrEmpty(order.AcceptCustomerService) && !string.IsNullOrEmpty(order.Accept))
                    {
                        var key = $"{order.AcceptCustomerService}_{order.Accept}";
                        if (userBrokerData.TryGetValue(key, out var response))
                        {
                            if (response.TryGetProperty("user", out var user) &&
                                response.TryGetProperty("broker", out var broker))
                            {
                                dto.CustomerServiceEmail = user.GetProperty("email").ToString();
                                dto.CustomerServiceName = user.GetProperty("fullName").ToString();
                                dto.BrokerEmail = broker.GetProperty("email").ToString();
                                dto.BrokerName = broker.GetProperty("fullName").ToString();
                            }
                        }
                    }

                    return dto;
                }).ToList();

                return Ok(new
                {
                    Page = Page,
                    TotalPages = totalPages,
                    totalUser = totalCount,
                    data = ordersDTOs
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        //السجل للمخلص
        [Authorize(Roles = "Broker,User,Company")]
        [HttpGet("Get-All-Orders-Brokers/{Page}")]
        public async Task<IActionResult> getAllOrdersBrokers(int Page)
        {
            try
            {
                const int PageSize = 10;
                var ID = User.FindFirstValue("ID");
                var Role = User.FindFirstValue("Role");

                if (string.IsNullOrEmpty(ID))
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });

                if (Role == "Broker")
                {
                    // ✅ تحسين: استخدام AsNoTracking و Projection
                    var baseQuery = _db.values
                        .AsNoTracking()
                        .Where(v => v.BrokerID == ID)
                        .GroupBy(v => v.newOrderId)
                        .Select(g => g.First().newOrderId)
                        .Join(_db.newOrders,
                              newOrderId => newOrderId,
                              order => order.Id,
                              (newOrderId, order) => order);

                    int totalCount = await baseQuery.CountAsync();
                    int totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                    var result = await baseQuery
                        .OrderByDescending(o => o.Date)
                        .Skip((Page - 1) * PageSize)
                        .Take(PageSize)
                        .ToListAsync();

                    var ordersList = result.Select(order => new GetOrdersDTO
                    {
                        Date = order.Date?.ToString("dddd, dd MMMM yyyy", ArabicCulture),
                        statuOrder = TranslateStatus(order.statuOrder!),
                        Location = order.Location,
                        Id = order.Id.ToString(),
                        Notes = order.Notes
                    }).ToList();

                    return Ok(new { Page, PageSize, TotalPages = totalPages, TotalCount = totalCount, Data = ordersList });
                }
                else if (Role == "User" || Role == "Company")
                {
                    // ✅ تحسين: استخدام AsNoTracking و Projection
                    var baseQuery = _db.newOrders
                        .AsNoTracking()
                        .Where(l => l.UserId == ID);

                    int totalCount = await baseQuery.CountAsync();
                    int totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                    var result = await baseQuery
                        .OrderByDescending(o => o.Date)
                        .Skip((Page - 1) * PageSize)
                        .Take(PageSize)
                        .ToListAsync();

                    var ordersList = result.Select(order => new GetOrdersDTO
                    {
                        Date = order.Date?.ToString("dddd, dd MMMM yyyy", ArabicCulture),
                        statuOrder = TranslateStatus(order.statuOrder!),
                        Location = order.Location,
                        Id = order.Id.ToString(),
                        Notes = order.Notes
                    }).ToList();

                    return Ok(new { Page = Page, TotalPages = totalPages, totalUser = totalCount, data = ordersList });
                }

                return Ok(new { Page = Page, TotalPages = 0, totalUser = 0, data = new List<GetOrdersDTO>() });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        private string TranslateStatus(string statuOrder)
        {
            return statuOrder switch
            {
                "تحت الإجراء" => "في إنتظارك لقبول الطلب",
                "قيد الإنتظار" => "فى إنتظار تقديم العروض",
                "لم يتم التحويل" => "في إنتظار رد خدمة العملاء",
                "تم التحويل" => "تم تنفيذ العملية بنجاح",
                "منفذ" => "في إنتظار رد خدمة العملاء",
                "ملغى" => "في إنتظار رد خدمة العملاء",
                "محذوفة" => "تم حذف الطلب من قبل خدمة العملاء",
                "لم يتم التنفيذ" => "في إنتظار رد خدمة العملاء",
                "تم التنفيذ" => "في إنتظار رد المحاسب",
                _ => statuOrder
            };
        }

        [Authorize(Roles = "Broker,Admin,Manager")]
        [HttpGet("Number-Of-Operations-Broker")]
        public async Task<IActionResult> Numberofoperations()
        {
            try
            {
                var Role = User.FindFirstValue("Role");
                var ID = User.FindFirstValue("ID");
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
                }

                // ✅ تحسين: استخدام استعلام واحد بدلاً من استعلامات منفصلة
                var NumberOfAllOrders = await _db.newOrders
                    .AsNoTracking()
                    .Where(l => l.statuOrder == "قيد الإنتظار" && l.Date!.Value.AddDays(7) > DateTime.Now)
                    .CountAsync();

                if (Role == "Admin")
                {
                    // ✅ تحسين: استخدام استعلام واحد مع GroupBy
                    var adminStats = await _db.values
                        .AsNoTracking()
                        .GroupBy(v => v.newOrderId)
                        .Select(g => g.First().newOrderId)
                        .Join(_db.newOrders,
                            newOrderId => newOrderId,
                            order => order.Id,
                            (newOrderId, order) => order.statuOrder)
                        .GroupBy(status => status)
                        .Select(g => new { Status = g.Key, Count = g.Count() })
                        .ToListAsync();

                    var currentOrdersAdmin = adminStats.FirstOrDefault(s => s.Status != "قيد الإنتظار" && s.Status !="تم التحويل")?.Count ?? 0;
                    var applyOrdersAdmin = adminStats.FirstOrDefault(s => s.Status == "قيد الإنتظار")?.Count ?? 0;
                    var customerServiceOrdersAdmin = adminStats.FirstOrDefault(s => s.Status == "لم يتم التنفيذ")?.Count ?? 0;

                    return Ok(new
                    {
                        NumberOfAllOrders = NumberOfAllOrders,
                        currentOrders = currentOrdersAdmin,
                        applyOrders = applyOrdersAdmin,
                        customerServiceOrders = customerServiceOrdersAdmin
                    });
                }

                // ✅ تحسين: استخدام استعلام واحد مع GroupBy للـ Broker
                var brokerStats = await _db.values
                    .AsNoTracking()
                    .Where(v => v.BrokerID == ID)
                    .GroupBy(v => v.newOrderId)
                    .Select(g => g.First().newOrderId)
                    .Join(_db.newOrders,
                        newOrderId => newOrderId,
                        order => order.Id,
                        (newOrderId, order) => order.statuOrder)
                    .GroupBy(status => status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();


                var currentOrders = brokerStats.FirstOrDefault(s => s.Status != "قيد الإنتظار" && s.Status != "تم التحويل")?.Count ?? 0;
                var applyOrders = brokerStats.FirstOrDefault(s => s.Status == "قيد الإنتظار")?.Count ?? 0;

                var customerServiceOrders = await _db.values.Where(v => v.BrokerID == ID).GroupBy(v => v.newOrderId).Select(g => g.First().newOrderId) // استخراج newOrderId فقط
                    .Join(
                        _db.newOrders,
                        newOrderId => newOrderId,
                        order => order.Id,
                        (newOrderId, order) => new
                        {
                            statuOrder = order.statuOrder
                        }).Where(order => order.statuOrder == "لم يتم التنفيذ").CountAsync();

                return Ok(new
                {
                    NumberOfAllOrders = NumberOfAllOrders,
                    currentOrders = currentOrders,
                    applyOrders = applyOrders,
                    customerServiceOrders = customerServiceOrders
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "Broker,Admin,Manager")]
        [HttpPost("Trace-Order-Broker")]
        public async Task<IActionResult> TraceCodeBroker(TraceDTO traceDTO)
        {
            try
            {
                if (traceDTO.newOrderId != null)
                {
                    var result = await _db.newOrders.Where(l => l.Id == traceDTO.newOrderId).FirstOrDefaultAsync();
                    result!.step1 = traceDTO.step1;
                    result.step2 = traceDTO.step2;
                    result.step3 = traceDTO.step3;
                    await _db.SaveChangesAsync();
                    return Ok("تم تحديث الطلب بنجاح");
                }
                return BadRequest(new ApiResponse { Message = "لم يتم العثور على الطلب" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "Broker,Admin,Manager,User,Company")]
        [HttpGet("Get-Trace-Order-User/{OrderId}")]
        public async Task<IActionResult> TraceCodeUser(string OrderId)
        {
            try
            {
                int NewOrderId = int.Parse(_protector.Unprotect(OrderId.ToString()));
                if (NewOrderId != 0)
                {
                    var result = await _db.newOrders.Where(l => l.Id == NewOrderId).Select(l => new { l.step1, l.step2, l.step3 }).FirstOrDefaultAsync();
                    if (result != null)
                    {
                        return Ok(result);
                    }
                }
                return BadRequest(new ApiResponse { Message = "لم يتم العثور على الطلب" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize]
        [HttpPost("Data-Protection")]
        public IActionResult data(OrderIdDTO orderId)
        {
            if (orderId.orderId != 0)
            {
                var portected = _protector.Protect(orderId.orderId.ToString());
                return Ok(portected);
            }
            return Ok();
        }
    }
}
