using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using PharmaMoov.Models.Orders;
using PharmaMoov.Models.User;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class DashboardRepository : APIBaseRepo, IDashboardRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public DashboardRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public APIResponse GetPharmacyAdminDashboard(DashboardParamModel dashboardParamModel)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetFullDashboard");
            try
            {
                PharmacyAdminDashboardModel model = new PharmacyAdminDashboardModel();

                if (dashboardParamModel.SalesReportType != (int)SalesReportTypeEnum.SalesByDate && dashboardParamModel.SalesReportType != (int)SalesReportTypeEnum.SalesByProduct)
                {
                    aResp.Message = "Indiquez un type de rapport de vente correct.";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return aResp;
                }

                List<Order> orderList = DbContext.Orders.Where(s=>s.ShopId == dashboardParamModel.ShopId).ToList();
                
                model.CompletedOrder = orderList.Where(s => s.OrderProgressStatus == OrderProgressStatus.COMPLETED).Count();
                model.RefusedOrder = orderList.Where(s => s.OrderProgressStatus == OrderProgressStatus.REJECTED).Count();
                model.TotalOrder = orderList.Where(s => s.OrderProgressStatus != OrderProgressStatus.PENDING).Count();

                var paymentList = DbContext.Payments.AsNoTracking().Where(s => s.CreatedDate >= dashboardParamModel.DateFrom && s.CreatedDate <= dashboardParamModel.DateTo).ToList();
                orderList = orderList.Where(s => s.CreatedDate >= dashboardParamModel.DateFrom && s.CreatedDate <= dashboardParamModel.DateTo).ToList();
                var totalDays = (dashboardParamModel.DateTo - dashboardParamModel.DateFrom).TotalDays;
                
                if (dashboardParamModel.SalesReportType == (int)SalesReportTypeEnum.SalesByDate)
                {
                    var totalSale = paymentList.Sum(s => s.DebitedFundsAmount);
                 
                    model.TotalSalesInThePeriod = totalSale;
                    if(totalDays != 0)
                    {
                        model.AvgSalesInThePeriod = Math.Round((totalSale / (decimal)totalDays), 2);
                    }
                    else
                    {
                        model.AvgSalesInThePeriod = 0;
                    }
                    

                    model.TotalOrderInThePeriod = orderList.Count();
                    model.TotalPurchaseInThePeriod = orderList
                                          .Join(DbContext.OrderItems, p => p.OrderID, po => po.OrderID, (p, po) => new { p, po })
                                          .Sum(s => s.po.ProductQuantity);
                }
                else
                {
                    if(dashboardParamModel.ProductRecordIdList != null && dashboardParamModel.ProductRecordIdList.Count() > 0)
                    {
                        var orderIdList = orderList
                                          .Join(DbContext.OrderItems, p => p.OrderID, po => po.OrderID, (p, po) => new { p, po })
                                          .Where(s => dashboardParamModel.ProductRecordIdList.Contains(s.po.ProductRecordId))
                                          .Select(s => s.p.OrderID).ToList();
                       
                        var totalSale = paymentList.Where(s => orderIdList.Contains(s.OrderId)).Sum(s => s.DebitedFundsAmount);

                        model.TotalSalesInThePeriod = totalSale;
                        model.TotalPurchaseInThePeriod = orderList
                                          .Join(DbContext.OrderItems, p => p.OrderID, po => po.OrderID, (p, po) => new { p, po })
                                          .Where(s => dashboardParamModel.ProductRecordIdList.Contains(s.po.ProductRecordId))
                                          .Sum(s => s.po.ProductQuantity);
                    }                    
                }

                aResp.Message = "Tous les éléments ont été récupérés avec succès.";
                aResp.Status = "Succès";
                aResp.Payload = model;
                aResp.StatusCode = System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetFullDashboard");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetSuperAdminDashboard()
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetSuperAdminDashboard");
            try
            {
                SuperAdminDashboardModel model = new SuperAdminDashboardModel();
                model.TotalUser = DbContext.Users.AsNoTracking().Where(s=>s.IsEnabled == true && s.AccountType == AccountTypes.APPUSER).Count();
                model.TotalShop = DbContext.Shops.AsNoTracking().Where(s => s.IsEnabled == true && s.RegistrationStatus == RegistrationStatus.APPROVE).Count();
                model.TotalPendingRequest = DbContext.Shops.AsNoTracking().Where(s =>  s.RegistrationStatus == RegistrationStatus.PENDING).Count();
                model.TotalOrder = DbContext.Orders.AsNoTracking().Where(s => s.OrderProgressStatus != OrderProgressStatus.PENDING).Count();
                
                aResp.Message = "Tous les éléments ont été récupérés avec succès.";
                aResp.Status = "Succès";
                aResp.Payload = model;
                aResp.StatusCode = System.Net.HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetSuperAdminDashboard");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetOrdersNotification(Guid ShopId)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetOrdersNotification: ShopId " + ShopId);

            try
            {
                IEnumerable<OrderListNotification> orderList = null;

                orderList = DbContext.Orders
                                .Join(DbContext.Users, o => o.UserId, u => u.UserId, (o, u) => new { o, u })
                                .Where(w => w.o.OrderProgressStatus == OrderProgressStatus.PLACED && w.o.ShopId == ShopId)
                                .Select(p => new OrderListNotification
                                {
                                    OrderID = p.o.OrderID,
                                    ShopId = p.o.ShopId,
                                    UserId = p.o.UserId,
                                    FullName = p.u.FirstName + " " + p.u.LastName,
                                    CreatedDate = p.o.CreatedDate
                                }).OrderByDescending(p => p.OrderID).Take(15).ToList();

                aResp.Message = "Articles récupérés avec succès.";
                aResp.Status = "Succès";
                aResp.Payload = orderList;
                aResp.StatusCode = System.Net.HttpStatusCode.OK;

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetOrdersNotification");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        /// <summary>
        /// Get the full dashboard data
        /// </summary>
        /// <param name="_dateRange"></param>
        /// <returns></returns>
        //public APIResponse GetFullDashboard(DashBoardDateRangeFilter _dateRange)
        //{
        //    APIResponse returnResp = new APIResponse();
        //    try
        //    {
        //        LogManager.LogInfo("GetFullDashboard");
        //        LogManager.LogInfo("Fech data for dashboard with daterange : ");
        //        LogManager.LogDebugObject(_dateRange);

        //        DashboardViewModel dashboardVM = new DashboardViewModel();
        //        //
        //        // get the total number of users
        //        dashboardVM.TotalUsersToDate = DbContext.Users.Count();
        //        //
        //        //

        //        // get t0tap registration for the period
        //        dashboardVM.TotalRegistrationForThePeriod = DbContext.Users.Where(u => u.CreatedDate > _dateRange.From && u.CreatedDate < _dateRange.To).Count();
        //        //

        //        // Get user registration range for the the period
        //        dashboardVM.UserRegistrationRangeForThePeriod = DbContext.DayKeyPairReports.FromSqlRaw("SELECT " +
        //                                                                                            "CONVERT(nvarchar, CreatedDate, 7) as 'Day', " +
        //                                                                                            "CONVERT(nvarchar(100),Count(UserId)) as 'Value', " +
        //                                                                                            "MAX(CreatedDate) as 'OrderBYC' " +
        //                                                                                            "FROM Users " +
        //                                                                                            "WHERE CreatedDate > {0} AND CreatedDate < {1} " +
        //                                                                                            "Group By CONVERT(nvarchar, CreatedDate, 7) ORDER BY OrderBYC", _dateRange.From, _dateRange.To).ToList();


        //        //
        //        // get transactions range for the period
        //        dashboardVM.TransactionsRangeForThePeriod = DbContext.DayKeyPairReports.FromSqlRaw("SELECT " +
        //                                                                                        "CONVERT(nvarchar, CreatedDate, 7) as 'Day', " +
        //                                                                                        "CONVERT(nvarchar(100),Count(RecordTransID)) as 'Value', " +
        //                                                                                        "MAX(CreatedDate) as 'OrderBYC'" +
        //                                                                                        "FROM POSRecieptTransactions " +
        //                                                                                        "WHERE CreatedDate > {0} AND CreatedDate < {1} " +
        //                                                                                        "Group By CONVERT(nvarchar, CreatedDate, 7)  ORDER BY OrderBYC", _dateRange.From, _dateRange.To).ToList();

        //        //
        //        // get redeem points range for the period
        //        dashboardVM.RedeemPointsRangeForThePeriod = DbContext.DayKeyPairReports.FromSqlRaw("SELECT " +
        //                                                                                        "CONVERT(nvarchar, CreatedDate, 7) as 'Day', " +
        //                                                                                        "CONVERT(nvarchar, CAST(Sum(RedeemedPointsAmount) AS INT)) as 'Value', " +
        //                                                                                        "MAX(CreatedDate) as 'OrderBYC' " +
        //                                                                                        "FROM RedeemedPoints " +
        //                                                                                        "WHERE CreatedDate > {0} AND CreatedDate < {1} " +
        //                                                                                        "Group By CONVERT(nvarchar, CreatedDate, 7)  ORDER BY OrderBYC", _dateRange.From, _dateRange.To).ToList();

        //        // get earn points range for the period
        //        dashboardVM.EarnedPointsRangeForThePeriod = DbContext.DayKeyPairReports.FromSqlRaw("SELECT " +
        //                                                                                        "CONVERT(nvarchar, CreatedDate, 7) as 'Day', " +
        //                                                                                        "CONVERT(nvarchar, CAST(Sum(EarnedPointsAmount) AS INT)) as 'Value', " +
        //                                                                                        "MAX(CreatedDate) as 'OrderBYC' " +
        //                                                                                        "FROM EarnedPoints " +
        //                                                                                        "WHERE CreatedDate > {0} AND CreatedDate < {1} " +
        //                                                                                        "Group By CONVERT(nvarchar, CreatedDate, 7)  ORDER BY OrderBYC", _dateRange.From, _dateRange.To).ToList();

        //        returnResp.Message = "Fetch Dashboard Data success!";
        //        returnResp.Status = "Succès";
        //        returnResp.StatusCode = System.Net.HttpStatusCode.OK;
        //        returnResp.Payload = dashboardVM;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.LogInfo("GetFullDashboard");
        //        LogManager.LogError(ex.InnerException.Message);
        //        LogManager.LogError(ex.StackTrace);
        //        returnResp.Message = "La nouvelle adresse a été ajoutée avec succès.!";
        //        returnResp.Status = "Erreur de serveur interne";
        //        returnResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
        //        returnResp.ModelError = GetStackError(ex.InnerException);
        //    }
        //    return returnResp;
        //}

    }
}
