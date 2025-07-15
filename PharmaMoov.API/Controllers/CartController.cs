using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models.Cart;
using System;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Cart")]
    public class CartController : APIBaseController
    {
        ICartRepository CartRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public CartController(ICartRepository _aRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            CartRepo = _aRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [AllowAnonymous]
        [HttpPost("SyncCart")]
        public IActionResult SyncCartItem([FromBody] CartSyncDTO _cart, [FromHeader] string Authorization)
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
                return Ok(CartRepo.SyncCartItem(_cart, Authorization != null ? Authorization.Split(' ')[1] : string.Empty));
            }
        }

        [AllowAnonymous]
        [HttpPost("SyncCartForHealthProfessional")]
        public IActionResult SyncCartForHealthProfessional([FromBody] CartSyncForHealthProfessional model)
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
                return Ok(CartRepo.SyncCartItemForHealthProfessional(model));
            }
        }

        [HttpPost("Checkout")]
        public IActionResult CheckoutCartItems([FromHeader] string Authorization, [FromBody] CheckoutCartItem _cart)
        {
            if (ModelState.IsValid)
            {
                if (_cart.PaymentType == OrderPaymentType.ONLINEPAYMENT)
                {
                    APIResponse apiResp = CartRepo.CheckoutCartItemsViaDirectCardPayIn(Authorization.Split(' ')[1], _cart, _cart.CardId, "Web");
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
                    APIResponse apiResp = CartRepo.CheckoutCartItems(Authorization.Split(' ')[1], _cart);
                    if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return Ok(apiResp);
                    }
                    else
                    {
                        return BadRequest(apiResp);
                    }
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

        [HttpPost("CheckoutViaDirectCardPayment/{cardId}")]
        public IActionResult CheckoutCartItemsViaDirectCardPayIn([FromHeader] string Authorization, [FromBody] CheckoutCartItem _cart, string cardId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = CartRepo.CheckoutCartItemsViaDirectCardPayIn(Authorization.Split(' ')[1], _cart, cardId, "Mobile");
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
        [HttpGet("MinimumCartAmount")]
        public IActionResult GetMinimumCartAmount()
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
                return Ok(CartRepo.GetMinimumCartAmount());
            }
        }

        [HttpGet("ValidateCartItems/{_shop}/{_address}/{_dType}")]
        public IActionResult ValidateCartItems(Guid _shop, int _address, OrderDeliveryType _dType)
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
                return Ok(CartRepo.ValidateCartItems(_shop, _address, _dType));
            }
        }
    }
}