using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using PharmaMoov.Models.Cart;
using PharmaMoov.Models.External.Medipim;
using PharmaMoov.Models.Orders;
using System;

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
