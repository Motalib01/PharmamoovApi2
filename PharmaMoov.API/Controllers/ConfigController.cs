using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using PharmaMoov.Models.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Config")]
    public class ConfigController : APIBaseController
    {
        IConfigRepository ConfigRepo { get; }
        private IMainHttpClient MainHttpClient { get; } 

        public ConfigController(IConfigRepository _cRepo, IMainHttpClient _mhttpc)
        {
            ConfigRepo = _cRepo;
            MainHttpClient = _mhttpc; 
        }

        [HttpGet("GetAllConfigurations")]
        public IActionResult GetAllConfigurations() 
        {
            APIResponse aResp = ConfigRepo.GetAllConfigurations(); 
            if (aResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(aResp);
            }
            else
            {
                return BadRequest(aResp);
            }
        }

        [HttpPost("UpdateOrderConfig")]
        public IActionResult UpdateOrderConfig([FromBody] List<OrderConfiguration> _configs) 
        {
            APIResponse aResp = ConfigRepo.UpdateOrderConfig(_configs);
            if (aResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(aResp);
            }
            else
            {
                return BadRequest(aResp);
            } 
        }
    }
}
