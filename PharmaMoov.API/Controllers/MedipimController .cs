using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaMoov.API.DataAccessLayer;
using PharmaMoov.API.Helpers;
using PharmaMoov.API.Services.Abstractions;
using PharmaMoov.API.Services.CartExternal;
using PharmaMoov.Models;
using PharmaMoov.Models.Cart;
using PharmaMoov.Models.External.Medipim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PharmaMoov.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedipimController : ControllerBase
    {
        private readonly IMedipimService _medipimService;
        private readonly APIDBContext _dbContext;

        public MedipimController(IMedipimService medipimService, APIDBContext dbContext)
        {
            _medipimService = medipimService;
            _dbContext = dbContext;
        }

        [HttpGet("categories")]
        public async Task<ActionResult<List<MedipimCategoryDto>>> GetCategories()
        {
            var categories = await _medipimService.GetPublicCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetAllProducts([FromQuery] int page = 0, [FromQuery] int size = 100)
        {
            var request = new GetMedipimProductsRequest
            {
                Page = page,
                PageSize = size
            };

            var products = await _medipimService.GetProductsAsync(request);
            return Ok(products);
        }

        [HttpGet("product/{id}")]
        public async Task<ActionResult<MedipimProductDto>> GetProductById(string id)
        {
            try
            {
                var product = await _medipimService.GetProductByIdAsync(id);
                return Ok(product);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"External API error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return NotFound($"Product not found: {ex.Message}");
            }
        }

        [HttpGet("products/limited")]
        public async Task<IActionResult> GetLimitedProducts()
        {
            var request = new GetMedipimProductsRequest
            {
                Page = 0,
                PageSize = 10
            };

            var products = await _medipimService.GetProductsAsync(request);

            // Return only first 6 manually
            var limited = products.Take(6).ToList();
            return Ok(limited);
        }

        [HttpGet("categories/limited")]
        public async Task<ActionResult<List<MedipimCategoryDto>>> GetLimitedCategories()
        {
            var categories = await _medipimService.GetPublicCategoriesAsync();

            var limitedCategories = categories.Take(8).ToList();

            return Ok(limitedCategories);
        }

        [HttpGet("products/by-category-fr")]
        public async Task<IActionResult> GetProductsByCategoryFrName([FromQuery] string categoryFrName)
        {
            var request = new GetMedipimProductsRequest
            {
                Page = 0,
                PageSize = 100
            };

            var products = await _medipimService.GetProductsAsync(request);

            var filtered = products
                .Where(p => p.PublicCategories != null && p.PublicCategories.Any(c =>
                    c.Name != null &&
                    c.Name.TryGetValue("fr", out var nameFr) &&
                    string.Equals(nameFr, categoryFrName, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return Ok(filtered);
        }

        [HttpPost("add-medipim-to-cart")]
        public async Task<IActionResult> AddMedipimToCart([FromBody] AddMedipimToCartRequest request)
        {
            var sessionKey = $"external-cart-{request.PatientId}";

            var cart = HttpContext.Session.GetObject<List<ExternalCartItem>>(sessionKey) ?? new List<ExternalCartItem>();

            var existing = cart.FirstOrDefault(x => x.MedipimProductId == request.MedipimProductId);
            if (existing != null)
                existing.Quantity += request.Quantity;
            else
                cart.Add(new ExternalCartItem
                {
                    ShopId = request.ShopId,
                    MedipimProductId = request.MedipimProductId,
                    PatientId = request.PatientId,
                    PrescriptionRecordId = request.PrescriptionRecordId,
                    Quantity = request.Quantity
                });

            HttpContext.Session.SetObject(sessionKey, cart);
            return Ok(new { success = true, count = cart.Count });
        }


        [AllowAnonymous]
        [HttpGet("GetCombinedCart")]
        public async Task<IActionResult> GetCombinedCart([FromQuery] Guid patientId)
        {
            var sessionKey = $"external-cart-{patientId}";
            var externalCart = HttpContext.Session.GetObject<List<ExternalCartItem>>(sessionKey) ??
                               new List<ExternalCartItem>();

            var combined = new List<UserCartItem>();

            foreach (var ext in externalCart)
            {
                var medipimProduct = await _medipimService.GetProductByIdAsync(ext.MedipimProductId);
                var price = medipimProduct.PublicPrice ?? medipimProduct.PharmacistPrice ?? 0;
                var photo = medipimProduct.Photos?.FirstOrDefault()?.Formats?.GetValueOrDefault("thumbnailWebp");

                combined.Add(new UserCartItem
                {
                    ShopId = ext.ShopId,
                    ShopName = "Medipim External",
                    ProductRecordId = 0,
                    ProductName = medipimProduct.Name.GetValueOrDefault("fr") ?? medipimProduct.Name.GetValueOrDefault("en"),
                    ProductQuantity = ext.Quantity,
                    ProductPrice = price,
                    TotalAmount = price * ext.Quantity,
                    ProductTaxValue = 0,
                    ProductTaxAmount = 0,
                    ProductIcon = photo,
                    SalePrice = price
                });
            }

            return Ok(combined);
        }

        [HttpDelete("ClearMedipimCart/{patientId}")]
        public IActionResult ClearMedipimCart(Guid patientId)
        {
            var sessionKey = $"external-cart-{patientId}";
            HttpContext.Session.Remove(sessionKey);
            return Ok();
        }


    }


}