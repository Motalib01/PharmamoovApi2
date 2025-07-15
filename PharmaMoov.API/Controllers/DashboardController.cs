using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Dashboard")]
    public class DashboardController : APIBaseController
    {
        IDashboardRepository DashboardRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }
        public DashboardController(IDashboardRepository _nRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            DashboardRepo = _nRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [HttpPost("GetPharmacyAdminDashboard")]
        public IActionResult GetPharmacyAdminDashboard([FromBody] DashboardParamModel dashBoardParamModel)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = DashboardRepo.GetPharmacyAdminDashboard(dashBoardParamModel);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                return BadRequest(apiResp);
            }
            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }
        [HttpGet("GetSuperAdminDashboard")]
        public IActionResult GetSuperAdminDashboard()
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = DashboardRepo.GetSuperAdminDashboard();
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                return BadRequest(apiResp);
            }
            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }

        //[HttpPost("GetFullDashboard")]
        //public IActionResult GetFullDashboard([FromBody] DashBoardDateRangeFilter _dateRange)
        //{
        //    APIResponse aResp = DashboardRepo.GetFullDashboard(_dateRange);
        //    if (aResp.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        return Ok(aResp);
        //    }
        //    else
        //    {
        //        return BadRequest(aResp);
        //    }
        //}

        [HttpGet("GetOrdersNotification/{ShopId}")]
        public IActionResult GetOrdersNotification(Guid ShopId)
        {
            APIResponse apiResp = DashboardRepo.GetOrdersNotification(ShopId);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            return BadRequest(apiResp);   
        }

    }
}
