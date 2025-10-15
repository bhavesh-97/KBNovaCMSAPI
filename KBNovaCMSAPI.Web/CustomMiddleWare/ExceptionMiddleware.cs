using KBNovaCMS.Common;
using System.Net;
using System.Text.Json;

namespace KBNovaCMSAPI.Web.CustomMiddleWare
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string userMessage = "An unexpected error occurred. Please try again later.";
            string title = "Error";

            switch (ex)
            {
                case KeyNotFoundException keyNotFound:
                    statusCode = (int)HttpStatusCode.NotFound;
                    userMessage = keyNotFound.Message;
                    title = "Not Found";
                    break;

                case InvalidOperationException invalidOpEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    userMessage = invalidOpEx.Message;
                    title = "Invalid Operation";
                    break;

                case UnauthorizedAccessException _:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    userMessage = "You are not authorized to perform this action.";
                    title = "Unauthorized";
                    break;

                case ArgumentException argEx:
                //case ArgumentNullException argNullEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    userMessage = ex.Message;
                    title = "Invalid Argument";
                    break;

                case TimeoutException _:
                    statusCode = (int)HttpStatusCode.RequestTimeout;
                    userMessage = "The request timed out. Please try again.";
                    title = "Timeout";
                    break;

                default:
                    if (ex.InnerException != null)
                        Log("InnerException", ex.InnerException.Message, ex.InnerException, context.Request.Path);
                    break;
            }

            // Log the main exception
            Log("Error", ex.Message, ex, context.Request.Path);

            var response = new JsonResponseModel
            {
                IsError = true,
                StrMessage = _env.IsDevelopment() ? ex.Message : userMessage,
                Title = title,
                Type = "Error",
                Result = _env.IsDevelopment() ? new
                {
                    exception = ex.GetType().Name,
                    innerException = ex.InnerException?.Message,
                    traceId = context.TraceIdentifier,
                    stackTrace = ex.StackTrace
                } : null
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private void Log(string logType, string message, Exception ex, string fileName)
        {
            _logger.LogError(ex, "{LogType}: {Message} in {FileName}", logType, message, fileName);
        }
    }
}
