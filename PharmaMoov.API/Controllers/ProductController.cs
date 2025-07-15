using PharmaMoov.Models;
using PharmaMoov.Models.Product;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Product")]
    public class ProductController : APIBaseController
    {
        IProductRepository ProductRepo { get; }
        private APIConfigurationManager MConf { get; }     

        public ProductController(IProductRepository _prodRepo, IWebHostEnvironment _hEnvironment, APIConfigurationManager _conf)
        {
            ProductRepo = _prodRepo;
            MConf = _conf;
        }

        [AllowAnonymous]
        [HttpGet("GetAllProducts/{_shop}/{_product}")]
        public IActionResult GetAllProducts(Guid _shop, int _product)
        {
            APIResponse aResp = new APIResponse();
            aResp = ProductRepo.GetAllProducts(_shop, _product);
            if (aResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(aResp);
            }
            else
            {
                return BadRequest(aResp);
            }
        }
        [AllowAnonymous]
        [HttpGet("GetAllExternalProducts/{_product}")]
        public IActionResult GetAllExternalProducts(int _product)
        {
            APIResponse aResp = new APIResponse();
            aResp = ProductRepo.GetAllExternalProducts(_product);
            if (aResp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(aResp);
            }
            else
            {
                return BadRequest(aResp);
            }
        }

        [HttpPost("AddProduct")]
        public IActionResult AddProduct([FromBody] Product _product)
        {
            APIResponse aResp = new APIResponse();
            if (ModelState.IsValid)
            {
                aResp = ProductRepo.AddProduct(_product);
                if (aResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(aResp);
                }
                else
                {
                    return BadRequest(aResp);
                }

            }
            else
            {
                aResp = new APIResponse
                {
                    Message = "Model Objet Invalid",
                    Status = "Model Erreur!",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ModelError = ModelState.Errors()
                };
                return BadRequest(aResp);
            }
        }

		[HttpPost("AddProductFromExtern")]
		public IActionResult AddProductFromExtern([FromBody] AddProductRequest addProductRequest)
		{
			APIResponse aResp = new APIResponse();
			if (ModelState.IsValid)
			{
				aResp = ProductRepo.AddProductFromExtern(addProductRequest.Shop, addProductRequest.ProductRecordId);
				if (aResp.StatusCode == System.Net.HttpStatusCode.OK)
				{
					return Ok(aResp);
				}
				else
				{
					return BadRequest(aResp);
				}

			}
			else
			{
				aResp = new APIResponse
				{
					Message = "Model Objet Invalid",
					Status = "Model Erreur!",
					StatusCode = System.Net.HttpStatusCode.BadRequest,
					ModelError = ModelState.Errors()
				};
				return BadRequest(aResp);
			}
		}

		[HttpPost("EditProduct")]
        public IActionResult EditProduct([FromBody] Product _product)
        {
            APIResponse aResp = new APIResponse();
            if (ModelState.IsValid)
            {
                aResp = ProductRepo.EditProduct(_product);
                if (aResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(aResp);
                }
                else
                {
                    return BadRequest(aResp);
                }
            }
            else
            {
                aResp = new APIResponse
                {
                    Message = "Model Objet Invalid",
                    Status = "Model Erreur!",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ModelError = ModelState.Errors()
                };
                return BadRequest(aResp);
            }
        }

        [HttpPost("ChangeProductStatus")]
        public IActionResult ChangeProductStatus([FromBody] ChangeProdStatus _product)
        {
            APIResponse aResp = new APIResponse();
            if (ModelState.IsValid)
            {
                aResp = ProductRepo.ChangeProductStatus(_product);
                if (aResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(aResp);
                }
                else
                {
                    return BadRequest(aResp);
                }
            }
            else
            {
                aResp = new APIResponse
                {
                    Message = "Model Objet Invalid",
                    Status = "Model Erreur!",
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ModelError = ModelState.Errors()
                };
                return BadRequest(aResp);
            }
        }

        [AllowAnonymous]
        [HttpGet("PopulateProductAndPharmacy")]
        public IActionResult PopulateProductAndPharmacy([FromQuery] int _productCategoryId, [FromQuery] int _isActive, [FromQuery] string _searchKey, [FromQuery] int sortBy,[FromQuery] bool isProductFeature)
        {
            if (ModelState.IsValid)
            {               
               return Ok(ProductRepo.PopulateProductAndPharmacy(_productCategoryId, _isActive, _searchKey, sortBy, isProductFeature));
            }
            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }

        [HttpGet("PopulateProductBySort")]
        public IActionResult PopulateProductBySort([FromQuery] int productCategoryId, [FromQuery] Guid shopId, [FromQuery] int sortBy, [FromQuery] int pageNo, [FromQuery] string searchKey)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ProductRepo.PopulateProductBySort(productCategoryId, shopId, sortBy, pageNo, searchKey);
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

        [HttpPost("ImportProduct")]
        public IActionResult ImportProduct([FromHeader] ImportProductParamModel model)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ProductRepo.ImportProduct(model);
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
        
        [HttpGet("GetProductsForPrescription/{shopId}/{productRecordId}")]
        public IActionResult GetProductsForPrescription(Guid shopId, int productRecordId)
        {
            APIResponse aResp = new APIResponse();
            aResp = ProductRepo.GetProductsForPrescription(shopId, productRecordId);
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