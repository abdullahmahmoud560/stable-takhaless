using Admin.ApplicationDbContext;
using Admin.DTO;
using Admin.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace Admin.Controllers
{
    [Route("api/")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly DB _db;
        private readonly Functions _functions;


        public LogsController(DB db,Functions functions)
        {
            _db = db;
            _functions = functions;
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Logs/{NewOrderId}")]
        public async Task<IActionResult> GetFilteredLogs(int NewOrderId)
        {
            try
            {
                var culture = new CultureInfo("ar-SA");
                culture.DateTimeFormat.Calendar = new GregorianCalendar();
                culture.NumberFormat.DigitSubstitution = DigitShapes.NativeNational;

                var logs = await _db.Logs
                    .Where(log => log.NewOrderId == NewOrderId)
                    .ToListAsync();

                if (!logs.Any())
                {
                    return NotFound(new { message = "لا توجد سجلات مطابقة" });
                }

                var logsDTO = new List<LogsDTO>();

                foreach (var log in logs)
                {
                    var response = await _functions.SendAPI(log.UserId!);

                    if (response.HasValue &&
                        response.Value.TryGetProperty("fullName", out JsonElement fullName) &&
                        response.Value.TryGetProperty("email", out JsonElement email))
                    {
                        string formattedDate = string.Empty;
                        if (log.TimeStamp != null)
                        {
                            formattedDate = ((DateTime)log.TimeStamp).ToString("dddd, dd MMMM yyyy, hh:mm tt", culture);
                        }

                        logsDTO.Add(new LogsDTO
                        {
                            Message = log.Message,
                            NewOrderId = log.NewOrderId,
                            TimeStamp = formattedDate,
                            fullName = fullName.GetString(),
                            Email = email.GetString(),
                            Notes = log.Notes
                        });
                    }
                }

                return Ok(logsDTO);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,new ApiResponse { Message = "حدث خطأ، برجاء المحاولة لاحقًا" });
            }
        }

        [Authorize]
        [HttpPost("Add-Logs")]
        public async Task<IActionResult> addLogs(AddLogs addLogs)
        {
            if(addLogs == null)
            {
                return BadRequest(new ApiResponse { Message = "البيانات المدخلة غير صحيحة" });
            }
            try
            {
                var log = new Logs
                {
                    Message = addLogs.Message!,
                    NewOrderId = addLogs.NewOrderId ?? 0,
                    UserId = addLogs.UserId!,
                    Notes = addLogs.Notes!
                };
                _db.Logs.Add(log);
                await _db.SaveChangesAsync();
                return Ok(new ApiResponse { Message = "تم إضافة السجل بنجاح" });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ أثناء إضافة السجل" });
            }
        }
    }
}
