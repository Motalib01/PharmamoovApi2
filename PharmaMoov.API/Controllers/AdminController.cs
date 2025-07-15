using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.User;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using PharmaMoov.API.Helpers;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Admin")]
    public class AdminController : APIBaseController
    {
        IAdminRepository AdminRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public AdminController(IAdminRepository _aRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            AdminRepo = _aRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [HttpPost("RegisterAdmin")]
        public IActionResult RegisterAdmin([FromBody] AdminProfile _admin)
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
                return Ok(AdminRepo.RegisterAdmin(_admin));
            }
        }

        [AllowAnonymous]
        [HttpPost("AdminLogin")]
        public IActionResult AdminLogin([FromBody] AdminLogin _admin)
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
                return Ok(AdminRepo.AdminLogin(_admin));
            }
        }

        [AllowAnonymous]
        [HttpPost("ReGenerateTokens")]
        public IActionResult ReGenerateTokens([FromBody] UserLoginTransaction _admin)
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
                return Ok(AdminRepo.ReGenerateTokens(_admin));
            }
        }

        [HttpGet("AllAdmins/{_shop}/{_admin}")]
        public IActionResult GetAllAdmins(Guid _shop, int _admin)
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
                return Ok(AdminRepo.GetAllAdmins(_shop, _admin));
            }
        }

        [HttpPost("EditAdminProfile")]
        public IActionResult EditAdminProfile([FromBody] EditAdminProfile _admin)
        {
            ModelState.Remove("ConfirmPassword");
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
                return Ok(AdminRepo.EditAdminProfile(_admin));
            }
        }

        [HttpPost("ChangeAdminStatus")]
        public IActionResult ChangeAdminStatus([FromBody] ChangeRecordStatus _admin)
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
                return Ok(AdminRepo.ChangeAdminStatus(_admin));
            }
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public IActionResult ForgotPassword([FromBody] AdminForgotPassword _admin)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = AdminRepo.ForgotPassword(_admin, MainHttpClient, MConf);
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

        [HttpGet("GetAdminList")]
        public IActionResult GetAdminList()
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
                return Ok(AdminRepo.GetAdminList());
            }
        }
    }
}