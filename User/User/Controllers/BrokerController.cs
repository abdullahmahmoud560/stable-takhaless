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

                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                var filteredOrders = await _db.newOrders
                    .Where(order => order.Date.HasValue &&
                                    order.Date.Value.AddDays(7) > DateTime.Now &&
                                    order.statuOrder == "قيد الإنتظار")
                    .OrderByDescending(order => order.Date)
                    .ToListAsync();

                List<GetOrdersDTO> ordersList = filteredOrders
                    .Select(order => new GetOrdersDTO
                    {
                        Id = order.Id.ToString(),
                        Location = order.Location,
                        Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                    })
                    .ToList();

                if (!ordersList.Any())
                {
                    return NotFound(new ApiResponse { Message = "لا توجد طلبات قيد الإنتظار" });
                }

                var totalCount = ordersList.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                var paginatedOrders = ordersList
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalOrders = totalCount,
                    data = paginatedOrders
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
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

                var order = await _db.newOrders.FirstOrDefaultAsync(o => o.Id == NewOrderId);
                if (order == null)
                {
                    return NotFound(new ApiResponse { Message = "الطلب غير موجود" });
                }

                var detailsOfOrder = await _db.typeOrders.Where(t => t.newOrderId == NewOrderId).ToListAsync();

                var fileNames = await _db.uploadFiles
                                        .Where(f => f.newOrderId == order.Id)
                                        .Select(f => new { f.fileName, f.fileUrl })
                                        .ToArrayAsync();
                List<GetDetailsOfOrders> details = new List<GetDetailsOfOrders>();

                if (!fileNames.Any()) { return Ok(details); }
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };


                details.Add(new GetDetailsOfOrders
                {
                    Id = order.Id,
                    Location = order.Location,
                    Date = order.Date?.ToString("dddd, dd MMMM yyyy", culture),
                    Notes = order.Notes,
                    numberOflicense = order.numberOfLicense,
                    typeOrder = detailsOfOrder[0].typeOrder,
                    Weight = detailsOfOrder[0].Weight,
                    Number = detailsOfOrder[0].Number,
                    Size = detailsOfOrder[0].Size,
                    fileName = fileNames.Select(g => g.fileName).ToArray()!,
                    City = order.City,
                    Town = order.Town,
                    zipCode = order.zipCode,
                    fileUrl = fileNames.Select(g => g.fileUrl).ToArray()!
                });

                for (int i = 1; i < detailsOfOrder.Count; i++)
                {
                    details.Add(new GetDetailsOfOrders
                    {
                        Id = order.Id,
                        Location = order.Location,
                        Date = order.Date?.ToString("dddd, dd MMMM yyyy", culture),
                        Notes = order.Notes,
                        numberOflicense = order.numberOfLicense,
                        typeOrder = detailsOfOrder[i].typeOrder,
                        Weight = detailsOfOrder[i].Weight,
                        Number = detailsOfOrder[i].Number,
                        Size = detailsOfOrder[i].Size,
                    });
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
                        var getAllValue = await _db.values.Where(l => l.newOrderId == getValue.newOrderId).ToListAsync();
                        var Count = await _db.newOrders.Where(l => l.Accept == ID && l.statuOrder == "تم التحويل").CountAsync();
                        if (!getAllValue.Any())
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

                var result = await _db.values
                    .Where(l => l.newOrderId == NewOrderId)
                    .ToListAsync();

                List<GetValue> values = new List<GetValue>();

                if (!result.Any())
                {
                    return Ok(new
                    {
                        TotalPages = 0,
                        Page = Page,
                        totalValues = 0,
                        data = values
                    });
                }

                foreach (var order in result)
                {
                    var count = await _db.newOrders
                        .Where(l => l.Accept == order.BrokerID && l.statuOrder == "تم التحويل")
                        .CountAsync();

                    values.Add(new GetValue
                    {
                        BrokerID = order.BrokerID,
                        Value = order.Value!.Value,
                        Count = count
                    });
                }

                var totalCount = values.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                var paginatedValues = values
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalValues = totalCount,
                    data = paginatedValues
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
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
                }

                List<GetOrdersDTO> getOrders = new();

                if (Role == "Broker")
                {
                    var result = await _db.values
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
                        .OrderByDescending(order => order.Date)
                        .ToListAsync();

                    getOrders = result.Select(order => new GetOrdersDTO
                    {
                        Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                        Location = order.Location,
                        statuOrder = "في إنتظار العميل لقبول عرضك",
                        Id = order.Id.ToString()
                    }).ToList();
                }
                else if (Role == "Admin" || Role == "Manager")
                {
                    var result = await _db.values
                        .GroupBy(v => v.newOrderId)
                        .Select(g => new { newOrderId = g.First().newOrderId, BrokerID = g.First().BrokerID })
                        .Join(_db.newOrders,
                            grouped => grouped.newOrderId,
                            order => order.Id,
                            (grouped, order) => new
                            {
                                order.Date,
                                order.statuOrder,
                                order.Location,
                                order.Id,
                                grouped.BrokerID
                            })
                        .Where(order => order.statuOrder == "قيد الإنتظار")
                        .OrderByDescending(order => order.Date)
                        .ToListAsync();

                    foreach (var order in result)
                    {
                        var ResponseUser = await _functions.SendAPI(order.BrokerID!);
                        if (ResponseUser.HasValue &&
                            ResponseUser.Value.TryGetProperty("fullName", out JsonElement fullName) &&
                            ResponseUser.Value.TryGetProperty("email", out JsonElement Email))
                        {
                            getOrders.Add(new GetOrdersDTO
                            {
                                Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                                statuOrder = "في إنتظار العميل لقبول عرض المخلص",
                                Location = order.Location,
                                Id = order.Id.ToString(),
                                BrokerName = fullName.ToString(),
                                BrokerEmail = Email.ToString(),
                            });
                        }
                    }
                }

                // Pagination
                var totalCount = getOrders.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                var paginatedOrders = getOrders
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalOrders = totalCount,
                    data = paginatedOrders
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

                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                List<GetOrdersDTO> ordersDTOs = new();

                if (Role == "Broker")
                {
                    var result = await _db.newOrders
                        .Where(l => l.Accept == ID && l.statuOrder == "تحت الإجراء")
                        .OrderByDescending(l => l.Date)
                        .ToListAsync();

                    foreach (var order in result)
                    {
                        var typeOrder = await _db.typeOrders.FirstOrDefaultAsync(l => l.newOrderId == order.Id);

                        ordersDTOs.Add(new GetOrdersDTO
                        {
                            Id = order.Id.ToString(),
                            statuOrder = order.statuOrder,
                            Location = order.Location,
                            typeOrder = typeOrder?.typeOrder,
                            Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                            Notes = order.Notes,
                        });
                    }
                }
                else if (Role == "User" || Role == "Company")
                {
                    var result = await _db.newOrders
                        .Where(l => l.UserId == ID && l.statuOrder == "تحت الإجراء" && l.Accept != null)
                        .OrderByDescending(l => l.Date)
                        .ToListAsync();

                    foreach (var order in result)
                    {
                        var typeOrder = await _db.typeOrders.FirstOrDefaultAsync(l => l.newOrderId == order.Id);

                        ordersDTOs.Add(new GetOrdersDTO
                        {
                            Id = order.Id.ToString(),
                            statuOrder = "في إنتظار المخلص لتنفيذ طلبك",
                            Location = order.Location,
                            typeOrder = typeOrder?.typeOrder,
                            Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                        });
                    }
                }
                else if (Role == "Admin" || Role == "Manager")
                {
                    var result = await _db.newOrders
                        .Where(l => l.statuOrder == "تحت الإجراء" && l.Accept != null)
                        .OrderByDescending(l => l.Date)
                        .ToListAsync();

                    foreach (var order in result)
                    {
                        var typeOrder = await _db.typeOrders.FirstOrDefaultAsync(l => l.newOrderId == order.Id);
                        var Response = await _functions.BrokerandUser(order.UserId!, order.Accept!);

                        if (Response.Value.TryGetProperty("user", out JsonElement user) &&
                            Response.Value.TryGetProperty("broker", out JsonElement broker))
                        {
                            if (user.TryGetProperty("fullName", out JsonElement userName) &&
                                user.TryGetProperty("email", out JsonElement userEmail) &&
                                broker.TryGetProperty("fullName", out JsonElement brokerName) &&
                                broker.TryGetProperty("email", out JsonElement brokerEmail))
                            {
                                ordersDTOs.Add(new GetOrdersDTO
                                {
                                    Id = order.Id.ToString(),
                                    statuOrder = "فى إنتظار المخلص لتنفيذ طلب العميل",
                                    Location = order.Location,
                                    typeOrder = typeOrder?.typeOrder,
                                    Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                                    fullName = userName.ToString(),
                                    Email = userEmail.ToString(),
                                    BrokerName = brokerName.ToString(),
                                    BrokerEmail = brokerEmail.ToString(),
                                });
                            }
                        }
                    }
                }

                var totalCount = ordersDTOs.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);
                var paginatedOrders = ordersDTOs
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page,
                    totalOrders = totalCount,
                    data = paginatedOrders
                });
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
                if (getID.ID != 0 && getID.BrokerID == null && getID.statuOrder == "true")
                {
                    var order = await _db.newOrders.FirstOrDefaultAsync(l => l.Id == getID.ID);
                    var values = await _db.values.Where(l => l.newOrderId == getID.ID).FirstOrDefaultAsync();
                    if (order!.step1 != null && order.step2 != null && order.step3 != null)
                    {
                        order!.statuOrder = "منفذ";
                        BackgroundJob.Delete(order.JopID);
                        BackgroundJob.Delete(values!.JopID);
                        await _db.SaveChangesAsync();
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
                    return BadRequest(new ApiResponse { Message = "استكمل مراحل الطلب في الطلبات الجاريةٍ" });
                }
                else if (getID.ID != 0 && getID.BrokerID == null && getID.statuOrder == "false")
                {
                    var order = await _db.newOrders.FirstOrDefaultAsync(l => l.Id == getID.ID);
                    order!.statuOrder = "ملغى";
                    await _db.SaveChangesAsync();
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

                CultureInfo culture = new("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                List<GetOrdersDTO> ordersDTOs = new();

                var baseQuery = _db.newOrders
                    .Where(l => l.statuOrder == "محولة" || l.statuOrder == "لم يتم التنفيذ");

                if (Role == "Broker")
                    baseQuery = baseQuery.Where(l => l.Accept == ID);

                var result = await baseQuery
                    .OrderByDescending(o => o.Date)
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                int totalCount = await baseQuery.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                foreach (var order in result)
                {
                    var typeOrder = await _db.typeOrders.FirstOrDefaultAsync(t => t.newOrderId == order.Id);

                    var dto = new GetOrdersDTO
                    {
                        Id = order.Id.ToString(),
                        statuOrder = order.statuOrder,
                        Location = order.Location,
                        typeOrder = typeOrder?.typeOrder,
                        Date = order.Date?.ToString("dddd, dd MMMM yyyy", culture),
                        Notes = order.Notes
                    };

                    if (Role != "Broker")
                    {
                        var response = await _functions.BrokerandUser(order.AcceptCustomerService!, order.Accept!);

                        if (response.Value.TryGetProperty("user", out var user) &&
                            response.Value.TryGetProperty("broker", out var broker))
                        {
                            dto.CustomerServiceEmail = user.GetProperty("email").ToString();
                            dto.CustomerServiceName = user.GetProperty("fullName").ToString();
                            dto.BrokerEmail = broker.GetProperty("email").ToString();
                            dto.BrokerName = broker.GetProperty("fullName").ToString();
                        }
                    }

                    ordersDTOs.Add(dto);
                }

                return Ok(new
                {
                    Page,
                    PageSize,
                    TotalPages = totalPages,
                    TotalCount = totalCount,
                    Data = ordersDTOs
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

                CultureInfo culture = new("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                var ordersList = new List<GetOrdersDTO>();

                if (Role == "Broker")
                {
                    var baseQuery = _db.values
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

                    foreach (var order in result)
                    {
                        ordersList.Add(new GetOrdersDTO
                        {
                            Date = order.Date?.ToString("dddd, dd MMMM yyyy", culture),
                            statuOrder = TranslateStatus(order.statuOrder!),
                            Location = order.Location,
                            Id = order.Id.ToString(),
                            Notes = order.Notes
                        });
                    }

                    return Ok(new { Page, PageSize, TotalPages = totalPages, TotalCount = totalCount, Data = ordersList });
                }
                else if (Role == "User" || Role == "Company")
                {
                    var baseQuery = _db.newOrders.Where(l => l.UserId == ID);

                    int totalCount = await baseQuery.CountAsync();
                    int totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                    var result = await baseQuery
                        .OrderByDescending(o => o.Date)
                        .Skip((Page - 1) * PageSize)
                        .Take(PageSize)
                        .ToListAsync();

                    foreach (var order in result)
                    {
                        ordersList.Add(new GetOrdersDTO
                        {
                            Date = order.Date?.ToString("dddd, dd MMMM yyyy", culture),
                            statuOrder = TranslateStatus(order.statuOrder!),
                            Location = order.Location,
                            Id = order.Id.ToString(),
                            Notes = order.Notes
                        });
                    }

                    return Ok(new { Page, PageSize, TotalPages = totalPages, TotalCount = totalCount, Data = ordersList });
                }

                return Ok(new { Page, PageSize, TotalPages = 0, TotalCount = 0, Data = ordersList });
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


        [Authorize(Roles = "Broker,User,Manager,Admin ,Company")]
        [HttpGet("Get-Count-Accept-Failed-Wait-Orders-Broker")]
        public async Task<IActionResult> CountProcessor()
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                var Role = User.FindFirstValue("Role");

                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
                }
                // جلب الطلبات بناءً على دور المستخدم
                var getAllOrders = Role switch
                {
                    "Broker" => await _db.newOrders.Where(l => l.Accept == ID).ToListAsync(),
                    "User" => await _db.newOrders.Where(l => l.UserId == ID).ToListAsync(),
                    _ => new List<NewOrder>() // في حال كان الدور غير معرف
                };

                if (!getAllOrders.Any()) // التحقق من وجود طلبات
                {
                    return Ok(new
                    {
                        AcceptOrder = 0,
                        FailedOrder = 0,
                        WaitOrder = 0,
                        TotalSuccessfulRequests = 0,
                        TotalFailedRequests = 0,
                        TotalWaitRequests = 0,
                    });
                }

                // حساب عدد الطلبات بناءً على الحالة
                int countAcceptOrders = getAllOrders.Count(l => l.statuOrder == "تم التحويل");
                int countFailedOrders = getAllOrders.Count(l => l.statuOrder == "ملغى" || l.statuOrder == "لم يتم التحويل" || l.statuOrder == "لم يتم التنفيذ" || l.statuOrder == "محذوفة");
                int countWaitingOrder = getAllOrders.Count(l => l.statuOrder == "منفذ" || l.statuOrder == "تم التنفيذ" || l.statuOrder == "تحت الإجراء" || l.statuOrder == "قيد الإنتظار");

                var ordersSuccessful = getAllOrders.Where(l => l.statuOrder == "تم التحويل").Select(o => o.Id).ToList();
                double sumAcceptOrder = await _db.values
                    .Where(v => ordersSuccessful.Contains(v.newOrderId!.Value) && v.Accept == true)
                    .SumAsync(v => (double?)v.Value) ?? 0.0;

                var ordersFailed = getAllOrders.Where(l => l.statuOrder == "ملغى" || l.statuOrder == "لم يتم التحويل" || l.statuOrder == "لم يتم التنفيذ" || l.statuOrder == "محذوفة").Select(o => o.Id).ToList();
                double sumFailedOrder = await _db.values
                    .Where(v => ordersFailed.Contains(v.newOrderId!.Value) && v.Accept == true)
                    .SumAsync(v => (double?)v.Value) ?? 0.0;

                var ordersWait = getAllOrders.Where(l => l.statuOrder == "منفذ" || l.statuOrder == "تم التنفيذ" || l.statuOrder == "تحت الإجراء" || l.statuOrder == "قيد الإنتظار").Select(o => o.Id).ToList();
                double sumWaitOrder = await _db.values
                    .Where(v => ordersWait.Contains(v.newOrderId!.Value) && v.Accept == true)
                    .SumAsync(v => (double?)v.Value) ?? 0.0;

                return Ok(new
                {
                    AcceptOrder = countAcceptOrders,
                    FailedOrder = countFailedOrders,
                    WaitOrder = countWaitingOrder,
                    TotalSuccessfulRequests = sumAcceptOrder,
                    TotalFailedRequests = sumFailedOrder,
                    TotalWaitRequests = sumWaitOrder,
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
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

                var NumberOfAllOrders = await _db.newOrders.Where(l => l.statuOrder == "قيد الإنتظار" && l.Date!.Value.AddDays(7) > DateTime.Now).CountAsync();
                if (Role == "Admin")
                {
                    var currentOrdersAdmin = await _db.values.GroupBy(v => v.newOrderId).Select(g => g.First().newOrderId)
                   .Join(
                       _db.newOrders,
                       newOrderId => newOrderId,
                       order => order.Id,
                       (newOrderId, order) => new
                       {
                           statuOrder = order.statuOrder
                       }).Where(order => order.statuOrder == "تحت الإجراء").CountAsync();

                    var applyOrdersAdmin = await _db.values.GroupBy(v => v.newOrderId).Select(g => g.First().newOrderId)
                        .Join(
                            _db.newOrders,
                            newOrderId => newOrderId,
                            order => order.Id,
                            (newOrderId, order) => new
                            {
                                statuOrder = order.statuOrder
                            }).Where(order => order.statuOrder == "قيد الإنتظار").CountAsync();

                    var customerServiceOrdersAdmin = await _db.values.GroupBy(v => v.newOrderId).Select(g => g.First().newOrderId)
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
                        currentOrders = currentOrdersAdmin,
                        applyOrders = applyOrdersAdmin,
                        customerServiceOrders = customerServiceOrdersAdmin
                    });
                }
                var currentOrders = await _db.values.Where(v => v.BrokerID == ID).GroupBy(v => v.newOrderId).Select(g => g.First().newOrderId)
                    .Join(
                        _db.newOrders,
                        newOrderId => newOrderId,
                        order => order.Id,
                        (newOrderId, order) => new
                        {
                            statuOrder = order.statuOrder
                        }).Where(order => order.statuOrder == "تحت الإجراء").CountAsync();

                var applyOrders = await _db.values.Where(v => v.BrokerID == ID).GroupBy(v => v.newOrderId).Select(g => g.First().newOrderId) // استخراج newOrderId فقط
                    .Join(
                        _db.newOrders,
                        newOrderId => newOrderId,
                        order => order.Id,
                        (newOrderId, order) => new
                        {
                            statuOrder = order.statuOrder
                        }).Where(order => order.statuOrder == "قيد الإنتظار").CountAsync();

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