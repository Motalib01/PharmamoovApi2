using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/CardPayment")]
    public class CardPaymentController : Controller
    {
        APIConfigurationManager MConf { get; }
        ICardPaymentRepository CardPaymentRepo { get; }

        public CardPaymentController(APIConfigurationManager _conf, ICardPaymentRepository _cardRepo)
        {
            MConf = _conf;
            CardPaymentRepo = _cardRepo;
        }

        [AllowAnonymous]
        [HttpGet("UpdateCardRegistration/")]
        public IActionResult UpdateCardRegistration([FromQuery] int RegistrationRecordID, [FromQuery] string data)
        {
            APIResponse returnData = CardPaymentRepo.UpdateCardRegistration(RegistrationRecordID, data);
            if (returnData.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(returnData);
            }
            else
            {
                return BadRequest(returnData);
            }
        }

        [HttpGet("RegisterNewCard")]
        public IActionResult RegisterNewCard([FromHeader] string Authorization)
        {
            APIResponse returnData = CardPaymentRepo.NewCardRegistration(Authorization.Split(' ')[1]);
            if (returnData.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(returnData);
            }
            else
            {
                return BadRequest(returnData);
            }
        }

        [HttpGet("GetAvailableCards")]
        public IActionResult GetAvailableCards([FromHeader] string Authorization)
        {
            APIResponse returnData = CardPaymentRepo.GetAvailableCards(Authorization.Split(' ')[1]);
            if (returnData.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(returnData);
            }
            else
            {
                return BadRequest(returnData);
            }
        }

        [HttpGet("DeactivateCard/{cardID}")]
        public IActionResult DeactivateCard([FromHeader] string Authorization, string cardID)
        {
            APIResponse returnData = CardPaymentRepo.DeactivateCard(Authorization.Split(' ')[1], cardID);
            if (returnData.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(returnData);
            }
            else
            {
                return BadRequest(returnData);
            }
        }
    }
}
