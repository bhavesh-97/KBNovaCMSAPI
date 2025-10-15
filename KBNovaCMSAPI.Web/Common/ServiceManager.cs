using KBNovaCMS.IService;

namespace KBNovaCMSAPI.Services
{
    public class ServiceManager
    {
        public readonly IHttpContextAccessor _IHttpContextAccessor;

        public readonly IHttpClientFactory _IHttpClientFactory;
       
        public readonly IConfiguration _IConfiguration;

        public readonly INLog _INLog;

        public readonly ISessionManager _ISessionManager;
        
        public readonly IDbConnectionFactory _IDbConnectionFactory;


        // ------------------------------------ Constructor to inject all the required services ------------------

        public ServiceManager(
            IHttpContextAccessor _IHttpContextAccessor,          // HTTP Context for request and session data
            IHttpClientFactory _IHttpClientFactory,              // Factory for creating HTTP clients            
            IConfiguration _IConfiguration,                       // Service for configuration details
            IErrorManager _IErrorManager,                        // Service for error handling and logging
            INLog _INLog,                                        // Logging service using NLog
            ISessionManager _ISessionManager,                    // Service for managing session data
            IDbConnectionFactory _IDbConnectionFactory
          )
        {
            // Injecting all the dependencies into the ServiceManager
            this._IHttpContextAccessor = _IHttpContextAccessor;
            this._IHttpClientFactory = _IHttpClientFactory;
            this._IConfiguration = _IConfiguration;
            this._INLog = _INLog;
            this._ISessionManager = _ISessionManager;
            this._IDbConnectionFactory = _IDbConnectionFactory;
        }
    }
}
