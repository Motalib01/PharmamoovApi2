using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PharmaMoov.API.Services.Abstractions;
using PharmaMoov.API.Services.CartExternal;
using PharmaMoov.Models.Cart;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PharmaMoov.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedipimPaymentController : ControllerBase
    {
        private readonly IMedipimService _medipimService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MedipimPaymentController> _logger;

        public MedipimPaymentController(IMedipimService medipimService, IConfiguration config, ILogger<MedipimPaymentController> logger)
        {
            _medipimService = medipimService;
            _configuration = config;
            _logger = logger;
        }

        [HttpPost("CreateCheckoutSession")]
        [Consumes("application/json", "application/json-patch+json")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] StripeCheckoutRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || request.PatientId == Guid.Empty)
            {
                _logger.LogWarning("Email or PatientId is missing.");
                return BadRequest("Email and PatientId are required.");
            }

            var sessionKey = $"external-cart-{request.PatientId}";
            var cartItems = HttpContext.Session.GetObject<List<ExternalCartItem>>(sessionKey);

            if (cartItems == null || !cartItems.Any())
            {
                _logger.LogWarning("Cart is empty for PatientId {PatientId}", request.PatientId);
                return BadRequest("Cart is empty.");
            }

            var lineItems = new List<SessionLineItemOptions>();

            foreach (var item in cartItems)
            {
                if (string.IsNullOrWhiteSpace(item.MedipimProductId) || item.Quantity <= 0)
                {
                    _logger.LogWarning("Invalid cart item: {@Item}", item);
                    continue;
                }

                var product = await _medipimService.GetProductByIdAsync(item.MedipimProductId);
                if (product == null)
                {
                    _logger.LogWarning("Product not found: {ProductId}", item.MedipimProductId);
                    continue;
                }

                var price = product.PublicPrice ?? product.PharmacistPrice ?? 0;
                if (price <= 0)
                {
                    _logger.LogWarning("Invalid product price: {ProductId}", item.MedipimProductId);
                    continue;
                }

                lineItems.Add(new SessionLineItemOptions
                {
                    Quantity = (long)item.Quantity ,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur",
                        UnitAmount = (long)(price * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = product.Name?.GetValueOrDefault("fr") ??
                                   product.Name?.GetValueOrDefault("en") ??
                                   "Medipim Product"
                        }
                    }
                });
            }

            if (!lineItems.Any())
            {
                _logger.LogWarning("No valid line items for Stripe session.");
                return BadRequest("Aucun produit valide trouvé dans le panier.");
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                CustomerEmail = request.Email,
                Mode = "payment",
                LineItems = lineItems,
                SuccessUrl = "http://localhost:63523/success",
                CancelUrl = "http://localhost:63523/cancel"
            };

            try
            {
                var service = new SessionService();
                var session = await service.CreateAsync(options);

                return Ok(new { checkoutUrl = session.Url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating Stripe session.");
                return StatusCode(500, "Erreur lors de la création de la session de paiement.");
            }
        }


    }

    public class StripeCheckoutRequest
    {
        public string Email { get; set; }
        public Guid PatientId { get; set; }
    }


    public class PaymentRequestDto
    {
        public string Email { get; set; }
        public decimal Amount { get; set; }
    }
}
