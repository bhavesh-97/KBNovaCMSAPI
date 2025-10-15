using Newtonsoft.Json;
using System.Net;

public class CustomExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomExceptionMiddleware> _logger;

    public CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Main entry point for handling requests and exceptions
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Proceed with the next middleware in the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log exception details
            _logger.LogError(ex, "An error occurred while processing the request.");

            // If an exception occurs, handle it with custom logic
            await HandleExceptionAsync(context, ex);
        }
    }

    // Handles exceptions, generates custom error response, and logs details
    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Default HTTP status code for server errors
        var code = HttpStatusCode.InternalServerError; // 500 if unexpected

        // Customize error response based on the exception type
        if (exception is ArgumentNullException)
        {
            code = HttpStatusCode.BadRequest; // 400 for bad input
        }
        else if (exception is UnauthorizedAccessException)
        {
            code = HttpStatusCode.Unauthorized; // 401 for unauthorized access
        }
        // You can add more custom exception types here (e.g., NotFoundException, etc.)

        // Create a custom error response to return to the client
        var errorResponse = new
        {
            errorMessage = "An unexpected error occurred. Please try again later.", // Generic error message for security reasons
            errorCode = code.ToString(), // Custom error code to map with status code
            errorTime = DateTime.UtcNow // Timestamp of when the error occurred
        };

        // Serialize the error response to JSON
        var result = JsonConvert.SerializeObject(errorResponse);

        // Set response headers for JSON output
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code; // Set the HTTP status code

        // Write the serialized error response to the response body
        return context.Response.WriteAsync(result);
    }
}
