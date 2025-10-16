using KBNovaCMS.Common.Security;
using KBNovaCMS.Common.Security.EncryptDecrypt;
using System.Text;

namespace KBNovaCMSAPI.Web.CustomMiddleWare
{
    public class EncryptionMiddleware
    {
        private readonly RequestDelegate _next;

        public EncryptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var encryptHeader = context.Request.Headers["X-Encrypt-Payload"];
            bool useEncryption = !string.IsNullOrEmpty(encryptHeader) && encryptHeader == "true";

            // ------------------- Decrypt request -------------------
            if (useEncryption && context.Request.ContentLength > 0 && context.Request.Body.CanRead)
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                var encryptedBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                try
                {
                    var decryptedBody = EncryptDecrypt.FrontDecryptDecode(encryptedBody);
                    var bytes = Encoding.UTF8.GetBytes(decryptedBody);
                    context.Request.Body = new MemoryStream(bytes);
                    context.Request.ContentType = "application/json";  // <-- Add this
                    context.Request.ContentLength = bytes.Length;
                    context.Request.Body.Position = 0;
                }
                catch
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid encrypted payload.");
                    return;
                }
            }

            // ------------------- Capture response -------------------
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context); // Call controller

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseContent = await new StreamReader(context.Response.Body).ReadToEndAsync();

            // ------------------- Encrypt response if needed -------------------
            context.Response.Body = originalBodyStream;
            if (useEncryption && !string.IsNullOrEmpty(responseContent))
            {
                var encryptedResponse = EncryptDecrypt.FrontEncryptEncode(responseContent);
                context.Response.ContentType = "application/json";

                // Wrap encrypted string in quotes to make it valid JSON string
                await context.Response.WriteAsync($"\"{encryptedResponse}\"");
            }
            else
            {
                // Just return original response
                await context.Response.WriteAsync(responseContent);
            }
        }
    }
}
