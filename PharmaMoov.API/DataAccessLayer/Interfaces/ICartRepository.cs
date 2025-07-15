using PharmaMoov.Models;
using PharmaMoov.API.Helpers;
using System;
using PharmaMoov.Models.Orders;
using PharmaMoov.Models.Cart;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface ICartRepository
    {
        APIResponse SyncCartItem(CartSyncDTO CartSyncItems, string Authorization);
        APIResponse CheckoutCartItems(string Auth,CheckoutCartItem UserOrder);
        APIResponse GetMinimumCartAmount();
        APIResponse ValidateCartItems(Guid _shop, int _address, OrderDeliveryType _dType);
        APIResponse SyncCartItemForHealthProfessional(CartSyncForHealthProfessional CartSyncItems);
        APIResponse CheckoutCartItemsViaDirectCardPayIn(string Auth, CheckoutCartItem UserOrder, string cardId, string accessType);
    }
}
