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
        private const int PageSize = 10;

        public CustomerServicesController(DB db, Functions functions, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _functions = functions;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        private static CultureInfo GetArabicCulture() => new("ar-SA")
        {
            DateTimeFormat = { Calendar = new GregorianCalendar() },
            NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
        };

        private string GetCurrentUserId()
        {
            var userId = User.FindFirstValue("ID");
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("لم يتم العثور على معرف المستخدم في بيانات الاعتماد");
            return userId;
        }

        private async Task<Dictionary<string, (string fullName, string email)>> GetUserDataAsync(List<string?> userIds)
        {
            var userData = new Dictionary<string, (string fullName, string email)>();
            var distinctUserIds = userIds.Where(id => !string.IsNullOrEmpty(id)).Distinct();

            foreach (var userId in distinctUserIds)
            {
                var response = await _functions.SendAPI(userId!);
                if (response.HasValue &&
                    response.Value.TryGetProperty("fullName", out var fullName) &&
                    response.Value.TryGetProperty("email", out var email))
                {
                    userData[userId!] = (fullName.ToString(), email.ToString());
                }
            }
            return userData;
        }

        private async Task<Dictionary<string, JsonElement>> GetUserBrokerDataAsync(List<NewOrder> orders, Func<NewOrder, (string?, string?)> keySelector)
        {
            var userBrokerPairs = orders
                .Select(keySelector)
                .Where(pair => !string.IsNullOrEmpty(pair.Item1) && !string.IsNullOrEmpty(pair.Item2))
                .Distinct()
                .ToList();

            var userBrokerData = new Dictionary<string, JsonElement>();

            foreach (var (first, second) in userBrokerPairs)
            {
                var key = $"{first}_{second}";
                if (!userBrokerData.ContainsKey(key))
                {
                    var response = await _functions.BrokerandUser(first!, second!);
                    userBrokerData[key] = response.Value;
                }
            }

            return userBrokerData;
        }

        private object CreatePagedResponse<T>(int page, int totalCount, List<T> data)
        {
            return new
            {
                Page = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / PageSize),
                totalUser = totalCount,
                Data = data
            };
        }

        private IQueryable<NewOrder> GetOrdersQuery(string status, params string[] includes)
        {
            var query = _db.newOrders.AsNoTracking().Where(o => o.statuOrder == status);
            foreach (var include in includes)
                query = query.Include(include);
            return query.OrderByDescending(o => o.Date);
        }

        private GetOrdersDTO MapOrderToDTO(NewOrder order, CultureInfo culture, 
            string? fullName = null, string? email = null, 
            string? accountName = null, string? accountEmail = null,
            string? customerServiceName = null, string? customerServiceEmail = null,
            string? notes = null)
        {
            return new GetOrdersDTO
            {
                Id = order.Id.ToString(),
                statuOrder = order.statuOrder,
                Location = order.Location,
                typeOrder = order.numberOfTypeOrders?.FirstOrDefault()?.typeOrder,
                Date = order.Date?.ToString("dddd, dd MMMM yyyy", culture),
                Email = email,
                fullName = fullName,
                AccountName = accountName,
                AccountEmail = accountEmail,
                CustomerServiceName = customerServiceName,
                CustomerServiceEmail = customerServiceEmail,
                Notes = notes,
                BrokerID = order.Accept
            };
        }



        private async Task CreateLogAsync(string userId, int orderId, string message, string notes = "")
        {
            var log = new LogsDTO
            {
                UserId = userId,
                NewOrderId = orderId,
                Message = message,
                Notes = notes
            };
            await _functions.Logs(log);
        }

        private async Task<NewOrder?> GetOrderByIdAsync(int orderId)
        {
            return await _db.newOrders.FirstOrDefaultAsync(o => o.Id == orderId);
        }

        private async Task<string?> SaveFileAsync(IFormFile? file)
        {
            if (file == null) return null;

            var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
            var safeFileName = originalFileName.Replace(" ", "-");
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{safeFileName}_{Guid.NewGuid()}{fileExtension}";

            var uploadsFolder = Path.Combine(_env.WebRootPath, "CustomerFiles");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}";
            return $"{baseUrl}/CustomerFiles/{uniqueFileName}";
        }

        [Authorize(Roles = "CustomerService,Admin,Manager")]
        [HttpGet("Get-All-Accept-Orders/{Page}")]
        public async Task<IActionResult> GetAllAcceptOrders(int Page)
        {
            var culture = GetArabicCulture();
            var baseQuery = GetOrdersQuery("منفذ", nameof(NewOrder.numberOfTypeOrders), nameof(NewOrder.NotesCustomerServices));
            var totalCount = await baseQuery.CountAsync();
            var orders = await baseQuery.Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync();

            var userData = await GetUserDataAsync(orders.Select(o => o.UserId).ToList());

            var result = orders.Select(order =>
            {
                var (name, email) = userData.GetValueOrDefault(order.UserId!, ("", ""));
                return MapOrderToDTO(order, culture, name, email, 
                    notes: order.NotesCustomerServices?.FirstOrDefault()?.Notes ?? "");
            }).ToList();

            return Ok(CreatePagedResponse(Page, totalCount, result));
        }

        [Authorize(Roles = "CustomerService,Admin,Manager")]
        [HttpGet("Get-All-Refuse-Orders/{Page}")]
        public async Task<IActionResult> GetAllRefusedOrders(int Page)
        {
            var culture = GetArabicCulture();
            var baseQuery = GetOrdersQuery("ملغى", nameof(NewOrder.numberOfTypeOrders));
            var totalCount = await baseQuery.CountAsync();
            var orders = await baseQuery.Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync();

            var userData = await GetUserDataAsync(orders.Select(o => o.UserId).ToList());

            var result = orders.Select(order =>
            {
                var (name, email) = userData.GetValueOrDefault(order.UserId!, ("", ""));
                return MapOrderToDTO(order, culture, name, email);
            }).ToList();

            return Ok(CreatePagedResponse(Page, totalCount, result));
        }

        [Authorize(Roles = "CustomerService,Admin,Manager")]
        [HttpGet("Get-All-Transfer-From-Account/{Page}")]
        public async Task<IActionResult> GetAllTransferFromAccount(int Page)
        {
            var culture = GetArabicCulture();
            var baseQuery = GetOrdersQuery("لم يتم التحويل", nameof(NewOrder.numberOfTypeOrders), nameof(NewOrder.NotesCustomerServices));
            var totalCount = await baseQuery.CountAsync();
            var orders = await baseQuery.Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync();

            var userBrokerData = await GetUserBrokerDataAsync(orders, 
                order => (order.Accept, order.AcceptAccount));

            var result = orders.Select(order =>
            {
                var key = $"{order.Accept}_{order.AcceptAccount}";
                var response = userBrokerData.GetValueOrDefault(key);

                if (response.TryGetProperty("user", out JsonElement user) &&
                    response.TryGetProperty("broker", out JsonElement account) &&
                    user.TryGetProperty("fullName", out JsonElement userName) &&
                    user.TryGetProperty("email", out JsonElement userEmail) &&
                    account.TryGetProperty("fullName", out JsonElement accountName) &&
                    account.TryGetProperty("email", out JsonElement accountEmail))
                {
                    return MapOrderToDTO(order, culture, 
                        userName.ToString(), userEmail.ToString(),
                        accountName.ToString(), accountEmail.ToString(),
                        notes: order.notesAccountings?.FirstOrDefault()?.Notes ?? "");
                }

                return MapOrderToDTO(order, culture);
            }).ToList();

            return Ok(CreatePagedResponse(Page, totalCount, result));
        }

        [Authorize(Roles = "CustomerService,Admin,Manager")]
        [HttpGet("Get-Deleted-Orders/{Page}")]
        public async Task<IActionResult> GetDeletedOrders(int Page)
        {
            var culture = GetArabicCulture();
            var baseQuery = GetOrdersQuery("محذوفة", nameof(NewOrder.numberOfTypeOrders));
            var totalCount = await baseQuery.CountAsync();
            var orders = await baseQuery.Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync();

            var userCustomerServiceData = await GetUserBrokerDataAsync(orders, 
                order => (order.UserId, order.AcceptCustomerService));

            var result = orders.Select(order =>
            {
                var key = $"{order.UserId}_{order.AcceptCustomerService}";
                var response = userCustomerServiceData.GetValueOrDefault(key);

                if (response.TryGetProperty("user", out JsonElement user) &&
                    response.TryGetProperty("broker", out JsonElement customerService) &&
                    user.TryGetProperty("fullName", out JsonElement userName) &&
                    user.TryGetProperty("email", out JsonElement userEmail) &&
                    customerService.TryGetProperty("fullName", out JsonElement customerServiceName) &&
                    customerService.TryGetProperty("email", out JsonElement customerServiceEmail))
                {
                    return MapOrderToDTO(order, culture, 
                        userName.ToString(), userEmail.ToString(),
                        customerServiceName: customerServiceName.ToString(),
                        customerServiceEmail: customerServiceEmail.ToString(),
                        notes: order.Notes);
                }

                return MapOrderToDTO(order, culture, notes: order.Notes);
            }).ToList();

            return Ok(CreatePagedResponse(Page, totalCount, result));
        }

        [Authorize(Roles = "CustomerService,Admin,Manager")]
        [HttpPost("Change-Statu-CustomerService")]
        public async Task<IActionResult> ChangeStatusCustomerService(GetID getID)
        {
            
                var userId = GetCurrentUserId();
                var order = await GetOrderByIdAsync(getID.ID);
                
                if (order == null)
                    return BadRequest(new ApiResponse { Message = "الطلب غير موجود" });

                if (getID.statuOrder == "true")
                {
                    order.statuOrder = "تم التنفيذ";
                    order.AcceptCustomerService = userId;
                    await _db.SaveChangesAsync();
                    await CreateLogAsync(userId, getID.ID, "تم تحويل الطلب الي المحاسب");
                    return Ok(new ApiResponse { Message = "تم تغيير الحالة بنجاح" });
                }
                else if (getID.statuOrder == "false")
                {
                    order.statuOrder = "لم يتم التنفيذ";
                    order.Notes = getID.Notes;
                    order.AcceptCustomerService = userId;
                    await _db.SaveChangesAsync();
                    await CreateLogAsync(userId, getID.ID, "تم تحويل الطلب الي المخلص مرة أخرى", getID.Notes);
                    return Ok(new ApiResponse { Message = "تم تغيير الحالة بنجاح" });
                }

                return BadRequest(new ApiResponse { Message = "بيانات غير صحيحة" });
        }

        [Authorize(Roles = "CustomerService,Admin")]
        [HttpPost("Change-Statu-CustomerService-Broker")]
        public async Task<IActionResult> ChangeStatusCustomerServiceBroker(GetID getID)
        {
           
                var userId = GetCurrentUserId();
                var order = await GetOrderByIdAsync(getID.ID);
                
                if (order == null)
                    return BadRequest(new ApiResponse { Message = "الطلب غير موجود" });

                switch (getID.statuOrder)
                {
                    case "transfer":
                        order.statuOrder = "محولة";
                        order.Notes = getID.Notes;
                        order.AcceptCustomerService = userId;
                        await _db.SaveChangesAsync();
                        await CreateLogAsync(userId, getID.ID, "تم تحويل الطلب الي المخلص مرة أخرى");
                        return Ok(new ApiResponse { Message = "تم التحويل بنجاح" });

                    case "delete":
                        order.statuOrder = "محذوفة";
                        order.Notes = getID.Notes;
                        order.AcceptCustomerService = userId;
                        await _db.SaveChangesAsync();
                        await CreateLogAsync(userId, getID.ID, "تم تحويل الطلب الي الطلبات المحذوفة");
                        return Ok(new ApiResponse { Message = "تم الحذف بنجاح" });

                    case "send":
                        if (getID.BrokerID != null)
                        {
                            var deleteValues = await _db.values
                                .Where(v => v.newOrderId == order.Id && v.Accept == true && v.BrokerID == getID.BrokerID)
                                .ToListAsync();
                            _db.values.RemoveRange(deleteValues);
                        }
                        order.statuOrder = "قيد الإنتظار";
                        order.AcceptCustomerService = userId;
                        await _db.SaveChangesAsync();
                        await CreateLogAsync(userId, getID.ID, "تم تحويل الطلب الي الطلبات المتاحة");
                        return Ok(new ApiResponse { Message = "تم الإرسال بنجاح" });

                    default:
                        return BadRequest(new ApiResponse { Message = "بيانات غير صحيحة" });
                }
            
        }

        [Authorize(Roles = "CustomerService,Broker,Admin,Manager")]
        [HttpPost("Notes-From-CustomerService")]
        public async Task<IActionResult> AddNotesFromCustomerService([FromForm] NotesFromCustomerServiceDTO notesDTO)
        {
              var userId = GetCurrentUserId();
                
                if (notesDTO == null)
                    return BadRequest(new ApiResponse { Message = "البيانات المرسلة غير صحيحة" });

                if (string.IsNullOrEmpty(notesDTO.Notes) && notesDTO.formFile == null)
                    return BadRequest(new ApiResponse { Message = "يرجى إدخال ملاحظات أو إرفاق ملف" });

                var existingNote = await _db.notesCustomerServices
                    .FirstOrDefaultAsync(n => n.newOrderId == notesDTO.newOrderId);

                string? fileUrl = null;
                string? originalFileName = null;

                if (notesDTO.formFile != null)
                {
                    fileUrl = await SaveFileAsync(notesDTO.formFile);
                    originalFileName = Path.GetFileNameWithoutExtension(notesDTO.formFile.FileName);
                }

                if (existingNote != null)
                {
                    existingNote.Notes = notesDTO.Notes;
                    if (fileUrl != null)
                    {
                        existingNote.fileUrl = fileUrl;
                        existingNote.fileName = originalFileName;
                    }
                    _db.notesCustomerServices.Update(existingNote);
                }
                else
                {
                    var newNote = new NotesCustomerService
                    {
                        Notes = notesDTO.Notes,
                        newOrderId = notesDTO.newOrderId!.Value,
                        fileUrl = fileUrl,
                        fileName = originalFileName
                    };
                    await _db.notesCustomerServices.AddAsync(newNote);
                }

                var result = await _db.SaveChangesAsync();
                if (result > 0)
                {
                    await CreateLogAsync(userId, notesDTO.newOrderId!.Value, "تم إضافة ملاحظات", notesDTO.Notes);
                    return Ok(new ApiResponse { Message = "تم تقديم الملاحظات بنجاح" });
                }

                return BadRequest(new ApiResponse { Message = "فشل في حفظ الملاحظات" });
            
        }

        [Authorize(Roles = "CustomerService,Admin,Manager")]
        [HttpGet("Number-Of-Operations-CustomerService")]
        public async Task<IActionResult> GetNumberOfOperations()
        {
            var userId = GetCurrentUserId();
            
            var statistics = await Task.WhenAll(
                _db.newOrders.Where(o => o.statuOrder == "منفذ").CountAsync(),
                _db.newOrders.Where(o => o.statuOrder == "ملغى").CountAsync(),
                _db.newOrders.Where(o => o.statuOrder == "لم يتم التحويل").CountAsync(),
                _db.newOrders.Where(o => o.statuOrder == "محذوفة").CountAsync()
            );

            return Ok(new
            {
                NumberOfDoneOrders = statistics[0],
                NumberOfRefuseOrders = statistics[1],
                NumberOfNotDoneOrders = statistics[2],
                NumberOfDeletedOrders = statistics[3]
            });
        }
    }
}