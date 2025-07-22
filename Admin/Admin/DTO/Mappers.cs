using Admin.Model;

namespace Admin.DTO
{
    public static class Mappers
    {
        public static Logs ToLog(this AddLogs addLogs)
        {
            return new Logs
            {
                Message = addLogs.Message!,
                NewOrderId = addLogs.NewOrderId ?? 0,
                UserId = addLogs.UserId!,
                TimeStamp = addLogs.TimeStamp ?? DateTime.Now,
                Notes = addLogs.Notes!
            };
        }

        public static LogsDTO ToLogsDto(this Logs log, string fullName, string email, string formattedDate)
        {
            return new LogsDTO
            {
                Message = log.Message,
                NewOrderId = log.NewOrderId,
                TimeStamp = formattedDate,
                fullName = fullName,
                Email = email,
                Notes = log.Notes
            };
        }
    }
} 