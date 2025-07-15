//using MangoPay.SDK.Entities.POST;

using MangoPay.SDK.Entities.GET;
using MangoPay.SDK.Entities.POST;
using PharmaMoov.Models;
using PharmaMoov.Models.Shop;
using PharmaMoov.Models.User;
using System;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IPaymentRepository
    {
        //Payment CreateWebPayment(int iOrderId, PayInCardWebPostDTO iPaymentModel, string iCreatePayload, string iAuthToken);
        void UpdateOrderPaymentStatus(string RessourceId, string EventType, string Date);
        APIResponse CreateUserNaturalUserInMangoPay(User userInfo);
        APIResponse CreateUserNaturalCourierInMangoPay(User userInfo);
        APIResponse CreateUserLegalUserInMangoPay(Shop shopInfo);
        APIResponse GetAllPayments(PaymentListParamModel paymentListParamModel);
        APIResponse GetPaymentInvoice(int orderId);
        Payment CreateCardPayment(int iOrderId, PayInCardDirectDTO iPaymentModel, string iCreatePayload, string iAuthToken);
        APIResponse AddCard(Card _card);
        APIResponse GetAllCards(Guid _UserId);
        APIResponse DeactivateCard(CardDTO _card);
        PayInCardDirectPostDTO BuildCardDirectPaymentModel(decimal amount, int OrderId, UserAddresses uAddress, string cardId, User uProfile, string accessType);
        TransferPostDTO BuildFundTransferModel(decimal amount, int OrderId, MangoPayUser mPayUser, MangoPayShop mPayShop);
        TransferPostDTO BuildFundDeliveryModel(decimal amount, int OrderId, MangoPayShop mPayUser, MangoPayUser mPayDelivery);
        APIResponse GetPaymentStatus(string transactionId);
    }
}
