using System.Globalization;
using System.Text.Json;
using Admin.ApplicationDbContext;
using Admin.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Admin.Controllers
{
    [Route("api/")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly DB _db;
        private readonly ILogger<LogsController> _logger;
        private readonly Functions _functions;


        public LogsController(DB db, ILogger<LogsController> logger, Functions functions)
        {
            _db = db;
            _logger = logger;
            _functions = functions;
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Logs/{NewOrderId}")]
        public async Task<IActionResult> GetFilteredLogs(int NewOrderId)
        {
            try
            {
                CultureInfo culture = new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };

                var logs = await _db.Logs
                    .AsNoTracking()
                    .Where(log => log.Message!.Contains("OrderId:")) // فلترة السجلات التي تحتوي على OrderId
                    .Select(log => new { log.Message, log.TimeStamp })
                    .ToListAsync();
                var filteredLogs = logs
                    .Select(log => new
                    {
                        CleanMessage = ExtractCleanMessage(log.Message!),
                        UserId = ExtractUserId(log.Message!),
                        OrderId = ExtractOrderId(log.Message!),
                        Notes = ExtractNotes(log.Message!),
                        TimeStamp = log.TimeStamp!.Value.ToString("dddd, dd MMMM yyyy, hh:mm tt", culture)
                    })
                    .Where(log => log.OrderId == NewOrderId) // تصفية حسب OrderId
                    .ToList();
                if (!filteredLogs.Any())
                {
                    return NotFound(new { message = "لا توجد سجلات مطابقة" });
                }

                List<LogsDTO> logsDTO = new List<LogsDTO>();
                foreach (var log in filteredLogs)
                {
                    var response = await _functions.SendAPI(log.UserId!);
                    if (response.HasValue && response.Value.TryGetProperty("fullName", out JsonElement fullName) && response.Value.TryGetProperty("email", out JsonElement email))
                    {
                        logsDTO.Add(new LogsDTO
                        {
                            Message = log.CleanMessage,
                            NewOrderId = log.OrderId,
                            TimeStamp = log.TimeStamp,
                            fullName = fullName.GetString(),
                            Email = email.GetString(),
                            Notes = log.Notes
                        });
                    }
                }

                return Ok(logsDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء تنفيذ العملية");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "حدث خطأ، برجاء المحاولة لاحقًا" });
            }
        }

        // استخراج الجزء الأساسي من الرسالة قبل أول "|"
        private string ExtractCleanMessage(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message)) return string.Empty;
                return message.Split('|')[0].Trim(); // أول جزء قبل "|"
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء تنظيف الرسالة");
                return string.Empty;
            }
        }

        // استخراج UserId من الرسالة بنفس طريقة SQL
        private string ExtractUserId(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message) || !message.Contains("UserId:")) return null!;

                string userId = message.Split('|')
                    .FirstOrDefault(part => part.Contains("UserId:"))?
                    .Replace("UserId:", "").Trim()!;

                // إزالة علامات التنصيص المزدوجة إن وجدت
                return userId.Trim('"');
            }
            catch (Exception)
            {
                return null!;
            }
        }

        // استخراج OrderId من الرسالة وتحويله إلى عدد صحيح
        private int? ExtractOrderId(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message) || !message.Contains("OrderId:")) return null;
                string orderIdStr = message.Split('|').FirstOrDefault(part => part.Contains("OrderId:"))?.Replace("OrderId:", "").Trim()!;
                return int.TryParse(orderIdStr, out int orderId) ? orderId : (int?)null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // استخراج Notes وإزالة علامات التنصيص
        private string ExtractNotes(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message) || !message.Contains("Notes:")) return null!;
                string notes = message.Split('|').FirstOrDefault(part => part.Contains("Notes:"))?.Replace("Notes:", "").Trim()!;

                return notes?.Replace("\"", "")!; // إزالة علامات التنصيص
            }
            catch (Exception)
            {
                return null!;
            }
        }
    }
}
