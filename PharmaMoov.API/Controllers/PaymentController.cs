using MangoPay.SDK.Entities.GET;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PharmaMoov.API.DataAccessLayer;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using PharmaMoov.Models.Cart;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
//using RestSharp;
//using MangoPay.SDK;
//using MangoPay.SDK.Entities.POST;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Payment")]
    public class PaymentController : APIBaseController
    {
        IPaymentRepository PaymentRepo { get; }
        readonly APIDBContext _context;
        ILoggerManager LogManager { get;}
        IMainHttpClient MainHttpClient { get; }
        APIConfigurationManager MConf { get; }

        private readonly IConfiguration _configuration;

        public PaymentController(
            APIDBContext _dbCtxt,
            IMainHttpClient _mhttpc,
            APIConfigurationManager _conf,
            ILoggerManager _logManager,
            IPaymentRepository _paymentRepo,
            IConfiguration configuration)
        {
            PaymentRepo = _paymentRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
            LogManager = _logManager;
            _configuration = configuration;
            _context = _dbCtxt;
        }


        [HttpPost("GetAllPayments")]
        public IActionResult GetAllPayments([FromBody] PaymentListParamModel paymentListParamModel)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PaymentRepo.GetAllPayments(paymentListParamModel);
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

        [HttpGet("GetPaymentInvoice/{orderId}")]
        public IActionResult GetPaymentInvoice(int orderId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PaymentRepo.GetPaymentInvoice(orderId);
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

        //[HttpPost("WebPayin/{orderId}")]
        //public IActionResult WebPayin(int orderId, [FromHeader] string Authorization, [FromBody] PayInCardWebPostDTO model)
        //{
        //    var apiResponse = new APIResponse();
        //    try
        //    {
        //        MangoPayApi api = new MangoPayApi();
        //        api.Config.ClientId = MConf.PaymentConfig.ClientId;
        //        api.Config.ClientPassword = MConf.PaymentConfig.ApiKey;
        //        api.Config.BaseUrl = MConf.PaymentConfig.BaseUrl;

        //        // payments will go to wallet id from ph1 implementation
        //        if(model.AuthorId == null || model.AuthorId.Trim() == string.Empty)
        //        {
        //            model.AuthorId = MConf.PaymentConfig.CreditedId;
        //            model.CreditedUserId = MConf.PaymentConfig.CreditedId;
        //            model.CreditedWalletId = MConf.PaymentConfig.WalletId;
        //        }

        //        var paymentDetails = api.PayIns.CreateCardWeb(model);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
        //        apiResponse.Payload = paymentDetails;

        //        // create db record if payment page is created
        //        if(paymentDetails.Status == MangoPay.SDK.Core.Enumerations.TransactionStatus.CREATED)
        //        {
        //            PaymentRepo.CreateWebPayment(orderId, model, JsonConvert.SerializeObject(paymentDetails), Authorization.Split(' ')[1]);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LogManager.LogError("Erreur: WebPayin");
        //        LogManager.LogDebugObject(e);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
        //        apiResponse.Message = "La nouvelle adresse a été ajoutée avec succès.";
        //    }

        //    return Ok(apiResponse);
        //}

        //[HttpPost("DirectPayin")]
        //public IActionResult DirectPayin([FromBody] PayInCardDirectPostDTO model)
        //{
        //    var apiResponse = new APIResponse();
        //    try
        //    {
        //        MangoPayApi api = new MangoPayApi();
        //        api.Config.ClientId = MConf.PaymentConfig.ClientId;
        //        api.Config.ClientPassword = MConf.PaymentConfig.ApiKey;
        //        api.Config.BaseUrl = MConf.PaymentConfig.BaseUrl;

        //        // payments will go to wallet id from ph1 implementation
        //        if (model.AuthorId == null || model.AuthorId.Trim() == string.Empty)
        //        {
        //            model.AuthorId = MConf.PaymentConfig.CreditedId;
        //            model.CreditedUserId = MConf.PaymentConfig.CreditedId;
        //            model.CreditedWalletId = MConf.PaymentConfig.WalletId;
        //        }

        //        var paymentDetails = api.PayIns.CreateCardDirect(model);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
        //        apiResponse.Payload = paymentDetails;
        //    }
        //    catch (Exception e)
        //    {
        //        LogManager.LogError("Erreur: DirectPayin");
        //        LogManager.LogDebugObject(e);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
        //        apiResponse.Message = "La nouvelle adresse a été ajoutée avec succès.";
        //    }

        //    return Ok(apiResponse);
        //}

        [AllowAnonymous]
        [HttpGet("WebPaymentHook")]
        public IActionResult WebPaymentHook([FromQuery] string RessourceId, [FromQuery] string EventType, [FromQuery] string Date)
        {
            if (EventType == "TRANSFER_NORMAL_CREATED" || EventType == "TRANSFER_NORMAL_SUCCEEDED" || EventType == "TRANSFER_NORMAL_FAILED")
            {
                LogManager.LogInfo("Recieved webhook request from Mangopay with event_type : " + EventType.ToString());
                LogManager.LogInfo(" -- Return ok to webhook and ignore process -- ");
                return Ok();
            }
            LogManager.LogInfo("WebPaymentHook Called!");
            LogManager.LogInfo("RessourceId:" + RessourceId + " EventType:" + EventType + " Date:" + Date);
            PaymentRepo.UpdateOrderPaymentStatus(RessourceId, EventType, Date);
            return Ok();
        }

        [HttpPost("AddCard")]
        public IActionResult AddCard([FromBody] Card _card)
        {
            APIResponse aResp = new APIResponse();
            if (ModelState.IsValid)
            {
                aResp = PaymentRepo.AddCard(_card);
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

        [HttpGet("GetAllCards/{_UserId}")]
        public IActionResult GetAllCards(Guid _UserId)
        {
            APIResponse aResp = new APIResponse();
            if (ModelState.IsValid)
            {
                aResp = PaymentRepo.GetAllCards(_UserId);
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

        [HttpPost("DeactivateCard")]
        public IActionResult DeactivateCard([FromBody] CardDTO _card)
        {
            APIResponse aResp = new APIResponse();
            if (ModelState.IsValid)
            {
                aResp = PaymentRepo.DeactivateCard(_card);
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
        [HttpGet("GetPaymentStatus")]
        public IActionResult GetPaymentStatus(string transactionId)
        {
            return Ok(PaymentRepo.GetPaymentStatus(transactionId));
        }
        //[HttpPost("CreateCheckoutSession")]
        //[AllowAnonymous]
        //public IActionResult CreateCheckoutSession()
        //{
        //    try
        //    {
        //        var lineItems = new List<SessionLineItemOptions>
        //        {
        //            new SessionLineItemOptions
        //            {
        //                PriceData = new SessionLineItemPriceDataOptions
        //                {
        //                    UnitAmount = 1500,
        //                    Currency = "eur",
        //                    ProductData = new SessionLineItemPriceDataProductDataOptions
        //                    {
        //                        Name = "PharmaMoov Order"
        //                    }
        //                },
        //                Quantity = 1
        //            }
        //        };

        //        var options = new SessionCreateOptions
        //        {
        //            PaymentMethodTypes = new List<string> { "card" },
        //            LineItems = lineItems,
        //            Mode = "payment",
        //            SuccessUrl = _configuration["Stripe:SuccessUrl"] + "?session_id={CHECKOUT_SESSION_ID}",
        //            CancelUrl = _configuration["Stripe:CancelUrl"]
        //        };

        //        var service = new SessionService();
        //        Session session = service.Create(options);

        //        return Ok(new APIResponse
        //        {
        //            StatusCode = System.Net.HttpStatusCode.OK,
        //            Payload = session.Url
        //        });
        //    }
        //    catch (Exception ex)
        //        {
        //        LogManager.LogError("Stripe Checkout Session Error: " + ex.Message);
        //        return BadRequest(new APIResponse
        //        {
        //            StatusCode = System.Net.HttpStatusCode.BadRequest,
        //            Message = "Erreur lors de la création de la session Stripe."
        //        });
        //    }
        //}
        [HttpPost("CreateCheckoutSession")]
        [AllowAnonymous]
        public IActionResult CreateCheckoutSession([FromBody] StripeCheckoutRequest model)
        {
            try
            {
                if (model == null || model.CartItems == null || !model.CartItems.Any())
                {
                    return BadRequest(new APIResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Message = "Le panier est vide ou invalide."
                    });
                }

                var productIds = model.CartItems.Select(ci => ci.ProductRecordId).ToList();

                // 🟢 Fetch product prices and names from DB
                var products = _context.Products
                    .Where(p => productIds.Contains(p.ProductRecordId))
                    .ToDictionary(
                        p => p.ProductRecordId,
                        p => (Price: p.SalePrice > 0 ? p.SalePrice : p.ProductPrice, Name: p.ProductName)
                    );

                var lineItems = new List<SessionLineItemOptions>();

                foreach (var item in model.CartItems)
                {
                    if (!products.TryGetValue(item.ProductRecordId, out var productInfo))
                    {
                        return BadRequest(new APIResponse
                        {
                            StatusCode = System.Net.HttpStatusCode.BadRequest,
                            Message = $"Le produit avec l'ID {item.ProductRecordId} est introuvable."
                        });
                    }

                    lineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(productInfo.Price * 100), // Stripe expects amount in cents
                            Currency = "eur",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = productInfo.Name
                            }
                        },
                        Quantity = (long)item.ProductQuantity
                    });
                }

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = lineItems,
                    Mode = "payment",
                    SuccessUrl = _configuration["Stripe:SuccessUrl"] + "?session_id={CHECKOUT_SESSION_ID}",
                    CancelUrl = _configuration["Stripe:CancelUrl"]
                };

                var service = new SessionService();
                var session = service.Create(options);

                return Ok(new APIResponse
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Payload = session.Url
                });
            }
            catch (Exception ex)
            {
                LogManager.LogError("Stripe Checkout Session Error: " + ex.Message);
                return BadRequest(new APIResponse
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Message = "Erreur lors de la création de la session Stripe."
                });
            }
        }

    }
}