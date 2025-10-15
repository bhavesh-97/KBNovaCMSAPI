using System.Net;

public class IpWhitelistingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpWhitelistingMiddleware> _logger;
    private readonly List<string> _whitelistedIps = new List<string> { "192.168.1.100", "203.0.113.10" };  // Add allowed IPs here

    public IpWhitelistingMiddleware(RequestDelegate next, ILogger<IpWhitelistingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress.ToString();

        if (!_whitelistedIps.Contains(ipAddress))
        {
            _logger.LogWarning($"Request from non-whitelisted IP: {ipAddress}");
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync("Forbidden");
            return;
        }

        await _next(context);
    }
}

// Extension Method for registering the middleware
public static class IpWhitelistingMiddlewareExtensions
{
    public static IApplicationBuilder UseIpWhitelisting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IpWhitelistingMiddleware>();
    }
}
