using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using PharmaMoov.Models.Review;
using PharmaMoov.Models.Shop;
using PharmaMoov.Models.Promo;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.Campaign;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Promo")]
    public class PromoController : APIBaseController
    {
        IPromoRepository PromoRepo { get; } 

        public PromoController(IPromoRepository _aRepo, IWebHostEnvironment _hEnvironment)
        {
            PromoRepo = _aRepo; 
        }

        [HttpPost("AddPromo")]
        public IActionResult AddPromo([FromHeader] string Authorization, [FromBody] PromoDTO _promo)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PromoRepo.AddPromo(Authorization.Split(' ')[1], _promo);
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

        [HttpPost("EditPromo")]
        public IActionResult EditPromo([FromHeader] string Authorization, [FromBody] PromoDTO _promo)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PromoRepo.EditPromo(Authorization.Split(' ')[1], _promo);
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

        [HttpGet("GetPromoValue/{_code}")]
        public IActionResult GetPromoValue([FromHeader] string Authorization, string _code)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PromoRepo.GetPromoValue(Authorization.Split(' ')[1], _code);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return Ok(apiResp);
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

        [HttpGet("GetCampaignByShopId/{_id}")]
        public IActionResult GetCampaignById([FromHeader] string Authorization, Guid _id)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PromoRepo.GetCampaignByShopId(Authorization.Split(' ')[1],_id);
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
         
        [HttpGet("GetPromoList/{_id}")]
        public IActionResult GetPromoList(int _id)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PromoRepo.GetPromoList(_id);
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

        [HttpPost("AddBanner")]
        public IActionResult AddBanner([FromHeader] string Authorization, [FromBody] Campaign _campaign)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PromoRepo.AddBanner(Authorization.Split(' ')[1], _campaign);
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

        [HttpPost("ChangePromoStatus")]
        public IActionResult ChangePromoStatus([FromBody] ChangeRecordStatus _admin)
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
                return Ok(PromoRepo.ChangePromoStatus(_admin));
            }
        }

        [HttpGet("GetShopBanners")]
        public IActionResult GetShopBanners()
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PromoRepo.GetShopBanners();
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

        [HttpGet("GetCampaignByBannerId/{_id}")]
        public IActionResult GetCampaignByBannerId([FromHeader] string Authorization, int _id)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PromoRepo.GetCampaignByBannerId(Authorization.Split(' ')[1], _id);
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