using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaMoov.API.DataAccessLayer;
using PharmaMoov.API.Services.Abstractions;
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

        public MedipimController(IMedipimService medipimService)
        {
            _medipimService = medipimService;
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

        [HttpGet("products/by-category-fr/{frenchName}")]
        public async Task<IActionResult> GetProductsByFrenchCategoryName(
            string frenchName,
            [FromQuery] int page = 0,
            [FromQuery] int size = 100)
        {
            // 1. Get all categories
            var categories = await _medipimService.GetPublicCategoriesAsync();

            // 2. Match French name (case-insensitive)
            var category = categories.FirstOrDefault(c =>
                c.Name?.ContainsKey("fr") == true &&
                string.Equals(c.Name["fr"], frenchName, StringComparison.OrdinalIgnoreCase));

            if (category == null)
                return NotFound(new { Message = $"Category with French name '{frenchName}' not found." });

            // 3. Use category ID to fetch products
            var request = new GetMedipimProductsRequest
            {
                Page = page,
                PageSize = size,
                PublicCategoryId = category.Id
            };

            var products = await _medipimService.GetProductsAsync(request);

            return Ok(products);
        }


        [HttpPost("add-medipim-to-cart")]
        public async Task<IActionResult> AddMedipimProductToCart(
            [FromBody] AddMedipimProductToCartRequest request,
            [FromHeader] string Authorization,
            [FromServices] IMedipimService medipimService,
            [FromServices] APIDBContext dbContext)
        {
            var token = Authorization?.Split(' ')[1];

            var userLogin = await dbContext.UserLoginTransactions
                .FirstOrDefaultAsync(x => x.Token == token && x.IsActive);

            if (userLogin == null)
                return Unauthorized(new APIResponse { Message = "Unauthorized user" });

            var existingCartItem = await dbContext.CartItems.FirstOrDefaultAsync(x =>
                x.UserId == userLogin.UserId &&
                x.ShopId == request.ShopId &&
                x.ProductRecordId == 0 &&
                x.ExternalProductId == request.MedipimProductId);

            if (existingCartItem != null)
            {
                existingCartItem.ProductQuantity += request.Quantity;
                existingCartItem.LastEditedDate = DateTime.UtcNow;
            }
            else
            {
                var newCartItem = new CartItem
                {
                    ShopId = request.ShopId,
                    UserId = userLogin.UserId,
                    PatientId = request.PatientId,
                    ProductRecordId = 0, 
                    ProductQuantity = request.Quantity,
                    PrescriptionRecordId = request.PrescriptionRecordId,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userLogin.UserId,
                    IsEnabled = true,
                    ExternalProductId = request.MedipimProductId, 
                    LastEditedDate = DateTime.UtcNow,
                    LastEditedBy = userLogin.UserId
                };

                dbContext.CartItems.Add(newCartItem);
            }

            await dbContext.SaveChangesAsync();

            return Ok(new APIResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Status = "Success",
                Message = "Medipim product added to cart."
            });
        }

    }
}