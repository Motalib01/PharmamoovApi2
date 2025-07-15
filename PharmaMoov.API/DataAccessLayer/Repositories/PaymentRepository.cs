using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
//using MangoPay.SDK.Entities.POST;
using PharmaMoov.Models.Orders;
using Newtonsoft.Json;
//using MangoPay.SDK.Entities.GET;
using PharmaMoov.Models.Cart;
using PharmaMoov.Models.Shop;
using MangoPay.SDK.Entities.GET;
using MangoPay.SDK;
using MangoPay.SDK.Core.Enumerations;
using MangoPay.SDK.Entities;
using MangoPay.SDK.Entities.POST;
using PharmaMoov.Models.User;
using System.Net.Http;
using System.Net.Http.Headers;
using MangoPay.SDK.Entities.PUT;
using System.Text.RegularExpressions;
using PharmaMoov.Models.DeliveryUser;
using System.Threading;
using PharmaMoov.PushNotification;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class PaymentRepository : APIBaseRepo, IPaymentRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private IMainHttpClient MainHttpClient { get; }
        public PaymentRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, IMainHttpClient _mhttpc)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            MainHttpClient = _mhttpc;
        }

        //public Payment CreateWebPayment(int iOrderId, PayInCardWebPostDTO iPaymentModel, string iCreatePayload, string iAuthToken)
        //{
        //    Payment paymentModel = null;
        //    try
        //    {
        //        var loginTrx = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(x => x.Token == iAuthToken);
        //        if(loginTrx != null)
        //        {
        //            paymentModel = new Payment();
        //            paymentModel.OrderId = iOrderId;
        //            paymentModel.Status = EPaymentStatus.CREATED;
        //            paymentModel.Tag = iPaymentModel.Tag;
        //            paymentModel.AuthorId = iPaymentModel.AuthorId;
        //            paymentModel.DebitedFundsCurrency = iPaymentModel.DebitedFunds.Currency.ToString();
        //            paymentModel.DebitedFundsAmount = iPaymentModel.DebitedFunds.Amount;
        //            paymentModel.FeesCurrency = iPaymentModel.Fees.Currency.ToString();
        //            paymentModel.FeesAmount = iPaymentModel.Fees.Amount;
        //            paymentModel.CreditedWalletId = iPaymentModel.CreditedWalletId;
        //            paymentModel.ReturnURL = iPaymentModel.ReturnURL;
        //            paymentModel.Culture = iPaymentModel.Culture.ToString();
        //            paymentModel.CardType = iPaymentModel.CardType.ToString();
        //            paymentModel.SecureMode = iPaymentModel.SecureMode.ToString();
        //            paymentModel.CreditedUserId = iPaymentModel.CreditedUserId;
        //            paymentModel.StatementDescriptor = iPaymentModel.StatementDescriptor;
        //            paymentModel.CreatePayload = iCreatePayload; 

        //            paymentModel.CreatedBy = loginTrx.UserId;
        //            paymentModel.CreatedDate = DateTime.Now;

        //            DbContext.Payments.Add(paymentModel);
        //            DbContext.SaveChanges();
        //        }
        //        else
        //        {
        //            LogManager.LogError("Erreur: CreateWebPayment - Login transaction not found");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LogManager.LogError("Erreur: CreateWebPayment");
        //        LogManager.LogDebugObject(e);
        //    }

        //    return paymentModel;
        //}

        public void UpdateOrderPaymentStatus(string RessourceId, string EventType, string Date)
        {
            LogManager.LogInfo("UpdateOrderPaymentStatus: resourceID:" + RessourceId + " EventyType: " + EventType + " Date: " + Date);
            try
            {
                Payment recordedPayment = DbContext.Payments.FirstOrDefault(p => p.CreatePayload.Contains(RessourceId));
                if (recordedPayment == null)
                {
                    // the call is for a refund
                    // get the record from paymenttransaction table
                    PaymentTransaction payTransactionForRefund = null;
                    int attempts = 0;
                    while (payTransactionForRefund == null)
                    {
                        attempts++;
                        LogManager.LogInfo("Looking for payment transaction with Resource ID : " + RessourceId + " Attempt:" + attempts.ToString());
                        payTransactionForRefund = DbContext.PaymentTransactions.FirstOrDefault(pt => pt.PaymentToken.Contains(RessourceId));
                        Thread.Sleep(500);
                        if (attempts >= 50)
                        {
                            LogManager.LogInfo("Transaction must be from other soures : " + RessourceId + " Attempt:" + attempts.ToString() + " exiting this tread");
                            return;
                        }
                    }

                    if (payTransactionForRefund != null)
                    {
                        // get the actual payment transaction record
                        RefundDTO refundRecord = JsonConvert.DeserializeObject<RefundDTO>(payTransactionForRefund.PaymentToken);
                        recordedPayment = DbContext.Payments.FirstOrDefault(p => p.CreatePayload.Contains(refundRecord.InitialTransactionId));
                    }
                    else
                    {
                        LogManager.LogError("Invalid Resource ID from WebHook CAll :" + RessourceId);
                        return;
                    }
                }

                if (recordedPayment != null)
                {
                    LogManager.LogInfo("Found Payment Record:");
                    LogManager.LogDebugObject(recordedPayment);
                    Order getTheOrder = DbContext.Orders.FirstOrDefault(o => o.OrderID == recordedPayment.OrderId);
                    if (getTheOrder != null)
                    {
                        LogManager.LogInfo("Found Order Record:");
                        LogManager.LogDebugObject(getTheOrder);
                        recordedPayment.StatusMessage = "EVENT_TYPE: " + EventType;
                        if (EventType == "PAYIN_NORMAL_SUCCEEDED")
                        {
                            LogManager.LogInfo("Eventy Type is:" + EventType);
                            getTheOrder.OrderPaymentStatus = OrderPaymentStatus.PAID;
                            getTheOrder.OrderProgressStatus = OrderProgressStatus.PLACED;
                            getTheOrder.LastEditedDate = DateTime.Now;
                            recordedPayment.Status = EPaymentStatus.SUCCEEDED;
                            recordedPayment.LastEditedDate = DateTime.Now;
                            recordedPayment.CreatePayload = GetPaymentFullPayLoad(recordedPayment.CreditedUserId, RessourceId);

                            // payment is success delete the cart items
                            List<CartItem> UserCartItems = DbContext.CartItems.Where(ci => ci.UserId == getTheOrder.UserId).ToList();
                            DbContext.RemoveRange(UserCartItems);

                            // payment succeeded lets log the promo code in the order
                            //if (getTheOrder.PromoCode != null && getTheOrder.PromoCode != "")
                            //{
                            //    PromoCodeUsed codeUse = PromoRepository.UseCode(getTheOrder);
                            //    DbContext.Add(codeUse);
                            //}

                            DbContext.Payments.Update(recordedPayment);
                            DbContext.Orders.Update(getTheOrder);
                            DbContext.SaveChanges();

                            //send push notification to customer
                            LogManager.LogInfo("Calling SendPushForOrderStatus (Succès Payment) with userId:" + getTheOrder.UserId.ToString() + " and orderid: " + getTheOrder.OrderID);
                            SendPushForOrderStatus(getTheOrder.UserId, getTheOrder.OrderID, true);

                            //send email to shop and chip chap
                            LogManager.LogInfo("Calling SendOrderDetailsToShop (Succès Payment) with shopid:" + getTheOrder.ShopId.ToString() + " and orderid: " + getTheOrder.OrderID);
                            SendOrderDetailsToShop(getTheOrder.ShopId, getTheOrder.OrderID);

                            //send email when order is placed during closed shop hours
                            LogManager.LogInfo("Calling CheckForClosedShop (Succès Payment) with shopid:" + getTheOrder.ShopId.ToString() + " and orderid: " + getTheOrder.OrderID);
                            CheckForClosedShop(getTheOrder.UserId, getTheOrder.ShopId, getTheOrder.OrderID, getTheOrder.OrderDeliveryType);
                        }
                        else if (EventType == "PAYIN_REFUND_SUCCEEDED")
                        {
                            LogManager.LogInfo("Eventy Type is:" + EventType);
                            getTheOrder.LastEditedDate = DateTime.Now;
                            recordedPayment.LastEditedDate = DateTime.Now;

                            getTheOrder.OrderPaymentStatus = OrderPaymentStatus.REFUND;
                            recordedPayment.Status = EPaymentStatus.REFUNDED;
                            recordedPayment.CreatePayload = GetPaymentFullPayLoad(recordedPayment.CreditedUserId, RessourceId);
                            //if (getTheOrder.OrderProgressStatus == OrderProgressStatus.REFUNDED)
                            //{
                            //    getTheOrder.OrderRefundNo = GenerateRefundNo(getTheOrder.ShopId);
                            //    getTheOrder.OrderShopRefundNo = getTheOrder.OrderRefundNo.Replace("S", "");
                            //}

                            DbContext.Payments.Update(recordedPayment);
                            DbContext.Orders.Update(getTheOrder);
                            DbContext.SaveChanges();

                            //send refund invoice
                            //if (getTheOrder.OrderProgressStatus == OrderProgressStatus.REFUNDED)
                            //{
                            //    LogManager.LogInfo("Calling SendRefundInvoiceLink (Refund Payment) with userId:" + getTheOrder.UserId.ToString() + " and orderrefid: " + getTheOrder.OrderReferenceID);
                            //    SendRefundInvoiceLink(getTheOrder.UserId, getTheOrder.OrderReferenceID, getTheOrder.ShopId);
                            //}
                            //else
                            //{
                                LogManager.LogInfo("Calling SendCancelledOrder (Refund Payment) with userId:" + getTheOrder.UserId.ToString() + " and orderrefid: " + getTheOrder.OrderReferenceID);
                                SendCancelledOrder(getTheOrder.UserId, getTheOrder.OrderReferenceID);
                            //}

                            //send push notification
                            LogManager.LogInfo("Calling SendPushForOrderStatus (Refund Payment) with userId:" + getTheOrder.UserId.ToString() + " and orderid: " + getTheOrder.OrderID);
                            SendPushForOrderStatus(getTheOrder.UserId, getTheOrder.OrderID, null);

                            //send email when order is placed during closed shop hours
                            CheckForClosedShop(getTheOrder.UserId, getTheOrder.ShopId, getTheOrder.OrderID, getTheOrder.OrderDeliveryType);
                        }
                        else if (EventType == "PAYIN_NORMAL_FAILED")
                        {
                            LogManager.LogInfo("Eventy Type is:" + EventType);
                            getTheOrder.OrderPaymentStatus = OrderPaymentStatus.ERROR;
                            getTheOrder.OrderProgressStatus = OrderProgressStatus.PENDING;
                            getTheOrder.LastEditedDate = DateTime.Now;
                            recordedPayment.Status = EPaymentStatus.FAILED;
                            recordedPayment.LastEditedDate = DateTime.Now;
                            recordedPayment.CreatePayload = GetPaymentFullPayLoad(recordedPayment.CreditedUserId, RessourceId);

                            DbContext.Payments.Update(recordedPayment);
                            DbContext.Orders.Update(getTheOrder);
                            DbContext.SaveChanges();

                            //send push notification
                            LogManager.LogInfo("Calling SendPushForOrderStatus (Échec Payment) with userId:" + getTheOrder.UserId.ToString() + " and orderid: " + getTheOrder.OrderID);
                            SendPushForOrderStatus(getTheOrder.UserId, getTheOrder.OrderID, false);
                        }

                        else
                        {
                            LogManager.LogInfo("Eventy Type is:" + EventType + " IF all ELSE fails, this is executed.");
                            getTheOrder.OrderPaymentStatus = OrderPaymentStatus.ERROR;
                            getTheOrder.OrderProgressStatus = OrderProgressStatus.PENDING;
                            getTheOrder.LastEditedDate = DateTime.Now;
                            recordedPayment.Status = EPaymentStatus.NotSpecified;
                            recordedPayment.LastEditedDate = DateTime.Now;

                            DbContext.Payments.Update(recordedPayment);
                            DbContext.Orders.Update(getTheOrder);
                            DbContext.SaveChanges();

                            //send push notification
                            LogManager.LogInfo("Calling SendPushForOrderStatus (Échec Payment) with userId:" + getTheOrder.UserId.ToString() + " and orderid: " + getTheOrder.OrderID);
                            SendPushForOrderStatus(getTheOrder.UserId, getTheOrder.OrderID, false);
                        }
                    }
                    else
                    {
                        LogManager.LogInfo("Invalid OrderId:" + recordedPayment.OrderId.ToString() + " No Order with that ID!");
                    }
                }
                else
                {
                    LogManager.LogInfo("Invalid RessourceID:" + RessourceId + " No payment record with that ID!");
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("UpdateOrderPaymentStatus Erreur! resourceID: " + RessourceId + " EventyType: " + EventType + " Date: " + Date);
                LogManager.LogDebugObject(ex);
            }
        }

        public string GenerateOrderNo()
        {
            LogManager.LogInfo("-- START GenerateOrderNo");
            string OrderNo = string.Empty;

            var lastOrderNo = DbContext.Orders.AsNoTracking().Where(x => x.OrderProgressStatus != OrderProgressStatus.PENDING && x.OrderReferenceID != null)
                .OrderByDescending(x => x.OrderReferenceID).FirstOrDefault();

            if (lastOrderNo == null)
            {
                var firstOrderNo = 1;
                OrderNo = firstOrderNo.ToString("D" + 6);
            }
            else
            {
                var getOrderNo = lastOrderNo.OrderReferenceID.Substring(lastOrderNo.OrderReferenceID.Length - 6);
                int convertedNo = Int32.Parse(getOrderNo);
                convertedNo++;
                OrderNo = convertedNo.ToString("D" + 6);
            }

            LogManager.LogInfo("-- END GenerateOrderNo >> " + OrderNo);
            return OrderNo;
        }

        public string GetPaymentFullPayLoad(string MangoPayUserID, string TransactioNId)
        {
            MangoPayApi api = new MangoPayApi();
            api.Config.ClientId = APIConfig.PaymentConfig.ClientId;
            api.Config.ClientPassword = APIConfig.PaymentConfig.ApiKey;
            api.Config.BaseUrl = APIConfig.PaymentConfig.BaseUrl;

            //Pagination paging = new Pagination
            //{
            //    ItemsPerPage = 100,
            //    Page = 1,
            //};
            //ListPaginated<PayInDTO> listOfTrans =  api.PayIns.Get(TransactioNId);
            //for (int currPage = 1; currPage <= listOfTrans.TotalPages; currPage++)
            //{
            //    TransactionDTO foundTrans = listOfTrans.FirstOrDefault(lt => lt.Id == TransactioNId);
            //    if (foundTrans != null)
            //    {
            //        return JsonConvert.SerializeObject(foundTrans);
            //    }
            //    paging.Page = currPage;
            //    listOfTrans = api.Users.GetTransactions(MangoPayUserID, paging, new MangoPay.SDK.Core.FilterTransactions { Type = TransactionType.PAYIN }, null);
            //}
            return JsonConvert.SerializeObject(api.PayIns.Get(TransactioNId));

        }

        public void SendOrderDetailsToShop(Guid _shop, int _order, IMainHttpClient _mhttpc = null)
        {
            LogManager.LogInfo("SendOrderDetailsToShop >> Start" + _shop);

            Shop FoundShop = DbContext.Shops.AsNoTracking().FirstOrDefault(s => s.ShopId == _shop);

            var emailMessage = String.Format(_GenerateOrderPlacedTemplate(_order, FoundShop.ShopName, true));
            var EmailParam = APIConfig.MailConfig;
            EmailParam.To = new List<string>() { FoundShop.Email, APIConfig.MailConfig.Username };
            EmailParam.Subject = "Nouvelle commande Chip Chap";
            EmailParam.Body = emailMessage;
            var RetStatus = base.SendEmailAsync(EmailParam, LogManager);

            if (RetStatus.IsFaulted == true)
            {
                LogManager.LogInfo("SendOrderDetailsToShop");
                LogManager.LogError("Sending Échec >> " + _shop);
            }
        }

        private string _GenerateOrderPlacedTemplate(int _order, string _recipientName, bool _forShop = false, bool IsClosedShop = false)
        {
            var template = APIConfig.MsgConfigs.OrderPlaced.ToString();
            if (_forShop == true && IsClosedShop == false)
            {
                template = APIConfig.MsgConfigs.OrderPlacedForShop.ToString();
            }

            if (_forShop == false && IsClosedShop == true)
            {
                template = APIConfig.MsgConfigs.ClosedShop.ToString();
            }

            List<NewOrderItems> orderItemsLists = new List<NewOrderItems>();
            orderItemsLists = GetOrderItems(_order);

            var itemTemplate = string.Empty;
            var itemRow = string.Empty;
            var itemCount = orderItemsLists.Count();

            decimal totalProductPrice = 0;
            decimal totalProductTax = 0;
            for (var i = 0; i < itemCount; i++)
            {
                var item = orderItemsLists.ElementAt(i);
                decimal taxValue = (item.ProductTaxValue / 100) + 1;
                decimal totalPrice = (item.ProductPrice * item.ProductQuantity) / taxValue;

                itemRow = string.Empty;
                itemRow = "<tr>";
                itemRow += "<td>" + item.ProductName + "</td> <td> " + (int)item.ProductQuantity + " </td> <td>"
                    + Math.Round(totalPrice, 2, MidpointRounding.AwayFromZero).ToString("F2") + "</td> <td> "
                    + Math.Round(item.ProductTaxValue, 2, MidpointRounding.AwayFromZero).ToString("F1") + "%" + "</td> <td> "
                    + Math.Round(item.ProductTaxAmount, 2, MidpointRounding.AwayFromZero).ToString("F2") + "</td> <td> "
                    + Math.Round(item.SubTotal, 2, MidpointRounding.AwayFromZero).ToString("F2") + " </td>";
                itemRow += "</tr>";
                itemTemplate += itemRow;

                totalProductPrice += Math.Round(totalPrice, 2, MidpointRounding.AwayFromZero);
                totalProductTax += Math.Round(item.ProductTaxAmount, 2, MidpointRounding.AwayFromZero);
            }

            //build content
            string deliveryType = string.Empty;
            string lblAddress = string.Empty;
            string buildAddress = string.Empty;

            Order order = DbContext.Orders.AsNoTracking().FirstOrDefault(x => x.OrderID == _order);
            UserAddresses userAddress = DbContext.UserAddresses.AsNoTracking().FirstOrDefault(a => a.UserAddressID == order.DeliveryAddressId);
            Shop shop = DbContext.Shops.AsNoTracking().FirstOrDefault(x => x.ShopId == order.ShopId);

            if (_forShop == true && IsClosedShop == false)
            {
                if (order.OrderDeliveryType == OrderDeliveryType.FORDELIVERY)
                {
                    deliveryType = "Livraison";
                    lblAddress = "Adresse de livraison : ";
                    buildAddress = userAddress.Street + ", " + userAddress.City + ", " + userAddress.PostalCode;
                }
                else
                {
                    deliveryType = "Retrait en boutique";
                    lblAddress = string.Empty;
                    buildAddress = string.Empty;
                }

                return String.Format(template,
                       _recipientName,
                       order.CustomerName,
                       TranslateDoW(order.DeliveryDate.DayOfWeek) + " " + order.DeliveryDate.Day  + " " + TranslateMonth(order.DeliveryDate) + " " + order.DeliveryDate.Year,
                       "",
                       deliveryType,
                       lblAddress,
                       buildAddress,
                       itemTemplate,
                       totalProductPrice.ToString("F2"),
                       totalProductTax.ToString("F2"),
                       (totalProductPrice + totalProductTax).ToString("F2"),
                       order.OrderDeliveryFee.ToString("F2"),
                       order.OrderPromoAmount.ToString("F2"),
                       order.OrderGrossAmount.ToString("F2"));
            }
            else
            {
                if (order.OrderDeliveryType == OrderDeliveryType.FORDELIVERY)
                {
                    deliveryType = "Livraison";
                    lblAddress = "Adresse de livraison : ";
                    buildAddress = userAddress.Street + ", " + userAddress.City + ", " + userAddress.PostalCode;
                }
                else
                {
                    deliveryType = "Retrait en boutique";
                    lblAddress = "Adresse de retrait : ";
                    buildAddress = shop.Address;
                }

                if (IsClosedShop == true)
                {
                    return String.Format(template,
                       _recipientName,
                       order.OrderReferenceID,
                       itemTemplate,
                       totalProductPrice.ToString("F2"),
                       totalProductTax.ToString("F2"),
                       (totalProductPrice + totalProductTax).ToString("F2"),
                       order.OrderDeliveryFee.ToString("F2"),
                       order.OrderPromoAmount.ToString("F2"),
                       order.OrderGrossAmount.ToString("F2"));
                }
                else
                {
                    return String.Format(template,
                       _recipientName,
                       order.OrderReferenceID,
                       deliveryType,
                       lblAddress,
                       buildAddress,
                       itemTemplate,
                       totalProductPrice.ToString("F2"),
                       totalProductTax.ToString("F2"),
                       (totalProductPrice + totalProductTax).ToString("F2"),
                       order.OrderDeliveryFee.ToString("F2"),
                       order.OrderPromoAmount.ToString("F2"),
                       order.OrderGrossAmount.ToString("F2"));
                }
            }
        }

        List<NewOrderItems> GetOrderItems(int OrderId)
        {
            List<NewOrderItems> cpList = new List<NewOrderItems>();
            //
            //get all the items in the order
            List<OrderItem> OItem = DbContext.OrderItems.Where(oit => oit.OrderID == OrderId).ToList();
            foreach (OrderItem sItem in OItem)
            {
                cpList.Add(DbContext.Products.Where(p => p.ProductRecordId == sItem.ProductRecordId)
                    .Select(sp => new NewOrderItems
                    {
                        ProductRecordId = sItem.ProductRecordId,
                        ProductName = sp.ProductName,
                        ProductUnit = sItem.ProductUnit,
                        ProductQuantity = Convert.ToDecimal(string.Format("{0:F1}", sItem.ProductQuantity)),
                        ProductPrice = Convert.ToDecimal(string.Format("{0:F2}", sItem.ProductPrice)),
                        ProductTaxValue = Convert.ToDecimal(string.Format("{0:F2}", sItem.ProductTaxValue)),
                        ProductTaxAmount = Convert.ToDecimal(string.Format("{0:F2}", sItem.ProductTaxAmount)),
                        SubTotal = Convert.ToDecimal(string.Format("{0:F2}", sItem.SubTotal)),
                    }).FirstOrDefault());
            }
            return cpList;
        }

        public void SendPushForOrderStatus(Guid _user, int _order, bool? IsPaymentSuccèsful)
        {
            var user = DbContext.Users.AsNoTracking().FirstOrDefault(x => x.UserId == _user);
            var order = DbContext.Orders.AsNoTracking().FirstOrDefault(x => x.OrderID == _order);
            var pushDesc = string.Empty;

            LogManager.LogDebugObject("Statut de la commande >>" + order.OrderProgressStatus);
            LogManager.LogDebugObject("Payment Status >>" + order.OrderPaymentStatus);

            if (order.OrderProgressStatus == OrderProgressStatus.PLACED)
            {
                pushDesc = "Votre paiement pour votre commande numéro " + order.OrderReferenceID + " a été accepté.";
            }
            else if (order.OrderProgressStatus == OrderProgressStatus.ACCEPTED)
            {
                pushDesc = "Votre commande numéro " + order.OrderReferenceID + " est confirmée par le commerçant.";
            }
            else if (order.OrderProgressStatus == OrderProgressStatus.READYFORPICKUP)
            {
                pushDesc = "Votre commande numéro " + order.OrderReferenceID + " est prête à être collectée chez votre commerçant.";
            }
            else if (order.OrderProgressStatus == OrderProgressStatus.OUTFORDELIVERY)
            {
                pushDesc = "Votre commande numéro " + order.OrderReferenceID + " a été remise au livreur et vous sera bientôt livrée.";
            }
            else if (order.OrderProgressStatus == OrderProgressStatus.CANCELORDER)
            {
                //if (order.IsCancelledBy == AccountTypes.APPUSER)
                //{
                //    pushDesc = "Votre commande numéro " + order.OrderReferenceID + " a été annulée.";
                //}
                //else
                //{
                    pushDesc = "Votre commande numéro " + order.OrderReferenceID + " a été annulée par le commerçant.";
                //}
            }
            else
            {
                pushDesc = "Votre paiement a été refusé. Merci de bien vouloir réessayer.";
            }

            PushNotificationData pnD = new PushNotificationData
            {
                UserId = order.UserId,
                Header = "Bonjour " + user.FirstName,
                Subtitle = "",
                Description = pushDesc,
                Platform = (int)user.RegistrationPlatform,
                orderId = _order,
                IsPaymentSuccessful = IsPaymentSuccèsful
            };

            NonSilentNotification nsNotif = new NonSilentNotification
            {
                sound = "",
                title = "Bonjour " + user.FirstName,
                subtitle = "",
                body = pushDesc,
                icon = "",
                android_channel_id = ""
            };

            LogManager.LogInfo("---- Push Notification for Orders ----");
            LogManager.LogDebugObject(SendPushNotification(LogManager, pnD, nsNotif, MainHttpClient as MainHttpClient, DbContext));
        }

        public void CheckForClosedShop(Guid _user, Guid _shop, int _order, OrderDeliveryType _dType)
        {
            LogManager.LogInfo("SendEmailForClosedShop to > " + _user);

            //check if shop is closed
            Shop getShop = DbContext.Shops.AsNoTracking().FirstOrDefault(x => x.ShopId == _shop);

            DateTime datePlaced = DateTime.Now;
            TimeSpan timePlaced = datePlaced.TimeOfDay;
            int getPrepTime = _dType == OrderDeliveryType.FORDELIVERY ? Convert.ToInt32(getShop.PreparationTimeForDelivery) : Convert.ToInt32(getShop.PreparationTime);
            int getHour = getPrepTime / 60;
            int getMin = getPrepTime - (getHour * 60);

            TimeSpan prepTime = new TimeSpan(0, getHour, getMin, 0, 0);
            TimeSpan totalCurrentPrepTime = timePlaced.Add(prepTime);
            TimeSpan closeTime = TimeSpan.Parse("00:00:00");

            //ShopOpeningHour shopHourPlaced = DbContext.ShopOpeningHours.AsNoTracking().FirstOrDefault(x => x.ShopId == _shop && x.DayOfWeek == datePlaced.DayOfWeek && x.DeliveryType == _dType);
            ShopOpeningHour shopHourPlaced = DbContext.ShopOpeningHours.AsNoTracking().FirstOrDefault(x => x.ShopId == _shop && x.DayOfWeek == datePlaced.DayOfWeek );

            //when shop is closed for the whole day get the next opening time of the next opening day
            if (shopHourPlaced.NowOpen == false)
            {
                SendOrderDetailsToCustomer(_user, _order, true);
            }
            else
            {
                //when shop is open
                //when current time is within AM shop hours display prep time
                if (timePlaced >= TimeSpan.Parse(shopHourPlaced.StartTimeAM) && timePlaced <= TimeSpan.Parse(shopHourPlaced.EndTimeAM) && TimeSpan.Parse(shopHourPlaced.StartTimeAM) != closeTime)
                {
                    //when no break time
                    if (TimeSpan.Parse(shopHourPlaced.EndTimeAM) != TimeSpan.Parse(shopHourPlaced.StartTimePM))
                    {
                        //evaluate if current time + preparation time is within AM Shop Hours
                        if (totalCurrentPrepTime >= TimeSpan.Parse(shopHourPlaced.StartTimeAM) && totalCurrentPrepTime <= TimeSpan.Parse(shopHourPlaced.EndTimeAM) && TimeSpan.Parse(shopHourPlaced.StartTimeAM) != closeTime)
                        {
                            //do nothing
                        }
                        else
                        {
                            SendOrderDetailsToCustomer(_user, _order, true);
                        }
                    }
                }

                //when current time is within PM shop hours display prep time
                else if (timePlaced >= TimeSpan.Parse(shopHourPlaced.StartTimePM) && timePlaced <= TimeSpan.Parse(shopHourPlaced.EndTimePM) && TimeSpan.Parse(shopHourPlaced.StartTimePM) != closeTime)
                {
                    if (totalCurrentPrepTime >= TimeSpan.Parse(shopHourPlaced.StartTimePM) && totalCurrentPrepTime <= TimeSpan.Parse(shopHourPlaced.EndTimePM) && TimeSpan.Parse(shopHourPlaced.StartTimePM) != closeTime)
                    {
                        //do nothing
                    }
                    else
                    {
                        SendOrderDetailsToCustomer(_user, _order, true);
                    }
                }

                //falls before and after shop hours
                else
                {
                    SendOrderDetailsToCustomer(_user, _order, true);
                }
            }
        }

        public void SendOrderDetailsToCustomer(Guid _user, int _order, bool IsClosedShop = false)
        {
            LogManager.LogInfo("-- SendOrderDetailsToCustomer >> " + _user + " for Order Id " + _order + " Is Shop Closed? " + IsClosedShop);

            User FoundUser = DbContext.Users.FirstOrDefault(r => r.UserId == _user);

            var EmailParam = APIConfig.MailConfig;
            EmailParam.To = new List<string>() { FoundUser.Email };
            EmailParam.Subject = IsClosedShop == false ? "Nouvelle commande Chip Chap" : "Chip Chap - Commande envoyée au commerçant";
            EmailParam.Body = String.Format(_GenerateOrderPlacedTemplate(_order, FoundUser.FirstName, false, IsClosedShop));
            var RetStatus = base.SendEmailAsync(EmailParam, LogManager);

            if (RetStatus.IsFaulted == true)
            {
                LogManager.LogInfo("-- SendOrderDetailsToCustomer");
                LogManager.LogError("-- Sending Échec >> " + _user);
            }
        }

        public void SendCancelledOrder(Guid _user, string _order)
        {
            LogManager.LogInfo("SendCancelledOrder to > " + _user + " with Order Id > " + _order);
            User FoundUser = DbContext.Users.FirstOrDefault(r => r.UserId == _user);

            var emailBody = String.Format(APIConfig.MsgConfigs.OrderCancelled, FoundUser.FirstName, _order);

            var EmailParam = APIConfig.MailConfig;
            EmailParam.To = new List<string>() { FoundUser.Email };
            EmailParam.Subject = "Annulation commande Chip Chap";
            EmailParam.Body = emailBody;

            LogManager.LogInfo("Sending email with email parameters:");
            LogManager.LogDebugObject(EmailParam);

            var RetStatus = base.SendEmailAsync(EmailParam, LogManager);
            if (RetStatus.IsFaulted == true)
            {
                LogManager.LogInfo("SendRefundInvoiceLink >> order cancelled.");
                LogManager.LogError("Sending Échec >> " + FoundUser.Email);
            }
        }

        public APIResponse GetAllPayments(PaymentListParamModel paymentListParamModel)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetAllPayments");
            try
            {
                List<PaymentListModel> paymentList = new List<PaymentListModel>();
                if (paymentListParamModel.ShopId != null && paymentListParamModel.ShopId != Guid.Empty)
                {
                    //Payments related to a Pharmacy
                    paymentList = DbContext.Payments.AsNoTracking()
                                  .Join(DbContext.PaymentTransactions, p => p.OrderId, po => po.OrderId, (p, po) => new { p, po })
                                  .Where(s => s.p.CreatedDate >= paymentListParamModel.DateFrom && s.p.CreatedDate <= paymentListParamModel.DateTo 
                                        && s.p.Order.ShopId == paymentListParamModel.ShopId
                                        && s.p.Status == EPaymentStatus.SUCCEEDED || s.p.Status == EPaymentStatus.FAILED)
                                  .Select(s => new PaymentListModel
                                  {
                                      OrderId = s.p.OrderId,
                                      PaymentDate = s.po.CreatedDate.GetValueOrDefault(),
                                      CreatedDate = s.p.CreatedDate.GetValueOrDefault(),
                                      Status = (int)s.p.Status,
                                      TotalAmount = s.p.DebitedFundsAmount / 100
                                  }).ToList();
                }
                else
                {
                    //Get all Payments for Super Admin display
                    paymentList = DbContext.Payments.AsNoTracking()
                                  .Join(DbContext.PaymentTransactions, p => p.OrderId, po => po.OrderId, (p, po) => new { p, po })
                                  .Where(s => s.p.CreatedDate >= paymentListParamModel.DateFrom && s.p.CreatedDate <= paymentListParamModel.DateTo
                                        && s.p.Status == EPaymentStatus.SUCCEEDED || s.p.Status == EPaymentStatus.FAILED)
                                  .Select(s => new PaymentListModel
                                  {
                                      OrderId = s.p.OrderId,
                                      PaymentDate = s.po.CreatedDate.GetValueOrDefault(),
                                      CreatedDate = s.p.CreatedDate.GetValueOrDefault(),
                                      Status = (int)s.p.Status,
                                      TotalAmount = s.p.DebitedFundsAmount / 100
                                  }).ToList();
                }

                if (paymentList.Count() > 0)
                {
                    aResp.Message = "Tous les éléments ont été récupérés avec succès.";
                    aResp.Status = "Succès";
                    aResp.Payload = paymentList;
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    aResp.Message = "Aucun article n'a été récupéré.";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetAllPayments");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetPaymentInvoice(int orderId)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetPaymentInvoice");
            try
            {
                var invoiceDetail = DbContext.Orders.AsNoTracking()
                                  .Join(DbContext.Users, o => o.UserId, u => u.UserId, (o, u) => new { o, u })
                                  .Where(s => s.o.OrderID == orderId)
                                  .Select(s => new PaymentInvoiceModel
                                  {
                                      UserId = s.u.UserId,
                                      PatientId = s.o.PatientId,
                                      CustomerName = s.o.CustomerName,
                                      Email = s.u.Email,
                                      ContactNumber = s.u.MobileNumber,
                                      DeliveryAddressId = s.o.DeliveryAddressId,
                                      OrderReferenceID = s.o.OrderReferenceID,
                                      OrderType = (int)s.o.OrderDeliveryType,
                                      PlacedDate = s.o.CreatedDate.GetValueOrDefault(),                                     
                                      DeliveryDate = s.o.DeliveryDate,
                                      OrderStatus = (int)s.o.OrderProgressStatus,
                                      //AmountDue = s.o.OrderSubTotalAmount,
                                      OrderSubTotalAmount = s.o.OrderSubTotalAmount,
                                      OrderVatAmount = s.o.OrderVatAmount,
                                      OrderDeliveryFee = s.o.OrderDeliveryFee,
                                      OrderGrossAmount = s.o.OrderGrossAmount
                                  }).FirstOrDefault();

                if(invoiceDetail != null)
                {
                    if(invoiceDetail.PatientId != null && invoiceDetail.PatientId != Guid.Empty) // Patient Address detail
                    {
                        var patientDetail = DbContext.Patients.FirstOrDefault(s => s.PatientId == invoiceDetail.PatientId);
                        if(patientDetail != null)
                        {
                            invoiceDetail.Address = patientDetail.Street  + ", " + patientDetail.City + ", " + patientDetail.POBox;
                        }
                    }
                    else
                    {
                        var userAddressDetail = DbContext.UserAddresses.FirstOrDefault(s => s.UserAddressID == invoiceDetail.DeliveryAddressId);
                        if(userAddressDetail != null)
                        {
                            invoiceDetail.Address = userAddressDetail.Street + ", " + userAddressDetail.Building + ", " + userAddressDetail.Area + ", " + userAddressDetail.City + ", " + userAddressDetail.PostalCode;
                        }
                    }

                    invoiceDetail.OrderItemListModel = DbContext.OrderItems.AsNoTracking()
                                  .Join(DbContext.Products, o => o.ProductRecordId, p => p.ProductRecordId, (o, p) => new { o, p })
                                  .Where(s => s.o.OrderID == orderId)
                                   .Select(s => new OrderItemListModel
                                   {
                                       ProductName = s.p.ProductName,
                                       ProductPrice = s.o.ProductPrice,
                                       ProductQuantity = s.o.ProductQuantity,
                                       SubTotal = s.o.SubTotal,
                                       ProductTaxValue = s.o.ProductTaxValue
                                   }).ToList();

                    aResp.Message = "Tous les éléments ont été récupérés avec succès.";
                    aResp.Status = "Succès";
                    aResp.Payload = invoiceDetail;
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;

                }
                else
                {
                    aResp.Message = "Aucun article n'a été récupéré.";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                }



            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetPaymentInvoice");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public WalletPostDTO GetWalletPostDTO(string ownerId, string description)
        {
            List<string> walletOwners = new List<string>();
            walletOwners.Add(ownerId);
            return new WalletPostDTO(
                        walletOwners,
                        description,
                        CurrencyIso.EUR
                    );
        }

        public APIResponse CreateUserNaturalUserInMangoPay(User userInfo)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("CreateUserNaturalUserInMangoPay: " + userInfo.Email + " Platform: " + userInfo.RegistrationPlatform);

            try
            {
                // build the mangopay api
                MangoPayApi api = new MangoPayApi();
                api.Config.ClientId = APIConfig.PaymentConfig.ClientId;
                api.Config.ClientPassword = APIConfig.PaymentConfig.ApiKey;
                api.Config.BaseUrl = APIConfig.PaymentConfig.BaseUrl;
                MangoPayUser newMPayUSer = new MangoPayUser();
                // lets check if user already exist in mangopay
                UserDTO existingMangoPayUser = GetUserInMangoPayViaEmail(api, userInfo.Email, PersonType.NATURAL);
                if (existingMangoPayUser != null)
                {
                    // get the user info user from mangopay
                    UserNaturalDTO existingMpayUserDTO = api.Users.GetNatural(existingMangoPayUser.Id);
                    // get wallet of user from mango pay
                    ListPaginated<WalletDTO> userWallets = GetUserWallets(existingMangoPayUser.Id, api);
                    WalletDTO userWallet = null;
                    if (userWallets.Count == 0) // user has no wallet
                    {
                        // build the wallet payload
                        LogManager.LogInfo("-- Build WalletPostDTO Payload --");
                        WalletPostDTO NewUserWallet = GetWalletPostDTO(existingMpayUserDTO.Id, userInfo.FirstName + " " + userInfo.LastName + "'s Wallet " + userInfo.UserId.ToString());
                        LogManager.LogDebugObject(NewUserWallet);
                        userWallet = CreateuserWallet(NewUserWallet, api);
                    }
                    else
                    {
                        userWallet = userWallets.FirstOrDefault();
                    }

                    // create the mango pay user object for our db
                    newMPayUSer = CreateMangoPayUserObject(existingMpayUserDTO, userInfo, userWallet.Id);
                    LogManager.LogInfo("-- Creating MangoPayUser for database saving --");
                    LogManager.LogDebugObject(newMPayUSer);
                    // create the mango pay shop object for our db
                    LogManager.LogInfo("-- Creating MangoPayUserWallet for database saving --");
                    MangoPayUserWallet mPayUserWallet = CreateMangoPayWalletObject(userWallet, null, userInfo.UserId);

                    DbContext.MangoPayUsers.Add(newMPayUSer);
                    DbContext.MangoPayUserWallets.Add(mPayUserWallet);
                    DbContext.SaveChanges();
                }
                else
                {
                    newMPayUSer = CreateNewMangoPayNaturalUser(userInfo, api);
                }

                apiResp = new APIResponse
                {
                    Message = "Compte créé avec succès",
                    Status = "Succès",
                    Payload = newMPayUSer,
                    StatusCode = System.Net.HttpStatusCode.OK
                };

                return apiResp;

            }
            catch (Exception ex)
            {
                LogManager.LogError("CreateUserNaturalUserInMangoPay: " + userInfo.Email + " Platform: " + userInfo.RegistrationPlatform);
                LogManager.LogDebugObject(ex);
                apiResp = new APIResponse
                {
                    Message = "Erreur serveur !",
                    Status = "Créer une erreur d'utilisateur Mango Pay!",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }

            return apiResp;

        }

        public APIResponse CreateUserNaturalCourierInMangoPay(User userInfo)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("CreateUserNaturalUserInMangoPay: " + userInfo.Email + " Platform: " + userInfo.RegistrationPlatform);

            try
            {
                // build the mangopay api
                MangoPayApi api = new MangoPayApi();
                api.Config.ClientId = APIConfig.PaymentConfig.ClientId;
                api.Config.ClientPassword = APIConfig.PaymentConfig.ApiKey;
                api.Config.BaseUrl = APIConfig.PaymentConfig.BaseUrl;
                MangoPayCouriers newMPayUSer = new MangoPayCouriers();
                // lets check if user already exist in mangopay
                UserDTO existingMangoPayUser = GetUserInMangoPayViaEmail(api, userInfo.Email, PersonType.NATURAL);
                if (existingMangoPayUser != null)
                {
                    // get the user info user from mangopay
                    UserNaturalDTO existingMpayUserDTO = api.Users.GetNatural(existingMangoPayUser.Id);
                    // get wallet of user from mango pay
                    ListPaginated<WalletDTO> userWallets = GetUserWallets(existingMangoPayUser.Id, api);
                    WalletDTO userWallet = null;
                    if (userWallets.Count == 0) // user has no wallet
                    {
                        // build the wallet payload
                        LogManager.LogInfo("-- Build WalletPostDTO Payload --");
                        WalletPostDTO NewUserWallet = GetWalletPostDTO(existingMpayUserDTO.Id, userInfo.FirstName + " " + userInfo.LastName + "'s Wallet " + userInfo.UserId.ToString());
                        LogManager.LogDebugObject(NewUserWallet);
                        userWallet = CreateuserWallet(NewUserWallet, api);
                    }
                    else
                    {
                        userWallet = userWallets.FirstOrDefault();
                    }

                    // create the mango pay user object for our db
                    newMPayUSer = CreateMangoPayCourierObject(existingMpayUserDTO, userInfo, userWallet.Id);
                    LogManager.LogInfo("-- Creating MangoPayUser for database saving --");
                    LogManager.LogDebugObject(newMPayUSer);
                    // create the mango pay shop object for our db
                    LogManager.LogInfo("-- Creating MangoPayUserWallet for database saving --");
                    MangoPayCourierWallet mPayUserWallet = CreateMangoPayCourierWalletObject(userWallet, null, userInfo.UserId);

                    MangoPayCouriers FoundUser = DbContext.MangoPayCouriers.AsNoTracking().Where(u => u.PharmaMUserId == userInfo.UserId).FirstOrDefault();
                    MangoPayCourierWallet FoundWallet = DbContext.MangoPayCourierWallets.AsNoTracking().Where(u => u.UserId == userInfo.UserId).FirstOrDefault();

                    if (FoundUser == null)
                    {
                        DbContext.MangoPayCouriers.Add(newMPayUSer);
                    }
                    if (FoundWallet == null)
                    {
                        DbContext.MangoPayCourierWallets.Add(mPayUserWallet);
                    }
                    DbContext.SaveChanges();
                }
                else
                {
                    newMPayUSer = CreateNewMangoPayNaturalCourier(userInfo, api);
                }

                apiResp = new APIResponse
                {
                    Message = "Compte créé avec succès",
                    Status = "Succès",
                    Payload = newMPayUSer,
                    StatusCode = System.Net.HttpStatusCode.OK
                };

                return apiResp;

            }
            catch (Exception ex)
            {
                LogManager.LogError("CreateUserNaturalUserInMangoPay: " + userInfo.Email + " Platform: " + userInfo.RegistrationPlatform);
                LogManager.LogDebugObject(ex);
                apiResp = new APIResponse
                {
                    Message = "Erreur serveur !",
                    Status = "Créer une erreur d'utilisateur Mango Pay!",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }

            return apiResp;

        }

        public MangoPayCouriers CreateNewMangoPayNaturalCourier(User userInfo, MangoPayApi api)
        {
            //build the create user payload
            LogManager.LogInfo("-- Build UserNaturalPostDTO Payload --");
            UserNaturalPostDTO CreateUserPayload = GetuserNaturalPostDTO(userInfo);
            LogManager.LogDebugObject(CreateUserPayload);

            // call mangopay to create the natural user
            LogManager.LogInfo("-- Calling api.users.create with the above UserNaturalPostDTO Payload--");
            UserNaturalDTO responsePayload = api.Users.Create(CreateUserPayload);
            LogManager.LogInfo("-- Response from api.users.create responsePayload: --");
            LogManager.LogDebugObject(responsePayload);

            // build the wallet payload
            LogManager.LogInfo("-- Build WalletPostDTO Payload --");
            WalletPostDTO NewUserWallet = GetWalletPostDTO(responsePayload.Id, userInfo.FirstName + " " + userInfo.LastName + "'s Wallet " + userInfo.UserId.ToString());
            LogManager.LogDebugObject(NewUserWallet);

            // call mangopay to create the user wallet 
            LogManager.LogInfo("-- Calling api.Wallets.Create with the above WalletPostDTO Payload--");
            WalletDTO returnNewWallet = api.Wallets.Create(NewUserWallet);
            LogManager.LogInfo("-- Response from api.Wallets.create responsePayload: --");
            LogManager.LogDebugObject(returnNewWallet);

            // build mangopay user object for DB
            LogManager.LogInfo("-- Creating MangoPayUser for database saving --");
            MangoPayCouriers mPayUser = CreateMangoPayCourierObject(responsePayload, userInfo, returnNewWallet.Id);
            LogManager.LogDebugObject(mPayUser);

            // build mangopay user wallet object for DB
            LogManager.LogInfo("-- Creating MangoPayUserWallet for database saving --");
            MangoPayCourierWallet mPayUserWallet = CreateMangoPayCourierWalletObject(returnNewWallet, null, userInfo.UserId);
            LogManager.LogDebugObject(mPayUserWallet);

            DbContext.MangoPayCouriers.Add(mPayUser);
            DbContext.MangoPayCourierWallets.Add(mPayUserWallet);
            DbContext.SaveChanges();

            return mPayUser;
        }

        public MangoPayCourierWallet CreateMangoPayCourierWalletObject(WalletDTO returnNewWallet, Guid? shopId = null, Guid? userId = null)
        {
            Guid objectOwnerID = Guid.NewGuid();
            if (shopId != null)
            {
                objectOwnerID = (Guid)shopId;
            }
            else
            {
                objectOwnerID = (Guid)userId;
            }

            MangoPayCourierWallet mpuWallet = new MangoPayCourierWallet
            {
                Balance = returnNewWallet.Balance.Amount.ToString(),
                Currency = returnNewWallet.Currency.ToString(),
                Description = returnNewWallet.Description,
                CreatedBy = objectOwnerID,
                CreatedDate = DateTime.Now,
                DateEnabled = DateTime.Now,
                FundsType = returnNewWallet.FundsType.ToString(),
                IsEnabled = true,
                IsEnabledBy = objectOwnerID,
                IsLocked = false,
                LastEditedBy = objectOwnerID,
                LastEditedDate = DateTime.Now,
                LockedDateTime = null,
                OwnerID = returnNewWallet.Owners[0],
                Tag = returnNewWallet.Tag,
                WalletID = returnNewWallet.Id
            };

            if (shopId != null)
            {
                mpuWallet.ShopID = objectOwnerID;
            }
            else
            {
                mpuWallet.UserId = objectOwnerID;
            }

            return mpuWallet;
        }

        public MangoPayCouriers CreateMangoPayCourierObject(UserNaturalDTO responsePayload, User userInfo, string walletId)
        {
            MangoPayCouriers mPayUser = new MangoPayCouriers
            {
                ID = responsePayload.Id, // mangopay user id
                FirstName = responsePayload.FirstName,
                LastName = responsePayload.LastName,
                Nationality = responsePayload.Nationality.ToString(),
                KYCLevel = responsePayload.KYCLevel.ToString(),
                CountryOfResidence = responsePayload.CountryOfResidence.ToString(),
                Address = responsePayload.Address.AddressLine1 + ", " +
                responsePayload.Address.AddressLine2 + ", " +
                responsePayload.Address.City + ", " +
                responsePayload.Address.PostalCode + ", " +
                responsePayload.Address.Region + ", " +
                responsePayload.Address.Country,
                Birthday = responsePayload.Birthday,
                PharmaMUserId = userInfo.UserId,
                IsLocked = false,
                WalletID = walletId,
                IsEnabled = true,
                CreatedBy = userInfo.UserId,
                CreatedDate = DateTime.Now,
                IsEnabledBy = userInfo.UserId,
                LastEditedBy = userInfo.UserId,
                LastEditedDate = DateTime.Now,
                DateEnabled = DateTime.Now,
            };
            return mPayUser;
        }

        public MangoPayUser CreateMangoPayUserObject(UserNaturalDTO responsePayload, User userInfo, string walletId)
        {
            MangoPayUser mPayUser = new MangoPayUser
            {
                ID = responsePayload.Id, // mangopay user id
                FirstName = responsePayload.FirstName,
                LastName = responsePayload.LastName,
                Nationality = responsePayload.Nationality.ToString(),
                KYCLevel = responsePayload.KYCLevel.ToString(),
                CountryOfResidence = responsePayload.CountryOfResidence.ToString(),
                Address = responsePayload.Address.AddressLine1 + ", " +
                responsePayload.Address.AddressLine2 + ", " +
                responsePayload.Address.City + ", " +
                responsePayload.Address.PostalCode + ", " +
                responsePayload.Address.Region + ", " +
                responsePayload.Address.Country,
                Birthday = responsePayload.Birthday,
                PharmaMUserId = userInfo.UserId,
                IsLocked = false,
                WalletID = walletId,
                IsEnabled = true,
                CreatedBy = userInfo.UserId,
                CreatedDate = DateTime.Now,
                IsEnabledBy = userInfo.UserId,
                LastEditedBy = userInfo.UserId,
                LastEditedDate = DateTime.Now,
                DateEnabled = DateTime.Now,
            };
            return mPayUser;
        }


        public UserDTO GetUserInMangoPayViaEmail(MangoPayApi mAPI, string userEmail, PersonType pType)
        {

            Pagination paging = new Pagination
            {
                ItemsPerPage = 100,
                Page = 1,
            };
            ListPaginated<UserDTO> usersInMangoPay = mAPI.Users.GetAll(paging, null);
            for (int currPage = 1; currPage <= usersInMangoPay.TotalPages; currPage++)
            {
                UserDTO userDtoMPay = usersInMangoPay.FirstOrDefault(uimp => uimp.Email == userEmail && uimp.PersonType == pType);
                if (userDtoMPay != null)
                {
                    return userDtoMPay;
                }
                paging.Page = currPage;
                usersInMangoPay = mAPI.Users.GetAll(paging, null);
            }
            return null;

        }

        public ListPaginated<WalletDTO> GetUserWallets(string userID, MangoPayApi mAPI)
        {
            Pagination paging = new Pagination
            {
                ItemsPerPage = 100,
                Page = 0,
            };
            return mAPI.Users.GetWallets(userID, paging, null);
        }

        public WalletDTO CreateuserWallet(WalletPostDTO NewUserWallet, MangoPayApi api)
        {
            // call mangopay to create the user wallet 
            LogManager.LogInfo("-- Calling api.Wallets.Create with the above WalletPostDTO Payload--");
            WalletDTO returnNewWallet = api.Wallets.Create(NewUserWallet);
            LogManager.LogInfo("-- Response from api.Wallets.create responsePayload: --");
            LogManager.LogDebugObject(returnNewWallet);
            return returnNewWallet;
        }

        public APIResponse CreateUserLegalUserInMangoPay(Shop shopInfo)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("CreateUserLegalUserInMangoPay: " + shopInfo.Email);

            try
            {
                // build the mangopay api
                MangoPayApi api = new MangoPayApi();
                api.Config.ClientId = APIConfig.PaymentConfig.ClientId;
                api.Config.ClientPassword = APIConfig.PaymentConfig.ApiKey;
                api.Config.BaseUrl = APIConfig.PaymentConfig.BaseUrl;
                MangoPayShop mPayShop = new MangoPayShop();
                // lets check first if the shop already exist in mango pay
                UserDTO currentMPayUsers = GetUserInMangoPayViaEmail(api, shopInfo.Email, PersonType.LEGAL);
                if (currentMPayUsers != null)
                {
                    // get the shopinfo user from mangopay
                    UserLegalDTO shopLegalUserProf = api.Users.GetLegal(currentMPayUsers.Id);
                    // get wallet of shop from mango pay
                    ListPaginated<WalletDTO> shopWallets = GetUserWallets(shopLegalUserProf.Id, api);
                    WalletDTO userWallet = null;
                    if (shopWallets.Count == 0) // user has no wallet
                    {
                        // build the wallet payload
                        LogManager.LogInfo("-- Build WalletPostDTO Payload --");
                        WalletPostDTO NewUserWallet = GetWalletPostDTO(shopLegalUserProf.Id, shopInfo.ShopName + "'s Wallet " + shopInfo.ShopId.ToString());
                        LogManager.LogDebugObject(NewUserWallet);
                        userWallet = CreateuserWallet(NewUserWallet, api);
                    }
                    else
                    {
                        userWallet = shopWallets.FirstOrDefault();
                    }
                    // create the mango pay shop object for our db
                    // check if the mangopay legaluser id exist in our db
                    mPayShop = DbContext.MangoPayShops.FirstOrDefault(mps => mps.Id == shopLegalUserProf.Id && mps.PharmaShopId == shopInfo.ShopId);
                    if (mPayShop == null)
                    {
                        LogManager.LogInfo("-- Creating MangoPayShop for database saving --");
                        mPayShop = CreateMangoPayShopObject(shopInfo, shopLegalUserProf, userWallet.Id);
                        LogManager.LogDebugObject(mPayShop);
                        DbContext.MangoPayShops.Add(mPayShop);
                    }

                    // check if the wallet exist in our db
                    MangoPayUserWallet mPayUserWallet = DbContext.MangoPayUserWallets.FirstOrDefault(mpuw => mpuw.WalletID == userWallet.Id);
                    if (mPayUserWallet == null)
                    {
                        // create the mango pay shop object for our db
                        LogManager.LogInfo("-- Creating MangoPayUserWallet for database saving --");
                        mPayUserWallet = CreateMangoPayWalletObject(userWallet, shopInfo.ShopId, null);
                        DbContext.MangoPayUserWallets.Add(mPayUserWallet);
                    }

                    DbContext.SaveChanges();

                    apiResp = new APIResponse
                    {
                        Message = "Compte créé avec succès",
                        Status = "Succès",
                        Payload = mPayShop,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };

                }
                else // no shop is registered 
                {
                    //
                    // as per request of lauriene 02/17/2021 remove the creation of shop in mangopay
                    // on shop registration
                    // mPayShop = CreateNewLegalUserInMangoPay(shopInfo, api);
                    mPayShop = null;
                    apiResp = new APIResponse
                    {
                        Message = "Pas de compte mangopay",
                        Status = "Échouer",
                        Payload = mPayShop,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
                return apiResp;

            }
            catch (Exception ex)
            {
                LogManager.LogError("CreateUserLegalUserInMangoPay Erreur: " + shopInfo.Email);
                LogManager.LogDebugObject(ex);
                apiResp = new APIResponse
                {
                    Message = "Erreur serveur !",
                    Status = "Créer une erreur d'utilisateur Mango Pay!",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }

            return apiResp;
        }

        public MangoPayShop CreateMangoPayShopObject(Shop shopInfo, UserLegalDTO userLegalResponse, string walletId)
        {
            string repEmail = "";
            if (userLegalResponse.LegalRepresentativeEmail == null)
            {
                repEmail = shopInfo.Email;
            }
            else
            {
                repEmail = userLegalResponse.LegalRepresentativeEmail;
            }
            MangoPayShop shopObject = new MangoPayShop
            {
                CreatedDate = DateTime.Now,
                PharmaShopId = shopInfo.ShopId,
                CompanyNumber = userLegalResponse.CompanyNumber,
                CreatedBy = shopInfo.LastEditedBy,
                Id = userLegalResponse.Id,
                IsEnabled = true,
                LastEditedBy = shopInfo.LastEditedBy,
                DateEnabled = DateTime.Now,
                HeadquartersAddress = userLegalResponse.HeadquartersAddress.AddressLine1 + ", " +
                    userLegalResponse.HeadquartersAddress.AddressLine2 + ", " +
                    userLegalResponse.HeadquartersAddress.City + ", " +
                    userLegalResponse.HeadquartersAddress.Region + ", " +
                    userLegalResponse.HeadquartersAddress.Country + ", " +
                    userLegalResponse.HeadquartersAddress.PostalCode,
                IsEnabledBy = shopInfo.IsEnabledBy,
                IsLocked = false,
                LastEditedDate = shopInfo.LastEditedDate,
                LegalPersonType = userLegalResponse.LegalPersonType.ToString(),
                LegalRepresentativeAddress = userLegalResponse.LegalRepresentativeAddress.AddressLine1 + ", " +
                    userLegalResponse.LegalRepresentativeAddress.AddressLine2 + ", " +
                    userLegalResponse.LegalRepresentativeAddress.City + ", " +
                    userLegalResponse.LegalRepresentativeAddress.Region + ", " +
                    userLegalResponse.LegalRepresentativeAddress.Country + ", " +
                    userLegalResponse.LegalRepresentativeAddress.PostalCode,
                LegalRepresentativeEmail = userLegalResponse.LegalRepresentativeEmail,
                LegalRepresentativeBirthday = userLegalResponse.LegalRepresentativeBirthday,
                LockedDateTime = null,
                LegalRepresentativeFirstName = userLegalResponse.LegalRepresentativeFirstName,
                LegalRepresentativeNationality = userLegalResponse.LegalRepresentativeNationality.ToString(),
                LegalRepresentativeLastName = userLegalResponse.LegalRepresentativeLastName,
                Name = userLegalResponse.Name,
                WalletID = walletId,
                LegalRepresentativeCountryOfResidence = userLegalResponse.LegalRepresentativeCountryOfResidence.ToString(),
                KYCLevels = userLegalResponse.KYCLevel

            };
            return shopObject;
        }


        public MangoPayUser CreateNewMangoPayNaturalUser(User userInfo, MangoPayApi api)
        {
            //build the create user payload
            LogManager.LogInfo("-- Build UserNaturalPostDTO Payload --");
            UserNaturalPostDTO CreateUserPayload = GetuserNaturalPostDTO(userInfo);
            LogManager.LogDebugObject(CreateUserPayload);

            // call mangopay to create the natural user
            LogManager.LogInfo("-- Calling api.users.create with the above UserNaturalPostDTO Payload--");
            UserNaturalDTO responsePayload = api.Users.Create(CreateUserPayload);
            LogManager.LogInfo("-- Response from api.users.create responsePayload: --");
            LogManager.LogDebugObject(responsePayload);

            // build the wallet payload
            LogManager.LogInfo("-- Build WalletPostDTO Payload --");
            WalletPostDTO NewUserWallet = GetWalletPostDTO(responsePayload.Id, userInfo.FirstName + " " + userInfo.LastName + "'s Wallet " + userInfo.UserId.ToString());
            LogManager.LogDebugObject(NewUserWallet);

            // call mangopay to create the user wallet 
            LogManager.LogInfo("-- Calling api.Wallets.Create with the above WalletPostDTO Payload--");
            WalletDTO returnNewWallet = api.Wallets.Create(NewUserWallet);
            LogManager.LogInfo("-- Response from api.Wallets.create responsePayload: --");
            LogManager.LogDebugObject(returnNewWallet);

            // build mangopay user object for DB
            LogManager.LogInfo("-- Creating MangoPayUser for database saving --");
            MangoPayUser mPayUser = CreateMangoPayUserObject(responsePayload, userInfo, returnNewWallet.Id);
            LogManager.LogDebugObject(mPayUser);

            // build mangopay user wallet object for DB
            LogManager.LogInfo("-- Creating MangoPayUserWallet for database saving --");
            MangoPayUserWallet mPayUserWallet = CreateMangoPayWalletObject(returnNewWallet, null, userInfo.UserId);
            LogManager.LogDebugObject(mPayUserWallet);


            DbContext.MangoPayUsers.Add(mPayUser);
            DbContext.MangoPayUserWallets.Add(mPayUserWallet);
            DbContext.SaveChanges();

            return mPayUser;
        }

        public MangoPayUserWallet CreateMangoPayWalletObject(WalletDTO returnNewWallet, Guid? shopId = null, Guid? userId = null)
        {
            Guid objectOwnerID = Guid.NewGuid();
            if (shopId != null)
            {
                objectOwnerID = (Guid)shopId;
            }
            else
            {
                objectOwnerID = (Guid)userId;
            }

            MangoPayUserWallet mpuWallet = new MangoPayUserWallet
            {
                Balance = returnNewWallet.Balance.Amount.ToString(),
                Currency = returnNewWallet.Currency.ToString(),
                Description = returnNewWallet.Description,
                CreatedBy = objectOwnerID,
                CreatedDate = DateTime.Now,
                DateEnabled = DateTime.Now,
                FundsType = returnNewWallet.FundsType.ToString(),
                IsEnabled = true,
                IsEnabledBy = objectOwnerID,
                IsLocked = false,
                LastEditedBy = objectOwnerID,
                LastEditedDate = DateTime.Now,
                LockedDateTime = null,
                OwnerID = returnNewWallet.Owners[0],
                Tag = returnNewWallet.Tag,
                WalletID = returnNewWallet.Id
            };

            if (shopId != null)
            {
                mpuWallet.ShopID = objectOwnerID;
            }
            else
            {
                mpuWallet.UserId = objectOwnerID;
            }

            return mpuWallet;
        }

        public UserNaturalPostDTO GetuserNaturalPostDTO(User userInfo)
        {
            // get user id
            UserAddresses uAddress = DbContext.UserAddresses.FirstOrDefault(uadd => uadd.IsCurrentAddress == true && uadd.UserId == userInfo.UserId);
            UserNaturalPostDTO CreateUserPayload = new UserNaturalPostDTO(
                        userInfo.Email,
                        userInfo.FirstName,
                        userInfo.LastName,
                        DateTime.Now,
                        CountryIso.FR,
                        CountryIso.FR
                    );
            //if (uAddress != null)
            //{
            //    Address naturalUserdd = new Address();
            //    naturalUserdd.AddressLine1 = uAddress.Building + ", " + uAddress.Street + ", " + uAddress.Area;
            //    naturalUserdd.City = uAddress.City;
            //    naturalUserdd.PostalCode = uAddress.PostalCode;
            //    CreateUserPayload.Address = naturalUserdd;

            //}
            CreateUserPayload.Tag = "PharmaMMobileAppUser_" + userInfo.UserId.ToString();
            return CreateUserPayload;
        }

        public Payment CreateCardPayment(int iOrderId, PayInCardDirectDTO iPaymentModel, string iCreatePayload, string iAuthToken)
        {
            Payment paymentModel = null;
            try
            {
                var loginTrx = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(x => x.Token == iAuthToken);
                if (loginTrx != null)
                {
                    paymentModel = new Payment();
                    paymentModel.OrderId = iOrderId;
                    paymentModel.Status = EPaymentStatus.CREATED;
                    paymentModel.Tag = iPaymentModel.Tag;
                    paymentModel.AuthorId = iPaymentModel.AuthorId;
                    paymentModel.DebitedFundsCurrency = iPaymentModel.DebitedFunds.Currency.ToString();
                    paymentModel.DebitedFundsAmount = iPaymentModel.DebitedFunds.Amount;
                    paymentModel.FeesCurrency = iPaymentModel.Fees.Currency.ToString();
                    paymentModel.FeesAmount = iPaymentModel.Fees.Amount;
                    paymentModel.CreditedWalletId = iPaymentModel.CreditedWalletId;
                    paymentModel.ReturnURL = iPaymentModel.SecureModeReturnURL;
                    paymentModel.CardType = iPaymentModel.CardType;
                    paymentModel.SecureMode = iPaymentModel.SecureMode;
                    paymentModel.CreditedUserId = iPaymentModel.CreditedUserId;
                    paymentModel.StatementDescriptor = iPaymentModel.StatementDescriptor;
                    paymentModel.CreatePayload = iCreatePayload;
                    paymentModel.LastEditedDate = DateTime.Now;

                    paymentModel.CreatedBy = loginTrx.UserId;
                    paymentModel.CreatedDate = DateTime.Now;

                    DbContext.Payments.Add(paymentModel);
                    DbContext.SaveChanges();
                }
                else
                {
                    LogManager.LogError("Erreur: CreateWebPayment - Login transaction not found");
                }
            }
            catch (Exception e)
            {
                LogManager.LogError("Erreur: CreateWebPayment");
                LogManager.LogDebugObject(e);
            }

            return paymentModel;
        }

        public APIResponse AddCard(Card _card)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("AddCard");

            try
            {
                // -- Check if User exist on our system --//
                User FoundUser = DbContext.Users.AsNoTracking().Where(u => u.UserId == _card.UserId).FirstOrDefault();

                LogManager.LogInfo("-- Create or Get MangoPayUser and Wallet if existing --");
                // -- Create or Get Mangpay User details --//
                APIResponse MangoPayUser = CreateUserNaturalUserInMangoPay(FoundUser);
                MangoPayUser User = (MangoPayUser)MangoPayUser.Payload;
                MangoPayApi api = new MangoPayApi();
                api.Config.ClientId = APIConfig.PaymentConfig.ClientId;
                api.Config.ClientPassword = APIConfig.PaymentConfig.ApiKey;
                api.Config.BaseUrl = APIConfig.PaymentConfig.BaseUrl;

                // -- Create Token for Card Registration --//
                LogManager.LogInfo("-- Create Token for Card Registration --");

                CardRegistrationPostDTO cardInfo = new CardRegistrationPostDTO(
                    User.ID,
                    CurrencyIso.EUR,
                    GetCreditCardType(_card.CardNumber)
                    );

                CardRegistrationDTO card = api.CardRegistrations.Create(cardInfo);

                // -- Post Card Information to MangoPay --//
                LogManager.LogInfo("-- Post Card Information Registration --");
                var httpClient = new HttpClient();
                var parameters = new Dictionary<string, string>();
                parameters["data"] = card.PreregistrationData;
                parameters["accessKeyRef"] = card.AccessKey;
                parameters["returnURL"] = "";
                parameters["cardNumber"] = _card.CardNumber;
                parameters["cardExpirationDate"] = _card.Expiry.Replace("/", "");
                parameters["cardCvx"] = _card.Cvv;
                var data = "";
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = client.PostAsync(card.CardRegistrationURL, new FormUrlEncodedContent(parameters)).Result;
                    data = response.Content.ReadAsStringAsync().Result;
                }

                // -- Put Card Information to MangoPay --//
                LogManager.LogInfo("-- Put Card Information to MangoPay --");
                CardRegistrationPutDTO putData = new CardRegistrationPutDTO();
                putData.RegistrationData = data;
                CardRegistrationDTO output = api.CardRegistrations.Update(putData, card.Id);

                aResp = new APIResponse
                {
                    Message = "Nouvelle carte ajoutée avec succès",
                    Status = "Succès!",
                    Payload = output,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AddCard");
                LogManager.LogError(ex.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex);
            }
            return aResp;
        }

        public APIResponse GetAllCards(Guid _UserId)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetAllCard");

            try
            {

                LogManager.LogInfo("-- Create or Get MangoPayUser and Wallet if existing --");
                // -- Create or Get Mangpay User details --//
                MangoPayUser mPayUser = DbContext.MangoPayUsers.FirstOrDefault(mpu => mpu.PharmaMUserId == _UserId);
                // get userinfo first

                User currentUser = DbContext.Users.FirstOrDefault(u => u.UserId == _UserId);
                if (mPayUser == null)
                {
                    //user is not yet linked to a mango pay                    
                    APIResponse MangoPayUserCreation = CreateUserNaturalUserInMangoPay(currentUser);
                    if (MangoPayUserCreation.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        mPayUser = (MangoPayUser)MangoPayUserCreation.Payload;
                    }
                    else
                    {
                        return MangoPayUserCreation;
                    }
                }

                MangoPayApi api = new MangoPayApi();
                api.Config.ClientId = APIConfig.PaymentConfig.ClientId;
                api.Config.ClientPassword = APIConfig.PaymentConfig.ApiKey;
                api.Config.BaseUrl = APIConfig.PaymentConfig.BaseUrl;
                
                // -- Get all registered card --//
                LogManager.LogInfo("-- Get all Registered Card --");

                Pagination paging = new Pagination
                {
                    ItemsPerPage = 100,
                    Page = 1,
                };
                List<CardDTO> output = api.Users.GetCards(mPayUser.ID, paging, null).Where(x => x.Active == true).ToList();


                aResp = new APIResponse
                {
                    Message = "Nouvelle carte ajoutée avec succès",
                    Status = "Succès!",
                    Payload = output,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AddCard");
                LogManager.LogError(ex.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex);
            }
            return aResp;
        }

        public APIResponse DeactivateCard(CardDTO _card)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("AddCard");

            try
            {
                MangoPayApi api = new MangoPayApi();
                api.Config.ClientId = APIConfig.PaymentConfig.ClientId;
                api.Config.ClientPassword = APIConfig.PaymentConfig.ApiKey;
                api.Config.BaseUrl = APIConfig.PaymentConfig.BaseUrl;

                // -- Deactivate Card Registration --//
                LogManager.LogInfo("-- Deactivate Card Registration --");

                CardPutDTO input = new CardPutDTO();
                input.Active = false;
                CardDTO output = api.Cards.Update(input, _card.Id);

                aResp = new APIResponse
                {
                    Message = "La carte a été supprimée avec succès",
                    Status = "Succès!",
                    Payload = output,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AddCard");
                LogManager.LogError(ex.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex);
            }
            return aResp;
        }

        public CardType GetCreditCardType(string CreditCardNumber)
        {
            Regex regAmex = new Regex("^3[47][0-9]{13}$");
            Regex regDinersClub = new Regex("^3(?:0[0-5]|[68][0-9])[0-9]{11}$");
            Regex regMastercard = new Regex("^(5[1-5][0-9]{14}|2(22[1-9][0-9]{12}|2[3-9][0-9]{13}|[3-6][0-9]{14}|7[0-1][0-9]{13}|720[0-9]{12}))$");
            Regex regVisa = new Regex("^4[0-9]{12}(?:[0-9]{3})?$");
            Regex regVisaMasterCard = new Regex("^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14})$");
            Regex regMaestro = new Regex("^(5018|5020|5038|6304|6759|6761|6763)[0-9]{8,15}$");
            if (regAmex.IsMatch(CreditCardNumber))
                return CardType.AMEX;
            else if (regDinersClub.IsMatch(CreditCardNumber))
                return CardType.DINERS;
            else if (regVisaMasterCard.IsMatch(CreditCardNumber))
                return CardType.CB_VISA_MASTERCARD;
            else if (regMastercard.IsMatch(CreditCardNumber))
                return CardType.CB_VISA_MASTERCARD;
            else if (regVisa.IsMatch(CreditCardNumber))
                return CardType.CB_VISA_MASTERCARD;
            else if (regMaestro.IsMatch(CreditCardNumber))
                return CardType.MAESTRO;
            else
                return CardType.NotSpecified;
        }

        public PayInCardDirectPostDTO BuildCardDirectPaymentModel(decimal amount, int OrderId, UserAddresses uAddress, string cardId, User uProfile, string accessType)
        {
            LogManager.LogInfo("BuildCardDirectPaymentModel amount:" + amount.ToString() + " for OrderID: " + OrderId.ToString());
            decimal amountInIntWholeNumber = amount * 100;
            LogManager.LogInfo("amount as whole number:" + amountInIntWholeNumber.ToString());
            long amountIntolong = Convert.ToInt64(amountInIntWholeNumber);
            Address BillingAdd = new Address();
            BillingAdd.AddressLine1 = uAddress.Street;
            BillingAdd.City = uAddress.City;
            BillingAdd.Country = MangoPay.SDK.Core.Enumerations.CountryIso.FR;
            BillingAdd.PostalCode = uAddress.PostalCode;
            BillingAdd.Region = uAddress.City;

            PayInCardDirectPostDTO newPaymentMOdel = new PayInCardDirectPostDTO(
                    APIConfig.PaymentConfig.CreditedId,
                    APIConfig.PaymentConfig.WalletId,
                    new Money
                    {
                        Amount = amountIntolong,
                        Currency = MangoPay.SDK.Core.Enumerations.CurrencyIso.EUR
                    },
                    new Money
                    {
                        Amount = 0 * 100,
                        Currency = MangoPay.SDK.Core.Enumerations.CurrencyIso.EUR
                    },
                    APIConfig.PaymentConfig.WalletId,
                    accessType == "Mobile" ? APIConfig.PaymentConfig.ReturlUrl : APIConfig.PaymentConfig.WebReturlUrl,
                    cardId,
                    "PharmaM" + OrderId.ToString().Substring(OrderId.ToString().Length - 2, 2),
                    new Billing
                    {
                        FirstName = uProfile.FirstName,
                        LastName = uProfile.LastName,
                        Address = BillingAdd
                    }, null
                );
            newPaymentMOdel.Tag = "PharmM_CardPayment_" + OrderId.ToString();
            newPaymentMOdel.SecureMode = MangoPay.SDK.Core.Enumerations.SecureMode.DEFAULT;
            //newPaymentMOdel.SecureMode = MangoPay.SDK.Core.Enumerations.SecureMode.FORCE;
            LogManager.LogInfo("BuildCardDirectPaymentModel return object:");
            LogManager.LogDebugObject(newPaymentMOdel);
            return newPaymentMOdel;
        }

        public TransferPostDTO BuildFundTransferModel(decimal amount, int OrderId, MangoPayUser mPayUser, MangoPayShop mPayShop)
        {
            LogManager.LogInfo("BuildFundTransferModel amount:" + amount.ToString() + " for OrderID: " + OrderId.ToString());
            decimal amountInIntWholeNumber = amount * 100;
            LogManager.LogInfo("amount as whole number:" + amountInIntWholeNumber.ToString());
            long amountIntolong = Convert.ToInt64(amountInIntWholeNumber);

            TransferPostDTO newPaymentMOdel = new TransferPostDTO(
                   mPayUser.ID,
                   mPayShop.Id,
                    new Money
                    {
                        Amount = amountIntolong,
                        Currency = MangoPay.SDK.Core.Enumerations.CurrencyIso.EUR
                    },
                    new Money
                    {
                        Amount = 0,
                        Currency = MangoPay.SDK.Core.Enumerations.CurrencyIso.EUR
                    },
                    mPayUser.WalletID,
                    mPayShop.WalletID
                );
            LogManager.LogInfo("BuildFundTransferModel return object:");
            LogManager.LogDebugObject(newPaymentMOdel);
            return newPaymentMOdel;
        }

        public TransferPostDTO BuildFundDeliveryModel(decimal amount, int OrderId, MangoPayShop mPayUser, MangoPayUser mPayDelivery)
        {
            LogManager.LogInfo("BuildFundDeliveryModel amount:" + amount.ToString() + " for OrderID: " + OrderId.ToString());
            decimal amountInIntWholeNumber = amount * 100;
            LogManager.LogInfo("amount as whole number:" + amountInIntWholeNumber.ToString());
            long amountIntolong = Convert.ToInt64(amountInIntWholeNumber);

            TransferPostDTO newPaymentMOdel = new TransferPostDTO(
                   mPayUser.Id,
                   mPayDelivery.ID,
                    new Money
                    {
                        Amount = amountIntolong,
                        Currency = MangoPay.SDK.Core.Enumerations.CurrencyIso.EUR
                    },
                    new Money
                    {
                        Amount = 0,
                        Currency = MangoPay.SDK.Core.Enumerations.CurrencyIso.EUR
                    },
                    mPayUser.WalletID,
                    mPayDelivery.WalletID
                );
            LogManager.LogInfo("BuildFundDeliveryModel return object:");
            LogManager.LogDebugObject(newPaymentMOdel);
            return newPaymentMOdel;
        }

        public APIResponse GetPaymentStatus(string transactionId)
        {
            Thread.Sleep(1000);
            LogManager.LogInfo("GetPaymentStatus with id:" + transactionId);
            Payment recordedPayment = DbContext.Payments.FirstOrDefault(p => p.CreatePayload.Contains(transactionId));
            if (recordedPayment != null)
            {
                PayInCardDirectDTO returnPaymentPayload = JsonConvert.DeserializeObject<PayInCardDirectDTO>(recordedPayment.CreatePayload);
                LogManager.LogInfo("-- returnPaymentPayload details --");
                LogManager.LogDebugObject(returnPaymentPayload);
                var pushDesc = string.Empty;
                Order ReturningOrder = DbContext.Orders.FirstOrDefault(o => o.OrderID == recordedPayment.OrderId);

                if (ReturningOrder.OrderProgressStatus == OrderProgressStatus.PLACED)
                {
                    pushDesc = "Votre paiement pour votre commande numéro " + ReturningOrder.OrderReferenceID + " a été accepté.";
                }
                else if (ReturningOrder.OrderProgressStatus == OrderProgressStatus.ACCEPTED)
                {
                    pushDesc = "Votre commande numéro " + ReturningOrder.OrderReferenceID + " est confirmée par le commerçant.";
                }
                else if (ReturningOrder.OrderProgressStatus == OrderProgressStatus.READYFORPICKUP)
                {
                    pushDesc = "Votre commande numéro " + ReturningOrder.OrderReferenceID + " est prête à être collectée chez votre commerçant.";
                }
                else if (ReturningOrder.OrderProgressStatus == OrderProgressStatus.OUTFORDELIVERY)
                {
                    pushDesc = "Votre commande numéro " + ReturningOrder.OrderReferenceID + " a été remise au livreur et vous sera bientôt livrée.";
                }
                else if (ReturningOrder.OrderProgressStatus == OrderProgressStatus.CANCELORDER)
                {
                    //if (ReturningOrder.IsCancelledBy == AccountTypes.APPUSER)
                    //{
                    //    pushDesc = "Votre commande numéro " + ReturningOrder.OrderReferenceID + " a été annulée.";
                    //}
                    //else
                    //{
                        pushDesc = "Votre commande numéro " + ReturningOrder.OrderReferenceID + " a été annulée par le commerçant.";
                    //}
                }
                else
                {
                    pushDesc = "Votre paiement a été refusé. Merci de bien vouloir réessayer.";
                }

                return new APIResponse
                {
                    Payload = ReturningOrder,
                    Message = pushDesc,
                    Status = recordedPayment.Status.ToString(),
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            else
            {
                return new APIResponse
                {
                    Message = "Aucun enregistrement de paiement avec cet identifiant!",
                    Status = "Identification de transaction non valide",
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
        }
    }
}
