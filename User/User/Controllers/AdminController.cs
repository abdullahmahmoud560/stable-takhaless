using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
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
                var CountActiveOrders = _db.newOrders.Where(l => (l.statuOrder != "لم يتم التحويل" || l.statuOrder != "تم التحويل" || l.statuOrder != "ملغى" || l.statuOrder != "محذوفة") && l.Date!.Value.AddDays(7) > DateTime.Now).Count();
                var CountDoneOrders = _db.newOrders.Where(l => l.statuOrder == "تم التحويل").Count();
                var Exports = await _db.values
                    .Where(v => v.Accept == true &&
                           _db.newOrders.Any(o => o.Id == v.newOrderId && o.statuOrder == "تم التحويل"))
                    .SumAsync(v => (double?)v.Value) ?? 0.0;
                return Ok(new { CountAllUsers = Response, CountActiveOrders = CountActiveOrders, CountDoneOrders = CountDoneOrders, Exports = Exports });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق" });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Evaluation-Broker")]
        public async Task<IActionResult> evaluationBroker()
        {
            try
            {
                var Response = await _functions.GetAllBroker();
                var brokersList = Response.Value.EnumerateArray()
                                .Select(broker => new
                                {
                                    FullName = broker.GetProperty("fullName").GetString(),
                                    Id = broker.GetProperty("id").GetString(),
                                    Email = broker.GetProperty("email").GetString(),
                                })
                                .ToList();
                if (brokersList.Any())
                {
                    List<StatiticsDTO> statitics = new List<StatiticsDTO>();
                    foreach (var broker in brokersList)
                    {
                        var count = await _db.newOrders.Where(l => l.Accept == broker.Id && l.statuOrder == "تم التحويل").ToListAsync();
                        statitics.Add(new StatiticsDTO
                        {
                            fullName = broker.FullName,
                            Email = broker.Email,
                            Count = count.Count
                        });
                    }
                    return Ok(statitics);
                }
                return Ok(new string[] { });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Get-All-Orders-For-Admin")]
        public async Task<IActionResult> getAllOrdersForAdmin()
        {
            try
            {
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                var orders = await _db.newOrders
                    .Select(l => new
                    {
                        l.Id,
                        l.Location,
                        l.statuOrder,
                        l.Date,
                    })
                    .ToListAsync();
                List<GetOrdersDTO> ordersList = new List<GetOrdersDTO>();
                if (orders.Any())
                {
                    foreach (var order in orders)
                    {
                        ordersList.Add(new GetOrdersDTO
                        {
                            Id = order.Id.ToString(),
                            Location = order.Location,
                            statuOrder = order.statuOrder,
                            Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                        });
                    }
                }
                return Ok(ordersList);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
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
        [HttpGet("Orders-Expired-Date")]
        public async Task<IActionResult> orderExpiredDate()
        {
            try
            {
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };
                var sevenDaysAgo = DateTime.Now.Date.AddDays(-7);
                var orders = await _db.newOrders.Where(l => l.Date != null && l.Date.Value.Date <= sevenDaysAgo).ToListAsync();
                List<GetOrdersDTO> ordersList = new List<GetOrdersDTO>();
                foreach (var order in orders)
                {
                    ordersList.Add(new GetOrdersDTO
                    {
                        Id = order.Id.ToString(),
                        Location = order.Location,
                        statuOrder = order.statuOrder,
                        Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                    });
                }
                return Ok(ordersList);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
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