using PharmaMoov.Models;
using PharmaMoov.Models.User;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Account")]
    public class AccountController : APIBaseController
    {
        IAccountRepository AccountRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public AccountController(IAccountRepository _aRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            AccountRepo = _aRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [HttpGet("GetUserProfile")]
        public IActionResult GetUserProfile([FromHeader] string Authorization)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = AccountRepo.GetUserProfile(Authorization.Split(' ')[1]);
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

        [HttpPost("EditUserProfile")]
        public IActionResult EditUserProfile([FromBody] UserProfile _user)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = AccountRepo.EditUserProfile(_user);
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

        [HttpPost("ChangePassword")]
        public IActionResult ChangeUserPassword([FromBody] UserChangePassword _user)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = AccountRepo.ChangeUserPassword(_user);
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

        [HttpGet("GetDeliveryAddressBook/{_address}")]
        public IActionResult GetDeliveryAddressBook([FromHeader] string Authorization, int _address)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = AccountRepo.GetDeliveryAddressBook(Authorization.Split(' ')[1], _address);
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

        [HttpPost("AddDeliveryAddress")]
        public IActionResult AddUserDeliveryAddress([FromHeader] string Authorization, [FromBody] UserAddress _address)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = AccountRepo.AddUserDeliveryAddress(Authorization.Split(' ')[1], _address);
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

        [HttpPost("EditDeliveryAddress")]
        public IActionResult EditUserDeliveryAddress([FromBody] UserAddress _address)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = AccountRepo.EditUserDeliveryAddress(_address);
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

        [HttpPost("DeleteDeliveryAddress")]
        public IActionResult DeleteUserDeliveryAddress([FromBody] UserAddressToDel _address)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = AccountRepo.DeleteUserDeliveryAddress(_address);
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
    }
}