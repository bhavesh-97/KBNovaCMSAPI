using KBNovaCMS.IService;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace KBNovaCMS.Service
{
    public class SSessionManager : ISessionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        public string CurrentURL { get; private set; } = string.Empty;

        public SSessionManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public HttpContext? GetCurrentContext()
        {
            return _httpContextAccessor.HttpContext;
        }

        public T? Get<T>(string sessionKey)
        {
            if (_httpContextAccessor.HttpContext == null)
                return default;

            try
            {
                if (_httpContextAccessor.HttpContext.Session.TryGetValue(sessionKey, out var myArray))
                {
                    return myArray != null ? JsonSerializer.Deserialize<T>(myArray) : default;
                }

                return default;
            }
            catch (Exception)
            {
                // Log exception here if necessary
                // throw new InvalidOperationException("Failed to get session value", ex);
                throw;
            }
        }

        public void Set<T>(string sessionKey, T value)
        {
            if (_httpContextAccessor.HttpContext == null)
                return;

            try
            {
                var serializedValue = JsonSerializer.SerializeToUtf8Bytes(value);
                _httpContextAccessor.HttpContext.Session.Set(sessionKey, serializedValue);
            }
            catch (Exception)
            {
                // Log exception here if necessary
                // throw new InvalidOperationException("Failed to set session value", ex);
                throw;
            }
        }

        public void Remove(string sessionKey)
        {
            if (_httpContextAccessor.HttpContext == null)
                return;

            try
            {
                _httpContextAccessor.HttpContext.Session.Remove(sessionKey);
            }
            catch (Exception)
            {
                // Log exception here if necessary
                // throw new InvalidOperationException("Failed to remove session value", ex);
                throw;
            }
        }

        public void Clear()
        {
            if (_httpContextAccessor.HttpContext == null)
                return;

            try
            {
                _httpContextAccessor.HttpContext.Session.Clear();
            }
            catch (Exception)
            {
                // Log exception here if necessary
                // throw new InvalidOperationException("Failed to clear session", ex);
                throw;
            }
        }

        public string GetCurrentRequestFullURL()
        {
            if (_httpContextAccessor.HttpContext == null)
                return string.Empty;

            try
            {
                var context = _httpContextAccessor.HttpContext;
                var scheme = context.Request.Scheme;
                var host = context.Request.Host.Value;
                var path = context.Request.Path;
                var queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
                //var routeValues = string.Join("/", context.Request.RouteValues.Values);

                //CurrentURL = $"{scheme}://{host}{path}/{routeValues}{queryString}";
                CurrentURL = $"{scheme}://{host}{path}/{queryString}";

                return CurrentURL;
            }
            catch (Exception)
            {
                // Log exception here if necessary
                // throw new InvalidOperationException("Failed to get current request full URL", ex);
                throw;
            }
        }
    }
}
