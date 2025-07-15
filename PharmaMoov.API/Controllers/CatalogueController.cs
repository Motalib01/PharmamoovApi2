using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaMoov.API.Helpers;
using System;
using PharmaMoov.Models.Product;
using PharmaMoov.Models.Shop;

namespace PharmaMoov.API.Controllers
{
    [AllowAnonymous]
    [Produces("application/json")]
    [Route("api/Catalogue")]
    public class CatalogueController : APIBaseController
    {
        ICatalogueRepository CatalogueRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public CatalogueController(ICatalogueRepository _aRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            CatalogueRepo = _aRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [HttpPost("MainCatalogue")]
        public IActionResult GetMainCatalogue([FromBody] FilterMain _filter)
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
                return Ok(CatalogueRepo.GetMainCatalogue(_filter));
            }
        }

        [HttpGet("AllShopsCategories")]
        public IActionResult GetAllShopsCategories([FromQuery]FilterShopCategoriesModel _filterShopCategories)
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
                return Ok(CatalogueRepo.GetAllShopsCategories(_filterShopCategories));
            }
        }

        [HttpPost("FilterShops")]
        public IActionResult FilterShops([FromBody] FilterShops _filter)
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
                return Ok(CatalogueRepo.FilterShops(_filter));
            }
        }

        [HttpGet("ShopsProductCategories/{_shop}")]
        public IActionResult GetShopsProductCategories(Guid _shop)
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
                return Ok(CatalogueRepo.GetShopsProductCategories(_shop));
            }
        }

        [HttpGet("ShopDetails/{_shop}")]
        public IActionResult GetShopDetails(Guid _shop)
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
                return Ok(CatalogueRepo.GetShopDetails(_shop));
            }
        }

        [HttpPost("FilterShopProducts")]
        public IActionResult FilterShopProducts([FromBody] FilterProducts _filter)
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
                return Ok(CatalogueRepo.FilterShopsProducts(_filter));
            }
        }

        [HttpGet("ShopProductDetails/{_product}")]
        public IActionResult GetShopProductDetails(Guid _product)
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
                return Ok(CatalogueRepo.GetShopsProductDetails(_product));
            }
        }

        [HttpGet("CatalougeForRegularCustomer")]
        public IActionResult CatalougeForRegularCustomer() 
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
                return Ok(CatalogueRepo.CatalougeForRegularCustomer());
            }
        }

        [HttpGet("GetShopAddressDetailsForMap")]
        public IActionResult GetShopAddressDetailsForMap([FromQuery] FilterShopAddress filterShopAddress)
        {
            if (ModelState.IsValid)
            {
                return Ok(CatalogueRepo.GetShopAddressDetailsForMap(filterShopAddress));
            }
            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }

        [HttpGet("GetAllShop")]
        public IActionResult GetAllShop([FromQuery] ShopListParamModel model)
        {
            if (ModelState.IsValid)
            {
                var apiResp = CatalogueRepo.GetAllShop(model);
                if (!string.IsNullOrEmpty(model.SearchKey))
                {
                    return Ok(apiResp);
                }
                else
                {
                    if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return Ok(apiResp);
                    }
                    return BadRequest(apiResp);
                }
            }
            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }

        [HttpGet("GetTopShops")]
        public IActionResult GetTopShops()
        {
            if (ModelState.IsValid)
            {
                var apiResp = CatalogueRepo.GetTopShops();
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }
            }
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