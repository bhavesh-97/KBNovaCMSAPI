using System.Text.RegularExpressions;

public class SqlInjectionPreventionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SqlInjectionPreventionMiddleware> _logger;

    // Constructor to inject the next middleware and logger
    public SqlInjectionPreventionMiddleware(RequestDelegate next, ILogger<SqlInjectionPreventionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Inspect all incoming HTTP request elements (Query string, headers, and body)
        if (HasSqlInjectionAttempt(context.Request))
        {
            // If suspicious input is detected, log the attempt and return a 400 Bad Request
            _logger.LogWarning("SQL Injection attempt detected and blocked! IP: {IpAddress}, RequestPath: {RequestPath}",
                                context.Connection.RemoteIpAddress?.ToString(), context.Request.Path);

            // Return a 400 Bad Request to indicate the request was blocked
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("SQL Injection detected. Your request is blocked.");
            return;
        }

        // If no issues found, proceed to the next middleware in the pipeline
        await _next(context);
    }

    // Method to check if the request contains potential SQL injection patterns
    private bool HasSqlInjectionAttempt(HttpRequest request)
    {
        // Define a regular expression pattern to match common SQL injection keywords and patterns
        var sqlInjectionPattern = new Regex(@"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|EXEC|UNION|--|\bOR\b|\bAND\b|\bINSERT\b|\bJOIN\b|\b--|\b;|#)\b)", RegexOptions.IgnoreCase);

        // Check query string
        if (request.QueryString.HasValue && sqlInjectionPattern.IsMatch(request.QueryString.Value))
        {
            return true;
        }

        // Check headers (e.g., Referer, User-Agent, etc.)
        // if (request.Headers.Any(header => sqlInjectionPattern.IsMatch(header.Value.ToString())))
        // {
        //     return true;
        // }

        // Check the body of the request (POST/PUT methods)
        if (request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
            request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
        {
            // Read and inspect the body
            try
            {
                request.EnableBuffering();  // Enable buffering to read the request body multiple times
                var body = new StreamReader(request.Body).ReadToEnd();
                request.Body.Seek(0, SeekOrigin.Begin);  // Reset the stream position for further middleware

                // Check for SQL injection patterns in the body
                if (sqlInjectionPattern.IsMatch(body))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Handle any error that occurs while reading the body (e.g., non-readable streams)
                _logger.LogError(ex, "Error reading request body for SQL injection check.");
                return false;
            }
        }

        // No SQL injection detected
        return false;
    }
}
