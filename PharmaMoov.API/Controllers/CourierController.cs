using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models;

namespace PharmaMoov.API.Controllers
{   
    [Authorize]
    [Produces("application/json")]
    [Route("api/Courier")]
    public class CourierController : APIBaseController
    {
        ICourierRepository CourierRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public CourierController(ICourierRepository _aRepo, APIConfigurationManager _conf)
        {
            CourierRepo = _aRepo;
            MConf = _conf;
        }

        [HttpGet("GetCouriers/{_id}")]
        public IActionResult GetCouriers([FromHeader] string Authorization, int _id)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = CourierRepo.GetCouriers(Authorization.Split(' ')[1], _id);
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
