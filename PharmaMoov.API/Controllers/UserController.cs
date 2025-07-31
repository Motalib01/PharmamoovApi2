using PharmaMoov.Models;
using PharmaMoov.Models.User;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using PharmaMoov.Models.Admin;

namespace PharmaMoov.API.Controllers
{ 
    [Produces("application/json")]
    [Route("api/User")]
    public class UserController : APIBaseController
    {
        IUserRepository UserRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public UserController(IUserRepository _aRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            UserRepo = _aRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [AllowAnonymous]
        [HttpPost("FullUserRegistration")]
        public IActionResult FullUserRegistration([FromBody] FullUserRegForm _user)
        {
            APIResponse returnResp = UserRepo.FullUserRegistration(_user);
            if (returnResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(returnResp);
            }
            else
            {
                return BadRequest(returnResp);
            }
        }

        [HttpPost("RegisterViaEmail")]
        public IActionResult RegisterUserViaEmail([FromBody] UserRegistrationViaEmail _user)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserRepo.RegisterUserViaEmail(_user);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }

        [HttpPost("VerifyMobileNumber")]
        public IActionResult VerifyUserMobile([FromBody] VerifyMobileOrEmail _user)
        {
            APIResponse returnResp = UserRepo.VerifyUserMobileOrEmail(_user);
            if (returnResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(returnResp);
            }
            else
            {
                return BadRequest(returnResp);
            }
        }

        [HttpPost("RegisterViaMobileNumber")]
        public IActionResult RegisterViaMobileNumber([FromBody] UserRegistrationViaMobileNumber _user)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserRepo.RegisterUserViaMobileNumber(_user, MainHttpClient, MConf);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }

        [HttpPost("VerifyCode")]
        public IActionResult VerifyUserCode([FromBody] UserVerifyCode _userCode)
        {
            if (ModelState.IsValid)
            {
                APIResponse returnResp = UserRepo.VerifyUserCode(_userCode);
                if (returnResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(returnResp);
                }
                else
                {
                    return BadRequest(returnResp);
                }

            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }

        [HttpPost("ResendVerificationCode")]
        public IActionResult SendUserVerificationCode([FromBody] UserVerifyCode _user)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserRepo.SendUserVerificationCode(_user, MainHttpClient, MConf);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }

        [HttpPost("MobileLogin")]
        public IActionResult MobileLogin([FromBody] LoginCredentials _user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new APIResponse
                {
                    Message = "Mot de passe invalide. Merci de réessayer",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
            else
            {
                return Ok(UserRepo.MobileLogin(_user, MainHttpClient, MConf));
            }
        }

        [AllowAnonymous]
        [HttpPost("MobileLogout")]
        public IActionResult Logout([FromBody] LogoutCredentials _user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new APIResponse
                {
                    Message = "Mauvaise demande",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
            else
            {
                return Ok(UserRepo.MobileLogout(_user));
            }
        }

        [AllowAnonymous]
        [HttpPost("ReGenerateTokens")]
        public IActionResult ReGenerateTokens([FromBody] UserLoginTransaction _loginParam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new APIResponse
                {
                    Message = "Mauvaise demande",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
            else
            {
                return Ok(UserRepo.ReGenerateTokens(_loginParam));
            }
        }

        [Authorize]
        [HttpGet("SetFCMToken")]
        public IActionResult SetFCMToken([FromHeader] string Authorization, string FCMToken, DevicePlatforms DeviceType)
        {
            APIResponse returnResp = UserRepo.SetFCMToken(Authorization, FCMToken, DeviceType);
            if (returnResp.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return BadRequest(returnResp);
            }

            return Ok(returnResp);
        }

        [HttpPost("LoginEmailOrUsername")]
        public IActionResult LoginEmailOrUsername([FromBody] LoginEmailUsername _user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new APIResponse
                {
                    Message = "Mot de passe invalide. Merci de réessayer",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
            else
            {
                return Ok(UserRepo.LoginEmailOrUsername(_user));
            }


        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword([FromBody] ResetPasswordModel resetPasswordModel)
        {            

            APIResponse aResp = new APIResponse();

            if (ModelState.IsValid)
            {
                aResp = UserRepo.ResetPassword(resetPasswordModel);
                if (aResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(aResp);
                }
                return BadRequest(aResp);
            }

            aResp = new APIResponse
            {
                Message = "Model Objet Invalid",
                Status = "Model Erreur!",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                ModelError = ModelState.Errors()
            };
            return BadRequest(aResp);
        }

        #region Section: For Customer, Courier and Health Prof under Super Admin
        [HttpPost("ChangeUserStatus")]
        public IActionResult ChangeUserStatus([FromHeader] string Authorization, [FromBody] ChangeRecordStatus _userStat)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
            else
            {
                return Ok(UserRepo.ChangeUserStatus(Authorization.Split(' ')[1], _userStat));
            }
        }

        [HttpPost("EditUserProfile")]
        public IActionResult EditUserProfile([FromHeader] string Authorization, [FromBody] User _user)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserRepo.EditUserProfile(Authorization.Split(' ')[1], _user);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }

            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "Model Objet Invalid",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Status = "Object level error.",
                    ModelError = ModelState.Errors()
                });
            }
        }
        #endregion

        [HttpPost("ChangeAcceptOrDeclineRequest")]
        public IActionResult ChangeAcceptOrDeclineRequest([FromHeader] string Authorization, [FromBody] ChangeAcceptOrDeclineRequestStatus model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new APIResponse
                {
                    Message = "Invalid Model Object",
                    ModelError = ModelState.Errors(),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
            else
            {
                return Ok(UserRepo.ChangeAcceptOrDeclineRequest(Authorization.Split(' ')[1], model, MainHttpClient, MConf));
            }
        }

    }
}