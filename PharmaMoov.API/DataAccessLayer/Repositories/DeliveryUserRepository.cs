using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using System;
using PharmaMoov.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using PharmaMoov.Models.DeliveryUser;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using MangoPay.SDK.Entities.POST;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class DeliveryUserRepository : APIBaseRepo, IDeliveryUserRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        private readonly LocalizationService localization;
        private IHttpContextAccessor accessor;
        private IMainHttpClient MainHttpClient { get; }
        IPaymentRepository IPaymentRepo { get; }
        public DeliveryUserRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, IPaymentRepository _promoRep, APIConfigurationManager _apiCon, IHttpContextAccessor _accessor, LocalizationService _localization, IMainHttpClient _mhttpc)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            localization = _localization;
            accessor = _accessor;
            MainHttpClient = _mhttpc;
            IPaymentRepo = _promoRep;
        }
        public APIResponse UpdateLocation(string auth, UpdateLocationParamModel model)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == auth && ult.IsActive == true);
            LogManager.LogInfo("UpdateLocation");

            try
            {
                if (IsUserLoggedIn != null)
                {
                    var isDriveruser = DbContext.Users.AsNoTracking().Where(s => s.UserId == IsUserLoggedIn.UserId && s.AccountType == AccountTypes.COURIER && s.IsEnabled == true).Any();
                    if (isDriveruser)
                    {
                        var deliveryUserDetail = DbContext.DeliveryUserLocation.FirstOrDefault(s => s.DeliveryUserId == IsUserLoggedIn.UserId && s.IsEnabled == true);
                        if (deliveryUserDetail != null) // Existing delivery user
                        {
                            deliveryUserDetail.Latitude = model.Latitude;
                            deliveryUserDetail.Longitude = model.Longitude;
                            deliveryUserDetail.LastEditedDate = DateTime.Now;
                            deliveryUserDetail.LastEditedBy = IsUserLoggedIn.UserId;

                            DbContext.DeliveryUserLocation.Update(deliveryUserDetail);
                            DbContext.SaveChanges();

                            aResp.Message = "Emplacement mis à jour avec succès";
                            aResp.Status = "Succès";
                            aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        }
                        else //New delivery user
                        {
                            DateTime RegistrationDate = DateTime.Now;

                            DeliveryUserLocation deliveryUserLocation = new DeliveryUserLocation
                            {
                                Latitude = model.Latitude,
                                Longitude = model.Longitude,
                                DeliveryUserId = IsUserLoggedIn.UserId,
                                ReceiveOrder = false,

                                CreatedDate = RegistrationDate,
                                CreatedBy = IsUserLoggedIn.UserId,
                                IsEnabled = true,
                                IsEnabledBy = IsUserLoggedIn.UserId,
                                DateEnabled = RegistrationDate,
                            };

                            DbContext.DeliveryUserLocation.Add(deliveryUserLocation);
                            DbContext.SaveChanges();

                            aResp = new APIResponse
                            {
                                Message = "Emplacement mis à jour avec succès",
                                Status = "Succès!",
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                    }
                    else
                    {
                        aResp.Message = "L'utilisateur n'est pas le livreur";
                        aResp.Status = "Échec";
                        aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
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
                LogManager.LogInfo("UpdateLocation");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetNewOrderList(string auth, Guid deliveryUserId, int pageNo)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == auth && ult.IsActive == true);
            LogManager.LogInfo("GetNewOrderList");
            try
            {
                if (IsUserLoggedIn != null)
                {
                    var deliveryUserDetail = DbContext.DeliveryUserLocation.FirstOrDefault(s => s.DeliveryUserId == deliveryUserId);
                    if (deliveryUserDetail != null)
                    {
                        var userDetail = DbContext.Users.AsNoTracking().FirstOrDefault(s => s.UserId == deliveryUserId);

                        var orderList = (from du in DbContext.DeliveryUserOrders
                                         join o in DbContext.Orders on du.OrderID equals o.OrderID
                                         join s in DbContext.Shops on o.ShopId equals s.ShopId
                                         where du.DeliveryStatus == DeliveryStatus.PENDING && du.DeliveryUserId == Guid.Empty && o.DeliveryMethod == userDetail.MethodDelivery && o.OrderProgressStatus == OrderProgressStatus.READYFORDELIVERY
                                         select new OrderListModel
                                         {
                                             ShopId = o.ShopId,
                                             ShopName = s.ShopName,
                                             ShopAddress = (s.Address ?? "") + " " + (s.SuiteAddress ?? "") + " " + (s.PostalCode ?? "") + " " + (s.City ?? ""),
                                             Latitude = s.Latitude,
                                             Longitude = s.Longitude,
                                             OrderID = o.OrderID,
                                             CustomerName = o.CustomerName,
                                             OrderReferenceID = o.OrderReferenceID,
                                             OrderProgressStatus = o.OrderProgressStatus,
                                             DeliveryDate = o.DeliveryDate,
                                             PatientId = o.PatientId,
                                             UserId = o.UserId,
                                             DeliveryAddressId = o.DeliveryAddressId
                                         }).ToList();

                        if (orderList.Count() > 0)
                        {
                            orderList = orderList.Select(s =>
                            {
                                //calculate distance
                                var distance = new Coordinates((double)deliveryUserDetail.Latitude, (double)deliveryUserDetail.Longitude)
                                    .DistanceTo(
                                        new Coordinates((double)s.Latitude, (double)s.Longitude)
                                    );

                                s.Distance = decimal.Round((decimal)distance, 2, MidpointRounding.AwayFromZero);
                                return s;
                            }).ToList();

                            decimal IsWithinDistance = 2;// display shops within 2 km 

                            orderList = orderList
                                .Where(w => w.Distance <= IsWithinDistance).OrderBy(x => x.Distance).ToList();

                            var pageSize = 10;
                            var paggedList = orderList.Where(w => w.OrderID != 0, pageNo, pageSize).ToList();

                            if (paggedList.Count() > 0)
                            {
                                // get total page count
                                var totalPageCount = 1;
                                var pageCount = orderList.Count;
                                if (pageCount > 0 && pageCount >= pageSize)
                                {
                                    totalPageCount = (int)Math.Ceiling((double)pageCount / pageSize);
                                }

                                paggedList = paggedList.Select(s =>
                                {
                                    //Get User address
                                    if (s.PatientId != null && s.PatientId != Guid.Empty)
                                    {
                                        var patientDetail = DbContext.Patients.FirstOrDefault(x => x.PatientId == s.PatientId);
                                        if (patientDetail != null)
                                        {
                                            s.DeliveryAddress = patientDetail.Street + ", " + patientDetail.POBox + ", " + patientDetail.City;
                                        }
                                    }
                                    else
                                    {
                                        var customerAddress = DbContext.UserAddresses.FirstOrDefault(x => x.UserAddressID == s.DeliveryAddressId);
                                        if (customerAddress != null)
                                        {
                                            s.DeliveryAddress = customerAddress.Street + ", " + customerAddress.Building + ", " + customerAddress.Area + ", " + customerAddress.City;
                                        }
                                    }
                                    return s;
                                }).ToList();

                                aResp.Message = "Tous les éléments ont été récupérés avec succès.";
                                aResp.Status = "Succès";
                                aResp.Payload = new
                                {
                                    OrderList = paggedList,
                                    PageCount = totalPageCount
                                };
                                aResp.StatusCode = System.Net.HttpStatusCode.OK;
                            }
                            else
                            {
                                aResp.Message = "Aucun article n'a été récupéré.";
                                aResp.Status = "Échec";
                                aResp.StatusCode = System.Net.HttpStatusCode.OK;
                            }
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
                LogManager.LogInfo("GetNewOrderList");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetOrderDetail(int orderId)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetOrderDetail");
            try
            {
                var orderDetail = GetOrderInfo(orderId);
                if (orderDetail != null)
                {
                    aResp.Message = "Tous les éléments ont été récupérés avec succès.";
                    aResp.Status = "Succès";
                    aResp.Payload = orderDetail;
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    aResp.Message = "Aucun article n'a été récupéré.";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetOrderDetail");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public async Task<APIResponse> AcceptOrder(AcceptOrderParamModel model)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("AcceptOrder");
            try
            {
                var isDriveruser = DbContext.Users.AsNoTracking().Where(s => s.UserId == model.DeliveryUserId && s.AccountType == AccountTypes.COURIER && s.IsEnabled == true).Any();
                if (isDriveruser)
                {
                    var isAcceptedOrder = DbContext.DeliveryUserOrders.Where(s => s.OrderID == model.OrderId && s.DeliveryStatus == DeliveryStatus.ACCEPT).Any();
                    if(!isAcceptedOrder)
                    {
                        var foundOrder = DbContext.Orders.FirstOrDefault(s => s.OrderID == model.OrderId);
                        if (foundOrder != null)
                        {
                            //Out for Delivery status will be updated by delivery person once order is accepted 
                            foundOrder.OrderProgressStatus = OrderProgressStatus.OUTFORDELIVERY;
                            foundOrder.LastEditedDate = DateTime.Now;
                            foundOrder.LastEditedBy = model.DeliveryUserId;

                            DbContext.Orders.Update(foundOrder);
                            DbContext.SaveChanges();

                            var orderDetail = DbContext.Orders.AsNoTracking()
                                        .Join(DbContext.Shops, o => o.ShopId, s => s.ShopId, (o, s) => new { o, s })
                                        .Where(x => x.o.OrderID == model.OrderId)
                                        .Select(d => new OrderListModel
                                        {
                                            ShopId = d.o.ShopId,
                                            ShopName = d.s.ShopName,
                                            ShopAddress = (d.s.Address ?? "") + " " + (d.s.SuiteAddress ?? "") + " " + (d.s.PostalCode ?? "") + " " + (d.s.City ?? ""),
                                            Latitude = d.s.Latitude,
                                            Longitude = d.s.Longitude,
                                            OrderID = d.o.OrderID,
                                            CustomerName = d.o.CustomerName,
                                            OrderReferenceID = d.o.OrderReferenceID,
                                            OrderProgressStatus = d.o.OrderProgressStatus,
                                            DeliveryDate = d.o.DeliveryDate,
                                            PatientId = d.o.PatientId,
                                            UserId = d.o.UserId,
                                            DeliveryAddressId = d.o.DeliveryAddressId
                                        }).FirstOrDefault();

                            if (orderDetail.PatientId != null && orderDetail.PatientId != Guid.Empty)
                            {
                                var patientDetail = DbContext.Patients.FirstOrDefault(x => x.PatientId == orderDetail.PatientId);
                                if (patientDetail != null)
                                {
                                    orderDetail.DeliveryAddress = patientDetail.Street + ", " + patientDetail.POBox + ", " + patientDetail.City;
                                    orderDetail.MobileNumber = patientDetail.MobileNumber;
                                }
                            }
                            else
                            {
                                var customerAddress = DbContext.UserAddresses.FirstOrDefault(x => x.UserAddressID == orderDetail.DeliveryAddressId);
                                if (customerAddress != null)
                                {
                                    orderDetail.DeliveryAddress = customerAddress.Street + ", " + customerAddress.Building + ", " + customerAddress.Area + ", " + customerAddress.City;
                                    orderDetail.MobileNumber = DbContext.Users.FirstOrDefault(x => x.UserId == orderDetail.UserId).MobileNumber;
                                }
                            }

                            var deliveryUserOrderDetail = DbContext.DeliveryUserOrders.FirstOrDefault(s => s.OrderID == model.OrderId);
                            if (deliveryUserOrderDetail != null)
                            {
                                //Update delivery User Order detail
                                deliveryUserOrderDetail.DeliveryUserId = model.DeliveryUserId;
                                deliveryUserOrderDetail.DeliveryStatus = DeliveryStatus.ACCEPT;
                                deliveryUserOrderDetail.LastEditedDate = DateTime.Now;
                                deliveryUserOrderDetail.LastEditedBy = model.DeliveryUserId;

                                DbContext.DeliveryUserOrders.Update(deliveryUserOrderDetail);
                                DbContext.SaveChanges();

                                //reuse order repository to send out of delivery notification
                                var orderRepo = new OrderRepository(DbContext, LogManager, APIConfig, accessor, localization, MainHttpClient);
                                await orderRepo.SendOrderNotificationAsync(foundOrder.OrderID, foundOrder.UserId, foundOrder.OrderReferenceID, "Out for delivery");

                                aResp.Message = "La commande a bien été acceptée.";
                                aResp.Status = "Succès";
                                aResp.Payload = orderDetail;
                                aResp.StatusCode = System.Net.HttpStatusCode.OK;
                            }
                            else
                            {
                                aResp.Message = "Aucun article n'a été récupéré.";
                                aResp.Status = "Échec";
                                aResp.StatusCode = System.Net.HttpStatusCode.OK;
                            }
                        }
                        else
                        {
                            aResp.Message = "Aucun article n'a été récupéré.";
                            aResp.Status = "Échec";
                            aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        }
                    }
                    else
                    {
                        aResp.Message = "Cette commande est déjà acceptée. Veuillez accepter une autre commande.";
                        aResp.Status = "Échec";
                        aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    }                   
                }
                else
                {
                    aResp.Message = "L'utilisateur n'est pas le livreur";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AcceptOrder");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse UpdateOrderStatus(UpdateOrderStatusParamModel model)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("UpdateOrderStatus");
            try
            {
                var isDriveruser = DbContext.Users.AsNoTracking().Where(s => s.UserId == model.DeliveryUserId && s.AccountType == AccountTypes.COURIER && s.IsEnabled == true).Any();
                if (isDriveruser)
                {
                    if (model.OrderStatus != OrderProgressStatus.COMPLETED && model.OrderStatus != OrderProgressStatus.CANCELORDER)
                    {
                        aResp = new APIResponse
                        {
                            Message = "Veuillez fournir le statut de la commande terminée/annulée.",
                            Status = "Échec!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                        return aResp;
                    }

                    var orderDetail = DbContext.Orders.FirstOrDefault(o => o.OrderID == model.OrderId);
                    if (orderDetail != null)
                    {
                        orderDetail.OrderProgressStatus = model.OrderStatus;
                        orderDetail.LastEditedDate = DateTime.Now;
                        orderDetail.LastEditedBy = model.DeliveryUserId;
                        DbContext.Orders.Update(orderDetail);
                        DbContext.SaveChanges();

                        var deliveryUserOrderDetail = DbContext.DeliveryUserOrders.FirstOrDefault(s => s.OrderID == model.OrderId);
                        if (model.OrderStatus == OrderProgressStatus.COMPLETED)
                        {
                            deliveryUserOrderDetail.DeliveryStatus = DeliveryStatus.DELIVERED;


                            // get the mangopay user data
                            MangoPayUser mPayUser = DbContext.MangoPayUsers.FirstOrDefault(mpu => mpu.PharmaMUserId == model.DeliveryUserId);
                            // get userinfo first
                            User currentUser = DbContext.Users.FirstOrDefault(u => u.UserId == model.DeliveryUserId);
                            if (mPayUser == null)
                            {
                                //user is not yet linked to a mango pay                    
                                APIResponse MangoPayUserCreation = IPaymentRepo.CreateUserNaturalUserInMangoPay(currentUser);
                                if (MangoPayUserCreation.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    mPayUser = (MangoPayUser)MangoPayUserCreation.Payload;
                                }
                                else
                                {
                                    return MangoPayUserCreation;
                                }
                            }

                            // get the mangopay shop data
                            MangoPayShop mShop = DbContext.MangoPayShops.FirstOrDefault(mpu => mpu.PharmaShopId == orderDetail.ShopId);
                            // get userinfo first
                            //User currentDelivery = DbContext.Users.FirstOrDefault(u => u.UserId == orderDetail.UserId);
                            //if (mShop == null)
                            //{
                            //    //user is not yet linked to a mango pay                    
                            //    APIResponse MangoPayUserCreation = IPaymentRepo.CreateUserNaturalUserInMangoPay(currentUser);
                            //    if (MangoPayUserCreation.StatusCode == System.Net.HttpStatusCode.OK)
                            //    {
                            //        mPayUser = (MangoPayUser)MangoPayUserCreation.Payload;
                            //    }
                            //    else
                            //    {
                            //        return MangoPayUserCreation;
                            //    }
                            //}

                            TransferPostDTO fund = IPaymentRepo.BuildFundDeliveryModel(orderDetail.OrderDeliveryFee, orderDetail.OrderID, mShop, mPayUser);
                            PaymentDelivery paymentCreated = BuildFundDeliveryModel(fund, orderDetail.OrderID);

                            DbContext.PaymentDelivery.Update(paymentCreated);
                            DbContext.SaveChanges();

                        }
                        else if (model.OrderStatus == OrderProgressStatus.CANCELORDER)
                        {
                            deliveryUserOrderDetail.DeliveryStatus = DeliveryStatus.CANCELLED;
                        }

                        deliveryUserOrderDetail.LastEditedDate = DateTime.Now;
                        deliveryUserOrderDetail.LastEditedBy = model.DeliveryUserId;

                        DbContext.DeliveryUserOrders.Update(deliveryUserOrderDetail);
                        DbContext.SaveChanges();

                        aResp.Message = "État de la commande mis à jour avec succès";
                        aResp.Status = "Succès";
                        aResp.Payload = GetOrderInfo(model.OrderId);
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        aResp.Message = "Aucun article n'a été récupéré.";
                        aResp.Status = "Échec";
                        aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    aResp.Message = "L'utilisateur n'est pas le livreur";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("UpdateOrderStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public PaymentDelivery BuildFundDeliveryModel(TransferPostDTO detail, int orderId)
        {
            LogManager.LogInfo("Build Delivery Payment Model based on return object:");
            LogManager.LogDebugObject(detail);

            PaymentDelivery model = new PaymentDelivery();
            model.OrderId = orderId;
            model.AuthorId = detail.AuthorId;
            model.CreditedUserId = detail.CreditedUserId;
            model.DebitedFundsCurrency = detail.DebitedFunds.Currency;
            model.DebitedFunds = detail.DebitedFunds.Amount;
            model.FeesCurrency = detail.Fees.Currency;
            model.Fees = detail.Fees.Amount;
            model.DebitedWalletId = detail.DebitedWalletId;
            model.CreditedWalletId = detail.CreditedWalletId;
            model.Status = EPaymentStatus.CREATED;
            LogManager.LogInfo("PaymentDelivery return object:");
            LogManager.LogDebugObject(model);

            return model;
        }
        public APIResponse UpdateReceiveOrder(UpdateReceiveOrderParamModel model)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("UpdateReceiveOrder");
            try
            {
                var deliveryUserDetail = DbContext.DeliveryUserLocation.FirstOrDefault(s => s.DeliveryUserId == model.DeliveryUserId && s.IsEnabled == true);
                if (deliveryUserDetail != null)
                {
                    deliveryUserDetail.ReceiveOrder = model.ReceiveOrder;
                    deliveryUserDetail.LastEditedDate = DateTime.Now;
                    deliveryUserDetail.LastEditedBy = model.DeliveryUserId;

                    DbContext.DeliveryUserLocation.Update(deliveryUserDetail);
                    DbContext.SaveChanges();

                    aResp.Message = "Recevoir la commande mise à jour avec succès";
                    aResp.Status = "Succès";
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;

                }
                else
                {
                    aResp.Message = "Aucun article n'a été récupéré.";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("UpdateReceiveOrder");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetOrdersList(OrderListParamModel model)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetOrdersList");
            try
            {
                if (model.DeliveryStatus != DeliveryStatus.DELIVERED && model.DeliveryStatus != DeliveryStatus.CANCELLED)
                {
                    aResp = new APIResponse
                    {
                        Message = "Veuillez fournir le statut de la livraison livrée/annulée.",
                        Status = "Échec!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                    return aResp;
                }

                var orderList = (from du in DbContext.DeliveryUserOrders
                                 join o in DbContext.Orders on du.OrderID equals o.OrderID
                                 join s in DbContext.Shops on o.ShopId equals s.ShopId
                                 where du.DeliveryStatus == model.DeliveryStatus && du.DeliveryUserId == model.DeliveryUserId
                                 select new OrderListModel
                                 {
                                     ShopId = o.ShopId,
                                     ShopName = s.ShopName,
                                     ShopAddress = (s.Address ?? "") + " " + (s.SuiteAddress ?? "") + " " + (s.PostalCode ?? "") + " " + (s.City ?? ""),
                                     Latitude = s.Latitude,
                                     Longitude = s.Longitude,
                                     OrderID = o.OrderID,
                                     CustomerName = o.CustomerName,
                                     OrderReferenceID = o.OrderReferenceID,
                                     OrderProgressStatus = o.OrderProgressStatus,
                                     DeliveryDate = o.DeliveryDate,
                                     PatientId = o.PatientId,
                                     UserId = o.UserId,
                                     DeliveryAddressId = o.DeliveryAddressId
                                 }).ToList();

                var pageSize = 10;
                var paggedList = orderList.Where(w => w.OrderID != 0, model.PageNo, pageSize).ToList();
                if (paggedList.Count() > 0)
                {
                    // get total page count
                    var totalPageCount = 1;
                    var pageCount = orderList.Count;
                    if (pageCount > 0 && pageCount >= pageSize)
                    {
                        totalPageCount = (int)Math.Ceiling((double)pageCount / pageSize);
                    }

                    paggedList = paggedList.Select(s =>
                    {
                        //Get User address
                        if (s.PatientId != null && s.PatientId != Guid.Empty)
                        {
                            var patientDetail = DbContext.Patients.FirstOrDefault(x => x.PatientId == s.PatientId);
                            if (patientDetail != null)
                            {
                                s.DeliveryAddress = patientDetail.Street + ", " + patientDetail.POBox + ", " + patientDetail.City;
                            }
                        }
                        else
                        {
                            var customerAddress = DbContext.UserAddresses.FirstOrDefault(x => x.UserAddressID == s.DeliveryAddressId);
                            if (customerAddress != null)
                            {
                                s.DeliveryAddress = customerAddress.Street + ", " + customerAddress.Building + ", " + customerAddress.Area + ", " + customerAddress.City;
                            }
                        }
                        return s;
                    }).ToList();

                    aResp.Message = "Tous les éléments ont été récupérés avec succès.";
                    aResp.Status = "Succès";
                    aResp.Payload = new
                    {
                        OrderList = paggedList,
                        PageCount = totalPageCount
                    };
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    aResp.Message = "Aucun article n'a été récupéré.";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                }

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetOrdersList");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetAcceptedOrderDetail(string auth, Guid deliveryUserId)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == auth && ult.IsActive == true);
            LogManager.LogInfo("GetAcceptedOrderDetail");
            try
            {
                if (IsUserLoggedIn != null)
                {
                    var deliveryUserDetail = DbContext.DeliveryUserLocation.FirstOrDefault(s => s.DeliveryUserId == deliveryUserId);
                    if (deliveryUserDetail != null)
                    {
                        var userDetail = DbContext.Users.AsNoTracking().FirstOrDefault(s => s.UserId == deliveryUserId);

                        var orderDetail = (from du in DbContext.DeliveryUserOrders
                                           join o in DbContext.Orders on du.OrderID equals o.OrderID
                                           join s in DbContext.Shops on o.ShopId equals s.ShopId
                                           where du.DeliveryUserId == deliveryUserId && du.DeliveryStatus == DeliveryStatus.ACCEPT && o.DeliveryMethod == userDetail.MethodDelivery
                                           select new OrderListModel
                                           {
                                               ShopId = o.ShopId,
                                               ShopName = s.ShopName,
                                               ShopAddress = (s.Address ?? "") + " " + (s.SuiteAddress ?? "") + " " + (s.PostalCode ?? "") + " " + (s.City ?? ""),
                                               Latitude = s.Latitude,
                                               Longitude = s.Longitude,
                                               OrderID = o.OrderID,
                                               CustomerName = o.CustomerName,
                                               OrderReferenceID = o.OrderReferenceID,
                                               OrderProgressStatus = o.OrderProgressStatus,
                                               DeliveryDate = o.DeliveryDate,
                                               PatientId = o.PatientId,
                                               UserId = o.UserId,
                                               DeliveryAddressId = o.DeliveryAddressId
                                           }).FirstOrDefault();

                        if (orderDetail != null)
                        {
                            //Get User address
                            if (orderDetail.PatientId != null && orderDetail.PatientId != Guid.Empty)
                            {
                                var patientDetail = DbContext.Patients.FirstOrDefault(x => x.PatientId == orderDetail.PatientId);
                                if (patientDetail != null)
                                {
                                    orderDetail.DeliveryAddress = patientDetail.Street + ", " + patientDetail.POBox + ", " + patientDetail.City;
                                    orderDetail.MobileNumber = patientDetail.MobileNumber;
                                }
                            }
                            else
                            {
                                var customerAddress = DbContext.UserAddresses.FirstOrDefault(x => x.UserAddressID == orderDetail.DeliveryAddressId);
                                if (customerAddress != null)
                                {
                                    orderDetail.DeliveryAddress = customerAddress.Street + ", " + customerAddress.Building + ", " + customerAddress.Area + ", " + customerAddress.City;
                                    orderDetail.MobileNumber = DbContext.Users.FirstOrDefault(x => x.UserId == orderDetail.UserId).MobileNumber;
                                }
                            }
                            aResp.Message = "Tous les éléments ont été récupérés avec succès.";
                            aResp.Status = "Succès";
                            aResp.Payload = orderDetail;
                            aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        }
                        else
                        {
                            aResp.Message = "Aucun article n'a été récupéré.";
                            aResp.Status = "Échec";
                            aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        }
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
                LogManager.LogInfo("GetAcceptedOrderDetail");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;

        }
        public APIResponse CheckAcceptedOrder(Guid deliveryUserId)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("CheckAcceptedOrder");
            try{
                aResp.Message = "Tous les éléments ont été récupérés avec succès.";
                aResp.Status = "Succès";
                aResp.Payload = new
                {
                    isAccept = DbContext.DeliveryUserOrders.Where(s => s.DeliveryUserId == deliveryUserId && s.DeliveryStatus == DeliveryStatus.ACCEPT).Any()
                };
                aResp.StatusCode = System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("CheckAcceptedOrder");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        private OrderDetailModel GetOrderInfo(int orderId)
        {
            OrderDetailModel orderDetail = null;
            orderDetail = (from du in DbContext.DeliveryUserOrders
                           join o in DbContext.Orders on du.OrderID equals o.OrderID
                           where o.OrderID == orderId
                           select new OrderDetailModel
                           {
                               OrderID = o.OrderID,
                               OrderReferenceID = o.OrderReferenceID,
                               DeliveryDate = o.DeliveryDate,
                               OrderDeliveryType = (int)o.OrderDeliveryType,
                               DeliveryMethod = o.DeliveryMethod,
                               OrderProgressStatus = o.OrderProgressStatus,
                               DeliveryStatus = du.DeliveryStatus,
                               PaymentMethod = o.OrderPaymentType,// Temp
                               UserId = o.UserId,
                               PatientId = o.PatientId,
                               CustomerName = o.CustomerName,
                               DeliveryAddressId = o.DeliveryAddressId
                           }).FirstOrDefault();

            if (orderDetail != null)
            {
                if (orderDetail.PatientId != null && orderDetail.PatientId != Guid.Empty)
                {
                    var patientDetail = DbContext.Patients.FirstOrDefault(x => x.PatientId == orderDetail.PatientId);
                    if (patientDetail != null)
                    {
                        orderDetail.MobileNumber = patientDetail.MobileNumber;
                        orderDetail.AddressName = patientDetail.AddressName;
                        orderDetail.Street = patientDetail.Street;
                        orderDetail.City = patientDetail.City;
                        orderDetail.PostalCode = patientDetail.POBox;
                        orderDetail.AddressNote = patientDetail.AddressNote;
                    }
                }
                else
                {
                    var customerAddress = DbContext.UserAddresses.FirstOrDefault(x => x.UserAddressID == orderDetail.DeliveryAddressId);
                    if (customerAddress != null)
                    {
                        orderDetail.MobileNumber = DbContext.Users.FirstOrDefault(x => x.UserId == orderDetail.UserId).MobileNumber;
                        orderDetail.AddressName = customerAddress.AddressName;
                        orderDetail.Street = customerAddress.Street;
                        orderDetail.Area = customerAddress.Area;
                        orderDetail.Building = customerAddress.Building;
                        orderDetail.City = customerAddress.City;
                        orderDetail.PostalCode = customerAddress.PostalCode;
                        orderDetail.AddressNote = customerAddress.AddressNote;
                    }
                }
            }
            return orderDetail;
        }
    }
}
