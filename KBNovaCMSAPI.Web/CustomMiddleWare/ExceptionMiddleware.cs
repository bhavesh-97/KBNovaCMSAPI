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
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var fileName = context.Request.Path;
            var logMessage = ex.Message;

            // 🧠 Use your existing logging style
            Log("Error", logMessage, ex, fileName);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            // 🧩 Build standard response model
            var response = new JsonResponseModel
            {
                IsError = true,
                StrMessage = _env.IsDevelopment()
                    ? ex.Message
                    : "An unexpected error occurred. Please try again later.",
                Title = "Error",
                Type = "Error",
                Result = _env.IsDevelopment() ? new
                {
                    exception = ex.GetType().Name,
                    traceId = context.TraceIdentifier,
                    stackTrace = ex.StackTrace
                } : null
            };

            var json = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(json);
        }

        // 🔹 Custom log wrapper using NLog / ILogger
        private void Log(string logType, string message, Exception ex, string fileName)
        {
            _logger.LogError(ex, "{LogType}: {Message} in {FileName}", logType, message, fileName);
        }
    }
}
