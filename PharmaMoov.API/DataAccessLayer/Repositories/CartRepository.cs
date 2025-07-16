using PharmaMoov.Models;
using PharmaMoov.Models.Product;
using PharmaMoov.Models.User;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PharmaMoov.Models.Orders;
using PharmaMoov.Models.Cart;
using PharmaMoov.Models.Shop;
using Microsoft.EntityFrameworkCore;
//using MangoPay.SDK;
//using MangoPay.SDK.Entities.POST;
using Newtonsoft.Json;
using PharmaMoov.Models.Promo;
using MangoPay.SDK.Entities.GET;
using MangoPay.SDK.Entities.POST;
using MangoPay.SDK;
using MangoPay.SDK.Entities;
using PharmaMoov.API.Services.Abstractions;
using PharmaMoov.Models.External.Medipim;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class CartRepository : APIBaseRepo, ICartRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private readonly LocalizationService localization;
        private IHttpContextAccessor accessor;
        private IMedipimService _medipimService;
        private IMainHttpClient MainHttpClient { get; }

        IPaymentRepository IPaymentRepo { get; }
        IOrderRepository IOrderRepo { get; }

        public CartRepository(APIDBContext _dbCtxt, IOrderRepository _orderRep, IPaymentRepository _promoRep, ILoggerManager _logManager, APIConfigurationManager _apiCon, IHttpContextAccessor _accessor, LocalizationService _localization, IMainHttpClient _mhttpc, IMedipimService medipimService)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            localization = _localization;
            accessor = _accessor;
            MainHttpClient = _mhttpc;
            _medipimService = medipimService;
            IPaymentRepo = _promoRep;
            IOrderRepo = _orderRep;
        }

        

        public APIResponse SyncCartItem(CartSyncDTO CartSyncItems, string Authorization)
        {
            LogManager.LogInfo("SyncCartItem");

            List<CartItemsDTO> CartItemsD = CartSyncItems.CartItemsSync;
            LogManager.LogDebugObject(CartItemsD);

            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.FirstOrDefault(u => u.Token == Authorization && u.IsActive == true);
            LogManager.LogDebugObject(IsUserLoggedIn);

            APIResponse aResp = new APIResponse();

            try
            {
                if (IsUserLoggedIn != null)
                {
                    //retrieve exisiting cart items
                    if (!CartSyncItems.Sync)
                    {
                        List<UserCartItem> cartItems = GetProductListFromCartItems(IsUserLoggedIn.UserId, CartSyncItems.ShopId);

                        aResp = new APIResponse
                        {
                            Message = "Synchronisation du panier",
                            Status = "Succès",
                            StatusCode = System.Net.HttpStatusCode.OK,
                            Payload = GetProductListFromCartItems(IsUserLoggedIn.UserId, CartSyncItems.ShopId)
                        };
                    }
                    else
                    {
                        //
                        // request to sync the 
                        // posted list to the db
                        // clear the current list first
                        List<CartItem> currentCartItems = DbContext.CartItems.Where(cci => cci.UserId == IsUserLoggedIn.UserId && cci.ShopId == CartSyncItems.ShopId).ToList();
                        if (currentCartItems.Count > 0) //list not null
                        {
                            DbContext.RemoveRange(currentCartItems);
                        }

                        //if(CartItemsD != null && CartItemsD.Count() > 0)

                        //
                        // save the new list to db
                        if (CartSyncItems.CartItemsSync != null && CartSyncItems.CartItemsSync.Count != 0)
                        {
                            foreach (CartItemsDTO item in CartItemsD)
                            {
                                CartItem NewCartItem = new CartItem
                                {
                                    ShopId = CartSyncItems.ShopId,
                                    UserId = IsUserLoggedIn.UserId,
                                    ProductRecordId = item.ProductRecordId,
                                    ProductQuantity = Convert.ToDecimal(string.Format("{0:F1}", item.ProductQuantity)),
                                    PrescriptionRecordId = CartSyncItems.PrescriptionRecordId,

                                    CreatedBy = IsUserLoggedIn.UserId,
                                    CreatedDate = DateTime.Now,
                                    DateEnabled = DateTime.Now,
                                    IsEnabled = true,
                                    IsEnabledBy = IsUserLoggedIn.UserId,
                                    LastEditedBy = IsUserLoggedIn.UserId,
                                    LastEditedDate = DateTime.Now
                                };
                                DbContext.Add(NewCartItem);
                            }
                        }

                        DbContext.SaveChanges();

                        aResp = new APIResponse
                        {
                            Message = "Article(s) ajouté(s) au panier avec succès.",
                            Status = "Succès",
                            StatusCode = System.Net.HttpStatusCode.OK,
                            Payload = GetProductListFromCartItems(IsUserLoggedIn.UserId, CartSyncItems.ShopId)
                        };
                    }
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Utilisateur non connecté",
                        Status = "Échec",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("Erreur in SyncCartItem");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "La nouvelle adresse a été ajoutée avec succès.!";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;

        }

        public List<UserCartItem> GetProductListFromCartItems(Guid UserId, Guid ShopId)
        {
            List<UserCartItem> UserCartItems = new List<UserCartItem>();

            //Get shop details
            Shop shopDetails = DbContext.Shops.AsNoTracking().FirstOrDefault(s => s.ShopId == ShopId);

            // Get all cart items of the user PER SHOP
            List<CartItem> UserCartItemsList = DbContext.CartItems.Where(ci => ci.UserId == UserId && ci.ShopId == ShopId).ToList();
            foreach (CartItem cartItem in UserCartItemsList)
            {
                UserCartItem uCartItem = DbContext.Products.Where(p => p.ProductRecordId == cartItem.ProductRecordId)
                                                            .Select(p => new UserCartItem
                                                            {
                                                                ShopId = p.ShopId,
                                                                ShopName = shopDetails.ShopName,
                                                                ShopAddress = shopDetails.Address,
                                                                ShopIcon = shopDetails.ShopIcon,
                                                                ProductIcon = p.ProductIcon,
                                                                SalePrice = p.SalePrice,
                                                                ProductRecordId = p.ProductRecordId,
                                                                ProductName = p.ProductName,
                                                                PrescriptionRecordId = cartItem.PrescriptionRecordId,
                                                                ProductQuantity = cartItem.ProductQuantity,
                                                                ProductPrice = p.ProductPrice,
                                                                ProductTaxValue = p.ProductTaxValue,
                                                                ProductTaxAmount = (p.ProductTaxValue / 100) * (cartItem.ProductQuantity * p.ProductPrice)
                                                            }).FirstOrDefault();

                uCartItem.ShopStatus = GetPharmacyOpenOrClose(uCartItem.ShopId);
                uCartItem.TotalAmount = (cartItem.ProductQuantity * uCartItem.ProductPrice) + uCartItem.ProductTaxAmount;

                UserCartItems.Add(uCartItem);
            }

            return UserCartItems;
        }

        public APIResponse CheckoutCartItems(string Auth, CheckoutCartItem UserOrder)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("CheckoutCartItems");
            LogManager.LogInfo("CheckoutCartItem Object Sent to the API:");
            LogManager.LogDebugObject(UserOrder);

            try
            {
                //reuse order repository
                var orderRepo = new OrderRepository(DbContext, LogManager, APIConfig, accessor, localization, MainHttpClient);
                //
                //clear cart items
                List<CartItem> UserCartItems = null;
                if (UserOrder.PatientId != null && UserOrder.PatientId != Guid.Empty)
                {
                    UserCartItems = DbContext.CartItems.Where(ci => ci.PatientId == UserOrder.PatientId).ToList();
                }
                else
                {
                    UserCartItems =  DbContext.CartItems.Where(ci => ci.UserId == UserOrder.UserId).ToList();
                }

                var prescriptionRecordId = UserCartItems.Where(s => s.PrescriptionRecordId != 0).Select(s => s.PrescriptionRecordId).FirstOrDefault();
               
                DbContext.RemoveRange(UserCartItems);
                DbContext.SaveChanges();
                //
                //get promo amount
                //there are no promo codes or discounts in pharmamoov
                //var PromoAmount = GetPromoAmount(UserOrder.PromoCode, UserOrder.CartItems);
                //
                // Compute cart
                CartComputation NewCartComputation = GetCartComputation(UserOrder.CartItems, UserOrder.DeliveryFee);//, PromoAmount
                //
                // create a new order
                NoVatOrder NewUserOrder = CreateNewOrderNoVat(UserOrder, NewCartComputation);
                //
                // save cart items
                SaveCartItems(NewUserOrder.OrderID, UserOrder);
                
                //create payment transaction
                CreatePaymentRecord(NewUserOrder.OrderID, NewUserOrder.UserId, NewUserOrder.PatientId);
     
                //create payment for COD - Temporary
                CreatePaymentForCOD(NewUserOrder.OrderID, NewUserOrder.UserId, NewCartComputation);

                //Update Prescription status
                if(prescriptionRecordId != 0)
                {
                    var prescriptionList = DbContext.PrescriptionProducts.Where(s => s.PrescriptionRecordId == prescriptionRecordId).ToList();
                    foreach(var item in prescriptionList)
                    {
                        item.PrescriptionProductStatus = PrescriptionProductStatus.Completed;
                        item.LastEditedDate = DateTime.Now;
                        item.LastEditedBy = UserOrder.UserId;

                        DbContext.PrescriptionProducts.Update(item);
                        DbContext.SaveChanges();
                    }
                }

                aResp.Message = "Votre commande est passée";
                aResp.Status = "Succès";
                aResp.StatusCode = System.Net.HttpStatusCode.OK;
                aResp.Payload = new CheckoutResponse
                {
                    Order = NewUserOrder,
                    OrderItems = orderRepo.GetOrderItems(NewUserOrder.OrderID),
                    PaymentData = null
                    //PaymentUrl = WebPayIn(NewUserOrder.OrderID,NewUserOrder.OrderGrossAmount, Auth)
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("CheckoutCartItems");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;

        }

        CartComputation GetCartComputation(List<CartItemsDTO> CartItemsList, decimal DeliveryFee) //, decimal PromoAmount
        {
            CartComputation NewCartComp = new CartComputation();
            foreach (CartItemsDTO cItem in CartItemsList)
            {
                Product foundP = DbContext.Products.FirstOrDefault(p => p.ProductRecordId == cItem.ProductRecordId);
                if (foundP != null)
                {
                    if (foundP.SalePrice != 0)
                    {
                        NewCartComp.OrderSubTotalAmount += foundP.SalePrice * cItem.ProductQuantity;
                        NewCartComp.OrderVatAmount += (foundP.SalePrice * cItem.ProductQuantity) * (foundP.ProductTaxValue / 100);
                    }
                    else
                    {
                        NewCartComp.OrderSubTotalAmount += foundP.ProductPrice * cItem.ProductQuantity;
                        NewCartComp.OrderVatAmount += (foundP.ProductPrice * cItem.ProductQuantity) * (foundP.ProductTaxValue / 100);
                    }
                }
            }
            NewCartComp.OrderDeliveryFee = DeliveryFee;

            decimal tempGross = (NewCartComp.OrderSubTotalAmount + NewCartComp.OrderVatAmount + NewCartComp.OrderDeliveryFee);
            decimal noVatTempGross = (NewCartComp.OrderSubTotalAmount + NewCartComp.OrderDeliveryFee);
            //if (tempGross > PromoAmount)
            //{
            //    tempGross -= PromoAmount;
            //}

            //NewCartComp.OrderPromoAmount = PromoAmount;
            NewCartComp.OrderGrossAmount = tempGross;
            NewCartComp.NoVatOrderGrossAmount = noVatTempGross;
            return NewCartComp;
        }

        public decimal GetPromoAmount(string PromoCode, List<CartItemsDTO> CartItemsList)
        {
            decimal promoValue = 0;
            CartComputation NewCartComp = new CartComputation();
            foreach (CartItemsDTO cItem in CartItemsList)
            {
                Product foundP = DbContext.Products.FirstOrDefault(p => p.ProductRecordId == cItem.ProductRecordId);
                if (foundP != null)
                {
                    NewCartComp.OrderSubTotalAmount += foundP.ProductPrice * cItem.ProductQuantity;
                    NewCartComp.OrderVatAmount += (foundP.ProductPrice * cItem.ProductQuantity) * (foundP.ProductTaxValue / 100);
                }
            }

            Promo getPromoCode = DbContext.Promos.FirstOrDefault(o => o.PromoCode == PromoCode && o.IsEnabled == true);
            if (getPromoCode != null)
            {
                if (getPromoCode.PType == PromoType.FIXED)
                {
                    promoValue = getPromoCode.PromoValue;
                }
                else
                {
                    promoValue = (NewCartComp.OrderSubTotalAmount + NewCartComp.OrderVatAmount) * (getPromoCode.PromoValue / 100);
                }
            }

            return decimal.Round((decimal)promoValue, 2, MidpointRounding.AwayFromZero); ;
        }

        Order CreateNewOrder(CheckoutCartItem UserOrder, CartComputation UserCartComputation)
        {
            DateTime NowDT = DateTime.Now;
            dynamic OrderCustomer = null;
            if (UserOrder.PatientId != null && UserOrder.PatientId != Guid.Empty)
            {
                OrderCustomer = DbContext.Patients.FirstOrDefault(u => u.PatientId == UserOrder.PatientId);
            }
            else
            {
                OrderCustomer = DbContext.Users.FirstOrDefault(u => u.UserId == UserOrder.UserId);
            }
               
            
            Shop OrderShop = DbContext.Shops.FirstOrDefault(u => u.ShopId == UserOrder.ShopId);

            Order NewUserOrder = new Order
            {
                CreatedBy = UserOrder.UserId,
                CreatedDate = NowDT,
                DateEnabled = NowDT,
                IsEnabled = true,
                IsEnabledBy = UserOrder.UserId,
                IsLocked = false,
                LastEditedBy = UserOrder.UserId,
                LastEditedDate = NowDT,

                UserId = UserOrder.UserId,
                PatientId = (UserOrder.PatientId != null && UserOrder.PatientId != Guid.Empty) ? UserOrder.PatientId : Guid.Empty,
                ShopId = UserOrder.ShopId,
                ShopName = OrderShop.ShopName,
                ShopAddress = OrderShop.Address,
                OrderReferenceID = GenerateUniqeCode(10, true, false),
                CustomerName = OrderCustomer.FirstName + " " + OrderCustomer.LastName,
                OrderNote = UserOrder.OrderNote,
                OrderDeliveryType = OrderDeliveryType.FORDELIVERY, //Fixed value 
                DeliveryMethod = UserOrder.DeliveryMethod,

                DeliveryAddressId = (UserOrder.PatientId != null && UserOrder.PatientId != Guid.Empty) ? 0 : UserOrder.DeliveryAddressId,
                DeliveryDate = UserOrder.DeliveryDate,
                DeliveryDay = UserOrder.DeliveryDate.Day.ToString(),
                DeliveryTime = UserOrder.DeliveryTime,
               // PromoCode = UserOrder.PromoCode,
               
                OrderSubTotalAmount = UserCartComputation.OrderSubTotalAmount,
                OrderVatAmount = UserCartComputation.OrderVatAmount,
                OrderPromoAmount = UserCartComputation.OrderPromoAmount,
                OrderDeliveryFee = UserCartComputation.OrderDeliveryFee,
                OrderGrossAmount = UserCartComputation.OrderGrossAmount,

                OrderProgressStatus = OrderProgressStatus.PLACED,
                OrderPaymentType = UserOrder.PaymentType
            };

            DbContext.Orders.Add(NewUserOrder);
            DbContext.SaveChanges();

            return NewUserOrder;
        }

        NoVatOrder CreateNewOrderNoVat(CheckoutCartItem UserOrder, CartComputation UserCartComputation)
        {
            DateTime NowDT = DateTime.Now;
            dynamic OrderCustomer = null;
            LogManager.LogInfo("CheckoutCartItem >>");
            LogManager.LogDebugObject(UserOrder);
            if (UserOrder.PatientId != null && UserOrder.PatientId != Guid.Empty)
            {
                OrderCustomer = DbContext.Patients.FirstOrDefault(u => u.PatientId == UserOrder.PatientId);
                LogManager.LogInfo("OrderCustomer >>");
                LogManager.LogDebugObject(OrderCustomer);
            }
            else
            {
                OrderCustomer = DbContext.Users.FirstOrDefault(u => u.UserId == UserOrder.UserId);
                LogManager.LogInfo("OrderCustomer >>");
                LogManager.LogDebugObject("OrderCustomer >>" + OrderCustomer);
            }


            Shop OrderShop = DbContext.Shops.FirstOrDefault(u => u.ShopId == UserOrder.ShopId);
            LogManager.LogInfo("OrderShop >>");
            LogManager.LogDebugObject(OrderShop);

            Order NewUserOrder = new Order
            {
                CreatedBy = UserOrder.UserId,
                CreatedDate = NowDT,
                DateEnabled = NowDT,
                IsEnabled = true,
                IsEnabledBy = UserOrder.UserId,
                IsLocked = false,
                LastEditedBy = UserOrder.UserId,
                LastEditedDate = NowDT,

                UserId = UserOrder.UserId,
                PatientId = (UserOrder.PatientId != null && UserOrder.PatientId != Guid.Empty) ? UserOrder.PatientId : Guid.Empty,
                ShopId = UserOrder.ShopId,
                ShopName = OrderShop.ShopName,
                ShopAddress = OrderShop.Address,
                OrderReferenceID = GenerateUniqeCode(10, true, false),
                CustomerName = OrderCustomer.FirstName + " " + OrderCustomer.LastName,
                OrderNote = UserOrder.OrderNote,
                OrderDeliveryType = OrderDeliveryType.FORDELIVERY, //Fixed value 
                DeliveryMethod = UserOrder.DeliveryMethod,

                //DeliveryAddressId = (UserOrder.PatientId != null && UserOrder.PatientId != Guid.Empty) ? 0 : UserOrder.DeliveryAddressId,
                DeliveryAddressId = UserOrder.DeliveryAddressId,
                DeliveryDate = UserOrder.DeliveryDate,
                DeliveryDay = UserOrder.DeliveryDate.Day.ToString(),
                DeliveryTime = UserOrder.DeliveryTime,
                // PromoCode = UserOrder.PromoCode,

                OrderSubTotalAmount = UserCartComputation.OrderSubTotalAmount,
                OrderVatAmount = UserCartComputation.OrderVatAmount,
                OrderPromoAmount = UserCartComputation.OrderPromoAmount,
                OrderDeliveryFee = UserCartComputation.OrderDeliveryFee,
                OrderGrossAmount = UserCartComputation.OrderGrossAmount,

                OrderProgressStatus = OrderProgressStatus.PLACED,
                OrderPaymentType = UserOrder.PaymentType
            };
            LogManager.LogInfo("Order Before Insert >>");
            LogManager.LogDebugObject(NewUserOrder);
            DbContext.Orders.Add(NewUserOrder);
            DbContext.SaveChanges();

            NoVatOrder ReturnNewUserOrder = new NoVatOrder
            {
                CreatedBy = NewUserOrder.CreatedBy,
                CreatedDate = NewUserOrder.CreatedDate,
                DateEnabled = NewUserOrder.DateEnabled,
                IsEnabled = NewUserOrder.IsEnabled,
                IsEnabledBy = NewUserOrder.IsEnabledBy,
                IsLocked = NewUserOrder.IsLocked,
                LastEditedBy = NewUserOrder.LastEditedBy,
                LastEditedDate = NewUserOrder.LastEditedDate,

                UserId = NewUserOrder.UserId,
                PatientId = NewUserOrder.PatientId,
                ShopId = NewUserOrder.ShopId,
                ShopName = NewUserOrder.ShopName,
                ShopAddress = NewUserOrder.ShopAddress,
                OrderReferenceID = NewUserOrder.OrderReferenceID,
                CustomerName = NewUserOrder.CustomerName,
                OrderNote = NewUserOrder.OrderNote,
                OrderDeliveryType = NewUserOrder.OrderDeliveryType, //Fixed value 
                DeliveryMethod = NewUserOrder.DeliveryMethod,

                //DeliveryAddressId = (UserOrder.PatientId != null && UserOrder.PatientId != Guid.Empty) ? 0 : UserOrder.DeliveryAddressId,
                DeliveryAddressId = NewUserOrder.DeliveryAddressId,
                DeliveryDate = NewUserOrder.DeliveryDate,
                DeliveryDay = NewUserOrder.DeliveryDay,
                DeliveryTime = NewUserOrder.DeliveryTime,
                // PromoCode = UserOrder.PromoCode,

                OrderSubTotalAmount = NewUserOrder.OrderSubTotalAmount,
                OrderVatAmount = NewUserOrder.OrderVatAmount,
                OrderPromoAmount = NewUserOrder.OrderPromoAmount,
                OrderDeliveryFee = NewUserOrder.OrderDeliveryFee,
                OrderGrossAmount = NewUserOrder.OrderGrossAmount,
                NoVatOrderGrossAmount = UserCartComputation.NoVatOrderGrossAmount,
                OrderProgressStatus = NewUserOrder.OrderProgressStatus,
                OrderPaymentType = NewUserOrder.OrderPaymentType,
                OrderID = NewUserOrder.OrderID,
            };
            LogManager.LogInfo("Order After Insert >>");
            LogManager.LogDebugObject(ReturnNewUserOrder);
            return ReturnNewUserOrder;
        }

        void SaveCartItems(int OrderId, CheckoutCartItem UserOrder)
        {
            List<OrderItem> OrderItemList = new List<OrderItem>();
            DateTime NowDT = DateTime.Now;

            foreach (CartItemsDTO CartItem in UserOrder.CartItems)
            {
                //save product price for traceablitiy of previous paid price
                Product product = DbContext.Products.FirstOrDefault(p => p.ProductRecordId == CartItem.ProductRecordId);
                OrderItem OItem = new OrderItem
                {
                    CreatedBy = UserOrder.UserId,
                    CreatedDate = NowDT,
                    DateEnabled = NowDT,
                    IsEnabled = true,
                    IsEnabledBy = UserOrder.UserId,
                    IsLocked = false,
                    LastEditedBy = UserOrder.UserId,
                    LastEditedDate = NowDT,
                    OrderID = OrderId,
                    ProductRecordId = CartItem.ProductRecordId,
                    ProductQuantity = Convert.ToDecimal(string.Format("{0:F1}", CartItem.ProductQuantity)),
                    ProductTaxValue = product.ProductTaxValue,
                    ProductTaxAmount = (product.ProductTaxValue / 100) * (CartItem.ProductQuantity * product.ProductPrice),
                    ProductPricePerKG = product.ProductPricePerKG,
                    ProductUnit = product.ProductUnit,
                };

                if (product.SalePrice != 0)
                {
                    OItem.ProductPrice = product.SalePrice;
                }
                else
                {
                    OItem.ProductPrice = product.ProductPrice;
                }

                OItem.SubTotal = (OItem.ProductPrice * CartItem.ProductQuantity) + OItem.ProductTaxAmount;
                OrderItemList.Add(OItem);
            }
            LogManager.LogInfo("OrderItemList >>");
            LogManager.LogDebugObject(OrderItemList);

            DbContext.AddRange(OrderItemList);
            DbContext.SaveChanges();
        }

        string WebPayIn(int OrderID, decimal amount, string auth)
        {
            PayInCardWebPostDTO model = BuildPaymentModel(amount, OrderID);
            MangoPayApi api = new MangoPayApi();
            api.Config.ClientId = APIConfig.PaymentConfig.ClientId;
            api.Config.ClientPassword = APIConfig.PaymentConfig.ApiKey;
            api.Config.BaseUrl = APIConfig.PaymentConfig.BaseUrl;
            // payments will go to wallet id from ph1 implementation
            if (model.AuthorId == null || model.AuthorId.Trim() == string.Empty)
            {
                model.AuthorId = APIConfig.PaymentConfig.CreditedId;
                model.CreditedUserId = APIConfig.PaymentConfig.CreditedId;
                model.CreditedWalletId = APIConfig.PaymentConfig.WalletId;
            }
            var paymentDetails = api.PayIns.CreateCardWeb(model);
            // create db record if payment page is created
            if (paymentDetails.Status == MangoPay.SDK.Core.Enumerations.TransactionStatus.CREATED)
            {
                PaymentRepository payRep = new PaymentRepository(DbContext, LogManager, APIConfig, MainHttpClient);
                //payRep.CreateWebPayment(OrderID, model, JsonConvert.SerializeObject(paymentDetails), auth);
            }
            return paymentDetails.RedirectURL;
        }

        PayInCardWebPostDTO BuildPaymentModel(decimal amount, int OrderId)
        {
            PayInCardWebPostDTO newPaymentMOdel = new PayInCardWebPostDTO(
                    APIConfig.PaymentConfig.CreditedId,
                    new MangoPay.SDK.Entities.Money
                    {
                        Amount = (long)amount * 100,
                        Currency = MangoPay.SDK.Core.Enumerations.CurrencyIso.EUR
                    },
                    new MangoPay.SDK.Entities.Money
                    {
                        Amount = 0 * 100,
                        Currency = MangoPay.SDK.Core.Enumerations.CurrencyIso.EUR
                    },
                    APIConfig.PaymentConfig.WalletId,
                    APIConfig.PaymentConfig.ReturlUrl,
                    MangoPay.SDK.Core.Enumerations.CultureCode.FR,
                    0,
                    "CC" + OrderId
                );
            newPaymentMOdel.Tag = "PharmaM " + OrderId.ToString();
            newPaymentMOdel.SecureMode = MangoPay.SDK.Core.Enumerations.SecureMode.DEFAULT;
            newPaymentMOdel.CardType = MangoPay.SDK.Core.Enumerations.CardType.CB_VISA_MASTERCARD;

            return newPaymentMOdel;

        }

        void CreatePaymentRecord(int orderId, Guid CustomerId, Guid PatientId)
        {
            PaymentTransaction hasDuplicate = DbContext.PaymentTransactions.FirstOrDefault(u => u.UserId == CustomerId && u.OrderId == orderId);

            if (hasDuplicate == null)
            {
                DateTime NowDT = DateTime.Now;
                string TransactionCode = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 10);

                PaymentTransaction newPaymentTrans = new PaymentTransaction
                {
                    CreatedBy = CustomerId,
                    CreatedDate = NowDT,
                    LastEditedDate = NowDT,
                    LastEditedBy = CustomerId,
                    DateEnabled = NowDT,
                    IsEnabledBy = CustomerId,
                    IsEnabled = true, 
                    OrderId = orderId,
                    UserId = CustomerId,
                    PatientId = (PatientId != null && PatientId != Guid.Empty) ? PatientId : Guid.Empty,
                    TransactionCode = TransactionCode,
                    //PaymentStatus = EPaymentStatus.NotSpecified
                    PaymentStatus = EPaymentStatus.SUCCEEDED // Temporary code for COD
                };
                DbContext.PaymentTransactions.Add(newPaymentTrans);
                DbContext.SaveChanges();
            }
        }

        // Temporary code for COD to add in payment table  
        private void CreatePaymentForCOD(int orderId, Guid userId, CartComputation newCartComputation)
        {
            Payment hasDuplicate = DbContext.Payments.FirstOrDefault(u => u.CreatedBy == userId && u.OrderId == orderId);
            if (hasDuplicate == null)
            {
                Payment paymentModel = new Payment
                {
                    OrderId = orderId,
                    Status = EPaymentStatus.CREATED,
                    Tag = "Pmoov " + orderId.ToString(),//iPaymentModel.Tag,
                    AuthorId = null,
                    DebitedFundsCurrency = "Euro",
                    DebitedFundsAmount = Convert.ToInt64(newCartComputation.OrderGrossAmount),
                    FeesCurrency = null,
                    FeesAmount = Convert.ToInt64(newCartComputation.OrderDeliveryFee),
                    CreditedWalletId = null,
                    ReturnURL = null,
                    Culture = null,
                    CardType = null,
                    SecureMode = null,
                    CreditedUserId = userId.ToString(),
                    StatementDescriptor = null,
                    CreatePayload = null,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                DbContext.Payments.Add(paymentModel);
                DbContext.SaveChanges();
            }                
        }

        public APIResponse GetMinimumCartAmount()
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetMinimumCartAmount");

            try
            {
                return aResp = new APIResponse
                {
                    Message = "Enregistrement récupéré avec succès.",
                    Status = "Succès!",
                    Payload = DbContext.OrderConfigurations.AsNoTracking().FirstOrDefault(x => x.ConfigType == OrderConfigType.MINORDER).ConfigDecValue,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetMinimumCartAmount");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse ValidateCartItems(Guid _shop, int _address, OrderDeliveryType _dType)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ValidateCartItems");

            try
            {
                bool IsCartItemsValid = false;
                var userAddress = DbContext.UserAddresses.FirstOrDefault(x => x.UserAddressID == _address);
                var shopAddress = DbContext.Shops.FirstOrDefault(x => x.ShopId == _shop);

                //calculate distance
                var distance = new Coordinates((double)userAddress.Latitude, (double)userAddress.Longitude)
                    .DistanceTo(
                        new Coordinates((double)shopAddress.Latitude, (double)shopAddress.Longitude)
                    );

                var catalogueRepo = new CatalogueRepository(DbContext, LogManager, APIConfig);
                decimal maxDistance = catalogueRepo.GetMaximumKMDistance(_dType);
                decimal shopDistance = decimal.Round((decimal)distance, 2, MidpointRounding.AwayFromZero);
                if (shopDistance <= maxDistance)
                {
                    IsCartItemsValid = true;
                }

                return aResp = new APIResponse
                {
                    Message = "Enregistrement récupéré avec succès.",
                    Status = "Succès!",
                    Payload = new
                    {
                        IsCartValid = IsCartItemsValid,
                        MaxDistance = maxDistance,
                        Distance = shopDistance,
                    },
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("ValidateCartItems");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse SyncCartItemForHealthProfessional(CartSyncForHealthProfessional CartSyncItems)
        {
            LogManager.LogInfo("SyncCartItemForHealthProfessional");

            List<CartItemsDTO> CartItemsD = CartSyncItems.CartItemsSync;
            LogManager.LogDebugObject(CartItemsD);

            APIResponse aResp = new APIResponse();

            try
            {
                int patientRecordId = 0;
                dynamic userResult = null;
                
                if(CartSyncItems.PatientId != null && CartSyncItems.PatientId != Guid.Empty)
                {
                    userResult = DbContext.Patients.FirstOrDefault(s => s.PatientId == CartSyncItems.PatientId);
                    patientRecordId = userResult.PatientRecordId;
                }
                else
                {
                    userResult = DbContext.Users.FirstOrDefault(s => s.UserId == CartSyncItems.UserId);
                }
               
                if (userResult != null)
                {
                    //retrieve exisiting cart items
                    if (!CartSyncItems.Sync)
                    {
                       // List<UserCartItem> cartItems = GetProductListFromCartItemsForHP(CartSyncItems.UserId, CartSyncItems.ShopId, userResult.FirstName, userResult.LastName);

                        aResp = new APIResponse
                        {
                            Message = "Synchronisation du panier",
                            Status = "Succès",
                            StatusCode = System.Net.HttpStatusCode.OK,
                            Payload = GetProductListFromCartItemsForHP(CartSyncItems.UserId, CartSyncItems.ShopId, userResult.FirstName, userResult.LastName, patientRecordId, CartSyncItems.PatientId)
                        };
                    }
                    else
                    {
                        //
                        // request to sync the 
                        // posted list to the db
                        // clear the current list first
                        List<CartItem> currentCartItems = null;
                        if (CartSyncItems.PatientId != null && CartSyncItems.PatientId != Guid.Empty)
                        {
                            currentCartItems = DbContext.CartItems.Where(cci => cci.PatientId == CartSyncItems.PatientId && cci.ShopId == CartSyncItems.ShopId).ToList();
                        }
                        else
                        {
                            currentCartItems = DbContext.CartItems.Where(cci => cci.UserId == CartSyncItems.UserId && cci.ShopId == CartSyncItems.ShopId && cci.PatientId == Guid.Empty).ToList();
                        }


                        if (currentCartItems.Count > 0)
                        {
                            DbContext.RemoveRange(currentCartItems);
                        }

                        //
                        // save the new list to db
                        foreach (CartItemsDTO item in CartItemsD)
                        {
                            CartItem NewCartItem = new CartItem
                            {
                                ShopId = CartSyncItems.ShopId,
                                UserId = CartSyncItems.UserId,
                                ProductRecordId = item.ProductRecordId,
                                PrescriptionRecordId = CartSyncItems.PrescriptionRecordId,
                                ProductQuantity = Convert.ToDecimal(string.Format("{0:F1}", item.ProductQuantity)),
                                PatientId = (CartSyncItems.PatientId != null && CartSyncItems.PatientId != Guid.Empty) ? CartSyncItems.PatientId : Guid.Empty,
                                CreatedBy = CartSyncItems.UserId,
                                CreatedDate = DateTime.Now,
                                DateEnabled = DateTime.Now,
                                IsEnabled = true,
                                IsEnabledBy = CartSyncItems.UserId,
                                LastEditedBy = CartSyncItems.UserId,
                                LastEditedDate = DateTime.Now
                            };
                            DbContext.Add(NewCartItem);
                        }

                        DbContext.SaveChanges();

                        aResp = new APIResponse
                        {
                            Message = "Article(s) ajouté(s) au panier avec succès.",
                            Status = "Succès",
                            StatusCode = System.Net.HttpStatusCode.OK,
                            Payload = GetProductListFromCartItemsForHP(CartSyncItems.UserId, CartSyncItems.ShopId, userResult.FirstName, userResult.LastName, patientRecordId, CartSyncItems.PatientId)
                        };
                    }
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Utilisateur non connecté",
                        Status = "Échec",
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("Erreur in SyncCartItemForHealthProfessional");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "La nouvelle adresse a été ajoutée avec succès.!";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;

        }
        public List<UserCartItemForHealthProfessional> GetProductListFromCartItemsForHP(Guid UserId, Guid ShopId,string FirstName, string LastName, int patientRecordId, Guid patientId)
        {
            List<UserCartItemForHealthProfessional> UserCartItems = new List<UserCartItemForHealthProfessional>();

            //Get shop details
            Shop shopDetails = DbContext.Shops.AsNoTracking().FirstOrDefault(s => s.ShopId == ShopId);

            // Get all cart items of the user PER SHOP
            List<CartItem> UserCartItemsList = null;
            
            if (patientId != null && patientId != Guid.Empty)
            {
                UserCartItemsList = DbContext.CartItems.Where(ci => ci.PatientId == patientId && ci.ShopId == ShopId).ToList();
            }
            else
            {
                UserCartItemsList = DbContext.CartItems.Where(ci => ci.UserId == UserId && ci.ShopId == ShopId && ci.PatientId == Guid.Empty).ToList();
            }

            foreach (CartItem cartItem in UserCartItemsList)
            {
                var uCartItem = DbContext.Products.Where(p => p.ProductRecordId == cartItem.ProductRecordId)
                                                            .Select(p => new UserCartItemForHealthProfessional
                                                            {
                                                                ShopId = p.ShopId,
                                                                ShopName = shopDetails.ShopName,
                                                                ShopAddress = shopDetails.Address,
                                                                ShopIcon = shopDetails.ShopIcon,
                                                                PatientRecordId = patientRecordId,
                                                                UserId = UserId,
                                                                PatientId = patientId,
                                                                FirstName = FirstName,
                                                                LastName = LastName,
                                                                ProductIcon = p.ProductIcon,
                                                                SalePrice = p.SalePrice,
                                                                PrescriptionRecordId = cartItem.PrescriptionRecordId,
                                                                ProductRecordId = p.ProductRecordId,
                                                                ProductName = p.ProductName,
                                                                ProductQuantity = cartItem.ProductQuantity,
                                                                ProductPrice = p.ProductPrice,
                                                                ProductTaxValue = p.ProductTaxValue,
                                                                ProductTaxAmount = (p.ProductTaxValue / 100) * (cartItem.ProductQuantity * p.ProductPrice)
                                                            }).FirstOrDefault();

                uCartItem.ShopStatus = GetPharmacyOpenOrClose(uCartItem.ShopId);

                uCartItem.TotalAmount = (cartItem.ProductQuantity * uCartItem.ProductPrice) + uCartItem.ProductTaxAmount;

                UserCartItems.Add(uCartItem);
            }

            return UserCartItems;
        }
        private string GetPharmacyOpenOrClose(Guid shopId)
        {
            var today = DateTime.Now;
            DayOfWeek dayOfweek = today.DayOfWeek;
            var shopOpeningHours = DbContext.ShopOpeningHours.AsNoTracking().Where(s => s.ShopId == shopId && s.IsEnabled == true && s.DayOfWeek == dayOfweek).FirstOrDefault();
            if (shopOpeningHours != null)
            {
                if (TimeSpan.Parse(shopOpeningHours.StartTimeAM) <= today.TimeOfDay && today.TimeOfDay <= TimeSpan.Parse(shopOpeningHours.EndTimeAM))
                {
                    return "Ouvert";
                }
                else if (TimeSpan.Parse(shopOpeningHours.StartTimePM) <= today.TimeOfDay && today.TimeOfDay <= TimeSpan.Parse(shopOpeningHours.EndTimePM))
                {
                    return "Ouvert";
                }
                else if (TimeSpan.Parse(shopOpeningHours.StartTimeEvening) <= today.TimeOfDay && today.TimeOfDay <= TimeSpan.Parse(shopOpeningHours.EndTimeEvening))
                {
                    return "Ouvert";
                }
                else
                {
                    return "Fermer";
                }
                //if(shopOpeningHours.StartTimePM == "00:00:00" || shopOpeningHours.EndTimePM == "00:00:00")
                //{
                //    return "Closed";
                //}
                //else if (shopOpeningHours.StartTimeEvening == "00:00:00" || shopOpeningHours.EndTimeEvening == "00:00:00")
                //{
                //    return "Closed";
                //}
                //return "Open";
            }
            return "Fermer";
        }

        public APIResponse CheckoutCartItemsViaDirectCardPayIn(string Auth, CheckoutCartItem UserOrder, string cardId, string accessType)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("CheckoutCartItemsViaDirectCardPayIn");
            LogManager.LogInfo("CheckoutCartItemsViaDirectCardPayIn Object Sent to the API:");
            LogManager.LogDebugObject(UserOrder);

            try
            {
                // get the mangopay user data
                MangoPayUser mPayUser = DbContext.MangoPayUsers.FirstOrDefault(mpu => mpu.PharmaMUserId == UserOrder.UserId);
                // get userinfo first
                User currentUser = DbContext.Users.FirstOrDefault(u => u.UserId == UserOrder.UserId);
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

                
                //get promo amount
                //var PromoAmount = IPromoRepo.GetPromoAmount(UserOrder.PromoCode, UserOrder.CartItems);
                //
                // Compute cart
                CartComputation NewCartComputation = GetCartComputation(UserOrder.CartItems, UserOrder.DeliveryFee);
                //
                // create a new order
                NoVatOrder NewUserOrder = CreateNewOrderNoVat(UserOrder, NewCartComputation);
                //
                // save cart items

                LogManager.LogInfo("SaveCartItems Parameters OrderID >>" + NewUserOrder.OrderID);
                LogManager.LogDebugObject(UserOrder);
                SaveCartItems(NewUserOrder.OrderID, UserOrder);
                //
                //create payment transaction
                CreatePaymentRecord(NewUserOrder.OrderID, NewUserOrder.UserId);
                UserAddresses uAdd = DbContext.UserAddresses.FirstOrDefault(uad => uad.UserAddressID == NewUserOrder.DeliveryAddressId);
                PayInCardDirectDTO paymentReturnData = CardDirectPayIn(NewUserOrder.OrderID, NewUserOrder, cardId, mPayUser, uAdd, currentUser, Auth, accessType);
                if (paymentReturnData.Status == MangoPay.SDK.Core.Enumerations.TransactionStatus.SUCCEEDED || paymentReturnData.Status == MangoPay.SDK.Core.Enumerations.TransactionStatus.CREATED)
                {
                    aResp.Message = "Votre commande est passée";
                    aResp.Status = "Succès";
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                    LogManager.LogInfo("Checkout Succès >>");
                    LogManager.LogDebugObject(NewUserOrder);
                    aResp.Payload = new CardPayinCheckoutResponse
                    {
                        
                        Order = NewUserOrder,
                        OrderItems = IOrderRepo.GetOrderItems(NewUserOrder.OrderID),
                        PaymentData = paymentReturnData
                    };
                }
                else
                {
                    var selected = DbContext.Orders.Where(x => x.OrderID == NewUserOrder.OrderID).FirstOrDefault();
                    //remove order
                    DbContext.Orders.Remove(selected);
                    //remove payments
                    Payment paymentToRemove = DbContext.Payments.FirstOrDefault(ptr => ptr.OrderId == NewUserOrder.OrderID);
                    DbContext.Payments.Remove(paymentToRemove);
                    //remove cart items
                    List<OrderItem> oItems = DbContext.OrderItems.Where(oi => oi.OrderID == NewUserOrder.OrderID).ToList();
                    DbContext.OrderItems.RemoveRange(oItems);
                    //remove payment transactions
                    PaymentTransaction pTrans = DbContext.PaymentTransactions.FirstOrDefault(pt => pt.OrderId == NewUserOrder.OrderID);
                    DbContext.PaymentTransactions.RemoveRange(pTrans);
                    DbContext.SaveChanges();
                    aResp.Message = paymentReturnData.ResultMessage + "[" + paymentReturnData.ResultCode + "]";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    aResp.Payload = new CardPayinCheckoutResponse
                    {
                        Order = NewUserOrder,
                        OrderItems = IOrderRepo.GetOrderItems(NewUserOrder.OrderID),
                        PaymentData = paymentReturnData
                    };
                }


            }
            catch (Exception ex)
            {
                LogManager.LogInfo("CheckoutCartItems");
                LogManager.LogDebugObject(ex);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;

        }

        void CreatePaymentRecord(int orderId, Guid CustomerId)
        {
            PaymentTransaction hasDuplicate = DbContext.PaymentTransactions.FirstOrDefault(u => u.UserId == CustomerId && u.OrderId == orderId);

            if (hasDuplicate == null)
            {
                DateTime NowDT = DateTime.Now;
                string TransactionCode = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 10);

                PaymentTransaction newPaymentTrans = new PaymentTransaction
                {
                    CreatedBy = CustomerId,
                    CreatedDate = NowDT,
                    LastEditedDate = NowDT,
                    LastEditedBy = CustomerId,
                    DateEnabled = NowDT,
                    IsEnabledBy = CustomerId,
                    IsEnabled = true,
                    OrderId = orderId,
                    UserId = CustomerId,
                    TransactionCode = TransactionCode,
                    PaymentStatus = EPaymentStatus.NotSpecified
                };
                DbContext.PaymentTransactions.Add(newPaymentTrans);
                DbContext.SaveChanges();
            }
        }

        PayInCardDirectDTO CardDirectPayIn(int OrderID, NoVatOrder amount, string cardId, MangoPayUser mPayUser, UserAddresses uAddress, User uProf, string auth, string accessType)
        {
            LogManager.LogInfo("CardDirectPayIn");
            LogManager.LogInfo("CardDirectPayIn with parameters OrderID:" + OrderID.ToString() + " amount: " + amount.ToString());
            PayInCardDirectPostDTO model = IPaymentRepo.BuildCardDirectPaymentModel(amount.OrderGrossAmount, OrderID, uAddress, cardId, uProf, accessType);
            LogManager.LogInfo("-- CardDirectPayIn Model: -- ");
            LogManager.LogDebugObject(model);
            MangoPayApi api = new MangoPayApi();
            api.Config.ClientId = APIConfig.PaymentConfig.ClientId;
            api.Config.ClientPassword = APIConfig.PaymentConfig.ApiKey;
            api.Config.BaseUrl = APIConfig.PaymentConfig.BaseUrl;
            //
            // set the user info into the payment model
            model.AuthorId = mPayUser.ID;
            model.CreditedUserId = mPayUser.ID;
            model.CreditedWalletId = mPayUser.WalletID;
            LogManager.LogInfo("CardDirectPayIn with Model :");
            LogManager.LogDebugObject(model);
            var paymentDetails = api.PayIns.CreateCardDirect(model);

            // remove fund transfer
            //TransferPostDTO fund = IPaymentRepo.BuildFundTransferModel(amount.OrderSubTotalAmount, OrderID, mPayUser, mPayShop);
            //var fundDetails = api.Transfers.Create(fund);
            // create db record if payment page is created
            var payM = IPaymentRepo.CreateCardPayment(OrderID, paymentDetails, JsonConvert.SerializeObject(paymentDetails), auth);
            LogManager.LogInfo("CardDirectPayIn Result :");
            LogManager.LogDebugObject(paymentDetails);
            return paymentDetails;
        }

        


    }
}

