using Microsoft.AspNetCore.Http;

namespace KBNovaCMS.IService
{
    public interface ISessionManager
    {
        HttpContext? GetCurrentContext();
        //Here The Session Get Method Have A #Nullable#
        T? Get<T>(string key);
        void Set<T>(string key, T value);
        void Remove(string key);
        void Clear();
        string GetCurrentRequestFullURL();
    }
}