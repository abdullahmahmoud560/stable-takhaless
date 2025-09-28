using System.Text.RegularExpressions;
using System.Web;
using System.Text;

namespace Infrastructure.Validation
{
    public static class InputSanitizer
    {
        // قائمة بالكلمات المفتاحية الخطيرة
        private static readonly HashSet<string> DangerousKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER", "EXEC", "EXECUTE",
            "UNION", "SCRIPT", "SCRIPTING", "JAVASCRIPT", "VBSCRIPT", "ONLOAD", "ONERROR",
            "ONCLICK", "ONMOUSEOVER", "ONFOCUS", "ONBLUR", "ONCHANGE", "ONSUBMIT", "ONRESET",
            "ONSELECT", "ONDBLCLICK", "ONKEYDOWN", "ONKEYUP", "ONKEYPRESS", "ONMOUSEDOWN",
            "ONMOUSEUP", "ONMOUSEMOVE", "ONMOUSEOUT", "ONMOUSEENTER", "ONMOUSELEAVE",
            "ONCONTEXTMENU", "ONDRAG", "ONDRAGEND", "ONDRAGENTER", "ONDRAGLEAVE",
            "ONDRAGOVER", "ONDRAGSTART", "ONDROP", "ONSCROLL", "ONRESIZE", "ONABORT",
            "ONBEFOREUNLOAD", "ONERROR", "ONHASHCHANGE", "ONLOAD", "ONMESSAGE", "ONOFFLINE",
            "ONONLINE", "ONPAGEHIDE", "ONPAGESHOW", "ONPOPSTATE", "ONSTORAGE", "ONUNLOAD",
            "ONAFTERPRINT", "ONBEFOREPRINT", "ONCANPLAY", "ONCANPLAYTHROUGH", "ONDURATIONCHANGE",
            "ONEMPTIED", "ONENDED", "ONLOADEDDATA", "ONLOADEDMETADATA", "ONLOADSTART",
            "ONPAUSE", "ONPLAY", "ONPLAYING", "ONPROGRESS", "ONRATECHANGE", "ONSEEKED",
            "ONSEEKING", "ONSTALLED", "ONSUSPEND", "ONTIMEUPDATE", "ONVOLUMECHANGE", "ONWAITING"
        };

        // قائمة بالرموز الخطيرة
        private static readonly HashSet<char> DangerousChars = new()
        {
            '<', '>', '"', '\'', '&', ';', '(', ')', '[', ']', '{', '}', '|', '\\', '/',
            '`', '~', '!', '@', '#', '$', '%', '^', '*', '+', '=', '?', ':', ' '
        };

        /// <summary>
        /// تنظيف النصوص من XSS و SQL Injection
        /// </summary>
        public static string SanitizeString(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // HTML Encoding للحماية من XSS
            input = HttpUtility.HtmlEncode(input);
            
            // إزالة HTML tags المتبقية
            input = Regex.Replace(input, @"<[^>]*>", string.Empty, RegexOptions.IgnoreCase);
            
            // إزالة Script tags
            input = Regex.Replace(input, @"<script[^>]*>.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            // إزالة JavaScript events
            input = Regex.Replace(input, @"on\w+\s*=\s*[""'][^""']*[""']", string.Empty, RegexOptions.IgnoreCase);
            
            // إزالة CSS expressions
            input = Regex.Replace(input, @"expression\s*\([^)]*\)", string.Empty, RegexOptions.IgnoreCase);
            
            // إزالة Data URLs
            input = Regex.Replace(input, @"data\s*:\s*[^;]*;base64,", string.Empty, RegexOptions.IgnoreCase);
            
            // إزالة JavaScript URLs
            input = Regex.Replace(input, @"javascript\s*:", string.Empty, RegexOptions.IgnoreCase);
            
            // إزالة VBScript URLs
            input = Regex.Replace(input, @"vbscript\s*:", string.Empty, RegexOptions.IgnoreCase);
            
            // فحص الكلمات المفتاحية الخطيرة
            input = RemoveDangerousKeywords(input);
            
            // إزالة الرموز الخطيرة
            input = RemoveDangerousCharacters(input);
            
            return input.Trim();
        }

        /// <summary>
        /// تنظيف البريد الإلكتروني
        /// </summary>
        public static string SanitizeEmail(string? email)
        {
            if (string.IsNullOrEmpty(email))
                return string.Empty;

            // إزالة المسافات والرموز الخطيرة
            email = email.Trim().ToLower();
            
            // التحقق من صحة تنسيق البريد الإلكتروني
            if (!IsValidEmailFormat(email))
                return string.Empty;
            
            // إزالة الرموز الخطيرة
            email = Regex.Replace(email, @"[^a-zA-Z0-9@._-]", string.Empty);
            
            // التحقق من عدم وجود كلمات مفتاحية خطيرة
            email = RemoveDangerousKeywords(email);
            
            return email;
        }

        /// <summary>
        /// تنظيف أرقام الهواتف
        /// </summary>
        public static string SanitizePhoneNumber(string? phone)
        {
            if (string.IsNullOrEmpty(phone))
                return string.Empty;

            // إزالة جميع الرموز ما عدا الأرقام و + و - و المسافات
            phone = Regex.Replace(phone, @"[^\d+\-\s]", string.Empty);
            
            // إزالة المسافات الزائدة
            phone = Regex.Replace(phone, @"\s+", " ");
            
            return phone.Trim();
        }

        /// <summary>
        /// تنظيف أرقام الهوية
        /// </summary>
        public static string SanitizeIdentity(string? identity)
        {
            if (string.IsNullOrEmpty(identity))
                return string.Empty;

            // إزالة جميع الرموز ما عدا الأرقام
            identity = Regex.Replace(identity, @"[^\d]", string.Empty);
            
            return identity.Trim();
        }

        /// <summary>
        /// تنظيف أسماء المستخدمين
        /// </summary>
        public static string SanitizeName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            // إزالة HTML tags
            name = Regex.Replace(name, @"<[^>]*>", string.Empty, RegexOptions.IgnoreCase);
            
            // إزالة الرموز الخطيرة
            name = Regex.Replace(name, @"[<>""'&;()\[\]{}|\\/`~!@#$%^&*+=?: ]", string.Empty);
            
            // إزالة الكلمات المفتاحية الخطيرة
            name = RemoveDangerousKeywords(name);
            
            return name.Trim();
        }

        /// <summary>
        /// إزالة الكلمات المفتاحية الخطيرة
        /// </summary>
        private static string RemoveDangerousKeywords(string input)
        {
            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var safeWords = new List<string>();

            foreach (var word in words)
            {
                if (!DangerousKeywords.Contains(word))
                {
                    safeWords.Add(word);
                }
            }

            return string.Join(" ", safeWords);
        }

        /// <summary>
        /// إزالة الرموز الخطيرة
        /// </summary>
        private static string RemoveDangerousCharacters(string input)
        {
            var result = new StringBuilder();
            
            foreach (char c in input)
            {
                if (!DangerousChars.Contains(c))
                {
                    result.Append(c);
                }
            }
            
            return result.ToString();
        }

        /// <summary>
        /// التحقق من صحة تنسيق البريد الإلكتروني
        /// </summary>
        private static bool IsValidEmailFormat(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            // التحقق من وجود @
            if (!email.Contains('@'))
                return false;

            // التحقق من عدم وجود @ متعددة
            if (email.Count(c => c == '@') > 1)
                return false;

            // التحقق من عدم بداية أو انتهاء بـ @
            if (email.StartsWith('@') || email.EndsWith('@'))
                return false;

            // التحقق من وجود نقطة بعد @
            var atIndex = email.IndexOf('@');
            if (atIndex == -1 || !email.Substring(atIndex).Contains('.'))
                return false;

            return true;
        }

        /// <summary>
        /// تنظيف شامل للنصوص
        /// </summary>
        public static string SanitizeAll(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // تطبيق جميع طرق التنظيف
            input = SanitizeString(input);
            input = RemoveDangerousKeywords(input);
            input = RemoveDangerousCharacters(input);
            
            return input.Trim();
        }
    }
}