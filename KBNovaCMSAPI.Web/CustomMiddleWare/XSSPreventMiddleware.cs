using System.Text.RegularExpressions;

public class XSSPreventMiddleware
{
    private readonly RequestDelegate _next;

    // Precompiled regex patterns for detecting XSS
    private static readonly Regex _xssPatternScriptTags = new Regex(@"(<script.*?>.*?</script>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _xssPatternJavascript = new Regex(@"(javascript\s*:|data\s*:|vbscript\s*:)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _xssPatternEventHandlers = new Regex(@"(<.*?[\s]+on\w+.*?=\s*['""]?[^'""]*['""]?)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public XSSPreventMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check for XSS in headers, cookies, path, and query string
        if (await IsXSSDetected(context))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Potential XSS content detected.");
            return;
        }

        // Proceed to next middleware if no XSS detected
        await _next(context);
    }

    private async Task<bool> IsXSSDetected(HttpContext context)
    {
        // Check headers for XSS
        if (context.Request.Headers.Any(header => ContainsXSS(header.Key) || ContainsXSS(header.Value)))
        {
            return true;
        }

        // Check URL (path and query string)
        if (ContainsXSS(context.Request.Path) || ContainsXSS(context.Request.QueryString.ToString()))
        {
            return true;
        }

        // Check cookies for XSS
        if (context.Request.Cookies.Any(cookie => ContainsXSS(cookie.Key) || ContainsXSS(cookie.Value)))
        {
            return true;
        }

        // Check POST body for XSS (only if method is POST)
        if (context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            context.Request.EnableBuffering();
            using (var reader = new StreamReader(context.Request.Body))
            {
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset body stream position

                if (ContainsXSS(body))
                {
                    return true; // XSS detected in request body
                }
            }
        }

        return false; // No XSS detected
    }

    // Simplified method to check input for XSS patterns
    private bool ContainsXSS(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        // Check for known XSS patterns
        return _xssPatternScriptTags.IsMatch(input) ||
               _xssPatternJavascript.IsMatch(input) ||
               _xssPatternEventHandlers.IsMatch(input);
    }
}
