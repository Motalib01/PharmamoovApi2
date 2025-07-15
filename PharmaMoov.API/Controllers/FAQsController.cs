using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using PharmaMoov.Models.Review;
using PharmaMoov.Models.Shop;
using PharmaMoov.Models.Admin;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/FAQs")]
    public class FAQsController : APIBaseController
    {
        IFAQsRepository FAQsRepo { get; }
        private IMainHttpClient MainHttpClient { get; }

        public FAQsController(IFAQsRepository _aRepo, IWebHostEnvironment _hEnvironment, IMainHttpClient _mhttpc)
        {
            FAQsRepo = _aRepo;
            MainHttpClient = _mhttpc;
        }

        [HttpPost("AddFAQuestion")]
        public IActionResult AddFAQuestion([FromHeader] string Authorization, [FromBody] ShopFAQdto _fAQ)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = FAQsRepo.AddFAQuestion(Authorization.Split(' ')[1], _fAQ);
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

        [HttpPost("EditFAQuestion")]
        public IActionResult EditFAQuestion([FromHeader] string Authorization, [FromBody] ShopFAQdto _fAQ)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = FAQsRepo.EditFAQuestion(Authorization.Split(' ')[1], _fAQ);
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

        [HttpGet("GetFAQsList/{_id}")]
        public IActionResult GetFAQsList([FromHeader] string Authorization, int _id)
        {
            APIResponse apiResp = FAQsRepo.GetFAQsList(Authorization.Split(' ')[1], _id);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("ChangeFAQStatus")]
        public IActionResult ChangeFAQStatus([FromBody] ChangeRecordStatus _admin)
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
                return Ok(FAQsRepo.ChangeFAQStatus(_admin));
            }
        }

        [AllowAnonymous]
        [HttpGet("GetFAQs")]
        public IActionResult GetFAQs()
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = FAQsRepo.GetFAQs();
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
        [HttpGet("GetTermsAndConditions")]
        public IActionResult GetTermsAndConditions()
        {
            APIResponse apiResp = FAQsRepo.GetTermsAndConditions();
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
        [HttpGet("ShopGetTermsAndConditions")]
        public IActionResult ShopGetTermsAndConditions()
        {
            APIResponse apiResp = FAQsRepo.ShopGetTermsAndConditions();
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
        [HttpGet("GetPrivacyPolicy")]
        public IActionResult GetPrivacyPolicy()
        {
            APIResponse apiResp = FAQsRepo.GetPrivacyPolicy();
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("SaveTermsAndConditions")]
        public IActionResult SaveTermsAndConditions([FromHeader] string Authorization, [FromBody] TermsAndCondition _termsAndCondition)
        {
            APIResponse apiResp = FAQsRepo.SaveTermsAndConditions(Authorization.Split(' ')[1], _termsAndCondition);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("ShopSaveTermsAndConditions")]
        public IActionResult ShopSaveTermsAndConditions([FromHeader] string Authorization, [FromBody] ShopTermsAndCondition _termsAndCondition)
        {
            APIResponse apiResp = FAQsRepo.ShopSaveTermsAndConditions(Authorization.Split(' ')[1], _termsAndCondition);
            if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(apiResp);
            }
            else
            {
                return BadRequest(apiResp);
            }
        }

        [HttpPost("SavePrivacyPolicy")]
        public IActionResult SavePrivacyPolicy([FromHeader] string Authorization, [FromBody] PrivacyPolicy _privPolicy)
        {
            APIResponse apiResp = FAQsRepo.SavePrivacyPolicy(Authorization.Split(' ')[1], _privPolicy);
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