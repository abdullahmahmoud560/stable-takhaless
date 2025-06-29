using DotNetEnv;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.StaticFiles;
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
    public class UserController : ControllerBase
    {
        private readonly DB _db;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly Functions _functions;
        private readonly HangFire _hangFire;
        private readonly IWebHostEnvironment _env;

        public UserController(DB db, IHubContext<ChatHub> hubContext, Functions functions, HangFire hangFire, IWebHostEnvironment env)
        {
            _db = db;
            _hubContext = hubContext;
            _functions = functions;
            _hangFire = hangFire;
            _env = env;

        }

        [Authorize(Roles = "User,Company")]
        [HttpPost("New-Order")]
        public async Task<IActionResult> AddNewOrders([FromForm] NewOrderDTO newOrderDTO)
        {
            if (newOrderDTO == null)
                return BadRequest(new ApiResponse { Message = "بيانات الطلب غير صحيحة" });

            var userId = User.FindFirstValue("ID");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse { Message = "يجب تسجيل الدخول" });

            if (string.IsNullOrWhiteSpace(newOrderDTO.Location) || string.IsNullOrWhiteSpace(newOrderDTO.numberOfLicense))
                return BadRequest(new ApiResponse { Message = "يجب إدخال جميع البيانات المطلوبة" });

            if (newOrderDTO.numberOfTypeOrders?.Any() != true)
                return BadRequest(new ApiResponse { Message = "يجب إدخال نوع الطلب على الأقل" });

            var allowedExtensions = new[] { ".jpg", ".png", ".pdf", ".jpeg" };
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
            var provider = new FileExtensionContentTypeProvider();

            if (newOrderDTO.uploadFile != null && newOrderDTO.uploadFile.Any())
            {
                if (newOrderDTO.uploadFile.Count < 1)
                    return BadRequest(new ApiResponse { Message = "يُسمح بإرسال بملف 1 علي الأقل" });

                foreach (var file in newOrderDTO.uploadFile)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                        return BadRequest(new ApiResponse { Message = "نوع الملف غير مدعوم" });

                    if (!provider.TryGetContentType(file.FileName, out string? mimeType) || !allowedMimeTypes.Contains(mimeType))
                        return BadRequest(new ApiResponse { Message = "نوع الملف غير مدعوم" });
                }
            }

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var newOrder = new NewOrder
                    {
                        Location = newOrderDTO.Location,
                        numberOfLicense = newOrderDTO.numberOfLicense,
                        Date = DateTime.UtcNow,
                        UserId = userId,
                        statuOrder = "قيد الإنتظار",
                        Notes = newOrderDTO.Notes,
                        City = newOrderDTO.City,
                        Town = newOrderDTO.Town,
                        zipCode = newOrderDTO.zipCode,
                    };

                    _db.newOrders.Add(newOrder);
                    await _db.SaveChangesAsync();

                    var typeOrders = newOrderDTO.numberOfTypeOrders!.Select(t => new NumberOfTypeOrder
                    {
                        typeOrder = t.typeOrder,
                        Weight = t.Weight,
                        Size = t.Size,
                        Number = t.Number,
                        newOrderId = newOrder.Id,
                    }).ToList();

                    _db.typeOrders.AddRange(typeOrders);

                    if (newOrderDTO.uploadFile != null && newOrderDTO.uploadFile.Any())
                    {
                        var uploadFiles = new List<UploadFile>();
                        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                        Directory.CreateDirectory(uploadsFolder);

                        foreach (var file in newOrderDTO.uploadFile)
                        {
                            var fileExtension = Path.GetExtension(file.FileName).ToLower();
                            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            var url = $"{Request.Scheme}://{Request.Host}/uploads/{uniqueFileName}";

                            uploadFiles.Add(new UploadFile
                            {
                                fileName = file.FileName,
                                fileUrl = url,
                                ContentType = file.ContentType,
                                newOrderId = newOrder.Id,
                            });
                        }

                        _db.uploadFiles.AddRange(uploadFiles);
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    List<GetOrdersDTO> getOrders = new List<GetOrdersDTO>();
                    CultureInfo culture = new CultureInfo("ar-SA")
                    {
                        DateTimeFormat = { Calendar = new GregorianCalendar() },
                        NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                    };
                    getOrders.Add(new GetOrdersDTO
                    {
                        Location = newOrder.Location,
                        Id = newOrder.Id.ToString(),
                        Date = newOrder.Date.Value.ToString("dddd, dd MMMM yyyy", culture),
                    });

                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", getOrders[0]);

                    var Logs = new LogsDTO
                    {
                        UserId = userId,
                        NewOrderId = newOrder.Id,
                        Notes = string.Empty,
                        Message = "تم اضافة طلب جديد"
                    };
                    return Ok (await _functions.Logs(Logs));

                    var expire = newOrder.Date.Value.AddDays(7);
                    var Response = await _functions.SendAPI(userId!);
                    if (Response.HasValue && Response.Value.TryGetProperty("email", out JsonElement Email))
                    {
                        var jopID = BackgroundJob.Schedule(() => _hangFire.sendEmail(Email.ToString(), newOrder.Id, DateTime.Now.AddDays(7)), TimeSpan.FromDays(6));
                        var order = await _db.newOrders.FirstOrDefaultAsync(l => l.Id == newOrder.Id);
                        if (order != null)
                        {
                            order.JopID = jopID;
                            await _db.SaveChangesAsync();
                        }
                    }

                    return Ok(new ApiResponse { Message = "تم تقديم الطلب بنجاح" });
                }
                catch (Exception)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
                }
            }
        }
        [Authorize(Roles = "User,Company")]
        [HttpPost("Cancel-Order")]
        public async Task<IActionResult> cancelOrder([FromBody] GetID getID)
        {
            try
            {
                if (getID.ID != 0)
                {
                    var result = await _db.newOrders.FirstOrDefaultAsync(user => user.Id == getID.ID);
                    if (result != null)
                    {
                        result.statuOrder = getID.statuOrder;
                        await _db.SaveChangesAsync();
                        return Ok(result);
                    }
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "User,Company")]
        [HttpPost("Change-Satue")]
        public async Task<IActionResult> changeStatu([FromBody] GetID getID)
        {
            if (getID.ID != 0 && getID.BrokerID != null && getID.statuOrder == null && getID.Value != null)
            {
                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var ID = User.FindFirstValue("ID");
                        var order = await _db.newOrders.FirstOrDefaultAsync(l => l.Id == getID.ID);
                        if (order == null)
                        {
                            return NotFound(new ApiResponse { Message = "لم يتم العثور على الطلب" });
                        }
                        var value = await _db.values.Where(l => l.BrokerID == getID.BrokerID && l.Value == getID.Value).FirstOrDefaultAsync();
                        if (value == null)
                        {
                            return NotFound(new ApiResponse { Message = "لم يتم العثور على القيمة المطلوبة" });
                        }
                        value.Accept = true;
                        order!.statuOrder = "تحت الإجراء";
                        order!.Accept = getID.BrokerID;
                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();
                        var Logs = new LogsDTO
                        {
                            UserId = ID,
                            NewOrderId = order.Id,
                            Notes = string.Empty,
                            Message = "تم قبول العرض من قبل العميل"
                        };
                        await _functions.Logs(Logs);
                        var Response = await _functions.SendAPI(value.BrokerID!);

                        if (Response.HasValue && Response.Value.TryGetProperty("email", out JsonElement Email))
                        {
                            await _hangFire.sendEmilToBroker(Email.ToString(), order.Id, order.Date!.Value);
                        }
                        return Ok(new ApiResponse { Message = "تم تحديث حالة الطلب بنجاح" });

                    }
                    catch (Exception)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
                    }
                }
            }
            return BadRequest(new ApiResponse { Message = "برجاء ملئ الحقول المطلوبة" });
        }

        [Authorize(Roles = "User ,Company")]
        [HttpGet("Get-Accept-Orders-Users")]
        public async Task<IActionResult> getAcceptOrderUsers()
        {

            try
            {
                var ID = User.FindFirstValue("ID");
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
                }
                var allOrders = await _db.newOrders.Where(user => user.UserId == ID).ToListAsync();

                if (!allOrders.Any())
                {
                    return NotFound(new ApiResponse { Message = "لا توجد طلبات مرتبطة بهذا المستخدم" });
                }

                List<GetOrdersDTO> orders = new List<GetOrdersDTO>();

                foreach (var order in allOrders)
                {
                    if (string.Equals(order.statuOrder, "تم التحويل", StringComparison.OrdinalIgnoreCase))
                    {
                        var typeOrder = await _db.typeOrders.FirstOrDefaultAsync(l => l.newOrderId == order.Id);
                        orders.Add(new GetOrdersDTO
                        {
                            Location = order.Location ?? "غير معروف",
                            typeOrder = typeOrder!.typeOrder ?? "غير معروف",
                            statuOrder = order.statuOrder ?? "غير معروف",
                            Id = order.Id.ToString(),
                        });
                    }
                }
                return Ok(orders);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "User,Company")]
        [HttpGet("Get-Orders")]
        public async Task<IActionResult> getOrders()
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest("لم يتم العثور على معرف المستخدم");
                }
                var Allorders = _db.newOrders.Where(l => l.UserId == ID && l.statuOrder == "قيد الإنتظار").ToList();
                if (!Allorders.Any())
                {
                    return Ok(new string[] { });
                }
                List<GetOrdersDTO> getOrdersDTOs = new List<GetOrdersDTO>();
                foreach (var order in Allorders)
                {
                    var typeOrder = await _db.typeOrders.FirstOrDefaultAsync(l => l.newOrderId == order.Id);
                    getOrdersDTOs.Add(new GetOrdersDTO
                    {
                        statuOrder = "فى إنتظار تقديم العروض",
                        Location = order.Location,
                        Id = order.Id.ToString(),
                        typeOrder = typeOrder!.typeOrder,
                    });
                }
                return Ok(getOrdersDTOs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-Orders-Admin")]
        public async Task<IActionResult> getOrdersAdmin()
        {
            try
            {
                var Allorders = _db.newOrders.Where(l => l.statuOrder == "قيد الإنتظار").ToList();
                if (!Allorders.Any())
                {
                    return Ok(new string[] { });
                }
                List<GetOrdersDTO> getOrdersDTOs = new List<GetOrdersDTO>();
                foreach (var order in Allorders)
                {
                    var typeOrder = await _db.typeOrders.FirstOrDefaultAsync(l => l.newOrderId == order.Id);
                    var Response = await _functions.SendAPI(order.UserId!);
                    if (Response.Value.TryGetProperty("fullName", out JsonElement fullName)
                        && Response.Value.TryGetProperty("email", out JsonElement Email))
                        getOrdersDTOs.Add(new GetOrdersDTO
                        {
                            statuOrder = "في إنتظار المخلص لتقديم العروض",
                            Location = order.Location,
                            Id = order.Id.ToString(),
                            typeOrder = typeOrder!.typeOrder,
                            fullName = fullName.ToString(),
                            Email = Email.ToString()
                        });
                }
                return Ok(getOrdersDTOs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = ("User, Broker,Company"))]
        [HttpGet("Wallet")]
        public async Task<IActionResult> Wallet()
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                var Role = User.FindFirstValue("Role");

                if (string.IsNullOrEmpty(ID))
                    return BadRequest(new ApiResponse { Message = "المستخدم غير موجود" });

                if (string.IsNullOrEmpty(Role))
                    return BadRequest(new ApiResponse { Message = "لا توجد ادوار لهذا المستخدم" });

                IQueryable<NewOrder> ordersQuery = _db.newOrders.AsNoTracking();

                if (Role == "User")
                {
                    ordersQuery = ordersQuery.Where(l => l.UserId == ID);
                }
                else if (Role == "Broker")
                {
                    ordersQuery = ordersQuery.Where(l => l.Accept == ID);
                }
                ;

                var orders = await ordersQuery.ToListAsync();

                if (!orders.Any())
                    return Ok(new List<GetOrdersDTO>());

                var orderIds = orders.Select(o => o.Id).ToList();

                var typeOrders = await _db.typeOrders
                    .AsNoTracking()
                    .Where(t => orderIds.Contains(t.newOrderId!.Value))
                    .ToListAsync();

                var values = await _db.values
                    .AsNoTracking()
                    .Where(v => orderIds.Contains(v.newOrderId!.Value) && v.Accept == true)
                    .ToListAsync();

                var result = orders.Select(order =>
                {
                    var typeOrder = typeOrders.FirstOrDefault(t => t.newOrderId == order.Id);
                    var value = values.FirstOrDefault(v => v.newOrderId == order.Id);

                    return new GetOrdersDTO
                    {
                        Id = order.Id.ToString(),
                        Location = order.Location,
                        statuOrder = order.statuOrder,
                        typeOrder = typeOrder?.typeOrder ?? "غير محدد",
                        Value = value?.Value ?? 0,
                        Notes = order.Notes
                    };
                }).ToList();

                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "User,Admin,Manager,Company")]
        [HttpGet("Number-Of-Operations-User")]
        public async Task<IActionResult> Numberofoperations()
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
                }

                var counts = await _db.newOrders
                    .AsNoTracking()
                    .Where(o => o.UserId == ID)
                    .GroupBy(o => 1)
                    .Select(g => new
                    {
                        NumberOfCurrentOffers = g.Count(o => o.statuOrder == "قيد الإنتظار"),
                        NumberOfRequestOrders = g.Count(o => o.statuOrder == "تحت الإجراء"),
                        NumberOfSuccessfulOrders = g.Count(o => o.statuOrder == "تم التحويل")
                    })
                    .FirstOrDefaultAsync() ?? new { NumberOfCurrentOffers = 0, NumberOfRequestOrders = 0, NumberOfSuccessfulOrders = 0 };

                return Ok(new
                {
                    counts.NumberOfCurrentOffers,
                    counts.NumberOfRequestOrders,
                    counts.NumberOfSuccessfulOrders
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

    }
}