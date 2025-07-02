using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using User.ApplicationDbContext;
using User.DTO;
using User.Model;
namespace User.Controllers
{
    [Route("api/")]
    [ApiController]
    public class CustomerServicesController : ControllerBase
    {
        private readonly DB _db;
        private readonly Functions _functions;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CustomerServicesController(DB db, Functions functions, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _functions = functions;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        [Authorize(Roles = "CustomerService,Admin,Manager")]
        [HttpGet("Get-All-Accept-Orders/{Page}")]
        public async Task<IActionResult> GetAllAcceptOrders(int Page)
        {
            try
            {
                const int PageSize = 10;
                CultureInfo culture = new("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                var baseQuery = _db.newOrders
                    .Where(l => l.statuOrder == "منفذ")
                    .OrderByDescending(o => o.Date);

                int totalCount = await baseQuery.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                var acceptedOrders = await baseQuery
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                List<GetOrdersDTO> ordersDTOs = new();
                foreach (var accept in acceptedOrders)
                {
                    var typeOrder = await _db.typeOrders.FirstOrDefaultAsync(t => t.newOrderId == accept.Id);
                    var Notes = await _db.notesCustomerServices.FirstOrDefaultAsync(n => n.newOrderId == accept.Id);
                    var response = await _functions.SendAPI(accept.UserId!);

                    if (response.HasValue &&
                        response.Value.TryGetProperty("fullName", out JsonElement fullName) &&
                        response.Value.TryGetProperty("email", out JsonElement Email))
                    {
                        ordersDTOs.Add(new GetOrdersDTO
                        {
                            Id = accept.Id.ToString(),
                            statuOrder = accept.statuOrder,
                            Location = accept.Location,
                            typeOrder = typeOrder?.typeOrder,
                            Date = accept.Date?.ToString("dddd, dd MMMM yyyy", culture),
                            Email = Email.ToString(),
                            fullName = fullName.ToString(),
                            BrokerID = accept.Accept,
                            Notes = Notes?.Notes ?? ""
                        });
                    }
                }

                return Ok(new
                {
                    Page = Page,
                    PageSize,
                    TotalPages = totalPages,
                    TotalCount = totalCount,
                    Data = ordersDTOs
                });
            }
            catch
            {
                return StatusCode(500, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }


        [Authorize(Roles = "CustomerService,Admin,Manager")]
        [HttpGet("Get-All-Refuse-Orders/{Page}")]
        public async Task<IActionResult> getAllRefusetOrders(int Page)
        {
            try
            {
                const int PageSize = 10;
                CultureInfo culture = new("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                var baseQuery = _db.newOrders
                    .Where(l => l.statuOrder == "ملغى")
                    .OrderByDescending(o => o.Date);

                int totalCount = await baseQuery.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                var result = await baseQuery
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                List<GetOrdersDTO> ordersDTOs = new();
                foreach (var order in result)
                {
                    var typeOrder = await _db.typeOrders.FirstOrDefaultAsync(t => t.newOrderId == order.Id);
                    var response = await _functions.SendAPI(order.UserId!);

                    if (response.HasValue &&
                        response.Value.TryGetProperty("fullName", out JsonElement fullName) &&
                        response.Value.TryGetProperty("email", out JsonElement Email))
                    {
                        ordersDTOs.Add(new GetOrdersDTO
                        {
                            Id = order.Id.ToString(),
                            statuOrder = order.statuOrder,
                            Location = order.Location,
                            typeOrder = typeOrder?.typeOrder,
                            Date = order.Date?.ToString("dddd, dd MMMM yyyy", culture),
                            Email = Email.ToString(),
                            fullName = fullName.ToString(),
                            BrokerID = order.Accept
                        });
                    }
                }

                return Ok(new
                {
                    Page = Page,
                    PageSize,
                    TotalPages = totalPages,
                    TotalCount = totalCount,
                    Data = ordersDTOs
                });
            }
            catch
            {
                return StatusCode(500, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }


        [Authorize(Roles = "CustomerService,Admin,Manager")]
        [HttpPost("Change-Statu-CustomerService")]
        public async Task<IActionResult> changeStatuCustomerService(GetID getID)
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                if (getID.ID != 0 && getID.BrokerID == null && getID.statuOrder == "true")
                {
                    var order = await _db.newOrders.FirstOrDefaultAsync(l => l.Id == getID.ID);
                    order!.statuOrder = "تم التنفيذ";
                    order.AcceptCustomerService = ID;
                    await _db.SaveChangesAsync();
                    var Logs = new LogsDTO
                    {
                        UserId = ID,
                        NewOrderId = getID.ID,
                        Message = "تم تحويل الطلب الي المحاسب",
                        Notes = string.Empty,
                    };
                    await _functions.Logs(Logs);
                    return Ok("تم تغيير الحالة بنجاح");
                }
                else if (getID.ID != 0 && getID.BrokerID == null && getID.statuOrder == "false")
                {
                    var order = await _db.newOrders.FirstOrDefaultAsync(l => l.Id == getID.ID);
                    order!.statuOrder = "لم يتم التنفيذ";
                    order.Notes = getID.Notes;
                    order.AcceptCustomerService = ID;
                    await _db.SaveChangesAsync();
                    var Logs = new LogsDTO
                    {
                        UserId = ID,
                        NewOrderId = getID.ID,
                        Message = "تم تحويل الطلب الي المخلص مرة أخرى",
                        Notes = getID.Notes,
                    };
                    await _functions.Logs(Logs);
                    var Log = new LogsDTO
                    {
                        UserId = ID,
                        NewOrderId = getID.ID,
                        Message = "تم إضافة ملاحظات من قبل خدمة العملاء",
                        Notes = order.Notes,
                    };
                    return Ok("تم تغيير الحالة بنجاح");
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }


        [Authorize(Roles = "CustomerService,Admin,Manager")]
        [HttpGet("Get-All-Transfer-From-Account/{Page}")]
        public async Task<IActionResult> getAllTransferFromAccount(int Page)
        {
            try
            {
                const int pageSize = 10;

                CultureInfo culture = new("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                var baseQuery = _db.newOrders
                    .Where(l => l.statuOrder == "لم يتم التحويل")
                    .OrderByDescending(o => o.Date);

                var totalCount = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var result = await baseQuery
                    .Skip((Page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                List<GetOrdersDTO> ordersDTOs = new();

                foreach (var order in result)
                {
                    var typeOrder = await _db.typeOrders
                        .Where(l => l.newOrderId == order.Id)
                        .FirstOrDefaultAsync();

                    var response = await _functions.BrokerandUser(order.Accept!, order.AcceptAccount!);

                    if (response.Value.TryGetProperty("user", out JsonElement User) &&
                        response.Value.TryGetProperty("broker", out JsonElement Account) &&
                        User.TryGetProperty("fullName", out JsonElement UserName) &&
                        User.TryGetProperty("email", out JsonElement UserEmail) &&
                        Account.TryGetProperty("fullName", out JsonElement AccountName) &&
                        Account.TryGetProperty("email", out JsonElement AccountEmail))
                    {
                        var notesAccounting = await _db.notesAccountings
                            .Where(l => l.newOrderId == order.Id)
                            .FirstOrDefaultAsync();

                        ordersDTOs.Add(new GetOrdersDTO
                        {
                            Id = order.Id.ToString(),
                            statuOrder = order.statuOrder,
                            Location = order.Location,
                            typeOrder = typeOrder?.typeOrder,
                            Date = order.Date?.ToString("dddd, dd MMMM yyyy", culture),
                            Email = UserEmail.ToString(),
                            fullName = UserName.ToString(),
                            AccountName = AccountName.ToString(),
                            AccountEmail = AccountEmail.ToString(),
                            Notes = notesAccounting?.Notes ?? "",
                            BrokerID = order.Accept
                        });
                    }
                }

                return Ok(new
                {
                    Page = Page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalCount = totalCount,
                    Data = ordersDTOs
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
                {
                    Message = "حدث خطأ برجاء المحاولة فى وقت لاحق "
                });
            }
        }


        [Authorize(Roles = "CustomerService,Admin")]
        [HttpPost("Change-Statu-CustomerService-Broker")]
        public async Task<IActionResult> chageStatueCustomerServiceBroker(GetID getID)
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                if (getID.ID != 0 && getID.BrokerID == null && getID.statuOrder == "transfer")
                {
                    var order = await _db.newOrders.FirstOrDefaultAsync(l => l.Id == getID.ID);
                    order!.statuOrder = "محولة";
                    order.Notes = getID.Notes;
                    order.AcceptCustomerService = ID;
                    var Logs = new LogsDTO
                    {
                        UserId = ID,
                        NewOrderId = getID.ID,
                        Message = "تم تحويل الطلب الي المخلص مرة أخرى",
                        Notes = string.Empty,
                    };
                    await _db.SaveChangesAsync();
                    await _functions.Logs(Logs);
                    return Ok();
                }
                else if (getID.ID != 0 && getID.BrokerID == null && getID.statuOrder == "delete")
                {
                    var order = await _db.newOrders.FirstOrDefaultAsync(l => l.Id == getID.ID);
                    order!.statuOrder = "محذوفة";
                    order.Notes = getID.Notes;
                    order.AcceptCustomerService = ID;
                    await _db.SaveChangesAsync();
                    var Logs = new LogsDTO
                    {
                        UserId = ID,
                        NewOrderId = getID.ID,
                        Message = "تم تحويل الطلب الي الطلبات المحذوفة",
                        Notes = string.Empty,
                    };
                    await _functions.Logs(Logs);
                    return Ok();
                }
                else if (getID.ID != 0 && getID.BrokerID != null && getID.statuOrder == "send")
                {
                    var order = await _db.newOrders.FirstOrDefaultAsync(l => l.Id == getID.ID);
                    var delete = await _db.values.Where(l => l.newOrderId == order!.Id && l.Accept == true && l.BrokerID == getID.BrokerID).ToListAsync();
                    _db.values.RemoveRange(delete);
                    order!.statuOrder = "قيد الإنتظار";
                    order.AcceptCustomerService = ID;
                    await _db.SaveChangesAsync();
                    var Logs = new LogsDTO
                    {
                        UserId = ID,
                        NewOrderId = getID.ID,
                        Message = "تم تحويل الطلب الي الطلبات المتاحة",
                        Notes = string.Empty,
                    };
                    await _functions.Logs(Logs);
                    return Ok();
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "CustomerService,Broker,Admin,Manager")]
        [HttpPost("Notes-From-CustomerService")]
        public async Task<IActionResult> notesFromCustomerService([FromForm] NotesFromCustomerServiceDTO notesFromCustomerServiceDTO)
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                if (notesFromCustomerServiceDTO == null)
                    return BadRequest("البيانات المرسلة غير صحيحة");

                if (!string.IsNullOrEmpty(notesFromCustomerServiceDTO.Notes) || notesFromCustomerServiceDTO.formFile != null)
                {
                    var isFound = await _db.notesCustomerServices
                        .FirstOrDefaultAsync(l => l.newOrderId == notesFromCustomerServiceDTO.newOrderId);

                    string? fileUrl = null;
                    string? originalFileName = null;

                    if (notesFromCustomerServiceDTO.formFile != null)
                    {
                        // تجهيز اسم الملف
                        originalFileName = Path.GetFileNameWithoutExtension(notesFromCustomerServiceDTO.formFile.FileName);
                        var safeFileName = originalFileName.Replace(" ", "-");
                        var fileExtension = Path.GetExtension(notesFromCustomerServiceDTO.formFile.FileName);
                        var uniqueFileName = $"{safeFileName}_{Guid.NewGuid()}{fileExtension}";

                        // تحديد مسار الحفظ باستخدام IWebHostEnvironment
                        var uploadsFolder = Path.Combine(_env.WebRootPath, "CustomerFiles");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // حفظ الملف
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await notesFromCustomerServiceDTO.formFile.CopyToAsync(stream);
                        }

                        // توليد رابط الوصول الكامل للملف
                        var request = _httpContextAccessor.HttpContext?.Request;
                        var baseUrl = $"{request?.Scheme}://{request?.Host}";
                        fileUrl = $"{baseUrl}/CustomerFiles/{uniqueFileName}";
                    }

                    if (isFound != null)
                    {
                        isFound.Notes = notesFromCustomerServiceDTO.Notes;
                        if (fileUrl != null)
                        {
                            isFound.fileUrl = fileUrl;
                            isFound.fileName = originalFileName;
                        }

                        _db.notesCustomerServices.Update(isFound);
                    }
                    else
                    {
                        var newFile = new NotesCustomerService
                        {
                            Notes = notesFromCustomerServiceDTO.Notes,
                            newOrderId = notesFromCustomerServiceDTO.newOrderId!.Value,
                            fileUrl = fileUrl,
                            fileName = originalFileName
                        };

                        await _db.notesCustomerServices.AddAsync(newFile);
                    }

                    var result = await _db.SaveChangesAsync();
                    if (result > 0)
                    {
                        var Logs = new LogsDTO
                        {
                            UserId = ID,
                            NewOrderId = notesFromCustomerServiceDTO.newOrderId!.Value,
                            Message = "تم إضافة ملاحظات",
                            Notes = notesFromCustomerServiceDTO.Notes,
                        };
                        await _functions.Logs(Logs);

                        return Ok(new ApiResponse
                        {
                            Message = "تم تقديم الملاحظات بنجاح",
                        });
                    }
                }

                return BadRequest("يرجى إدخال ملاحظات أو إرفاق ملف.");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }
        [Authorize(Roles = "CustomerService,Admin,Manager")]
        [HttpGet("Get-Deleted-Orders/{Page}")]
        public async Task<IActionResult> DeletedOrders(int Page)
        {
            try
            {
                const int pageSize = 10;
                var ID = User.FindFirstValue("ID");
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
                }

                CultureInfo culture = new("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                var baseQuery = _db.newOrders
                    .Where(l => l.statuOrder == "محذوفة")
                    .OrderByDescending(o => o.Date);

                var totalCount = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                var deletedOrders = await baseQuery
                    .Skip((Page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(order => new
                    {
                        order.Id,
                        order.statuOrder,
                        order.Location,
                        order.Date,
                        order.Notes,
                        order.AcceptCustomerService,
                        order.UserId,
                    }).ToListAsync();

                List<GetOrdersDTO> getOrdersDTOs = new();

                foreach (var order in deletedOrders)
                {
                    var typeOrder = await _db.typeOrders
                        .Where(l => l.newOrderId == order.Id)
                        .Select(l => new { l.typeOrder })
                        .FirstOrDefaultAsync();

                    var Response = await _functions.BrokerandUser(order.UserId!, order.AcceptCustomerService!);

                    if (Response.Value.TryGetProperty("user", out JsonElement User) &&
                        Response.Value.TryGetProperty("broker", out JsonElement CustomerService) &&
                        User.TryGetProperty("fullName", out JsonElement UserName) &&
                        User.TryGetProperty("email", out JsonElement UserEmail) &&
                        CustomerService.TryGetProperty("fullName", out JsonElement CustomerServiceName) &&
                        CustomerService.TryGetProperty("email", out JsonElement CustomerServiceEmail))
                    {
                        getOrdersDTOs.Add(new GetOrdersDTO
                        {
                            Id = order.Id.ToString(),
                            statuOrder = order.statuOrder,
                            Location = order.Location,
                            typeOrder = typeOrder?.typeOrder,
                            Date = order.Date?.ToString("dddd, dd MMMM yyyy", culture),
                            Notes = order.Notes,
                            fullName = UserName.ToString(),
                            Email = UserEmail.ToString(),
                            CustomerServiceEmail = CustomerServiceEmail.ToString(),
                            CustomerServiceName = CustomerServiceName.ToString(),
                        });
                    }
                }

                return Ok(new
                {
                    Page = Page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalCount = totalCount,
                    Data = getOrdersDTOs
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse
                {
                    Message = "حدث خطأ برجاء المحاولة فى وقت لاحق "
                });
            }
        }


        [Authorize(Roles = "CustomerService,Admin,Manager")]
        [HttpGet("Number-Of-Operations-CustomerService")]
        public async Task<IActionResult> Numberofoperations()
        {
            try
            {
                var ID = User.FindFirstValue("ID");
                if (string.IsNullOrEmpty(ID))
                {
                    return BadRequest(new ApiResponse { Message = "المستخدم غير موجود" });
                }
                var NumberOfDoneOrders = await _db.newOrders.Where(l => l.statuOrder == "منفذ").CountAsync();
                var NumberOfRefuseOrders = await _db.newOrders.Where(l => l.statuOrder == "ملغى").CountAsync();
                var NumberOfNotDoneOrders = await _db.newOrders.Where(l => l.statuOrder == "لم يتم التحويل").CountAsync();
                var NumberOfDeletedOrders = await _db.newOrders.Where(l => l.statuOrder == "محذوفة").CountAsync();
                return Ok(new
                {
                    NumberOfDoneOrders = NumberOfDoneOrders,
                    NumberOfRefuseOrders = NumberOfRefuseOrders,
                    NumberOfNotDoneOrders = NumberOfNotDoneOrders,
                    NumberOfDeletedOrders = NumberOfDeletedOrders
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }
    }
}