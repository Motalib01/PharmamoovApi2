using PharmaMoov.API.DataAccessLayer;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.Models;
using PharmaMoov.Models.Orders;
using PharmaMoov.Models.Shop;
using MangoPay.SDK;
using MangoPay.SDK.Entities.GET;
using MangoPay.SDK.Entities.POST;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaMoov.API.Helpers.HostedServices
{
    public class PaymentBackgroundProcess : IPaymentBackgroundProcess
    {
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private readonly IPaymentRepository PaymentMainRepo;
        private readonly APIDBContext DbContxt;

        public PaymentBackgroundProcess(ILoggerManager _logManager, APIConfigurationManager _apiCon,IServiceScopeFactory factory)
        { 
            APIConfig = _apiCon;
            LogManager = _logManager;
            DbContxt = factory.CreateScope().ServiceProvider.GetRequiredService<APIDBContext>();
            PaymentMainRepo = factory.CreateScope().ServiceProvider.GetRequiredService<IPaymentRepository>();

        }

        public void TransferFundsFromCustomerWalletToShopWallet() 
        { 
            try
            {
                List<Payment> SucceededPayments = DbContxt.Payments.Where(pa => pa.Status == EPaymentStatus.SUCCEEDED).ToList();
                LogManager.LogInfo("-- Total of " + SucceededPayments.Count() + " payment transaction with success status -- ");
                foreach (Payment pay in SucceededPayments)
                {
                    TimeSpan paymentDate = DateTime.Now - (DateTime)pay.LastEditedDate; 
                    if (paymentDate.TotalMinutes > 15)
                    {
                        LogManager.LogInfo("Payment was from " + paymentDate.TotalHours.ToString("#.##") + " hours ago.");
                        LogManager.LogInfo("TransferFundsInteval is set to: " + APIConfig.HostedServicesConfig.AutomaticFundsTransferIntervalHrs.ToString());
                        // check if the order is completed.
                        // As per Luc Ladouceur remove check of the order status,
                        // transfer payment regardless of order status
                        //Order PaymentOrder = DbContxt.Orders.FirstOrDefault(o => o.OrderID == pay.OrderId && o.OrderProgressStatus == OrderProgressStatus.COMPLETED); 
                        LogManager.LogInfo("-- Get Order Info For Transfer Payment Order ID: "+ pay.OrderId.ToString() +" -- ");
                        Order PaymentOrder = DbContxt.Orders.FirstOrDefault(o => o.OrderID == pay.OrderId && o.OrderProgressStatus == OrderProgressStatus.PLACED); 
                        LogManager.LogDebugObject(PaymentOrder);
                        if (PaymentOrder != null)
                        {
                            // get the shop
                            LogManager.LogInfo("-- Get Shopinfo Info For Transfer Payment Shop ID: " + PaymentOrder.ShopId.ToString() + " -- ");
                            Shop ShopInfo = DbContxt.Shops.FirstOrDefault(si => si.ShopId == PaymentOrder.ShopId); 
                            LogManager.LogDebugObject(ShopInfo);
                            // get shop wallet   
                            LogManager.LogInfo("-- Get MangoPayShopWallet Info For Transfer Payment Shop ID: " + PaymentOrder.ShopId.ToString() + " -- ");
                            MangoPayShop mPayShop = DbContxt.MangoPayShops.FirstOrDefault(mps => mps.PharmaShopId == PaymentOrder.ShopId);
                            LogManager.LogDebugObject(mPayShop);
                            // get the shop user id and wallet id from mango pay
                            if (mPayShop == null)
                            {
                                LogManager.LogInfo("No Wallet for shop:" + PaymentOrder.ShopId.ToString());
                                LogManager.LogInfo("-- Aborting transfer for this payment # " + pay.Id.ToString() + " --");
                                LogManager.LogInfo("-- Will try to transfer again for #" + pay.Id.ToString() + " when the wallet is available --");
                                continue;
                            }
                            LogManager.LogInfo("-- Get PaymentDetails Info For Transfer Payment Shop ID: " + PaymentOrder.ShopId.ToString() + " -- ");
                            LogManager.LogDebugObject(pay);
                            PayInCardWebDTO paymentWebDetails = JsonConvert.DeserializeObject<PayInCardWebDTO>(pay.CreatePayload);
                            LogManager.LogInfo("-- Start MangoPay API -- ");
                            MangoPayApi api = new MangoPayApi();
                            api.Config.ClientId = APIConfig.PaymentConfig.ClientId;
                            api.Config.ClientPassword = APIConfig.PaymentConfig.ApiKey;
                            api.Config.BaseUrl = APIConfig.PaymentConfig.BaseUrl;
                            // compute fees and debited funds
                            LogManager.LogDebugObject(api);
                            decimal deliveryFees = PaymentOrder.OrderDeliveryFee;
                            decimal totalAmount = PaymentOrder.OrderGrossAmount;
                            decimal totalPromoAmount = PaymentOrder.OrderPromoAmount;
                            //decimal amountTransferToShop = (totalAmount + totalPromoAmount - deliveryFees) * 0.7m;
                            //decimal totalFees = totalAmount - amountTransferToShop;
                            decimal totalFees = (totalAmount - deliveryFees) * 0.15m;
                            totalFees = totalFees * 100;
                            long totalFeesAsLong = Convert.ToInt64(totalFees);
                            paymentWebDetails.Fees.Amount = totalFeesAsLong;

                            TransferPostDTO transferPayload = new TransferPostDTO(
                                   paymentWebDetails.AuthorId,
                                   mPayShop.Id,
                                   paymentWebDetails.DebitedFunds,
                                   paymentWebDetails.Fees,
                                   paymentWebDetails.CreditedWalletId,
                                   mPayShop.WalletID
                                );
                            transferPayload.Tag = "CCOID_" + PaymentOrder.OrderID.ToString() + "_payrecid_" + pay.Id;
                            LogManager.LogInfo("-- MangoPay API Transfer Ready with Payload: -- ");
                            LogManager.LogDebugObject(transferPayload);

                            TransferDTO transferResult = api.Transfers.Create(transferPayload);
                            if (transferResult.Status == MangoPay.SDK.Core.Enumerations.TransactionStatus.SUCCEEDED)
                            {
                                LogManager.LogInfo("-- MangoPay API Transfer Response with Payload: -- ");
                                LogManager.LogDebugObject(transferResult);
                                pay.Status = EPaymentStatus.TRANSFERRED;
                                PaymentTransaction PaymentTrans = DbContxt.PaymentTransactions.FirstOrDefault(pt => pt.OrderId == PaymentOrder.OrderID);
                                PaymentTrans.PaymentToken = JsonConvert.SerializeObject(transferResult);
                                pay.CreatePayload = JsonConvert.SerializeObject(paymentWebDetails);
                                DbContxt.Update(pay);
                                DbContxt.Update(PaymentTrans);
                                DbContxt.SaveChanges();
                            }
                        }
                        else
                        {
                            LogManager.LogInfo("Order #" + pay.OrderId.ToString() + " not found! Or not yet completed!");
                            continue;
                        }
                    }
                } 
            }
            catch (Exception ex)
            { 
                LogManager.LogError("-- Automatic Transfer: TransferFundsFromCustomerWalletToShopWallet Encountered an error --");
                LogManager.LogError("- Logging stacktrace: -");
                LogManager.LogDebugObject(ex.StackTrace);
                LogManager.LogError("- Logging Exception : -");
                LogManager.LogDebugObject(ex);
                LogManager.LogError("- Logging InnerException : -");
                LogManager.LogDebugObject(ex.InnerException);
            }
        }
    }
}
