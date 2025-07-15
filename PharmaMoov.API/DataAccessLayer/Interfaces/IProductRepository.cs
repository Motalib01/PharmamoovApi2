using PharmaMoov.Models;
using PharmaMoov.Models.Cart;
using PharmaMoov.Models.Product;
using System;
using System.Collections.Generic;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IProductRepository
    {
        APIResponse AddProduct(Product _product);
        APIResponse AddProductFromExtern(Guid shop, int productRecordId);
        APIResponse EditProduct(Product _product);
        APIResponse GetAllProducts(Guid _shop, int _product);
        APIResponse GetAllExternalProducts(int _product);
        APIResponse ChangeProductStatus(ChangeProdStatus _product);
        APIResponse PopulateProductAndPharmacy(int _productCategoryId, int _isActive, string _searchKey, int sortBy, bool isProductFeature);
        APIResponse PopulateProductBySort(int productCategoryId, Guid shopId, int sortBy, int pageNo, string searchKey);
        APIResponse ImportProduct(ImportProductParamModel model);
        APIResponse GetProductsForPrescription(Guid shopId, int productRecordId);
    }
}
