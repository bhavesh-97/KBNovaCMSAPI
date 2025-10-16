using KBNovaCMS.Model;
using KBNovaCMS.Common;
using KBNovaCMS.IService;
using System.Data;
using System.Collections.Generic;

namespace KBNovaCMS.Service
{
    public class SUserLogin : IUserLogin
    {     
        #region Fields
         private readonly IDbConnectionFactory _IDbConnectionFactory;
         private bool _disposed = false;
        #endregion

        #region Constructor
        public SUserLogin(IDbConnectionFactory _IDbConnectionFactory)
        {
            this._IDbConnectionFactory = _IDbConnectionFactory;
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _IDbConnectionFactory.Dispose();
                }
                _disposed = true;
            }
        }

        ~SUserLogin()
        {
            Dispose(false);
        }
        #endregion

        public async Task<T> GetByMobileAndEmailID<T>(MUserLogin Model)
        {
            var dictionary = new Dictionary<string, object>
                {
                    { "p_UserID", Model.UserID },
                    { "p_MobileNumber", Model.MobileNumber },
                    { "p_EmailID", Model.EmailID},
                    { "p_Password", Model.Password}
                };

            return await _IDbConnectionFactory.GetJSONDataAsync<T>(
                   "GetUserApplicationLoanValidationData_JSON",  
                   CommandType.StoredProcedure,                  
                   dictionary                                    
               );
        }

       public async Task<T> GetByUserID<T>(int UserID)
        {

            var dictionary = new Dictionary<string, object>
                {
                    { "p_UserID", UserID }
                };

            return await _IDbConnectionFactory.GetJSONDataAsync<T>(
                   "GetUserApplicationLoanValidationData_JSON",  
                   CommandType.StoredProcedure,                  
                   dictionary                                           
                    );
        }
    }
}
