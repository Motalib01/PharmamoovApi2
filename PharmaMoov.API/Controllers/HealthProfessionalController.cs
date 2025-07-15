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
    [Route("api/HealthProfessional")]
    public class HealthProfessionalController : APIBaseController
    {
        IHealthProfessionalRepository HealthProfessionalRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public HealthProfessionalController(IHealthProfessionalRepository _aRepo, APIConfigurationManager _conf)
        {
            HealthProfessionalRepo = _aRepo;
            MConf = _conf;
        }

        [HttpGet("GetHealthProfessionals/{_id}")]
        public IActionResult GetHealthProfessionals([FromHeader] string Authorization, int _id)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = HealthProfessionalRepo.GetHealthProfessionals(Authorization.Split(' ')[1], _id);
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
