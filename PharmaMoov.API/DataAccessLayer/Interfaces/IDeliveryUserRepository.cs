using PharmaMoov.Models;
using PharmaMoov.Models.DeliveryUser;
using System;
using System.Threading.Tasks;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IDeliveryUserRepository
    {
        APIResponse UpdateLocation(string auth, UpdateLocationParamModel model);
        APIResponse GetNewOrderList(string auth, Guid deliveryUserId, int pageNo);
        APIResponse GetOrderDetail(int orderId);
        Task<APIResponse> AcceptOrder(AcceptOrderParamModel model);
        APIResponse UpdateOrderStatus(UpdateOrderStatusParamModel model);
        APIResponse UpdateReceiveOrder(UpdateReceiveOrderParamModel model);
        APIResponse GetOrdersList(OrderListParamModel model);
        APIResponse GetAcceptedOrderDetail(string auth, Guid deliveryUserId);
        APIResponse CheckAcceptedOrder(Guid deliveryUserId);
    }
}
