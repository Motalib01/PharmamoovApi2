using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
//using MangoPay.SDK.Entities.POST;
//using MangoPay.SDK;
using Newtonsoft.Json;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using PharmaMoov.Models.Cart;
using PharmaMoov.Models.DeliveryUser;
using PharmaMoov.Models.Orders;
using PharmaMoov.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using MangoPay.SDK.Entities.GET;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class OrderRepository : APIBaseRepo, IOrderRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private readonly LocalizationService localization;
        private IHttpContextAccessor accessor;
        private IMainHttpClient MainHttpClient { get; }

        public OrderRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, IHttpContextAccessor _accessor, LocalizationService _localization, IMainHttpClient _mhttpc)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            localization = _localization;
            accessor = _accessor;
            MainHttpClient = _mhttpc;
        }

        public APIResponse GetUserOrderHistory(string _auth)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("GetUserOrderHistory: " + _auth);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("GetUserOrderHistory: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    IQueryable<Order> filterResults = null;
                    IEnumerable<UserOrderList> userOrderLists = null;

                    if (IsUserLoggedIn.AccountType == AccountTypes.HEALTHPROFESSIONAL)
                    {
                        filterResults = DbContext.Orders.AsNoTracking()
                                            .Where(u => u.UserId == IsUserLoggedIn.UserId
                                                && u.PatientId == Guid.Empty
                                                && u.OrderProgressStatus != OrderProgressStatus.PENDING);
                    }
                    else
                    {
                        //Customer Order History
                        filterResults = DbContext.Orders.AsNoTracking().Where(u => u.UserId == IsUserLoggedIn.UserId && u.OrderProgressStatus != OrderProgressStatus.PENDING);
                    }

                    //filterResults = DbContext.Orders.AsNoTracking().Where(u => u.UserId == IsUserLoggedIn.UserId && (int)u.OrderProgressStatus <= (int)OrderProgressStatus.READYFORPICKUP && u.OrderProgressStatus != OrderProgressStatus.PENDING);

                    //if (takeNewOrders == false)
                    //{
                    //    //filterResults = DbContext.Orders.AsNoTracking().Where(u => u.UserId == IsUserLoggedIn.UserId && u.OrderProgressStatus == OrderProgressStatus.COMPLETED || u.OrderProgressStatus == OrderProgressStatus.CANCELORDER || u.OrderProgressStatus == OrderProgressStatus.REJECTED);
                    //    filterResults = DbContext.Orders.AsNoTracking().Where(u => u.UserId == IsUserLoggedIn.UserId && u.OrderProgressStatus == OrderProgressStatus.COMPLETED || u.OrderProgressStatus == OrderProgressStatus.PLACED || u.OrderProgressStatus == OrderProgressStatus.REJECTED);
                    //}

                    if (filterResults != null)
                    {
                        userOrderLists = filterResults.AsNoTracking().Select(o => new UserOrderList
                        {
                            OrderID = o.OrderID,
                            OrderReferenceID = o.OrderReferenceID,
                            ShopName = o.ShopName,
                            OrderGrossAmount = o.OrderGrossAmount,
                            DateAdded = o.CreatedDate.GetValueOrDefault(),
                            ScheduleDate = o.DeliveryDate,
                            ScheduleTiming = o.DeliveryTime,
                            OrderProgressStatus = o.OrderProgressStatus,
                            ShopId = o.ShopId,
                            ShopIcon = DbContext.Shops.FirstOrDefault(s => s.ShopId == o.ShopId).ShopIcon,
                            DeliveryAddressId = o.DeliveryAddressId,
                            UserId = o.UserId,
                            PatientId = o.PatientId
                        }).OrderByDescending(o => o.OrderProgressStatus).ThenBy(o => o.ScheduleDate).ToList();

                        foreach (UserOrderList item in userOrderLists)
                        {
                            //Get Number of Items
                            var countOrderItems = DbContext.OrderItems.AsNoTracking().Where(i => i.IsEnabled == true && i.OrderID == item.OrderID).Count();
                            item.NoOfProducts = countOrderItems;

                            //Get Delivery Address
                            if(item.PatientId != null && item.PatientId != Guid.Empty)
                            {
                                var patientDetail = DbContext.Patients.AsNoTracking().FirstOrDefault(s => s.PatientId == item.PatientId);
                                if(patientDetail != null)
                                {
                                    item.CompleteDeliveryAddress = patientDetail.Street + ", " + patientDetail.POBox + ", " + patientDetail.City;
                                }
                            }
                            else
                            {
                                UserAddresses getAddress = DbContext.UserAddresses.FirstOrDefault(a => a.UserAddressID == item.DeliveryAddressId);
                                if(getAddress != null)
                                {
                                    item.CompleteDeliveryAddress = getAddress.Street + ", " + getAddress.PostalCode + ", " + getAddress.City;
                                }
                            }
                           
                        };

                        if (userOrderLists.Count() > 0)
                        {
                            aResp = new APIResponse
                            {
                                Message = "Tous les éléments ont été récupérés avec succès.",
                                Status = "Succès!",
                                Payload = userOrderLists,
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            aResp = new APIResponse
                            {
                                Message = "Aucun article n'a été récupéré.",
                                Status = "Échec!",
                                Payload = userOrderLists,
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                    }
                }
                else
                {
                    aResp.Message = "Utilisateur non connecté";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetUserOrderHistory");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetUserOrderDetails(string _auth, int _order)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("GetUserOrderDetails: " + _order);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("GetUserOrderDetails: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    UserOrderDetails userOrder = new UserOrderDetails();
                    userOrder = DbContext.Orders.Where(i => i.OrderID == _order)
                        .Select(o => new UserOrderDetails
                        {
                            OrderID = o.OrderID,
                            UserID = o.UserId,
                            PatientId = o.PatientId,
                            OrderReferenceID = o.OrderReferenceID,
                            OrderProgressStatus = o.OrderProgressStatus,
                            DeliveryMethod = o.DeliveryMethod,
                            DeliveryAddressId = o.DeliveryAddressId,
                            DateAdded = o.CreatedDate.GetValueOrDefault(),
                            //ScheduleDate = o.DeliveryDate,
                            ScheduleTiming = o.DeliveryTime,
                            OrderSubTotalAmount = o.OrderSubTotalAmount,
                            OrderVatAmount = o.OrderVatAmount,
                            //OrderPromoAmount = o.OrderPromoAmount,
                            OrderDeliveryFee = o.OrderDeliveryFee,
                            OrderGrossAmount = o.OrderGrossAmount,
                            OrderNote = o.OrderNote,
                            ShopId = o.ShopId,
                            ShopName = o.ShopName,
                            ShopIcon = DbContext.Shops.FirstOrDefault(s => s.ShopId == o.ShopId).ShopIcon,
                            ShopAddress = o.ShopAddress
                        }).FirstOrDefault();

                    if (userOrder != null)
                    {
                        //get address details
                        var buildAddress = string.Empty;


                        if (userOrder.PatientId != null && userOrder.PatientId != Guid.Empty)
                        {
                            var patientDetail = DbContext.Patients.AsNoTracking().FirstOrDefault(s => s.PatientId == userOrder.PatientId);
                            if (patientDetail != null)
                            {
                                buildAddress = patientDetail.Street + ", " + patientDetail.City;
                                userOrder.AddressName = patientDetail.AddressName;
                                userOrder.CompleteAddress = buildAddress;
                                userOrder.IsAddressDisabled = patientDetail.IsEnabled == true ? true : false;
                                userOrder.MobileNumber = patientDetail.MobileNumber;
                            }
                        }
                        else
                        {
                            UserAddresses userAddress = DbContext.UserAddresses.AsNoTracking().FirstOrDefault(a => a.UserAddressID == userOrder.DeliveryAddressId);
                            //buildAddress = userAddress.Street + ", " + userAddress.Building + ", " + userAddress.Area + ", " + userAddress.City;
                            buildAddress = userAddress.Street + ", " + userAddress.PostalCode + ", " + userAddress.City;
                            userOrder.AddressName = userAddress.AddressName;
                            userOrder.CompleteAddress = buildAddress;
                            userOrder.IsAddressDisabled = userAddress.IsEnabled == true ? true : false;
                            //get user details
                            var userMobileNumber = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userAddress.UserId).MobileNumber;
                            userOrder.MobileNumber = userMobileNumber;
                        }

                        aResp = new APIResponse
                        {
                            Message = "Tous les éléments ont été récupérés avec succès.",
                            Status = "Succès!",
                            Payload = new OrderDetailsDTO
                            {
                                OrderDetails = userOrder,
                                OrderItems = GetOrderItems(userOrder.OrderID),
                            },
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Aucun article n'a été récupéré.",
                            Status = "Échec!",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
                else
                {
                    aResp.Message = "Utilisateur non connecté";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetUserOrderDetails");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public List<NewOrderItems> GetOrderItems(int OrderId)
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
                        NoVatSubTotal = Convert.ToDecimal(string.Format("{0:F2}", sItem.ProductPrice * sItem.ProductQuantity)),
                        ProductIcon = sp.ProductIcon
                    }).FirstOrDefault());
            }
            LogManager.LogInfo("GetOrderItems >>");
            LogManager.LogDebugObject(cpList);
            return cpList;
        }

        public APIResponse GetShopsOrders(Guid _shop, bool _takeHistory)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopsOrders: " + _shop);

            try
            {
                IEnumerable<ShopOrderList> shopOrderLists = null;
                IQueryable<Order> filterOrders = null;
                filterOrders = DbContext.Orders.AsNoTracking().Where(x => x.ShopId == _shop);

                if (!_takeHistory)
                {
                    filterOrders = filterOrders.Where(x => (int)x.OrderProgressStatus <= (int)OrderProgressStatus.OUTFORDELIVERY && x.OrderProgressStatus != OrderProgressStatus.PENDING);
                }
                else
                {
                    filterOrders = filterOrders.Where(x => x.OrderProgressStatus == OrderProgressStatus.CANCELORDER
                        || x.OrderProgressStatus == OrderProgressStatus.REJECTED || x.OrderProgressStatus == OrderProgressStatus.COMPLETED);
                }

                if (filterOrders != null)
                {
                    shopOrderLists = filterOrders
                        .Select(o => new ShopOrderList
                        {
                            OrderID = o.OrderID,
                            OrderReferenceID = o.OrderReferenceID,
                            CustomerName = o.CustomerName,
                            OrderGrossAmount = o.OrderGrossAmount,
                            DeliveryTypeId = o.OrderDeliveryType,
                            DeliveryDate = o.DeliveryDate,
                            OrderProgressStatus = o.OrderProgressStatus
                        });

                    aResp = new APIResponse
                    {
                        Message = "Tous les éléments ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = shopOrderLists,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun article n'a été récupéré.",
                        Status = "Échec!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetShopsOrders");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public async Task<APIResponse> ChangeOrderStatus(ChangeOrderStatus _order)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.UserId == _order.UserId && ult.IsActive == true);
            LogManager.LogInfo("ChangeOrderStatus: " + _order);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("ChangeOrderStatus: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    Order foundOrder = DbContext.Orders.Where(a => a.OrderID == _order.OrderId).FirstOrDefault();
                    if (foundOrder != null)
                    {
                        DateTime NowDate = DateTime.Now;
                        foundOrder.LastEditedBy = IsUserLoggedIn.UserId;
                        foundOrder.LastEditedDate = NowDate;
                        foundOrder.OrderProgressStatus = _order.ProgressStatus;

                        DbContext.Orders.Update(foundOrder);
                        DbContext.SaveChanges();

                        //In Progress to Ready for Delivery and send push notification 
                        if (_order.ProgressStatus == OrderProgressStatus.READYFORDELIVERY)
                        {
                            DateTime RegistrationDate = DateTime.Now;
                            DeliveryUserOrder obj = new DeliveryUserOrder
                            {
                                OrderID = _order.OrderId,
                                DeliveryStatus = DeliveryStatus.PENDING,
                                DeliveryUserId = Guid.Empty,

                                CreatedDate = RegistrationDate,
                                CreatedBy = IsUserLoggedIn.UserId,
                                IsEnabled = true,
                                IsEnabledBy = IsUserLoggedIn.UserId,
                                DateEnabled = RegistrationDate
                            };

                            DbContext.DeliveryUserOrders.Add(obj);
                            DbContext.SaveChanges();

                            //Send Push notification to customer
                            await SendOrderNotificationAsync(foundOrder.OrderID, foundOrder.UserId, foundOrder.OrderReferenceID, "Ready for delivery");
                            
                           //Send Push notification to driver - Not needed
                           //await SendNotificationAsync(foundOrder.ShopId, foundOrder.DeliveryMethod, _order.OrderId);

                        }

                        //create delivery job
                        if (_order.ProgressStatus == OrderProgressStatus.OUTFORDELIVERY)
                        {
                            //TO DO Integrate Stuart API
                        }

                        aResp = new APIResponse
                        {
                            Message = "La progression de la commande a été mise à jour avec succès",
                            Status = "Succès!",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Aucun enregistrement trouvé.",
                            Status = "Échec!",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("ChangeOrderStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse CancelOrder(string Auth, int _orderId)
        {

            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == Auth && ult.IsActive == true);
            LogManager.LogInfo("CancelOrder by userid: " + IsUserLoggedIn.UserId + " with OrderID:" + _orderId.ToString());

            try
            {
                if (IsUserLoggedIn != null)
                {
                    Order userOrder = DbContext.Orders.FirstOrDefault(o => o.OrderID == _orderId && o.UserId == IsUserLoggedIn.UserId);
                    Payment PayTrans = DbContext.Payments.FirstOrDefault(p => p.OrderId == _orderId);

                    if (userOrder != null)
                    {
                        userOrder.OrderProgressStatus = OrderProgressStatus.CANCELORDER;
                        userOrder.LastEditedBy = IsUserLoggedIn.UserId;
                        userOrder.LastEditedDate = DateTime.Now;

                        //check if the payment for the order was a success
                        // if its true intiate refund
                        if (PayTrans.Status == EPaymentStatus.SUCCEEDED)
                        {
                            //RefundDTO initRefund = RefundCardPayin(PayTrans);
                            PaymentTransaction newPaytrans = new PaymentTransaction
                            {
                                UserId = IsUserLoggedIn.UserId,
                                CreatedBy = IsUserLoggedIn.UserId,
                                CreatedDate = DateTime.Now,
                                DateEnabled = DateTime.Now,
                                IsEnabled = true,
                                IsEnabledBy = IsUserLoggedIn.UserId,
                                LastEditedBy = IsUserLoggedIn.UserId,
                                LastEditedDate = DateTime.Now,
                                PaymentStatus = EPaymentStatus.NotSpecified, // not specified is refunded
                                //PaymentToken = JsonConvert.SerializeObject(initRefund),
                                OrderId = userOrder.OrderID,
                                //TransactionCode = initRefund.Id.ToString()
                            };
                            DbContext.PaymentTransactions.Add(newPaytrans);
                        }

                        DbContext.Update(userOrder);
                        DbContext.SaveChanges();

                        aResp = new APIResponse
                        {
                            Message = "Commande Annulée!",
                            Status = "Succès!",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                        ;
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Id de commande invalide récupéré	Id de commande invalide récupéré.",
                            Status = "Échec!",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
                else
                {
                    aResp.Message = "Utilisateur non connecté";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("CancelOrder by userid: " + IsUserLoggedIn.UserId + " ERROR!");
                LogManager.LogDebugObject(ex);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex);
            }
            return aResp;

        }
        public APIResponse UpdateOrderStatus(ChangeOrderStatus order)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.UserId == order.UserId && ult.IsActive == true);
            LogManager.LogInfo("UpdateOrderStatus: " + order);
            try
            {
                if (IsUserLoggedIn != null)
                {
                    Order orderDetail = DbContext.Orders.Where(a => a.OrderID == order.OrderId).FirstOrDefault();
                    if (orderDetail != null)
                    {
                        DateTime NowDate = DateTime.Now;
                        orderDetail.LastEditedBy = IsUserLoggedIn.UserId;
                        orderDetail.LastEditedDate = NowDate;
                        orderDetail.OrderProgressStatus = order.ProgressStatus;

                        DbContext.Orders.Update(orderDetail);
                        DbContext.SaveChanges();

                        //create delivery user order
                        if (order.ProgressStatus == OrderProgressStatus.OUTFORDELIVERY)
                        {
                            DateTime RegistrationDate = DateTime.Now;
                            DeliveryUserOrder obj = new DeliveryUserOrder
                            {
                                OrderID = order.OrderId,
                                DeliveryStatus = DeliveryStatus.PENDING,
                                DeliveryUserId = Guid.Empty,

                                CreatedDate = RegistrationDate,
                                CreatedBy = IsUserLoggedIn.UserId,
                                IsEnabled = true,
                                IsEnabledBy = IsUserLoggedIn.UserId,
                                DateEnabled = RegistrationDate
                            };

                            DbContext.DeliveryUserOrders.Add(obj);
                            DbContext.SaveChanges();

                            //Send Push notication to driver
                        }

                        aResp = new APIResponse
                        {
                            Message = "La progression de la commande a été mise à jour avec succès",
                            Status = "Succès!",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Aucun enregistrement trouvé.",
                            Status = "Échec!",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
                else
                {
                    aResp.Message = "Utilisateur non connecté";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("UpdateOrderStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public async Task<APIResponse> TestPushNotifcation(string fcmToken)
        {
            APIResponse aResp = new APIResponse();
            try
            {
                LogManager.LogInfo("TestPushNotifcation - " + fcmToken);
                
                var fcmUrl = APIConfig.PNConfig.FireBLink;
                LogManager.LogInfo("fcmUrl - " + fcmUrl);
                var title = "Statut de la commande";
                LogManager.LogInfo("PushNotifMessages - " + APIConfig.PushNotifMessages.OrderStatus);
                var body = String.Format(APIConfig.PushNotifMessages.OrderStatus, "123456", "Ready for delivery -Testing");
                Dictionary<string, string> customvalue = new Dictionary<string, string>();
                customvalue.Add("orderId", "1");
                int accountType =  Convert.ToInt16(AccountTypes.APPUSER);
                customvalue.Add("accountType", accountType.ToString());

                LogManager.LogInfo("orderId - 1");
                var serverKey = APIConfig.PNConfig.AndroidFireBKey;
                var senderId = APIConfig.PNConfig.AndroidSenderID;
                //string serverKey = "AAAAWXk4A74:APA91bFLKd1cMnJeaPzOS7nDR35KcNdxWVgkMT_N9uFHsblM6Y06Iw4EuxQRCZReDoKyIsOkGbRaFfmpF_ttaVz4JhbjdQfasYnOZAXCU74IA3GKMsu1MuqimFwz_iVtJzz936R7kPL0";
                LogManager.LogInfo("serverKey - " + serverKey);

                //string senderId = "384285803454";

                LogManager.LogInfo("senderId - " + senderId);
                var isSent = await PharmaMoov.API.Helpers.PushNotification.SendNotifications(serverKey, senderId, fcmToken, title, body, fcmUrl, customvalue);
                LogManager.LogInfo("isSent - " + isSent);
                if (isSent)
                {
                    aResp = new APIResponse
                    {
                        Message = "Succès",
                        Status = "Succès!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Fail",
                        Status = "Fail!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                return aResp;
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("TestPushNotifcation");
                LogManager.LogError("StackTrace" + ex.StackTrace);
                LogManager.LogError("Message" + ex.Message);
                LogManager.LogError("InnerException" + ex.InnerException.Message);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;

        }
        public async Task SendOrderNotificationAsync(int orderId, Guid userId, string orderReferenceID, string status)
        {
            var userDeviceList = DbContext.UserDevices.Where(s => s.UserId == userId && s.IsEnabled == true).ToList();
            if (userDeviceList.Count() > 0)
            {
                var userDetails = DbContext.Users.FirstOrDefault(s => s.UserId == userId);
                int accountType = userDetails != null ? Convert.ToInt16(userDetails.AccountType) : 0;
                var fcmUrl = APIConfig.PNConfig.FireBLink;
                var title = "Statut de la commande";
                var body = String.Format(APIConfig.PushNotifMessages.OrderStatus, orderReferenceID, status);
                Dictionary<string, string> customvalue = new Dictionary<string, string>();
                customvalue.Add("orderId", orderId.ToString());
                customvalue.Add("accountType", accountType.ToString());
                //string serverKey = "AAAAWXk4A74:APA91bFLKd1cMnJeaPzOS7nDR35KcNdxWVgkMT_N9uFHsblM6Y06Iw4EuxQRCZReDoKyIsOkGbRaFfmpF_ttaVz4JhbjdQfasYnOZAXCU74IA3GKMsu1MuqimFwz_iVtJzz936R7kPL0";
                //string senderId = "384285803454";
                string serverKey = APIConfig.PNConfig.AndroidFireBKey;
                string senderId = APIConfig.PNConfig.AndroidSenderID;

                foreach (var item in userDeviceList)
                {
                    //string serverKey = null, senderId = null;
                    //if (item.DeviceType == DevicePlatforms.Android)
                    //{
                    //    //send android notification
                    //    serverKey = APIConfig.PNConfig.AndroidFireBKey;
                    //    senderId = APIConfig.PNConfig.AndroidSenderID;
                    //}
                    //else if (item.DeviceType == DevicePlatforms.IOS)
                    //{
                    //    //send ios notification
                    //    serverKey = APIConfig.PNConfig.IOSFireBKey;
                    //    senderId = APIConfig.PNConfig.IOSSenderID;
                    //}
                    
                    await PharmaMoov.API.Helpers.PushNotification.SendNotifications(serverKey, senderId, item.DeviceFCMToken, title, body, fcmUrl, customvalue);
                }
            }
        }


        //private async Task SendNotificationAsync(Guid shopId, DeliveryMethod deliveryMethod, int orderId)
        //{
        //    var shopDetail = DbContext.Shops.FirstOrDefault(s => s.ShopId == shopId);
        //    if (shopDetail != null)
        //    {

        //        var deliveryUserList = (from du in DbContext.DeliveryUserLocation
        //                                join u in DbContext.Users on du.DeliveryUserId equals u.UserId
        //                                join ud in DbContext.UserDevices on u.UserId equals ud.UserId
        //                                where du.ReceiveOrder == true && u.MethodDelivery == deliveryMethod && u.AccountType == AccountTypes.COURIER && (u.RegistrationPlatform == DevicePlatforms.Android || u.RegistrationPlatform == DevicePlatforms.IOS)
        //                                select new DeliveryUserNotificationModel
        //                                {
        //                                     DeliveryUserId = du.DeliveryUserId,
        //                                     Latitude = du.Latitude,
        //                                     Longitude = du.Longitude,
        //                                     DevicePlatforms = u.RegistrationPlatform,
        //                                     DeviceFCMToken = ud.DeviceFCMToken
        //                                }).ToList();

        //        deliveryUserList = deliveryUserList.Select(s =>
        //        {
        //            //calculate distance
        //            var distance = new Coordinates((double)shopDetail.Latitude, (double)shopDetail.Longitude)
        //            .DistanceTo(
        //                new Coordinates((double)s.Latitude, (double)s.Longitude)
        //            );
        //            s.Distance = decimal.Round((decimal)distance, 2, MidpointRounding.AwayFromZero);
        //            return s;
        //        }).ToList();

        //        decimal IsWithinDistance = 2;// display delivery user within 2 km 

        //        deliveryUserList = deliveryUserList.Where(w => w.Distance <= IsWithinDistance).OrderBy(x => x.Distance).ToList();
        //        if (deliveryUserList.Count() > 0)
        //        {
        //            var fcmUrl = APIConfig.PNConfig.FireBLink;
        //            var title = "Statut de la commande";
        //            var body = String.Format(APIConfig.PushNotifMessages.OrderStatus, shopDetail.ShopName);
        //            var androidUserList = deliveryUserList.Where(s => s.DevicePlatforms == DevicePlatforms.Android).Select(s => s.DeviceFCMToken).ToList();
        //            Dictionary<string, string> customvalue = new Dictionary<string, string>();
        //            customvalue.Add("orderId", orderId.ToString());
        //            if (androidUserList.Count() > 0)
        //            {
        //                //send android notification
        //                var serverKey = APIConfig.PNConfig.AndroidFireBKey;
        //                var senderId = APIConfig.PNConfig.AndroidSenderID;
        //                foreach(var item in androidUserList)
        //                {
        //                    await PushNotification.SendNotifications(serverKey, senderId, item, title, body, fcmUrl, customvalue);
        //                }
        //            }

        //            var iosUserList = deliveryUserList.Where(s => s.DevicePlatforms == DevicePlatforms.IOS).Select(s => s.DeviceFCMToken).ToList();
        //            if (iosUserList.Count() > 0)
        //            {
        //                //send ios notification
        //                var serverKey = APIConfig.PNConfig.IOSFireBKey;
        //                var senderId = APIConfig.PNConfig.IOSSenderID;
        //                foreach (var item in iosUserList)
        //                {
        //                    await PushNotification.SendNotifications(serverKey, senderId, item, title, body, fcmUrl, customvalue);
        //                }
        //            }
        //        }
        //    }
        //}


        //public RefundDTO RefundCardPayin(Payment PaymentTrans) 
        //{
        //    RefundPayInPostDTO payload = CreateRefundPayInPayload(PaymentTrans);
        //    MangoPayApi api = new MangoPayApi();
        //    api.Config.ClientId = APIConfig.PaymentConfig.ClientId;
        //    api.Config.ClientPassword = APIConfig.PaymentConfig.ApiKey;
        //    api.Config.BaseUrl = APIConfig.PaymentConfig.BaseUrl;
        //    return api.PayIns.CreateRefund(JsonConvert.DeserializeObject<PayInCardWebDTO>(PaymentTrans.CreatePayload).Id, payload);
        //}

        //public RefundPayInPostDTO CreateRefundPayInPayload(Payment PaymentTrans) 
        //{
        //    return new RefundPayInPostDTO( 
        //        PaymentTrans.AuthorId,
        //        new MangoPay.SDK.Entities.Money { 
        //            Amount = PaymentTrans.FeesAmount,
        //            Currency = MangoPay.SDK.Core.Enumerations.CurrencyIso.EUR
        //        },
        //        new MangoPay.SDK.Entities.Money {
        //            Amount = PaymentTrans.DebitedFundsAmount,
        //            Currency = MangoPay.SDK.Core.Enumerations.CurrencyIso.EUR
        //        }
        //    );        
        //} 
    }
}
