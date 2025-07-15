using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.User;
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
    [Route("api/Customer")]
    public class CustomerController : APIBaseController
    {
        ICustomerRepository CustomerRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public CustomerController(ICustomerRepository _aRepo, APIConfigurationManager _conf)
        {
            CustomerRepo = _aRepo;
            MConf = _conf;
        }

        [HttpGet("GetCustomers/{_id}")]
        public IActionResult GetCustomers([FromHeader] string Authorization, int _id)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = CustomerRepo.GetCustomers(Authorization.Split(' ')[1], _id);
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

        //[HttpPost("ChangeCustomerStatus")]
        //public IActionResult ChangeCustomerStatus([FromHeader] string Authorization, [FromBody] ChangeRecordStatus _customerStat)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new APIResponse
        //        {
        //            Message = "Invalid Model Object",
        //            ModelError = ModelState.Errors(),
        //            StatusCode = System.Net.HttpStatusCode.BadRequest
        //        });
        //    }
        //    else
        //    {
        //        return Ok(CustomerRepo.ChangeCustomerStatus(Authorization.Split(' ')[1], _customerStat));
        //    }
        //}

        [HttpPost("EditCustomerProfile")]
        public IActionResult EditCustomerProfile([FromHeader] string Authorization, [FromBody] User _user)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = CustomerRepo.EditCustomerProfile(Authorization.Split(' ')[1],_user);
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
