using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using PharmaMoov.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/UserHelpRequest")]
    public class UserHelpRequestController : APIBaseController
    {
        IUserHelpRequestRepository UserHelpReqRepo { get; } 
        private APIConfigurationManager MConf { get; }

        public UserHelpRequestController(IUserHelpRequestRepository _uhrReqRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            UserHelpReqRepo = _uhrReqRepo; 
            MConf = _conf;
        }

        [HttpGet("GetUserRequestList/{id}")]
        public IActionResult GetUserRequestList(int id)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserHelpReqRepo.GetUserRequestList(id);
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

        [HttpPost("AddUserRequest")]
        public IActionResult AddUserRequest([FromHeader] string Authorization, [FromBody] NewUserHelpRequest _uhReq)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserHelpReqRepo.AddUserRequest(Authorization.Split(' ')[1], _uhReq);
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

        [HttpGet("GetOrderNumbers")]
        public IActionResult GetOrderNumber([FromHeader] string Authorization)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserHelpReqRepo.GetOrderNumber(Authorization.Split(' ')[1]);
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

        [HttpGet("GetUserGeneralConcernList/{id}")]
        public IActionResult GetUserConcernList([FromHeader] string Authorization, int id)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserHelpReqRepo.GetUserConcernList(Authorization.Split(' ')[1], id);
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

        [HttpPost("AddUserGeneralConcern")]
        public IActionResult AddUserConcern([FromHeader] string Authorization, [FromBody] NewUserGeneralConcern _ugConcern)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserHelpReqRepo.AddUserConcern(Authorization.Split(' ')[1], _ugConcern);
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

        [HttpPost("ChangeHelpRequestStatus")]
        public IActionResult ChangeHelpRequestStatus([FromHeader] string Authorization, [FromBody] ChangeHelpRequestStatus _request)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = UserHelpReqRepo.ChangeHelpRequestStatus(Authorization.Split(' ')[1], _request);
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

        [AllowAnonymous]
        [HttpPost("SendInquiry")]
        public IActionResult SendInquiry([FromBody] GeneralInquiry _inquiry)
        {
            APIResponse apiResp = UserHelpReqRepo.SendInquiry(_inquiry);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [AllowAnonymous]
        [HttpPost("SendCareer")]
        public IActionResult SendCareer([FromBody] CareersForm _career)
        {
            APIResponse apiResp = UserHelpReqRepo.SendCareer(_career);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }
    }
}
