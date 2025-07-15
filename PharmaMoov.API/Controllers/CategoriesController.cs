using PharmaMoov.Models;
using PharmaMoov.Models.User;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaMoov.Models.Shop;
using PharmaMoov.Models.Product;
using System;
using PharmaMoov.Models.Admin;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Categories")]
    public class CategoriesController : APIBaseController
    {
        ICategoriesRepository CategoriesRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public CategoriesController(ICategoriesRepository _aRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            CategoriesRepo = _aRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [HttpGet("PopulateShopCategories/{_category}/{_isActive}")]
        public IActionResult PopulateShopCategories(int _category, int _isActive)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = CategoriesRepo.PopulateShopCategories(_category, _isActive);
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

        [HttpPost("AddShopCategory")]
        public IActionResult AddShopCategory([FromHeader] string Authorization, [FromBody] ShopCategoriesDTO _category)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = CategoriesRepo.AddShopCategory(Authorization.Split(' ')[1], _category);
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

        [HttpPost("EditShopCategory")]
        public IActionResult EditShopCategory([FromHeader] string Authorization, [FromBody] ShopCategoriesDTO _category)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = CategoriesRepo.EditShopCategory(Authorization.Split(' ')[1], _category);
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

        [HttpPost("ChangeShopCategoryStatus")]
        public IActionResult ChangeShopCategoryStatus([FromHeader] string Authorization, [FromBody] ChangeRecordStatus _category)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = CategoriesRepo.ChangeShopCategoryStatus(Authorization.Split(' ')[1], _category);
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
        [HttpGet("PopulateProductCategories/{_shopId}/{_category}/{_isActive}")]
        public IActionResult PopulateProductCategories(Guid _shopId, int _category, int _isActive)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = CategoriesRepo.PopulateProductCategories(_shopId, _category, _isActive);
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

        [HttpPost("AddProductCategory")]
        public IActionResult AddProductCategory([FromHeader] string Authorization, [FromBody] ProductCategoriesDTO _category)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = CategoriesRepo.AddProductCategory(Authorization.Split(' ')[1], _category);
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

        [HttpPost("EditProductCategory")]
        public IActionResult EditProductCategory([FromHeader] string Authorization, [FromBody] ProductCategoriesDTO _category)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = CategoriesRepo.EditProductCategory(Authorization.Split(' ')[1], _category);
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
        
        [HttpGet("FilterProductCategories/{_searchKey}")]
        public IActionResult FilterProductCategories(string _searchKey)
        {
            if (ModelState.IsValid)
            {               
               return Ok(CategoriesRepo.FilterProductCategories(_searchKey));               
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