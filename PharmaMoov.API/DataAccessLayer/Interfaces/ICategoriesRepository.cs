using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.Product;
using PharmaMoov.Models.Shop;
using System;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface ICategoriesRepository
    {
        APIResponse PopulateShopCategories(int _category, int IsActive);
        APIResponse AddShopCategory(string _auth, ShopCategoriesDTO _category);
        APIResponse EditShopCategory(string _auth, ShopCategoriesDTO _category);
        APIResponse ChangeShopCategoryStatus(string _auth, ChangeRecordStatus _category);
        APIResponse PopulateProductCategories(Guid _shopId, int _category, int IsActive);
        APIResponse AddProductCategory(string _auth, ProductCategoriesDTO _category);
        APIResponse EditProductCategory(string _auth, ProductCategoriesDTO _category);
        APIResponse FilterProductCategories(string _searchKey);
    }
}
