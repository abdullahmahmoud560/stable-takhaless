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
        [HttpGet("Get-All-Orders")]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };
                var filteredOrders = await _db.newOrders
                    .Where(order => order.Date.HasValue &&
                                    order.Date.Value.AddDays(7) > DateTime.Now &&
                                    order.statuOrder == "قيد الإنتظار")
                    .Select(order => new GetOrdersDTO
                    {
                        Id = order.Id.ToString(),
                        Location = order.Location,
                        Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                    })
                    .ToListAsync();

                if (!filteredOrders.Any())
                {
                    return NotFound(new ApiResponse { Message = "لا توجد طلبات قيد الإنتظار" });
                }

                return Ok(filteredOrders);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
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
                                        .Select(f =>new { f.fileName , f.fileUrl })
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
        [HttpGet("Get-all-Values/{OrderId}")]
        public async Task<IActionResult> getAllValues(string OrderId)
        {
            try
            {
                int NewOrderId = int.Parse(_protector.Unprotect(OrderId.ToString()));
                if (NewOrderId == 0) { return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف الطلب" }); }
                var result = await _db.values.Where(l => l.newOrderId == NewOrderId).ToListAsync();
                List<GetValue> values = new List<GetValue>();

                if (!result.Any())
                {
                    return Ok(values);
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

                return Ok(values);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        // الطلبات القائمة 
        [Authorize(Roles = "Broker,Admin,Manager")]
        [HttpGet("Current-Offers")]
        public async Task<IActionResult> currentOffers()
        {
            var ID = User.FindFirst("ID")?.Value;
            var Role = User.FindFirstValue("Role");
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
                if (Role == "Broker")
                {
                    var result = await _db.values
                    .Where(v => v.BrokerID == ID)
                    .GroupBy(v => v.newOrderId)
                    .Select(g => g.First().newOrderId) // استخراج newOrderId فقط
                    .Join(
                        _db.newOrders,
                        newOrderId => newOrderId,
                        order => order.Id,
                        (newOrderId, order) => new
                        {
                            // استرجاع الحقول المطلوبة فقط
                            Date = order.Date,
                            Location = order.Location,
                            statuOrder = order.statuOrder,
                            Id = order.Id
                        })
                    .Where(order => order.statuOrder == "قيد الإنتظار")
                    .Select(order => new GetOrdersDTO // تحويل النتائج إلى GetOrdersDTO
                    {
                        Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                        Location = order.Location,
                        statuOrder = "في إنتظار العميل لقبول عرضك",
                        Id = order.Id.ToString()
                    })
                    .ToListAsync();


                    return Ok(result);
                }
                else if (Role == "Admin")
                {

                    var result = await _db.values
                             .GroupBy(v => v.newOrderId)
                             .Select(g => new { newOrderId = g.First().newOrderId, BrokerID = g.First().BrokerID }) // تعريف الحقول بشكل صريح
                             .Join(
                                 _db.newOrders,
                                 grouped => grouped.newOrderId, // استخراج المفتاح الصحيح للمقارنة
                                 order => order.Id,
                                 (grouped, order) => new
                                 {
                                     // استرجاع الحقول المطلوبة فقط
                                     Date = order.Date,
                                     statuOrder = order.statuOrder,
                                     Location = order.Location,
                                     Id = order.Id,
                                     BrokerID = grouped.BrokerID // التأكد من استخدام الحقل الصحيح
                                 })
                             .Where(order => order.statuOrder == "قيد الإنتظار")
                             .ToListAsync();
                    List<GetOrdersDTO> getOrders = new List<GetOrdersDTO>();

                    if (!result.Any())

                    { return Ok(getOrders); }

                    foreach (var order in result)
                    {
                        var ResponseUser = await _functions.SendAPI(order.BrokerID!);
                        if (ResponseUser.HasValue && ResponseUser.Value.TryGetProperty("fullName", out JsonElement fullName) && ResponseUser.Value.TryGetProperty("email", out JsonElement Email))
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
                        return Ok(getOrders);
                    }
                }
                return Ok(new string[] { });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }

        }


        // الطلبات الجارية
        [Authorize(Roles = "Broker,User,Admin,Manager,Company")]
        [HttpGet("Order-Requests")]
        public async Task<IActionResult> orderRequests()
        {

            try
            {
                var ID = User.FindFirst("ID")?.Value;
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
                }
                var Role = User.FindFirst("Role")?.Value;
                if (string.IsNullOrEmpty(Role))
                {
                    return BadRequest(new ApiResponse { Message = "لا توجد ادوار لهذا المستخدم" });
                }
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                if (Role == "Broker")
                {
                    var resultBroker = await _db.newOrders.Where(l => l.Accept == ID).Where(l => l.statuOrder == "تحت الإجراء").ToListAsync();
                    List<GetOrdersDTO> ordersDTOs = new List<GetOrdersDTO>();
                    if (resultBroker.Any())
                    {


                        foreach (var order in resultBroker)
                        {
                            var typeOrder = await _db.typeOrders
                                .Where(l => l.newOrderId == order.Id)
                                .FirstOrDefaultAsync();

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
                        return Ok(ordersDTOs);
                    }
                }
                else if (Role == "User" || Role == "Company")
                {
                    var result = await _db.newOrders.Where(l => l.UserId == ID && l.statuOrder == "تحت الإجراء" && l.Accept != null).ToListAsync();
                    if (result.Any())
                    {
                        List<GetOrdersDTO> getOrdersDTOs = new List<GetOrdersDTO>();

                        foreach (var order in result)
                        {
                            var typeOrders = await _db.typeOrders
                                .Where(l => l.newOrderId == order.Id)
                                .ToListAsync();

                            getOrdersDTOs.Add(new GetOrdersDTO
                            {
                                Id = order.Id.ToString(),
                                statuOrder = "في إنتظار المخلص لتنفيذ طلبك",
                                Location = order.Location,
                                typeOrder = typeOrders.First().typeOrder,
                                Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                            });
                        }

                        return Ok(getOrdersDTOs);
                    }
                }
                else if (Role == "Admin")
                {
                    var result = await _db.newOrders.Where(l => l.statuOrder == "تحت الإجراء" && l.Accept != null).ToListAsync();
                    if (result.Any())
                    {
                        List<GetOrdersDTO> getOrdersDTOs = new List<GetOrdersDTO>();

                        foreach (var order in result)
                        {
                            var typeOrders = await _db.typeOrders
                                .Where(l => l.newOrderId == order.Id && order.Accept != null)
                                .ToListAsync();
                            var Response = await _functions.BrokerandUser(order.UserId!, order.Accept!);

                            if (Response.Value.TryGetProperty("user", out JsonElement User) &&
                                Response.Value.TryGetProperty("broker", out JsonElement Broker))
                            {
                                if (User.TryGetProperty("fullName", out JsonElement UserName) &&
                                    User.TryGetProperty("email", out JsonElement UserEmail) &&
                                    Broker.TryGetProperty("fullName", out JsonElement BrokerName) &&
                                    Broker.TryGetProperty("email", out JsonElement BrokerEmail))
                                    getOrdersDTOs.Add(new GetOrdersDTO
                                    {
                                        Id = order.Id.ToString(),
                                        statuOrder = "فى إنتظار المخلص لتنفيذ طلب العميل",
                                        Location = order.Location,
                                        typeOrder = typeOrders.First().typeOrder,
                                        Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                                        fullName = UserName.ToString(),
                                        Email = UserEmail.ToString(),
                                        BrokerEmail = BrokerEmail.ToString(),
                                        BrokerName = BrokerName.ToString(),
                                    });
                            }
                        }
                        return Ok(getOrdersDTOs);
                    }
                }
                return Ok(new string[] { });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
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
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        // الطلبات المحولة من خدمة العملاء
        [Authorize(Roles = "Broker,Admin,Manager")]
        [HttpGet("Order-Transfer-From-CustomerService")]
        public async Task<IActionResult> orderTransferFromCustomerService()
        {

            try
            {
                var ID = User.FindFirst("ID")?.Value;
                var Role = User.FindFirstValue("Role");
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
                }
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };
                if (Role == "Broker")
                {
                    var resultBroker = await _db.newOrders
                                .Where(l => l.Accept == ID && l.statuOrder == "محولة" || l.statuOrder == "لم يتم التنفيذ")
                                .Select(order => new
                                {
                                    order.Id,
                                    order.statuOrder,
                                    order.Location,
                                    order.Date,
                                    order.Notes
                                })
                                .ToListAsync();
                    List<GetOrdersDTO> ordersDTOs = new List<GetOrdersDTO>();
                    if (resultBroker.Any())
                    {
                        foreach (var order in resultBroker)
                        {
                            var typeOrder = await _db.typeOrders
                                    .Where(l => l.newOrderId == order.Id)
                                    .Select(l => new { l.typeOrder })
                                    .FirstOrDefaultAsync();

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
                        return Ok(ordersDTOs);
                    }
                }
                else if (Role == "Admin")
                {
                    var resultBroker = await _db.newOrders
                               .Where(l => l.statuOrder == "محولة" || l.statuOrder == "لم يتم التنفيذ")
                               .Select(order => new
                               {
                                   order.Id,
                                   order.statuOrder,
                                   order.Location,
                                   order.Date,
                                   order.Notes,
                                   order.Accept,
                                   order.AcceptCustomerService,
                               })
                               .ToListAsync();
                    List<GetOrdersDTO> ordersDTOs = new List<GetOrdersDTO>();

                    if (resultBroker.Any())
                    {
                        foreach (var order in resultBroker)
                        {
                            var typeOrder = await _db.typeOrders
                                    .Where(l => l.newOrderId == order.Id)
                                    .Select(l => new { l.typeOrder })
                                    .FirstOrDefaultAsync();
                            var Response = await _functions.BrokerandUser(order.AcceptCustomerService!, order.Accept!);
                            if (Response.Value.TryGetProperty("user", out JsonElement User) &&
                               Response.Value.TryGetProperty("broker", out JsonElement Broker))
                            {
                                if (User.TryGetProperty("fullName", out JsonElement CustomerServiceName) &&
                                    User.TryGetProperty("email", out JsonElement CustomerServiceEmail) &&
                                    Broker.TryGetProperty("fullName", out JsonElement BrokerName) &&
                                    Broker.TryGetProperty("email", out JsonElement BrokerEmail))
                                    ordersDTOs.Add(new GetOrdersDTO
                                    {
                                        Id = order.Id.ToString(),
                                        statuOrder = order.statuOrder,
                                        Location = order.Location,
                                        typeOrder = typeOrder?.typeOrder,
                                        Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                                        Notes = order.Notes,
                                        CustomerServiceEmail = CustomerServiceEmail.ToString(),
                                        CustomerServiceName = CustomerServiceName.ToString(),
                                        BrokerEmail = BrokerEmail.ToString(),
                                        BrokerName = BrokerName.ToString(),
                                    });
                            }
                            return Ok(ordersDTOs);
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


        //السجل للمخلص
        [Authorize(Roles = "Broker,User,Company")]
        [HttpGet("Get-All-Orders-Brokers")]
        public async Task<IActionResult> getAllOrdersBrokers()
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                var Role = User.FindFirstValue("Role");
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
                }
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };
                if (Role == "Broker")
                {
                    var result = await _db.values
                            .Where(v => v.BrokerID == ID)
                            .GroupBy(v => v.newOrderId)
                            .Select(g => g.First().newOrderId)
                            .Join(
                                _db.newOrders,
                                newOrderId => newOrderId,
                                order => order.Id,
                                (newOrderId, order) => new GetOrdersDTO
                                {
                                    Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                                    statuOrder = order.statuOrder == "تحت الإجراء" ? "في إنتظارك لقبول الطلب" :
                                                 order.statuOrder == "قيد الإنتظار" ? "فى إنتظار العميل قبول عرضك" :
                                                 order.statuOrder == "لم يتم التحويل" ? "في إنتظار رد خدمة العملاء" :
                                                 order.statuOrder == "تم التحويل" ? "تم تنفيذ العملية بنجاح" :
                                                 order.statuOrder == "منفذ" ? "في إنتظار رد خدمة العملاء" :
                                                 order.statuOrder == "ملغى" ? "في إنتظار رد خدمة العملاء" :
                                                 order.statuOrder == "محذوفة" ? "تم حذف الطلب من قبل خدمة العملاء" :
                                                 order.statuOrder == "لم يتم التنفيذ" ? "في إنتظار رد خدمة العملاء" :
                                                 order.statuOrder == "تم التنفيذ" ? "في إانتظار رد المحاسب" :
                                                 "",
                                    Location = order.Location,
                                    Id = order.Id.ToString(),
                                    Notes = order.Notes,
                                })
                            .ToListAsync();
                    return Ok(result);
                }
                else if (Role == "User" || Role == "Company")
                {
                    var result = await _db.newOrders.Where(l => l.UserId == ID).Select(g => new
                    {
                        g.Id,
                        g.Location,
                        g.Date,
                        g.statuOrder,
                        g.Notes,
                    }).ToListAsync();
                    List<GetOrdersDTO> orders = new List<GetOrdersDTO>();
                    foreach (var order in result)
                    {
                        orders.Add(new GetOrdersDTO
                        {
                            Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                            statuOrder = order.statuOrder == "تحت الإجراء" ? "في إنتظارك لقبول الطلب" :
                                        order.statuOrder == "قيد الإنتظار" ? "فى إنتظار تقديم العروض" :
                                        order.statuOrder == "لم يتم التحويل" ? "في إنتظار رد خدمة العملاء" :
                                        order.statuOrder == "تم التحويل" ? "تم تنفبذ العملية بنجاح" :
                                        order.statuOrder == "منفذ" ? "في إنتظار رد خدمة العملاء" :
                                        order.statuOrder == "ملغى" ? "في إنتظار رد خدمة العملاء" :
                                        order.statuOrder == "محذوفة" ? "تم حذف الطلب من قبل خدمة العملاء" :
                                        order.statuOrder == "لم يتم التنفيذ" ? "في إنتظار رد خدمة العملاء" :
                                        order.statuOrder == "تم التنفيذ" ? "في إانتظار رد المحاسب" :
                            "",
                            Location = order.Location,
                            Id = order.Id.ToString(),
                            Notes = order.Notes,
                        });
                    }
                    return Ok(orders);
                }
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
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
                if(Role == "Admin")
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
        public async Task<IActionResult> data(OrderIdDTO orderId)
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