using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.Models;
using PharmaMoov.Models.DeliveryUser;
using System;
using System.Threading.Tasks;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/DeliveryUser")]
    public class DeliveryUserController : ControllerBase
    {
        IDeliveryUserRepository DeliveryUserRepo { get; }
        public DeliveryUserController(IDeliveryUserRepository _deliveryUserRepo)
        {
            DeliveryUserRepo = _deliveryUserRepo;
        }

        [HttpPost("UpdateLocation")]
        public IActionResult UpdateLocation([FromHeader] string Authorization, [FromBody] UpdateLocationParamModel model)
        {
            APIResponse aResp = new APIResponse();

            if (ModelState.IsValid)
            {
                aResp = DeliveryUserRepo.UpdateLocation(Authorization.Split(' ')[1], model);
                if (aResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(aResp);
                }
                return BadRequest(aResp);
            }
            aResp = new APIResponse
            {
                Message = "Model Objet Invalid",
                Status = "Model Erreur!",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                ModelError = ModelState.Errors()
            };
            return BadRequest(aResp);
        }

        [HttpGet("GetNewOrderList/{deliveryUserId}/{pageNo}")]
        public IActionResult GetNewOrderList([FromHeader] string Authorization, Guid deliveryUserId,int pageNo)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = DeliveryUserRepo.GetNewOrderList(Authorization.Split(' ')[1], deliveryUserId, pageNo);
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

        [HttpGet("GetOrderDetail/{orderId}")]
        public IActionResult GetOrderDetail(int orderId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = DeliveryUserRepo.GetOrderDetail(orderId);
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

        [HttpPost("AcceptOrder")]
        public async Task<IActionResult> AcceptOrder([FromBody] AcceptOrderParamModel model)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = await DeliveryUserRepo.AcceptOrder(model);
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

        [HttpPut("UpdateOrderStatus")]
        public IActionResult UpdateOrderStatus([FromBody]UpdateOrderStatusParamModel model)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = DeliveryUserRepo.UpdateOrderStatus(model);
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

        [HttpPut("UpdateReceiveOrder")]
        public IActionResult UpdateReceiveOrder([FromBody] UpdateReceiveOrderParamModel model)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = DeliveryUserRepo.UpdateReceiveOrder(model);
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
        
        [HttpGet("GetOrdersList")]
        public IActionResult GetOrdersList([FromQuery] OrderListParamModel model)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = DeliveryUserRepo.GetOrdersList(model);
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

        [HttpGet("GetAcceptedOrderDetail/{deliveryUserId}")]
        public IActionResult GetAcceptedOrderDetail([FromHeader] string Authorization, Guid deliveryUserId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = DeliveryUserRepo.GetAcceptedOrderDetail(Authorization.Split(' ')[1], deliveryUserId);
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
        
        [HttpGet("CheckAcceptedOrder/{deliveryUserId}")]
        public IActionResult CheckAcceptedOrder(Guid deliveryUserId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = DeliveryUserRepo.CheckAcceptedOrder(deliveryUserId);
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

    }
}
