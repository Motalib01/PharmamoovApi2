using Microsoft.AspNetCore.Mvc;
using PharmaMoov.API.Services.Abstractions;
using PharmaMoov.Models.External.Medipim;
using System.Collections.Generic;
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
    }
}