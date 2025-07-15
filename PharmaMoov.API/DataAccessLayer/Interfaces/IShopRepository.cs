using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.Shop;
using System;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IShopRepository
    {
        APIResponse RegisterShop(ShopProfile _shop, IMainHttpClient _mhttp, APIConfigurationManager _conf = null);
        APIResponse AddShopRequest(ShopRequestDTO _shop);
        APIResponse GetShopRequests();
        APIResponse GetShopProfile(Guid _shop);
        APIResponse GetShopOpeningHours(Guid _shop);
        APIResponse EditShopOpeningHours(ShopHourList _shop);
        APIResponse EditShopProfile(Shop _shop);
        APIResponse SetShopConfigurations(ShopConfigs _shop);
        APIResponse AddShopDocument(string _auth, ShopDocumentDTO _shop);
        APIResponse GetShopDocuments(string _auth, Guid _shop);
        APIResponse DeleteShopDocuments(string _auth, int _shop);
        APIResponse GetShopList();
        APIResponse ChangeShopStatus(string _auth, ChangeRecordStatus _shop);
        APIResponse ShopCommisionInvoice(ShopCommissionDateRange _range, string Authorization);
        APIResponse ComissionInvoiceView(SingleShopComissionInvoiceParameters _sParams, string Authorization);
        APIResponse ChangePopularStatus(string _auth, ChangeRecordStatus _shop);

        #region "Pharmacy Request Registration"
        APIResponse ChangeRegistrationStatus(string _auth, ChangeRegistrationStatus _shop);
        APIResponse GetShopRegistrationRequest(int _shopId);
        APIResponse GetRequestList();
        #endregion
        APIResponse GetShopListForAutocomplete(string _searchKey);
        APIResponse GetPharmacyOwner(int shopRecordId);
        APIResponse UpdatePharmacyOwner(PharmacyOwner model);
    }
}
