using Microsoft.Extensions.Caching.Memory;

public class ResponseCachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ResponseCachingMiddleware> _logger;

    public ResponseCachingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<ResponseCachingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var cacheKey = $"response_{context.Request.Path}";
        if (_cache.TryGetValue(cacheKey, out string cachedResponse))
        {
            _logger.LogInformation("Serving cached response for {0}", context.Request.Path);
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cachedResponse);
            return;
        }

        var originalBody = context.Response.Body;
        using (var newBody = new MemoryStream())
        {
            context.Response.Body = newBody;
            await _next(context);

            newBody.Seek(0, SeekOrigin.Begin);
            var response = new StreamReader(newBody).ReadToEnd();
            _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5)); // Cache for 5 minutes

            newBody.Seek(0, SeekOrigin.Begin);
            await newBody.CopyToAsync(originalBody);
        }
    }
}

// Extension Method for registering the middleware
public static class ResponseCachingMiddlewareExtensions
{
    public static IApplicationBuilder UseResponseCaching(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ResponseCachingMiddleware>();
    }
}
