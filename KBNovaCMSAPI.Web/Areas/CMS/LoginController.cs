using KBNovaCMS.Common;
using KBNovaCMS.IService;
using KBNovaCMS.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet, Route("Login")]
        public async Task<JsonResult> Login(string jsonString)
        {
            JsonResponseModel _JsonResponseModel = new JsonResponseModel();
            if (!JsonUtility.ValidateIncomingJsonString<MUserLogin>(jsonString, ref _JsonResponseModel, out var MUserLogin))
            {
                return new JsonResult(_JsonResponseModel);
            }

            if (MUserLogin == null || !ModelState.IsValid)
            {
                return new JsonResult(Utility.CreateErrorResponse("Invalid or missing required parameters."));
            }


            // Fetch the user application data asynchronously
            jsonString = await _userLogin.GetByMobileAndEmailID<string>(MUserLogin);

            // Return success response with the fetched data
            return new JsonResult(Utility.CreateSuccessResponse("Data retrieved successfully.", jsonString));
        }

    }
}
