using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using User.ApplicationDbContext;
using User.DTO;

namespace User.Controllers
{
    [Route("api/")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly DB _db;
        private readonly Functions _functions;

        public AdminController(DB db, Functions functions)
        {
            _db = db;
            _functions = functions;
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Statistics")]
        public async Task<IActionResult> statistics()
        {
            try
            {
                var Response = await _functions.Admin();

                // ✅ تحسين: استخدام استعلامات منفصلة و AsNoTracking
                var CountActiveOrders = await _db.newOrders
                    .AsNoTracking()
                    .Where(l => (l.statuOrder != "لم يتم التحويل" || l.statuOrder != "تم التحويل" || l.statuOrder != "ملغى" || l.statuOrder != "محذوفة") && l.Date!.Value.AddDays(7) > DateTime.Now)
                    .CountAsync();

                var CountDoneOrders = await _db.newOrders
                    .AsNoTracking()
                    .Where(l => l.statuOrder == "تم التحويل")
                    .CountAsync();

                // ✅ تحسين: استخدام Join بدلاً من Any
                var Exports = await _db.values
                    .AsNoTracking()
                    .Where(v => v.Accept == true)
                    .Join(_db.newOrders,
                        value => value.newOrderId,
                        order => order.Id,
                        (value, order) => new { value.Value, order.statuOrder })
                    .Where(x => x.statuOrder == "تم التحويل")
                    .SumAsync(x => (double?)x.Value) ?? 0.0;

                return Ok(new { CountAllUsers = Response, CountActiveOrders = CountActiveOrders, CountDoneOrders = CountDoneOrders, Exports = Exports });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق" });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Evaluation-Broker/{Page}")]
        public async Task<IActionResult> EvaluationBroker(int Page)
        {
            try
            {
                const int PageSize = 10;

                var Response = await _functions.GetAllBroker(Page);
                if (!Response.Value.TryGetProperty("data", out var dataProperty))
                {
                    return BadRequest("الاستجابة لا تحتوي على خاصية 'data'");
                }

                if (dataProperty.ValueKind != JsonValueKind.Array)
                {
                    return BadRequest("خاصية 'data' ليست من النوع Array");
                }

                var brokersList = dataProperty.EnumerateArray()
                    .Select(broker => new
                    {
                        FullName = broker.GetProperty("fullName").GetString(),
                        Id = broker.GetProperty("id").GetString(),
                        Email = broker.GetProperty("email").GetString(),
                    })
                    .ToList();


                if (!brokersList.Any())
                {
                    return Ok(new string[] { });
                }

                // ✅ تحسين: جلب جميع الـ BrokerIDs مرة واحدة
                var brokerIds = brokersList.Select(b => b.Id).ToList();
                var brokerOrderCounts = await _db.newOrders
                    .AsNoTracking()
                    .Where(l => brokerIds.Contains(l.Accept!) && l.statuOrder == "تم التحويل")
                    .GroupBy(l => l.Accept)
                    .Select(g => new { BrokerID = g.Key, Count = g.Count() })
                    .ToListAsync();

                // ✅ تحسين: استخدام Select بدلاً من foreach
                var statitics = brokersList.Select(broker =>
                {
                    var count = brokerOrderCounts.FirstOrDefault(b => b.BrokerID == broker.Id)?.Count ?? 0;
                    return new StatiticsDTO
                    {
                        fullName = broker.FullName,
                        Email = broker.Email,
                        Count = count
                    };
                }).ToList();

                var totalCount = statitics.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);
                var paginatedStats = statitics
                    .OrderByDescending(s => s.Count) // ترتيب تنازلي حسب عدد الطلبات
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = totalCount,
                    data = paginatedStats
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " + ex.Message });
            }
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-All-Orders-For-Admin/{Page}")]
        public async Task<IActionResult> GetAllOrdersForAdmin(int Page)
        {
            try
            {
                const int PageSize = 10;

                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                // ✅ تحسين: استخدام AsNoTracking و Pagination في قاعدة البيانات
                var baseQuery = _db.newOrders
                    .AsNoTracking()
                    .OrderByDescending(l => l.Date);

                var totalCount = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                var orders = await baseQuery
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .Select(l => new GetOrdersDTO
                    {
                        Id = l.Id.ToString(),
                        Location = l.Location,
                        statuOrder = l.statuOrder,
                        Date = l.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                    })
                    .ToListAsync();

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = totalCount,
                    data = orders
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }


        [Authorize(Roles = "Admin,Manager,CustomerService")]
        [HttpPost("Delete-Notes-For-Admin")]
        public async Task<IActionResult> deleteNotes(GetID getID)
        {
            try
            {
                if (getID.ID != 0)
                {
                    var orderDetails = await _db.notesCustomerServices.Where(l => l.newOrderId == getID.ID).FirstOrDefaultAsync();
                    if (orderDetails == null)
                    {
                        return NotFound(new ApiResponse { Message = "الطلب غير موجود" });
                    }
                    orderDetails!.Notes = null;
                    await _db.SaveChangesAsync();
                    return Ok(new ApiResponse { Message = "تم حذف الملاحظة بنجاح" });
                }
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Orders-Expired-Date/{Page}")]
        public async Task<IActionResult> OrderExpiredDate(int Page)
        {
            try
            {
                const int PageSize = 10;

                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                var sevenDaysAgo = DateTime.Now.Date.AddDays(-7);

                // ✅ تحسين: استخدام AsNoTracking و Pagination في قاعدة البيانات
                var baseQuery = _db.newOrders
                    .AsNoTracking()
                    .Where(l => l.Date != null && l.Date.Value.Date <= sevenDaysAgo)
                    .OrderByDescending(l => l.Date);

                var totalCount = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);

                var orders = await baseQuery
                    .Skip((Page - 1) * PageSize)
                    .Take(PageSize)
                    .Select(order => new GetOrdersDTO
                    {
                        Id = order.Id.ToString(),
                        Location = order.Location,
                        statuOrder = order.statuOrder,
                        Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                    })
                    .ToListAsync();

                return Ok(new
                {
                    TotalPages = totalPages,
                    Page = Page,
                    totalUser = totalCount,
                    data = orders
                });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("Get-Count-Of-Orders-From-Active-Users")]
        public async Task<IActionResult> getCountOfOrdersFromActiveUsers([FromBody] GetID getID)
        {
            try
            {
                if (getID.BrokerID != null)
                {
                    var totalOrders = await _db.newOrders.Where(l => l.UserId == getID.BrokerID).CountAsync();
                    var successOrders = await _db.newOrders.Where(l => l.UserId == getID.BrokerID && l.statuOrder == "تم التحويل").CountAsync();
                    return Ok(new { totalOrders = totalOrders, successOrders = successOrders });
                }
                return BadRequest(new ApiResponse { Message = "الرجاء ادخال بيانات صحيحة" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق" });
            }
        }
    }
}