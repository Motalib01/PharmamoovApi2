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
    }
}