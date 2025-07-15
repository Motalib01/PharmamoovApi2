using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using PharmaMoov.Models.Orders;
using System.Threading.Tasks;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Order")]
    public class OrderController : APIBaseController
    {
        IOrderRepository OrderRepo { get; }
        private IMainHttpClient MainHttpClient { get; }
        private APIConfigurationManager MConf { get; }

        public OrderController(IOrderRepository _aRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf)
        {
            OrderRepo = _aRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
        }

        [HttpGet("UserOrderHistory")]
        public IActionResult GetUserOrderHistory([FromHeader] string Authorization)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = OrderRepo.GetUserOrderHistory(Authorization.Split(' ')[1]);
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

        [HttpGet("UserOrderDetails/{_order}")]
        public IActionResult GetUserOrderDetails([FromHeader] string Authorization, int _order)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = OrderRepo.GetUserOrderDetails(Authorization.Split(' ')[1], _order);
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

        [HttpGet("ShopsOrders/{_shop}/{_takeHistory}")]
        public IActionResult GetShopsOrders(Guid _shop, bool _takeHistory)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = OrderRepo.GetShopsOrders(_shop, _takeHistory);
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

        [HttpPost("ChangeOrderStatus")]
        public async Task<IActionResult> ChangeOrderStatus([FromBody] ChangeOrderStatus _order)
        {
            APIResponse aResp = new APIResponse();
            if (ModelState.IsValid)
            {
                aResp = await OrderRepo.ChangeOrderStatus(_order);
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

        [HttpGet("CancelOrder/{_orderId}")]
        public IActionResult CancelOrder([FromHeader] string Authorization, int _orderId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = OrderRepo.CancelOrder(Authorization.Split(' ')[1], _orderId);
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

        [HttpPost("UpdateOrderStatus")]
        public IActionResult UpdateOrderStatus([FromBody] ChangeOrderStatus order)
        {
            APIResponse aResp = new APIResponse();
            if (ModelState.IsValid)
            {
                aResp = OrderRepo.UpdateOrderStatus(order);
                if (aResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(aResp);
                }
                return BadRequest(aResp);
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
        [HttpGet("TestPushNotifcation")]
        public async Task<IActionResult> TestPushNotifcation(string fcmToken)
        {
            APIResponse aResp = await OrderRepo.TestPushNotifcation(fcmToken);
            return Ok(aResp);
        }

    }
}