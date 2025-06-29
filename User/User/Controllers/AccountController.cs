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
        [HttpGet("Get-All-Done-Accept-Orders")]
        public async Task<IActionResult> getAllDoneAcceptOrders()
        {
            try
            {
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };
                var result = await _db.newOrders
                    .Where(l => l.statuOrder == "تم التنفيذ")
                    .Select(order => new
                    {
                        order.Id,
                        order.statuOrder,
                        order.Location,
                        order.Date,
                        order.UserId,
                        order.Accept,
                        order.AcceptCustomerService,
                    })
                    .ToListAsync();

                if (!result.Any())
                {
                    return Ok(new string[] { });
                }

                List<GetOrdersDTO> ordersDTOs = new List<GetOrdersDTO>();

                foreach (var order in result)
                {

                    var typeOrder = await _db.typeOrders
                        .Where(l => l.newOrderId == order.Id)
                        .Select(l => l.typeOrder)
                        .FirstOrDefaultAsync();
                    var value = await _db.values.Where(l => l.newOrderId == order.Id).ToListAsync();
                    var Response = await _functions.BrokerandUser(order.Accept!, order.AcceptCustomerService!);

                    if (Response.Value.TryGetProperty("user", out JsonElement User) &&
                        Response.Value.TryGetProperty("broker", out JsonElement CustomerService))
                    {
                        if (User.TryGetProperty("fullName", out JsonElement UserName) &&
                            User.TryGetProperty("email", out JsonElement UserEmail) &&
                            CustomerService.TryGetProperty("fullName", out JsonElement CustomerServiceName) &&
                            CustomerService.TryGetProperty("email", out JsonElement CustomerServiceEmail))
                            ordersDTOs.Add(new GetOrdersDTO
                            {
                                Id = order.Id.ToString(),
                                statuOrder = order.statuOrder,
                                Location = order.Location,
                                typeOrder = typeOrder,
                                Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                                Email = UserEmail.ToString(),
                                fullName = UserName.ToString(),
                                CustomerServiceEmail = CustomerServiceEmail.ToString(),
                                CustomerServiceName = CustomerServiceName.ToString(),
                                BrokerID = order.Accept,
                                Value = value[0].Value,
                            });
                    }
                }
                return Ok(ordersDTOs);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
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
        [HttpGet("Get-All-Done-Transfer-Orders")]
        public async Task<IActionResult> getAllDoneTransferOrders()
        {
            try
            {
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };
                var result = await _db.newOrders.Where(l => l.statuOrder == "تم التحويل").ToListAsync();
                List<GetOrdersDTO> ordersDTOs = new List<GetOrdersDTO>();
                if (result.Any())
                {
                    foreach (var order in result)
                    {
                        var typeOrder = await _db.typeOrders
                            .Where(l => l.newOrderId == order.Id)
                            .FirstOrDefaultAsync();
                        var value = await _db.values.Where(l => l.newOrderId == order.Id).ToListAsync();
                        var Response = await _functions.BrokerandUser(order.Accept!, order.AcceptAccount!);

                        if (Response.Value.TryGetProperty("user", out JsonElement User) &&
                            Response.Value.TryGetProperty("broker", out JsonElement Account))
                        {
                            if (User.TryGetProperty("fullName", out JsonElement UserName) &&
                                User.TryGetProperty("email", out JsonElement UserEmail) &&
                                Account.TryGetProperty("fullName", out JsonElement AccountName) &&
                                Account.TryGetProperty("email", out JsonElement AccountEmail))
                                ordersDTOs.Add(new GetOrdersDTO
                                {
                                    Id = order.Id.ToString(),
                                    statuOrder = order.statuOrder,
                                    Location = order.Location,
                                    typeOrder = typeOrder?.typeOrder,
                                    Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                                    Email = UserEmail.GetString(),
                                    fullName = UserName.GetString(),
                                    AccountEmail = AccountEmail.GetString(),
                                    AccountName = AccountName.GetString(),
                                    BrokerID = order.Accept,
                                    Value = value[0].Value,
                                });
                        }
                    }
                    return Ok(ordersDTOs);
                }
                return Ok(new string[] { });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
            }
        }

        [Authorize(Roles = "Account,Admin,Manager")]
        [HttpGet("Get-All-Not-Done-Transfer-Orders")]
        public async Task<IActionResult> getAllNotDoneTransferOrders()
        {
            try
            {
                var result = await _db.newOrders.Where(l => l.statuOrder == "لم يتم التحويل").ToListAsync();
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };
                List<GetOrdersDTO> ordersDTOs = new List<GetOrdersDTO>();
                if (result.Any())
                {

                    foreach (var order in result)
                    {
                        var typeOrder = await _db.typeOrders
                            .Where(l => l.newOrderId == order.Id)
                            .FirstOrDefaultAsync();
                        var Response = await _functions.SendAPI(order.UserId!);
                        if (Response.HasValue && Response.Value.TryGetProperty("fullName", out JsonElement fullName) && Response.Value.TryGetProperty("phoneNumber", out JsonElement phoneNumber))
                        {

                            ordersDTOs.Add(new GetOrdersDTO
                            {
                                Id = order.Id.ToString(),
                                statuOrder = order.statuOrder,
                                Location = order.Location,
                                typeOrder = typeOrder?.typeOrder,
                                Date = order.Date!.Value.ToString("dddd, dd MMMM yyyy", culture),
                                fullName = fullName.ToString(),
                                phoneNumber = phoneNumber.ToString(),
                                BrokerID = order.Accept,
                            });
                        }
                    }
                    return Ok(ordersDTOs);
                }
                return Ok(new string[] { });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
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

        //[Authorize(Roles = "Account,CustomerService,Admin,Manager")]
        //[HttpPost("DownloadFiles-From-Account")]
        //public async Task<IActionResult> downloadFiles(GetNewOrderID getNewOrderID)
        //{
        //    try
        //    {
        //        var file = await _db.notesCustomerServices.Where(l => l.newOrderId == getNewOrderID.newOrderId).ToListAsync();
        //        var updatedFiles = file
        //            .Select((f) => new
        //            {
        //                fileName = f.fileName,
        //                fileData = f.fileData,
        //                newOrder = f.newOrder,
        //                ContentType = f.ContentType,
        //                newOrderId = f.newOrderId
        //            }).ToList();
        //        if (updatedFiles != null)
        //        {
        //            Response.Headers.Add("Content-Disposition", $"attachment; filename*=UTF-8''{Uri.EscapeDataString(updatedFiles[0].fileName)}");
        //            return File(updatedFiles[0].fileData!, updatedFiles[0].ContentType!, updatedFiles[0].fileName);
        //        }
        //        return Ok();
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ برجاء المحاولة فى وقت لاحق " });
        //    }
        //}

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