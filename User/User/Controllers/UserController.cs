using Hangfire;
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
    public class UserController : ControllerBase
    {
        private readonly DB _db;
        private readonly Functions _functions;
        private readonly IWebHostEnvironment _env;
        private readonly ChatHub _hubContext;
        private readonly HangFire _hangFire;
        private const int pageSize = 10;

        public UserController(DB db, Functions functions, IWebHostEnvironment env, ChatHub hubContext, HangFire hangFire)
        {
            _db = db;
            _functions = functions;
            _env = env;
            _hubContext = hubContext;
            _hangFire = hangFire;
        }

        [Authorize(Roles = "User,Company")]
        [HttpPost("New-Order")]
        public async Task<IActionResult> NewOrder([FromForm] NewOrderDTO newOrderDTO)
        {
            var userId = User.FindFirstValue("ID");
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
            }

            if (newOrderDTO.uploadFile != null && newOrderDTO.uploadFile.Any())
            {
                var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "application/pdf" };
                var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();

                foreach (var file in newOrderDTO.uploadFile)
                {
                    if (!provider.TryGetContentType(file.FileName, out string? mimeType) || !allowedMimeTypes.Contains(mimeType))
                    {
                        return BadRequest(new ApiResponse { Message = "نوع الملف غير مدعوم" });
                    }
                }
            }

            using (var transaction = await _db.Database.BeginTransactionAsync())
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
                    DateTimeFormat = { Calendar = new GregorianCalendar() }
                };
                culture.NumberFormat.DigitSubstitution = DigitShapes.NativeNational;
                
                getOrders.Add(new GetOrdersDTO
                {
                    Location = newOrder.Location,
                    Id = newOrder.Id.ToString(),
                    Date = newOrder.Date.Value.ToString("dddd, dd MMMM yyyy", culture),
                });

                await _hubContext.NotifyNewOrder();

                var Logs = new LogsDTO
                {
                    UserId = userId,
                    NewOrderId = newOrder.Id,
                    Notes = string.Empty,
                    Message = "تم اضافة طلب جديد"
                };
                await _functions.Logs(Logs);

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
        }

        [Authorize(Roles = "User,Company")]
        [HttpPost("Cancel-Order")]
        public async Task<IActionResult> cancelOrder([FromBody] GetID getID)
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

        [Authorize(Roles = "User,Company")]
        [HttpPost("Change-Satue")]
        public async Task<IActionResult> changeStatu([FromBody] GetID getID)
        {
            if (getID.ID != 0 && getID.BrokerID != null && getID.statuOrder == null && getID.Value != null)
            {
                using (var transaction = await _db.Database.BeginTransactionAsync())
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
            }
            return BadRequest(new ApiResponse { Message = "برجاء ملئ الحقول المطلوبة" });
        }

        [Authorize(Roles = "User ,Company")]
        [HttpGet("Get-Accept-Orders-Users/{Page}")]
        public async Task<IActionResult> getAcceptOrderUsers(int Page)
        {
            var ID = User.FindFirstValue("ID");
            if (string.IsNullOrEmpty(ID))
            {
                return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
            }

            // ✅ تحسين: استخدام Include و AsNoTracking
            var baseQuery = _db.newOrders
                .AsNoTracking()
                .Include(o => o.numberOfTypeOrders)
                .Where(user => user.UserId == ID && user.statuOrder == "تم التحويل")
                .OrderByDescending(order => order.Date);

            var totalCount = await baseQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var orders = await baseQuery
                .Skip((Page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var Response = await _functions.SendAPI(ID);
            var ordersDTO = new List<GetOrdersDTO>();

            foreach (var order in orders)
            {
                var orderDTO = new GetOrdersDTO
                {
                    Id = order.Id.ToString(),
                    Location = order.Location,
                    statuOrder = order.statuOrder,
                    Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", new CultureInfo("ar-SA")),
                    typeOrder = order.numberOfTypeOrders?.FirstOrDefault()?.typeOrder
                };

                if (Response.HasValue && Response.Value.TryGetProperty("email", out JsonElement Email))
                {
                    orderDTO.Email = Email.ToString();
                }

                ordersDTO.Add(orderDTO);
            }

            return Ok(new
            {
                TotalPages = totalPages,
                Page = Page,
                totalUser = totalCount,
                data = ordersDTO
            });
        }

        [Authorize(Roles = "User,Company")]
        [HttpGet("Get-All-Orders-Users/{Page}")]
        public async Task<IActionResult> getAllOrdersUsers(int Page)
        {
            var ID = User.FindFirstValue("ID");
            if (string.IsNullOrEmpty(ID))
            {
                return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
            }

            var baseQuery = _db.newOrders
                .AsNoTracking()
                .Include(o => o.numberOfTypeOrders)
                .Where(user => user.UserId == ID)
                .OrderByDescending(order => order.Date);

            var totalCount = await baseQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var orders = await baseQuery
                .Skip((Page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var Response = await _functions.SendAPI(ID);
            var ordersDTO = new List<GetOrdersDTO>();

            foreach (var order in orders)
            {
                var orderDTO = new GetOrdersDTO
                {
                    Id = order.Id.ToString(),
                    Location = order.Location,
                    statuOrder = order.statuOrder,
                    Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", new CultureInfo("ar-SA")),
                    typeOrder = order.numberOfTypeOrders?.FirstOrDefault()?.typeOrder
                };

                if (Response.Value.TryGetProperty("fullName", out JsonElement fullName)
                    && Response.Value.TryGetProperty("email", out JsonElement Email))
                {
                    orderDTO.fullName = fullName.ToString();
                    orderDTO.Email = Email.ToString();
                }

                ordersDTO.Add(orderDTO);
            }

            return Ok(new
            {
                TotalPages = totalPages,
                Page = Page,
                totalUser = totalCount,
                data = ordersDTO
            });
        }

        [Authorize(Roles = "User,Company")]
        [HttpGet("Get-Order-Details/{orderId}")]
        public async Task<IActionResult> getOrderDetails(int orderId)
        {
            var ID = User.FindFirstValue("ID");
            if (string.IsNullOrEmpty(ID))
            {
                return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
            }

            var order = await _db.newOrders
                .AsNoTracking()
                .Include(o => o.numberOfTypeOrders)
                .Include(o => o.uploadFiles)
                .Include(o => o.values)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == ID);

            if (order == null)
            {
                return NotFound(new ApiResponse { Message = "لم يتم العثور على الطلب" });
            }

            var Response = await _functions.SendAPI(ID);
            var orderDetails = new
            {
                Id = order.Id,
                Location = order.Location,
                statuOrder = order.statuOrder,
                Date = order.Date,
                Notes = order.Notes,
                City = order.City,
                Town = order.Town,
                zipCode = order.zipCode,
                numberOfTypeOrders = order.numberOfTypeOrders,
                uploadFiles = order.uploadFiles,
                values = order.values,
                UserInfo = Response
            };

            return Ok(orderDetails);
        }

        [Authorize(Roles = "User,Company")]
        [HttpGet("Get-User-Profile")]
        public async Task<IActionResult> getUserProfile()
        {
            var ID = User.FindFirstValue("ID");
            if (string.IsNullOrEmpty(ID))
            {
                return BadRequest(new ApiResponse { Message = "لم يتم العثور على معرف المستخدم في بيانات الاعتماد" });
            }

            var Response = await _functions.SendAPI(ID);
            return Ok(Response);
        }
    }
}