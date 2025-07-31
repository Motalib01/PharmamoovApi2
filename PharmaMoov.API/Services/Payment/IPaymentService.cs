using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe.Checkout;

namespace PharmaMoov.API.Services.Payment
{
    public interface IPaymentService
    {
        Task<string> CreateCheckoutSessionAsync(string customerEmail, decimal amount);
    }


    public class StripePaymentService : IPaymentService
    {
        public async Task<string> CreateCheckoutSessionAsync(string customerEmail, decimal amount)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" }, // Visa, MasterCard
                Mode = "payment",
                CustomerEmail = customerEmail,
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(amount * 100), // cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Product Purchase"
                            }
                        },
                        Quantity = 1
                    }
                },
                SuccessUrl = "http://localhost:63523/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "http://localhost:63523/cancel"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return session.Url;
        }
    }

}