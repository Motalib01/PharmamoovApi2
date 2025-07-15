using PharmaMoov.Models;
using PharmaMoov.Models.Product;
using PharmaMoov.Models.Shop;
using System;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface ICatalogueRepository
    {
        APIResponse GetMainCatalogue(FilterMain _filter);
        APIResponse GetAllShopsCategories(FilterShopCategoriesModel _filterShopCategories);
        APIResponse FilterShops(FilterShops _filter);
        APIResponse GetShopsProductCategories(Guid _shop);
        APIResponse GetShopDetails(Guid _shop);
        APIResponse FilterShopsProducts(FilterProducts _filter);
        APIResponse GetShopsProductDetails(Guid _product);
        APIResponse CatalougeForRegularCustomer();
        APIResponse GetShopAddressDetailsForMap(FilterShopAddress filterShopAddress);
        APIResponse GetAllShop(ShopListParamModel model);
        APIResponse GetTopShops();
    }
}
