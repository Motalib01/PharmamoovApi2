using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using System;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IDashboardRepository
    {
        APIResponse GetPharmacyAdminDashboard(DashboardParamModel dashBoardParamModel);
        APIResponse GetSuperAdminDashboard();
        APIResponse GetOrdersNotification(Guid ShopId);
    }
}
