using KBNovaCMS.IService;
using Microsoft.FeatureManagement;
using System.Data;
using System.Runtime.CompilerServices;

namespace KBNovaCMS.Service
{
    public class SErrorManager  //: IErrorManager
    {
        //private readonly IDapperManager _IDapperManager;
        //private readonly IsLong _INLog;
        //private readonly ISessionManager _ISessionManager;
        //private bool _disposed;

        //public SErrorManager(IDapperManager _IDapperManager, INLog _INLog, ISessionManager _ISessionManager)
        //{
        //    this._IDapperManager = _IDapperManager;
        //    this._INLog = _INLog;
        //    this._ISessionManager = _ISessionManager;
        //}

        //~SErrorManager()
        //{
        //    Dispose(false);
        //}

        //public void Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!_disposed)
        //    {
        //        if (disposing)
        //        {
        //            // Release managed resources.
        //            // Add any additional managed resource disposal here if needed
        //        }

        //        // Release unmanaged resources. If any
        //        _disposed = true;
        //    }
        //}

        //#region Insert Error
        //public void InsertError(MError _MError)
        //{
        //    try
        //    {
        //        var parameters = new Dictionary<string, object>()
        //        {
        //            { "p_ServiceName", _MError.ServiceName ?? string.Empty },
        //            { "p_MethodName", _MError.MethodName ?? string.Empty },
        //            { "p_AdditionalDetails", _MError.AdditionalDetails ?? string.Empty },
        //            { "p_ErrorMessage", _MError.ErrorMessage ?? string.Empty },
        //            { "p_UniqueGUID", Convert.ToString(_MError.UniqueGUID) ?? string.Empty },
        //            { "p_InnerException", string.Empty },
        //            { "p_StackTrace", string.Empty }
        //        };

        //        // Add Exception Object Values
        //        if (_MError.Exception != null)
        //        {
        //            parameters["p_InnerException"] = _MError.Exception.InnerException?.ToString() ?? string.Empty;
        //            parameters["p_StackTrace"] = _MError.Exception.StackTrace ?? string.Empty;

        //            // Override Error-Message if not already set
        //            if (string.IsNullOrEmpty(_MError.ErrorMessage))
        //            {
        //                parameters["p_ErrorMessage"] = _MError.Exception.Message;
        //            }
        //        }

        //        _IDapperManager.ExecuteWithoutResult(
        //            "InsertErrorDetials",
        //            CommandType.StoredProcedure,
        //            out string errorMessage,
        //            parameters
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        _INLog.Log(NLogType.Error, "Error In InsertError() Function", ex, NLogErrorFileName.ErrorLog);
        //        throw;
        //    }
        //}
        //public async Task InsertErrorAsync(MError _MError)
        //{
        //    try
        //    {
        //        var parameters = new Dictionary<string, object>()
        //        {
        //            { "p_ServiceName", _MError.ServiceName ?? string.Empty },
        //            { "p_MethodName", _MError.MethodName ?? string.Empty },
        //            { "p_AdditionalDetails", _MError.AdditionalDetails ?? string.Empty },
        //            { "p_ErrorMessage", _MError.ErrorMessage ?? string.Empty },
        //            { "p_UniqueGUID", Convert.ToString(_MError.UniqueGUID) ?? string.Empty },
        //            { "p_InnerException", string.Empty },
        //            { "p_StackTrace", string.Empty }
        //        };

        //        // Add Exception Object Values
        //        if (_MError.Exception != null)
        //        {
        //            parameters["p_InnerException"] = _MError.Exception.InnerException?.ToString() ?? string.Empty;
        //            parameters["p_StackTrace"] = _MError.Exception.StackTrace ?? string.Empty;

        //            // Override Error-Message if not already set
        //            if (string.IsNullOrEmpty(_MError.ErrorMessage))
        //            {
        //                parameters["p_ErrorMessage"] = _MError.Exception.Message;
        //            }
        //        }

        //        await _IDapperManager.ExecuteWithoutResultAsync(
        //            "InsertErrorDetials",
        //            CommandType.StoredProcedure,
        //            parameters
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        _INLog.Log(NLogType.Error, "Error In InsertErrorAsync() Function", ex, NLogErrorFileName.ErrorLog);
        //        throw;
        //    }
        //}
        //#endregion

        //#region Insert Error
        //public void InsertError(Exception Exception)
        //{
        //    try
        //    {
        //        var _MError = new MError();
        //        _MError.Exception = Exception;

        //        var parameters = CreateMErrorFromException(_MError);

        //        _IDapperManager.ExecuteWithoutResult(
        //            "InsertErrorDetials",
        //            CommandType.StoredProcedure,
        //            out string errorMessage,
        //            parameters
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        _INLog.Log(NLogType.Error, "Error In InsertError() Function", ex, NLogErrorFileName.ErrorLog);
        //        throw;
        //    }
        //}
        //public async Task InsertErrorAsync(Exception Exception)
        //{
        //    try
        //    {
        //        var _MError = new MError();
        //        _MError.Exception = Exception;

        //        var parameters = CreateMErrorFromException(_MError);

        //        _IDapperManager.ExecuteWithoutResult(
        //            "InsertErrorDetials",
        //            CommandType.StoredProcedure,
        //            out string errorMessage,
        //            parameters
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        await _INLog.LogAsync(NLogType.Error, "Error In InsertError() Function", ex, NLogErrorFileName.ErrorLog);
        //        throw;
        //    }
        //}


        //private Dictionary<string, object> CreateMErrorFromException(MError MError)
        //{
        //    // Collect contextual data (user, route info, etc.)
        //    var contextData = GetContextualData();

        //    // Setting default values directly using null-coalescing or ternary operator
        //    MError.ServiceName = contextData.GetValueOrDefault("ServiceName", "UnknownService");
        //    MError.MethodName = contextData.GetValueOrDefault("MethodName", "UnknownMethod");

        //    // Use a Dictionary initialization with direct values
        //    var parameters = new Dictionary<string, object>
        //    {
        //        { "p_ServiceName", MError.ServiceName },
        //        { "p_MethodName", MError.MethodName },
        //        { "p_AdditionalDetails", MError.AdditionalDetails ?? string.Empty },
        //        { "p_UniqueGUID", MError.UniqueGUID?.ToString() ?? string.Empty }
        //    };

        //    // Add exception details if available
        //    if (MError.Exception != null)
        //    {
        //        // Override ErrorMessage if it’s empty
        //        if (string.IsNullOrEmpty(MError.ErrorMessage))
        //        {
        //            parameters["p_ErrorMessage"] = MError.Exception.Message;
        //        }

        //        parameters["p_InnerException"] = MError.Exception.InnerException?.ToString() ?? string.Empty;
        //        parameters["p_StackTrace"] = MError.Exception.StackTrace ?? string.Empty;
        //    }

        //    return parameters;
        //}

        //// Structuring log message with contextual data
        //private Dictionary<string, string> GetContextualData()
        //{
        //    var contextData = new Dictionary<string, string>();

        //    var httpContext = _ISessionManager.GetCurrentContext();

        //    if (httpContext != null)
        //    {
        //        // Collecting common contextual information
        //        contextData["RequestPath"] = httpContext.Request.Path;
        //        contextData["Method"] = httpContext.Request.Method;
        //        contextData["UserAgent"] = httpContext.Request.Headers["User-Agent"];
        //        contextData["RequestId"] = httpContext.TraceIdentifier;

        //        // Collect route information (controller and action names)
        //        var routeData = httpContext.GetRouteData()?.Values;
        //        if (routeData != null)
        //        {
        //            contextData["ServiceName"] = routeData.ContainsKey("controller") ? routeData["controller"].ToString() : "UnknownController";
        //            contextData["MethodName"] = routeData.ContainsKey("action") ? routeData["action"].ToString() : "UnknownAction";
        //        }
        //    }

        //    return contextData;
        //}
        //#endregion
    }
}