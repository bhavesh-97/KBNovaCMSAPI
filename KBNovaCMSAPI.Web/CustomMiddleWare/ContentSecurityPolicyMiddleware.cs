public class ContentSecurityPolicyMiddleware
{
    private readonly RequestDelegate _next;

    public ContentSecurityPolicyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self';");

        await _next(context);
    }
}

// Extension Method for registering the middleware
public static class ContentSecurityPolicyMiddlewareExtensions
{
    public static IApplicationBuilder UseContentSecurityPolicy(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ContentSecurityPolicyMiddleware>();
    }
}
