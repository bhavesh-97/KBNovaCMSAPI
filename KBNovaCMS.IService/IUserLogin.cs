using KBNovaCMS.Common;
using KBNovaCMS.Model;

namespace KBNovaCMS.IService
{
    public interface IUserLogin : IDisposable
    {
         Task<T> GetByMobileAndEmailID<T>(MUserLogin Model);
         Task<T> GetByUserID<T>(int UserID);
    }
}
