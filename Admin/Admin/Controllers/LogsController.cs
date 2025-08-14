using Admin.ApplicationDbContext;
using Admin.DTO;
using Admin.Helpers;
using Admin.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ILogService = Admin.Services.ILogService;

namespace Admin.Controllers
{
    [Route("api/")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly DB _db;
        private readonly ILogService _logService;

        public LogsController(DB db, ILogService logService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("Logs/{NewOrderId}/{Page}")]
        public async Task<IActionResult> GetFilteredLogs(int NewOrderId, int Page)
        {
            if (!DTO.Helpers.Validation.IsValidPage(Page))
            {
                return ErrorHandler.HandleValidationError("رقم الصفحة غير صحيح");
            }

            
                var paginationResult = await GetPaginatedLogs(NewOrderId, Page);
                if (!paginationResult.IsSuccess)
                {
                    return ErrorHandler.HandleException(new Exception(paginationResult.ErrorMessage));
                }

                return Ok(paginationResult.Data);
           
        }

        [Authorize]
        [HttpPost("Add-Logs")]
        public async Task<IActionResult> AddLogs(AddLogs addLogs)
        {
            if (addLogs == null)
            {
                return ErrorHandler.HandleValidationError("البيانات المدخلة غير صحيحة");
            }

            if (!DTO.Helpers.Validation.IsValidId(addLogs.UserId))
            {
                return ErrorHandler.HandleValidationError("معرف المستخدم غير صحيح");
            }

            
                var log = addLogs.ToLog();
                _db.Logs.Add(log);
                await _db.SaveChangesAsync();
                
                return Ok(ApiResponse.Success("تم إضافة السجل بنجاح"));
            
        }

        private async Task<ApiResult<object>> GetPaginatedLogs(int newOrderId, int page)
        {
            var culture = DTO.Helpers.Culture.CreateArabicCulture();
            var query = _db.Logs
                .Where(log => log.NewOrderId == newOrderId)
                .OrderByDescending(l => l.TimeStamp);

            var totalCount = await query.CountAsync();
            var pageSize = DTO.Helpers.Constants.DEFAULT_PAGE_SIZE;

            var logs = await query
                .OrderBy(log => log.TimeStamp) // ترتيب تصاعدي
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            if (!logs.Any())
            {
                return ApiResult<object>.Success(PaginatedResponse<LogsDTO>.Create(page, pageSize, totalCount, new List<LogsDTO>()));
            }

            var logsDto = await ConvertLogsToDto(logs, culture);

            return ApiResult<object>.Success(PaginatedResponse<LogsDTO>.Create(page, pageSize, totalCount, logsDto));
        }

        private async Task<List<LogsDTO>> ConvertLogsToDto(List<Logs> logs, CultureInfo culture)
        {
            var logsDto = new List<LogsDTO>();

            foreach (var log in logs)
            {
                var userInfo = await _logService.GetUserInfoAsync(log.UserId!);
                if (userInfo.IsSuccess)
                {
                    var formattedDate = DTO.Helpers.Culture.FormatDateTime(log.TimeStamp, culture);
                    logsDto.Add(log.ToLogsDto(userInfo.Data.fullName, userInfo.Data.email, formattedDate));
                }
            }

            return logsDto;
        }
    }
}
