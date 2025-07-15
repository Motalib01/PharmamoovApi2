using PharmaMoov.Models;
using System;
using PharmaMoov.Models.Orders;
using System.Threading.Tasks;
using PharmaMoov.Models.Cart;
using System.Collections.Generic;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IOrderRepository
    {
        //APIResponse GetUserOrderHistory(string _auth, bool takeNewOrders);
        APIResponse GetUserOrderHistory(string _auth);
        APIResponse GetUserOrderDetails(string _auth, int _order);
        APIResponse GetShopsOrders(Guid _shop, bool _takeHistory);
        List<NewOrderItems> GetOrderItems(int OrderId);
        Task<APIResponse> ChangeOrderStatus(ChangeOrderStatus _order);
        APIResponse CancelOrder(string Auth, int _orderId);
        APIResponse UpdateOrderStatus(ChangeOrderStatus order);
        Task<APIResponse> TestPushNotifcation(string fcmToken);
    }
}
