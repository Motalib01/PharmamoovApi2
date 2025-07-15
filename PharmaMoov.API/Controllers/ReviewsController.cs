using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using PharmaMoov.Models.Review;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Reviews")]
    public class ReviewsController : APIBaseController
    {
        IReviewRepository ReviewRepo { get; }
        private IMainHttpClient MainHttpClient { get; }

        public ReviewsController(IReviewRepository _aRepo, IWebHostEnvironment _hEnvironment, IMainHttpClient _mhttpc)
        {
            ReviewRepo = _aRepo;
            MainHttpClient = _mhttpc;
        }

        [HttpPost("AddShopReview")]
        public IActionResult AddShopReview([FromHeader] string Authorization, [FromBody] ShopReviewRatingDTO _review)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ReviewRepo.AddShopReview(Authorization.Split(' ')[1], _review);
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

        [HttpGet("ShopReviews/{_shop}/{_review}")]
        public IActionResult GetShopReviews(Guid _shop, int _review)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ReviewRepo.GetShopReviews(_shop, _review);
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

        [HttpPost("SetShopFavorite")]
        public IActionResult SetShopFavorite([FromHeader] string Authorization, [FromBody] ShopFavorite _favorite)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = ReviewRepo.SetShopFavorite(Authorization.Split(' ')[1], _favorite);
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