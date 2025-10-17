using KBNovaCMS.Common;
using KBNovaCMS.Common.Security.EncryptDecrypt;
using KBNovaCMS.IService;
using KBNovaCMS.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace KBNovaCMSAPI.Web.Areas.CMS
{

    [Area("CMS")]
    [Route("/CMS/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IUserLogin _userLogin;
        public LoginController(IUserLogin userLogin )
        {
            this._userLogin = userLogin;
        }

        [HttpPost("Login")]
        public async Task<JsonResult> Login([FromBody] MUserLogin userLogin)
        {
            JsonResponseModel response = new JsonResponseModel();

            if (userLogin == null || !ModelState.IsValid)
            {
                response = Utility.CreateErrorResponse("Invalid or missing required parameters.");
                return new JsonResult(response);

            }

            //var userData = await _userLogin.GetByMobileAndEmailID<string>(userLogin);
            response = Utility.CreateSuccessResponse("Data retrieved successfully.");

            return new JsonResult(response); // middleware will encrypt automatically
        }

        //public async Task<JsonResult> Login([FromBody] MUserLogin userLogin)
        //{
        //    JsonResponseModel _JsonResponseModel = new JsonResponseModel();

        //    if (!JsonUtility.ValidateIncomingJsonString<MUserLogin>(jsonString, ref _JsonResponseModel, out var MUserLogin))
        //    {
        //        var encryptedError = EncryptDecrypt.FrontEncryptEncode(JsonConvert.SerializeObject(_JsonResponseModel));
        //        return new JsonResult(encryptedError);
        //    }

        //    if (MUserLogin == null || !ModelState.IsValid)
        //    {
        //        var errorResponse = Utility.CreateErrorResponse("Invalid or missing required parameters.");
        //        var encryptedError = EncryptDecrypt.FrontEncryptEncode(JsonConvert.SerializeObject(errorResponse));
        //        return new JsonResult(encryptedError);
        //    }

        //    var userData = await _userLogin.GetByMobileAndEmailID<string>(MUserLogin);
        //    var successResponse = Utility.CreateSuccessResponse("Data retrieved successfully.", userData);
        //    var encryptedPayload = EncryptDecrypt.FrontEncryptEncode(JsonConvert.SerializeObject(successResponse));

        //    return new JsonResult(encryptedPayload);
        //}



    }
}
