using System.Net;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static Dictionary<string, (DateTime Timestamp, int Count)> _requestCounts = new();
    private readonly int _maxRequests;  // Maximum number of requests per time window
    private readonly TimeSpan _timeWindow;  // Time window for rate limiting
    private readonly TimeSpan _blockedDuration;  // Block duration after exceeding rate limit

    public RateLimitingMiddleware(RequestDelegate next,
                                   ILogger<RateLimitingMiddleware> logger,
                                   IConfiguration configuration)
    {
        _next = next;
        _logger = logger;

        // Read rate limiting settings from configuration
        _maxRequests = configuration.GetValue<int>("RateLimiting:MaxRequests");
        _timeWindow = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:WindowDurationInSeconds"));
        _blockedDuration = TimeSpan.FromMinutes(configuration.GetValue<int>("RateLimiting:BlockedDurationInMinutes"));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress.ToString();

        // Check the request counts and rate limit
        if (_requestCounts.ContainsKey(ipAddress))
        {
            var (timestamp, count) = _requestCounts[ipAddress];

            if (DateTime.UtcNow - timestamp < _timeWindow)
            {
                if (count >= _maxRequests)
                {
                    _logger.LogWarning($"Rate limit exceeded for IP: {ipAddress}");
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    return;
                }

                _requestCounts[ipAddress] = (timestamp, count + 1);
            }
            else
            {
                _requestCounts[ipAddress] = (DateTime.UtcNow, 1);
            }
        }
        else
        {
            _requestCounts[ipAddress] = (DateTime.UtcNow, 1);
        }

        await _next(context);
    }
}

// Extension Method for registering the middleware
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
