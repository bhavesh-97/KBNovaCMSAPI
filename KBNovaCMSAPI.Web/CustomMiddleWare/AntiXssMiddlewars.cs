using Newtonsoft.Json;
using System.Net;
using System.Text.RegularExpressions;

namespace KBNovaCMS.CustomMiddleWare
{
    public static class AntiXssMiddlewareExtension
    {
        public static IApplicationBuilder UseAntiXssMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AntiXssMiddleware>();
        }
    }

    public class ErrorResponse
    {
        public int ErrorCode { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Class to handle basic Cross-Site Scripting validation
    /// </summary>
    public static class CrossSiteScriptingValidation
    {
        private static readonly char[] StartingChars = { '<', '&', '>' };

        private static bool IsAtoZ(char c)
        {
            return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z';
        }

        public static bool IsDangerousString(string s, out int matchIndex)
        {
            matchIndex = 0;
            for (var i = 0; ;)
            {
                var n = s.IndexOfAny(StartingChars, i);
                if (n < 0)
                    return false;

                if (n == s.Length - 1)
                    return false;

                matchIndex = n;
                switch (s[n])
                {
                    case '<':
                        if (IsAtoZ(s[n + 1]) || s[n + 1] == '!' || s[n + 1] == '/' || s[n + 1] == '?')
                            return true;
                        break;
                    case '&':
                        if (s[n + 1] == '#')
                            return true;
                        break;
                }

                i = n + 1;
            }
        }

        public static string ToJSON(this object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }

    public class AntiXssMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly int _statusCode = (int)HttpStatusCode.BadRequest;

        public AntiXssMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext context)
        {
            // Check XSS in various parts of the request (URL, Headers, Cookies, etc.)
            if (await IsXSSDetected(context))
            {
                await RespondWithAnError(context);
                return;
            }

            // If no XSS detected, proceed with the next middleware
            await _next(context);
        }

        private async Task<bool> IsXSSDetected(HttpContext context)
        {
            // Check URL Path and Query String
            if (IsXSSInString(context.Request.Path) || IsXSSInString(context.Request.QueryString.Value))
                return true;

            // Validate Request Headers for XSS
            foreach (var header in context.Request.Headers)
            {
                if (IsXSSInString(header.Value.ToString()))
                    return true;
            }

            // Validate Cookies for XSS
            foreach (var cookie in context.Request.Cookies)
            {
                if (IsXSSInString(cookie.Value))
                    return true;
            }

            // Validate Form Data (if available)
            if (context.Request.HasFormContentType)
            {
                foreach (var item in context.Request.Form)
                {
                    if (IsXSSInString(item.Value.ToString()))
                        return true;
                }
            }

            // Validate the Request Body
            string bodyContent = await ReadRequestBodyAsync(context);
            return IsXSSInString(bodyContent);
        }

        private bool IsXSSInString(string value)
        {
            // Use a simple regex to detect common XSS patterns in the value
            if (string.IsNullOrWhiteSpace(value)) return false;
            string pattern = @"<script.*?>.*?</script>|<.*?javascript:.*?>|<.*?on.*?=.*?>";
            return Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase);
        }

        private async Task<string> ReadRequestBodyAsync(HttpContext context)
        {
            // Read the request body and return it as a string
            var buffer = new MemoryStream();
            await context.Request.Body.CopyToAsync(buffer);
            context.Request.Body = buffer;
            buffer.Position = 0;
            var content = await new StreamReader(buffer).ReadToEndAsync();
            context.Request.Body.Position = 0;
            return content;
        }

        private async Task RespondWithAnError(HttpContext context)
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = _statusCode;

            var errorResponse = new ErrorResponse
            {
                Description = "XSS Detected in Request",
                ErrorCode = _statusCode
            };

            await context.Response.WriteAsync(errorResponse.ToJSON());
        }
    }
}
