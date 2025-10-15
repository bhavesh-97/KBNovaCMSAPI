using KBNovaCMS.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NLog;
using System.Text;

namespace KBNovaCMS.Common
{
    public static class NLogger
    {
        #region Fields
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Initialization
        public static void Initialize(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor), "IHttpContextAccessor is null");
            }
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        #region Private Methods
        private static string GetRouteInfo(RouteValueDictionary routeValues)
        {
            if (routeValues == null || routeValues.Count == 0)
                return "No route values available.";

            var sb = new StringBuilder();
            foreach (var item in routeValues)
            {
                sb.AppendLine($"||{item.Key} = {item.Value}||");
            }

            return sb.ToString();
        }

        private static void FormatExceptionDetails(ref StringBuilder logMessage, Exception exception)
        {
            if (exception == null) return;

            logMessage.AppendLine($"Error Message: {exception.Message}{Environment.NewLine}")
                      .AppendLine($"Inner Exception: {exception.InnerException?.Message}{Environment.NewLine}")
                      .AppendLine($"Stack Trace: {exception.StackTrace}{Environment.NewLine}");
        }

        // Structuring log message with contextual data
        private static Dictionary<string, string> GetContextualData()
        {
            var contextData = new Dictionary<string, string>();

            if (_httpContextAccessor?.HttpContext != null)
            {
                var httpContext = _httpContextAccessor.HttpContext;
                contextData["RequestPath"] = httpContext.Request.Path;
                contextData["Method"] = httpContext.Request.Method;
                contextData["UserAgent"] = httpContext.Request.Headers["User-Agent"];
                contextData["RequestId"] = httpContext.TraceIdentifier;

                // Additional contextual information can be added here
            }

            return contextData;
        }

        #endregion

        #region Methods
        private static async Task InsertLogAsync(NLogType logType, string logMessage, Dictionary<string, string> contextData)
        {
            if (_logger == null)
                throw new InvalidOperationException("Logger not initialized.");

            // Add contextual data to the log (using structured logging)
            var logEvent = new LogEventInfo
            {
                Level = GetLogLevel(logType),
                Message = logMessage
            };

            foreach (var data in contextData)
            {
                logEvent.Properties[data.Key] = data.Value;
            }

            // Log asynchronously
            await Task.Run(() => _logger.Log(logEvent));
        }

        private static LogLevel GetLogLevel(NLogType logType)
        {
            return logType switch
            {
                NLogType.Trace => LogLevel.Trace,
                NLogType.Debug => LogLevel.Debug,
                NLogType.Info => LogLevel.Info,
                NLogType.Warn => LogLevel.Warn,
                NLogType.Error => LogLevel.Error,
                NLogType.Fatal => LogLevel.Fatal,
                _ => LogLevel.Info, // Default level
            };
        }

        // Main log method for structured and contextual logging
        public static async Task LogAsync(NLogType logType, string logMessage, Exception? exception = null, NLogErrorFileName errorFileName = NLogErrorFileName.InfoLog)
        {
            try
            {
                // Set logger based on the error file name
                _logger = LogManager.GetLogger(errorFileName.ToString());

                // Collect contextual data (e.g., user, route info)
                var contextData = GetContextualData();

                // Add route info if available (Ensure route data is accessible after routing)
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext != null)
                {
                    var routeData = httpContext.GetRouteData()?.Values;
                    if (routeData != null)
                    {
                        var routeInfo = GetRouteInfo(routeData);
                        logMessage += $"{Environment.NewLine}Route Info: {routeInfo}";
                    }
                }

                // Format exception details if present
                if (exception != null)
                {
                    var sb = new StringBuilder(logMessage);
                    FormatExceptionDetails(ref sb, exception);
                    logMessage = sb.ToString();
                }

                // Insert the log asynchronously with contextual data
                await InsertLogAsync(logType, logMessage, contextData);
            }
            catch (Exception ex)
            {
                // Log the exception that occurred during logging asynchronously
                var errorMessage = $"An error occurred while logging: {ex.Message}";
                await InsertLogAsync(NLogType.Error, errorMessage, new Dictionary<string, string>());
                throw; // Optionally rethrow the exception or handle it
            }
        }
        #endregion
    }
}
