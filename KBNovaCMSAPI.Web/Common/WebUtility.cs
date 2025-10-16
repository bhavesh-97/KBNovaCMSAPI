using KBNovaCMS.Service;
using KBNovaCMS.Common.Enums;
using KBNovaCMS.IService;
using KBNovaCMSAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KBNovaCMS.Common
{
    public class WebUtility
    {
        // Instance of ServiceManager to resolve services dynamically
        public static ServiceManager _ServiceManager;

        // Constructor injection
        // A static method to initialize ServiceManager
        public static void InitializeServiceManager(IServiceProvider serviceProvider)
        {
            // This will use the DI container to resolve the dependencies
          //  _ServiceManager = serviceProvider.GetRequiredService<ServiceManager>();
        }
        public static void ConfigureServices(IServiceCollection services)
        {
            // -------------------- HTTP Context and Security Configuration --------------------
            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ISessionManager, SSessionManager>();
            services.AddSingleton<INLog, SNLog>();
            services.AddSingleton<IDbConnectionFactory, SDbConnectionFactory>();
            services.AddSingleton<IUserLogin, SUserLogin>();

            // AntiForgery configuration
            // services.AddAntiforgery(options =>
            // {
            //     options.FormFieldName = "AntiforgeryFieldname";
            //     options.HeaderName = "X-CSRF-TOKEN-HEADERNAME";
            //     options.Cookie.SameSite = SameSiteMode.Lax;
            //     options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            //     options.Cookie.HttpOnly = false;
            //     options.SuppressXFrameOptionsHeader = false;
            // });

            services.AddAntiforgery(options =>
            {
                // Using strict SameSite policy for maximum security
                options.FormFieldName = "AntiforgeryFieldname";
                options.HeaderName = "X-CSRF-TOKEN-HEADERNAME";
                options.Cookie.SameSite = SameSiteMode.Strict;  // More secure than Lax
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;  // Only send cookies over HTTPS
                options.Cookie.HttpOnly = true;  // Set HttpOnly for added security (prevents JavaScript from accessing cookies)

                // Optionally, disable X-Frame-Options suppression for clickjacking prevention
                options.SuppressXFrameOptionsHeader = false;
            });

        }

        /// <summary>
        /// Displays a popup message in the client-side with the provided parameters.
        /// </summary>
        /// <param name="controller">The current controller that is invoking the method.</param>
        /// <param name="errorMessage">The message content to be displayed in the popup.</param>
        /// <param name="msgType">The type of the message (e.g., success, error, warning, etc.).</param>
        /// <param name="msgTitle">The title of the popup message.</param>
        /// <param name="timeOutInMinSec">The duration for which the popup should be visible, in minutes and seconds. Default is 0.</param>
        /// <param name="msgShowButton">Determines whether the popup should have a button (true/false). Default is true.</param>
            public static void MessagePopup(Controller controller, string errorMessage, PopupMessageType msgType, PopupMessageType msgTitle, int timeOutInMinSec = 0, bool msgShowButton = true, string PopupMessageType = "CitizenUser")
        {
            // If the error message is empty or null, do not proceed.
            if (string.IsNullOrWhiteSpace(errorMessage)) return;

            // Prepare the JavaScript message string for the client-side popup.
            string message = $"ShowMessage('{errorMessage.Replace("\'", @"\'")}', '{msgTitle}', '{msgType}', {timeOutInMinSec}, {msgShowButton.ToString().ToLower()});";

            try
            {
                // Store the message as a JavaScript block in the session to be executed on the client-side.
                _ServiceManager._ISessionManager?.Set($"PopupMessage_{PopupMessageType}", $"$(document).ready(function () {{ {message} }});");
            }
            catch
            {
                // If an error occurs while setting the session, store a default value to avoid crashing.
                _ServiceManager._ISessionManager?.Set($"PopupMessage_{PopupMessageType}", "var a=1;");
            }
        }


        /// <summary>
        /// Retrieves all the validation error messages from the ModelState and formats them into a single string.
        /// </summary>
        /// <param name="modelState">The ModelState dictionary containing the model validation information.</param>
        /// <returns>A formatted string containing the validation errors.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided modelState is null.</exception>
        public static string GetMessageOfModelState(ModelStateDictionary modelState)
        {
            // Ensure the modelState is not null to avoid runtime errors.
            if (modelState == null)
            {
                throw new ArgumentNullException(nameof(modelState), "ModelState cannot be null.");
            }

            // Select all invalid model state entries and generate a human-readable message for each field.
            var errors = modelState
                .Where(x => x.Value?.ValidationState == ModelValidationState.Invalid)
                .Select(x => $"{x.Key} field has an invalid value.");

            // Join all error messages into a single string with line breaks for better readability.
            return string.Join("</br> ", errors);
        }

        /// <summary>
        /// Retrieves the client's IP address from the HTTP context, considering possible proxy headers.
        /// </summary>
        /// <param name="httpContext">The current HTTP context from which the IP address should be extracted.</param>
        /// <returns>The IP address of the client, or "Unknown IP" if it could not be determined.</returns>
        public static string GetClientIpAddress(HttpContext? httpContext)
        {
            // Default value in case the IP address cannot be determined.
            string clientIpAddress = "Unknown IP";

            // Check if the HTTP context is not null.
            if (httpContext != null)
            {
                // Look for the "X-Forwarded-For" header which may contain the client's IP address if behind a proxy.
                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

                // If the "X-Forwarded-For" header contains an IP, return the first IP in the list.
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    // The header might contain multiple IPs, so we take the first one.
                    clientIpAddress = forwardedFor.Split(',')[0].Trim();
                }
                else
                {
                    // If no forwarded header, fall back to the direct remote IP address from the connection.
                    clientIpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                }
            }

            // Return the determined IP address or the default "Unknown IP" if it could not be determined.
            return clientIpAddress;
        }

        // This method ensures that the IP address is valid and formatted correctly
        public static string ValidateIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return "Unknown IP";

            // You can use a regex or built-in IP parsing to validate it further.
            if (System.Net.IPAddress.TryParse(ipAddress, out var ip))
            {
                return ip.ToString(); // This ensures it is stored in the correct format
            }
            return "Invalid IP"; // If the address is invalid, return a placeholder
        }
    }
}