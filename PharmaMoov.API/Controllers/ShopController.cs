using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaMoov.Models.Shop;
using PharmaMoov.API.Helpers;
using System;
using PharmaMoov.Models.Admin;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Shop")]
    public class ShopController : APIBaseController
    {
        IShopRepository ShopRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public ShopController(IShopRepository _aRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            ShopRepo = _aRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [AllowAnonymous]
        [HttpPost("RegisterShop")]
        public IActionResult RegisterShop([FromBody] ShopProfile _shop)
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
                return Ok(ShopRepo.RegisterShop(_shop, MainHttpClient, MConf));
            }
        }

        [AllowAnonymous]
        [HttpPost("AddShopRequest")]
        public IActionResult AddShopRequest([FromBody] ShopRequestDTO _shop)
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
                return Ok(ShopRepo.AddShopRequest(_shop));
            }
        }

        [AllowAnonymous]
        [HttpGet("GetShopRequests")]
        public IActionResult GetShopRequests()
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.GetShopRequests();
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

        [HttpGet("GetShopProfile/{_shop}")]
        public IActionResult GetShopProfile(Guid _shop)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.GetShopProfile(_shop);
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
        [HttpGet("ShopOpeningHours/{_shop}")]
        public IActionResult GetShopOpeningHours(Guid _shop)
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
                return Ok(ShopRepo.GetShopOpeningHours(_shop));
            }
        }

        [HttpPost("EditShopOpeningHours")]
        public IActionResult EditShopOpeningHours([FromBody] ShopHourList _shop)
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
                return Ok(ShopRepo.EditShopOpeningHours(_shop));
            }
        }

        [HttpPost("EditShopProfile")]
        public IActionResult EditShopProfile([FromBody] Shop _shop)
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
                return Ok(ShopRepo.EditShopProfile(_shop));
            }
        }

        [HttpPost("SetShopConfigurations")]
        public IActionResult SetShopConfigurations([FromBody] ShopConfigs _shop)
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
                return Ok(ShopRepo.SetShopConfigurations(_shop));
            }
        }

        [HttpPost("AddShopDocument")]
        public IActionResult AddShopDocument([FromHeader] string Authorization, [FromBody] ShopDocumentDTO _shop)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.AddShopDocument(Authorization.Split(' ')[1], _shop);
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

        [HttpGet("ShopDocuments/{_shop}")]
        public IActionResult GetShopDocuments([FromHeader] string Authorization, Guid _shop)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.GetShopDocuments(Authorization.Split(' ')[1], _shop);
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

        [HttpPost("DeleteShopDocument")]
        public IActionResult DeleteShopDocuments([FromHeader] string Authorization, [FromBody] int _shop)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.DeleteShopDocuments(Authorization.Split(' ')[1], _shop);
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

        [HttpGet("ShopList")]
        public IActionResult GetShopList()
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.GetShopList();
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

        [HttpPost("ChangeShopStatus")]
        public IActionResult ChangeShopStatus([FromHeader] string Authorization, [FromBody] ChangeRecordStatus _shop)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.ChangeShopStatus(Authorization.Split(' ')[1], _shop);
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

        [HttpPost("ShopCommisionInvoice")]
        public IActionResult ShopCommisionInvoice([FromBody] ShopCommissionDateRange _range,[FromHeader] string Authorization) 
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.ShopCommisionInvoice(_range,Authorization.Split(' ')[1]);
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

        [HttpPost("ComissionInvoiceView")]
        public IActionResult ComissionInvoiceView([FromBody] SingleShopComissionInvoiceParameters _sParams,[FromHeader] string Authorization) 
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.ComissionInvoiceView(_sParams, Authorization.Split(' ')[1]);
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

        [HttpPost("ChangePopularStatus")]
        public IActionResult ChangePopularStatus([FromHeader] string Authorization, [FromBody] ChangeRecordStatus _shop)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.ChangePopularStatus(Authorization.Split(' ')[1], _shop);
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

        #region "Pharmacy Request Registration"

        [HttpPost("ChangeRegistrationStatus")]
        public IActionResult ChangeRegistrationStatus([FromHeader] string Authorization, [FromBody] ChangeRegistrationStatus _shop)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.ChangeRegistrationStatus(Authorization.Split(' ')[1], _shop);
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

        [HttpGet("GetShopRegistrationRequest/{_shopId}")]
        public IActionResult GetShopRegistrationRequest(int _shopId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.GetShopRegistrationRequest(_shopId);
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

        [HttpGet("GetRequestList")]
        public IActionResult GetRequestList()
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.GetRequestList();
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
        #endregion

        [HttpGet("GetShopListForAutocomplete/{_searchKey}")]
        public IActionResult GetShopListForAutocomplete(string _searchKey)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.GetShopListForAutocomplete(_searchKey);
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

        [HttpGet("GetPharmacyOwner/{shopRecordId}")]
        public IActionResult GetPharmacyOwner(int shopRecordId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ShopRepo.GetPharmacyOwner(shopRecordId);
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

        [HttpPost("UpdatePharmacyOwner")]
        public IActionResult UpdatePharmacyOwner([FromBody] PharmacyOwner model)
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
                return Ok(ShopRepo.UpdatePharmacyOwner(model));
            }
        }

    }
}