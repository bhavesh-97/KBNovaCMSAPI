public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");

        // Proceed with the request
        await _next(context);

        var endTime = DateTime.UtcNow;
        var timeTaken = endTime - startTime;
        _logger.LogInformation($"Response: {context.Response.StatusCode} took {timeTaken.TotalMilliseconds} ms");
    }
}

// Extension Method for registering the middleware
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
