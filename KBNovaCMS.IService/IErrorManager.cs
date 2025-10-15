using KBNovaCMS.Model;
namespace KBNovaCMS.IService
{
    public interface IErrorManager : IDisposable
    {
        #region Insert Error
        void InsertError(MError _MError);
        Task InsertErrorAsync(MError _MError);

        void InsertError(Exception Exception);
        Task InsertErrorAsync(Exception Exception);
        #endregion
    }
}
