using System.Globalization;

namespace Admin.DTO
{
    public static class Helpers
    {
        public static class Culture
        {
            public static CultureInfo CreateArabicCulture()
            {
                return new CultureInfo("ar-SA")
                {
                    DateTimeFormat = { Calendar = new GregorianCalendar() },
                    NumberFormat = { DigitSubstitution = DigitShapes.NativeNational }
                };
            }

            public static string FormatDateTime(DateTime? timeStamp, CultureInfo culture)
            {
                return timeStamp?.ToString("dddd, dd MMMM yyyy, hh:mm tt", culture) ?? string.Empty;
            }
        }

        public static class Validation
        {
            public static bool IsValidId(string? id)
            {
                return !string.IsNullOrWhiteSpace(id);
            }

            public static bool IsValidPage(int page)
            {
                return page > 0;
            }

            public static bool IsValidPageSize(int pageSize)
            {
                return pageSize > 0 && pageSize <= 100; // حد أقصى 100 عنصر في الصفحة
            }
        }

        public static class Constants
        {
            public const int DEFAULT_PAGE_SIZE = 10;
            public const int MAX_PAGE_SIZE = 100;
            public const string TOKEN_COOKIE_NAME = "token";
            public const string API_BASE_URL = "http://firstproject-service:9100/api";
        }
    }
} 