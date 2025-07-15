using PharmaMoov.Models;
using PharmaMoov.Models.Product;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.IO;
using NPOI.SS.Formula.Functions;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class ProductRepository : APIBaseRepo, IProductRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private readonly LocalizationService localization;
        private IHttpContextAccessor accessor;

        public ProductRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, IHttpContextAccessor _accessor, LocalizationService _localization)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            localization = _localization;
            accessor = _accessor;
        }
		public APIResponse AddProductFromExtern(Guid shop, int productRecordId)
		{
			APIResponse aResp = new APIResponse();
			LogManager.LogInfo("AddProduct");

			try
			{
				var externalProduct = DbContext.ExternalProducts.Where(c =>  c.ProductRecordId == productRecordId).FirstOrDefault();

				// check duplicate record for product (with Name and CategoryId)
				Product foundDuplicate = DbContext.Products.Where(c => c.ProductName == externalProduct.ProductName && c.ProductCategoryId == externalProduct.ProductCategoryId).FirstOrDefault();
				if (foundDuplicate == null)
				{
					DateTime NowDate = DateTime.Now;
					externalProduct.ProductId = Guid.NewGuid();
					Product insertedProduct = ProductMapper.MapFromExternal(externalProduct, shop);
					DbContext.Products.Add(insertedProduct);
					DbContext.SaveChanges();
					aResp = new APIResponse
					{
						Message = "Nouveau produit ajouté avec succès",
						Status = "Succès!",
						Payload = externalProduct,
						StatusCode = System.Net.HttpStatusCode.OK
					};
				}
				else
				{
					aResp = new APIResponse
					{
						Message = "Duplicat d'enregistrement trouvé.",
						Status = "Échec!",
						StatusCode = System.Net.HttpStatusCode.BadRequest
					};
				}
			}
			catch (Exception ex)
			{
				LogManager.LogInfo("AddProduct");
				LogManager.LogError(ex.InnerException.Message);
				LogManager.LogError(ex.StackTrace);
				aResp.Message = "Quelque chose s'est mal passé !";
				aResp.Status = "Erreur de serveur interne";
				aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
				aResp.ModelError = GetStackError(ex.InnerException);
			}
			return aResp;

		}



		public APIResponse AddProduct(Product _product)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("AddProduct");

            try
            {
                // check duplicate record for product
                Product foundDuplicate = DbContext.Products.Where(c => c.ProductName == _product.ProductName && c.ProductCategoryId == _product.ProductCategoryId).FirstOrDefault();
                if (foundDuplicate == null)
                {
                    DateTime NowDate = DateTime.Now;
                    _product.ProductId = Guid.NewGuid();
                    _product.IsEnabled = true;
                    _product.CreatedDate = NowDate;
                    _product.DateEnabled = NowDate;

                    DbContext.Products.Add(_product);
                    DbContext.SaveChanges();
                    aResp = new APIResponse
                    {
                        Message = "Nouveau produit ajouté avec succès",
                        Status = "Succès!",
                        Payload = _product,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Duplicat d'enregistrement trouvé.",
                        Status = "Échec!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AddProduct");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse EditProduct(Product _product)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("EditProduct");

            try
            {
                Product updProd = DbContext.Products.Where(a => a.ProductRecordId == _product.ProductRecordId).FirstOrDefault();
                if (updProd != null)
                {
                    // check duplicate record for product
                    var foundDuplicate = DbContext.Products.Where(c => c.ProductName == _product.ProductName && c.ProductCategoryId == updProd.ProductCategoryId);
                    if (foundDuplicate.FirstOrDefault(d => d.ProductRecordId != _product.ProductRecordId) == null)
                    {
                        DateTime NowDate = DateTime.Now;
                        updProd.LastEditedBy = _product.LastEditedBy;
                        updProd.LastEditedDate = _product.LastEditedDate;
                        updProd.IsEnabled = true;
                        updProd.IsEnabledBy = _product.IsEnabledBy;
                        updProd.DateEnabled = _product.DateEnabled;
                        updProd.IsLocked = _product.IsLocked;
                        updProd.LockedDateTime = _product.LockedDateTime;

                        updProd.ProductCategoryId = _product.ProductCategoryId;
                        updProd.ProductName = _product.ProductName;
                        updProd.ProductDesc = _product.ProductDesc;
                        updProd.ProductIcon = _product.ProductIcon;
                        updProd.ProductPrice = _product.ProductPrice;
                        updProd.ProductUnit = _product.ProductUnit;
                        //updProd.ProductPricePerKG = _product.ProductPricePerKG;
                        updProd.ProductTaxValue = _product.ProductTaxValue;
                        updProd.IsSale = _product.IsSale;
                        updProd.IsUnsold = _product.IsUnsold;
                        updProd.IsFragile = _product.IsFragile;
                        updProd.IsProductFeature = _product.IsProductFeature;
                        updProd.ProductStatus = _product.ProductStatus;
                        updProd.SalePrice = _product.SalePrice;

                        DbContext.Products.Update(updProd);
                        DbContext.SaveChanges();
                        aResp = new APIResponse
                        {
                            Message = "Produit mis à jour avec succès",
                            Status = "Succès!",
                            Payload = updProd,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Duplicat d'enregistrement trouvé.",
                            Status = "Échec!",
                            StatusCode = System.Net.HttpStatusCode.Found
                        };
                    }
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun enregistrement trouvé.",
                        Status = "Échec!",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("EditProduct");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetAllProducts(Guid _shop, int _product)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetAllProducts");

            try
            {
                if (_product == 0)
                {
                    IEnumerable<ProductList> products = null;
                    products = DbContext.Products
                        .Join(DbContext.ProductCategories, i => i.ProductCategoryId, c => c.ProductCategoryId, (i, c) => new { i, c })
                        .Where(w => w.i.ShopId == _shop)
                        .Select(p => new ProductList
                        {
                            ProductRecordId = p.i.ProductRecordId,
                            ProductIcon = p.i.ProductIcon,
                            ProductName = p.i.ProductName,
                            ProductPrice = p.i.ProductPrice,
                            ProductCategory = p.c.ProductCategoryName,
                            ProductStatus = p.i.ProductStatus,
                            ShopName = DbContext.Shops.FirstOrDefault(s => s.ShopId == _shop).ShopName
                        });

                    aResp = new APIResponse
                    {
                        Message = "Tous les produits en service ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = products,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    ProductDetails pDetails = null;
                    //product = DbContext.Products.FirstOrDefault(i => i.ProductRecordId == _product);

                    pDetails = DbContext.Products
                        .Where(i => i.ProductRecordId == _product)
                        .Select(i => new ProductDetails
                        {
                            ProductRecordId = i.ProductRecordId,
                            ProductIcon = i.ProductIcon,
                            ProductName = i.ProductName,
                            ProductDesc = i.ProductDesc,
                            ProductPrice = i.ProductPrice,
                            SalePrice = i.SalePrice,
                            ShopId = i.ShopId,
                            ShopName = DbContext.Shops.FirstOrDefault(s => s.ShopId == i.ShopId).ShopName,
                            ProductStatus = i.ProductStatus,
                            ProductUnit = i.ProductUnit,
                            ProductTaxValue = i.ProductTaxValue,
                            ProductPricePerKG = i.ProductPricePerKG,
                            IsProductFeature = i.IsProductFeature,
                            IsFragile = i.IsFragile,
                            ProductCategoryId = i.ProductCategoryId
                        }).FirstOrDefault();

                    aResp = new APIResponse
                    {
                        Message = "Détails du produit récupérés avec succès.",
                        Status = "Succès!",
                        Payload = pDetails,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetAllProducts");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public bool IsProductExist(string productName,int productCategoryId)
		{
			return DbContext.Products.Any(p => p.ProductName == productName && p.ProductCategoryId == productCategoryId);
		}
		public APIResponse GetAllExternalProducts(int _product)
		{
			APIResponse aResp = new APIResponse();
			LogManager.LogInfo("GetAllExternalProducts");

			try
			{
				if (_product == 0)
				{
					// Materialize the query results by calling .ToList() or .AsEnumerable()
					var products = DbContext.ExternalProducts
						.Join(DbContext.ProductCategories,
							  i => i.ProductCategoryId,
							  c => c.ProductCategoryId,
							  (i, c) => new { i, c })
						.Select(p => new ExternalProductList
						{
							ProductRecordId = p.i.ProductRecordId,
							ProductIcon = p.i.ProductIcon,
							ProductName = p.i.ProductName,
							ProductPrice = p.i.ProductPrice,
							ProductCategory = p.c.ProductCategoryName,
							ProductStatus = p.i.ProductStatus,
							ProductCategoryId = p.i.ProductCategoryId // Ensure this is included
						})
						.ToList(); // Materialize the query here

					// Now you can safely iterate and call IsProductExist
					foreach (var product in products)
					{
						product.ExistsInDatabase = IsProductExist(product.ProductName, product.ProductCategoryId);
					}

					aResp = new APIResponse
					{
						Message = "Tous les produits en service ont été récupérés avec succès.",
						Status = "Succès!",
						Payload = products,
						StatusCode = System.Net.HttpStatusCode.OK
					};
				}
				else
				{
					var pDetails = DbContext.ExternalProducts
						.Where(i => i.ProductRecordId == _product)
						.Select(i => new ExternalProductDetails
						{
							ProductRecordId = i.ProductRecordId,
							ProductName = i.ProductName,
							ProductDesc = i.ProductDesc,
							ProductPrice = i.ProductPrice,
							SalePrice = i.SalePrice,
							ProductStatus = i.ProductStatus,
							ProductUnit = i.ProductUnit,
							ProductTaxValue = i.ProductTaxValue,
							ProductPricePerKG = i.ProductPricePerKG,
							IsProductFeature = i.IsProductFeature,
							IsFragile = i.IsFragile,
							ProductCategoryId = i.ProductCategoryId
						})
						.FirstOrDefault();

					aResp = new APIResponse
					{
						Message = "Détails du produit récupérés avec succès.",
						Status = "Succès!",
						Payload = pDetails,
						StatusCode = System.Net.HttpStatusCode.OK
					};
				}
			}
			catch (Exception ex)
			{
				LogManager.LogInfo("GetAllProducts");
				LogManager.LogError(ex.InnerException?.Message ?? ex.Message); // Handle null InnerException
				LogManager.LogError(ex.StackTrace);
				aResp.Message = "Quelque chose s'est mal passé !";
				aResp.Status = "Erreur de serveur interne";
				aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
				aResp.ModelError = GetStackError(ex.InnerException ?? ex); // Handle null InnerException
			}
			return aResp;
		}

		public APIResponse ChangeProductStatus(ChangeProdStatus _product)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ChangeProductStatus");

            try
            {
                Product foundProduct = DbContext.Products.Where(a => a.ProductRecordId == _product.ProductRecordId).FirstOrDefault();
                if (foundProduct != null)
                {
                    DateTime NowDate = DateTime.Now;
                    foundProduct.LastEditedBy = _product.AdminId;
                    foundProduct.LastEditedDate = NowDate;
                    foundProduct.ProductStatus = _product.ProductStatus;

                    DbContext.Products.Update(foundProduct);
                    DbContext.SaveChanges();
                    aResp = new APIResponse
                    {
                        Message = "Produit mis à jour avec succès",
                        Status = "Succès!",
                        Payload = _product,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun enregistrement trouvé.",
                        Status = "Échec!",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("ChangeProductStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse PopulateProductAndPharmacy(int _productCategoryId, int _isActive, string _searchKey, int sortBy, bool isProductFeature)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("PopulateProductAndPharmacy: " + _productCategoryId);

            try
            {                
                List<ProductAndPharmacyModel> productAndPharmacyListModel = new List<ProductAndPharmacyModel>();
                var shopList = DbContext.Shops.AsNoTracking().Where(s => s.IsEnabled == true && s.RegistrationStatus == RegistrationStatus.APPROVE).Select(s=>new ProductAndPharmacyModel { ShopId = s.ShopId, ShopName = s.ShopName}).ToList();

                if (shopList.Count() > 0)
                {
                    foreach (var item in shopList)
                    {
                        List<ProductListModel> productList = null;                        
                        if(_productCategoryId > 0)
                        {
                            //DbContext.Products.Where(s => s.IsEnabled == (_isActive > 0 ? true : false) && s.ProductCategoryId == _productCategoryId && s.ShopId == item.ShopId)
                            productList = DbContext.Products.Where(s => s.ProductStatus != ProductStatus.INACTIVE && s.ProductCategoryId == _productCategoryId && s.ShopId == item.ShopId)
                               .Select(s => new ProductListModel
                               {
                                   ProductId = s.ProductId,
                                   ShopId = s.ShopId,
                                   ProductRecordId = s.ProductRecordId,
                                   ProductCategoryId = s.ProductCategoryId,
                                   ProductIcon = s.ProductIcon,
                                   ProductName = s.ProductName,
                                   ProductPrice = s.ProductPrice,
                                   SalePrice = s.SalePrice,
                                   IsActive = s.IsEnabled ?? false,
                                   IsProductFeature = s.IsProductFeature,
                                   ProductStatus = s.ProductStatus,
                                   PriceHolder = s.PriceHolder
                               }).ToList();

                            if(isProductFeature == true)
                            {
                                productList = productList.Where(s => s.IsProductFeature == true).ToList();
                            }

                            if (productList != null && productList.Count() > 0)
                            {
                                if (!string.IsNullOrEmpty(_searchKey))
                                {
                                    //productList = DbContext.Products.Where(s => s.ProductStatus != ProductStatus.INACTIVE && s.ProductName.ToLower().Contains(_searchKey.ToLower()) && s.ShopId == item.ShopId)
                                    //.Select(s => new ProductListModel
                                    //{
                                    //    ProductId = s.ProductId,
                                    //    ShopId = s.ShopId,
                                    //    ProductRecordId = s.ProductRecordId,
                                    //    ProductCategoryId = s.ProductCategoryId,
                                    //    ProductIcon = s.ProductIcon,
                                    //    ProductName = s.ProductName,
                                    //    ProductPrice = s.ProductPrice,
                                    //    SalePrice = s.SalePrice,
                                    //    IsActive = s.IsEnabled ?? false
                                    //}).ToList();
                                    productList = productList.Where(s=>s.ProductName.ToLower().Contains(_searchKey.ToLower())).ToList();
                                }

                                //if (sortBy == (int)ProductSortEnum.PriceHighToLow)
                                //{
                                //    productList = productList.OrderByDescending(s => s.SalePrice).ToList();
                                //}
                                //else if (sortBy == (int)ProductSortEnum.PriceLowToHigh)
                                //{
                                //    productList = productList.OrderBy(s => s.SalePrice).ToList();
                                //}

                                if (sortBy == (int)ProductSortEnum.PriceHighToLow || sortBy == (int)ProductSortEnum.PriceLowToHigh)
                                {
                                    foreach (var getItem in productList)
                                    {
                                        if (getItem.SalePrice != 0)
                                        {
                                            getItem.PriceHolder = getItem.SalePrice;
                                        }
                                        else
                                        {
                                            getItem.PriceHolder = getItem.ProductPrice;
                                        }
                                    }

                                    if (sortBy == (int)ProductSortEnum.PriceHighToLow)
                                    {
                                        productList = productList.OrderByDescending(s => s.PriceHolder).ToList();
                                    }
                                    else if (sortBy == (int)ProductSortEnum.PriceLowToHigh)
                                    {
                                        productList = productList.OrderBy(s => s.PriceHolder).ToList();
                                    }
                                }
                                else if (sortBy == (int)ProductSortEnum.AscendingProduct)
                                {
                                    productList = productList.OrderBy(s => s.ProductName).ToList();
                                }
                                else if(sortBy == (int)ProductSortEnum.DescendingProduct)
                                {
                                    productList = productList.OrderByDescending(s => s.ProductName).ToList();
                                }
                                
                                if (productList != null && productList.Count() > 0)
                                {
                                    productAndPharmacyListModel.Add(new ProductAndPharmacyModel { ShopId = item.ShopId, ShopName = item.ShopName, ProductListModel = productList });
                                }

                                if (productAndPharmacyListModel.Count() > 0)
                                {
                                    aResp = new APIResponse
                                    {
                                        Message = "Tous les éléments ont été récupérés avec succès.",
                                        Status = "Succès!",
                                        Payload = productAndPharmacyListModel,
                                        StatusCode = System.Net.HttpStatusCode.OK
                                    };
                                }
                                else
                                {
                                    aResp.Message = "Aucun article n'a été récupéré.";
                                    aResp.Status = "Échec";
                                    aResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(_searchKey))
                            {
                                productList = DbContext.Products.Where(s => s.ProductStatus != ProductStatus.INACTIVE && s.ProductName.ToLower().Contains(_searchKey.ToLower()) && s.ShopId == item.ShopId)
                                .Select(s => new ProductListModel
                                {
                                    ProductId = s.ProductId,
                                    ShopId = s.ShopId,
                                    ProductRecordId = s.ProductRecordId,
                                    ProductCategoryId = s.ProductCategoryId,
                                    ProductIcon = s.ProductIcon,
                                    ProductName = s.ProductName,
                                    ProductPrice = s.ProductPrice,
                                    SalePrice = s.SalePrice,
                                    IsActive = s.IsEnabled ?? false,
                                    IsProductFeature = s.IsProductFeature,
                                    ProductStatus = s.ProductStatus,
                                    PriceHolder = s.PriceHolder
                                }).ToList();
                            }
                            else
                            {
                                productList = DbContext.Products.Where(s => s.ProductStatus != ProductStatus.INACTIVE && s.ShopId == item.ShopId)
                                   .Select(s => new ProductListModel
                                   {
                                       ProductId = s.ProductId,
                                       ShopId = s.ShopId,
                                       ProductRecordId = s.ProductRecordId,
                                       ProductCategoryId = s.ProductCategoryId,
                                       ProductIcon = s.ProductIcon,
                                       ProductName = s.ProductName,
                                       ProductPrice = s.ProductPrice,
                                       SalePrice = s.SalePrice,
                                       IsActive = s.IsEnabled ?? false,
                                       IsProductFeature = s.IsProductFeature,
                                       ProductStatus = s.ProductStatus,
                                       PriceHolder = s.PriceHolder
                                   }).ToList();
                            }

                            if (isProductFeature == true)
                            {
                                productList = productList.Where(s => s.IsProductFeature == true).ToList();
                            }

                            if (productList != null && productList.Count() > 0)
                            {
                                //if (sortBy == (int)ProductSortEnum.PriceHighToLow)
                                //{
                                //    productList = productList.OrderByDescending(s => s.SalePrice).ToList();
                                //}
                                //else if (sortBy == (int)ProductSortEnum.PriceLowToHigh)
                                //{
                                //    productList = productList.OrderBy(s => s.SalePrice).ToList();
                                //}
                                if (sortBy == (int)ProductSortEnum.PriceHighToLow || sortBy == (int)ProductSortEnum.PriceLowToHigh)
                                {
                                    foreach (var getItem in productList)
                                    {
                                        if (getItem.SalePrice != 0)
                                        {
                                            getItem.PriceHolder = getItem.SalePrice;
                                        }
                                        else
                                        {
                                            getItem.PriceHolder = getItem.ProductPrice;
                                        }
                                    }

                                    if (sortBy == (int)ProductSortEnum.PriceHighToLow)
                                    {
                                        productList = productList.OrderByDescending(s => s.PriceHolder).ToList();
                                    }
                                    else if (sortBy == (int)ProductSortEnum.PriceLowToHigh)
                                    {
                                        productList = productList.OrderBy(s => s.PriceHolder).ToList();
                                    }
                                }
                                else if (sortBy == (int)ProductSortEnum.AscendingProduct)
                                {
                                    productList = productList.OrderBy(s => s.ProductName).ToList();
                                }
                                else if (sortBy == (int)ProductSortEnum.DescendingProduct)
                                {
                                    productList = productList.OrderByDescending(s => s.ProductName).ToList();
                                }                              
                                productAndPharmacyListModel.Add(new ProductAndPharmacyModel { ShopId = item.ShopId, ShopName = item.ShopName, ProductListModel = productList });
                            }
                        }                        
                    }
                }

                if (productAndPharmacyListModel.Count() > 0)
                {
                    aResp = new APIResponse
                    {
                        Message = "Tous les éléments ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = productAndPharmacyListModel,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };                 
                }
                else
                {
                    aResp.Message = "Aucun article n'a été récupéré.";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                }               
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("PopulateProductAndPharmacy");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse PopulateProductBySort(int productCategoryId, Guid shopId, int sortBy, int pageNo,string searchKey)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("PopulateProductBySort");
            try
            {
                if(sortBy != (int)ProductSortEnum.PriceLowToHigh && sortBy != (int)ProductSortEnum.PriceHighToLow && sortBy != (int)ProductSortEnum.AscendingProduct && sortBy != (int)ProductSortEnum.DescendingProduct)
                {
                    aResp.Message = "Fournissez une valeur de tri valide.";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return aResp;
                }

                var productList = DbContext.Products.AsNoTracking()
                           .Join(DbContext.Shops, p => p.ShopId, s => s.ShopId, (p, s) => new { p, s })
                           .Where(w => w.p.IsEnabled == true && w.s.IsEnabled == true)
                           .Select(i => new 
                           {
                               i.p.ProductId,
                               i.p.ProductRecordId,
                               i.p.ProductCategoryId,
                               i.p.ProductIcon,
                               i.p.ProductName,
                               i.p.ProductPrice,
                               i.p.SalePrice,
                               IsActive = i.p.IsEnabled ?? false,
                               i.p.ShopId
                           }).ToList();

                if(shopId != null && shopId != Guid.Empty)
                {
                    productList = productList.Where(s => s.ShopId == shopId).ToList();
                }

                if(productCategoryId > 0)
                {
                    productList = productList.Where(s=>s.ProductCategoryId == productCategoryId).ToList();
                }

                if(!string.IsNullOrEmpty(searchKey))
                {
                    productList = productList.Where(s=>s.ProductName.ToLower().Contains(searchKey.ToLower())).ToList();
                }

                var pageSize = 10;
                var skipCount = pageNo * pageSize;

                if (sortBy == (int)ProductSortEnum.PriceHighToLow)
                {
                    productList = productList.OrderByDescending(s => s.SalePrice).Skip(skipCount).Take(pageSize).ToList();
                }
                else if (sortBy == (int)ProductSortEnum.PriceLowToHigh)
                {
                    productList = productList.OrderBy(s => s.SalePrice).Skip(skipCount).Take(pageSize).ToList();
                }
                else if (sortBy == (int)ProductSortEnum.AscendingProduct)
                {
                    productList = productList.OrderBy(s => s.ProductName).Skip(skipCount).Take(pageSize).ToList();
                }
                else
                {
                    productList = productList.OrderByDescending(s => s.ProductName).Skip(skipCount).Take(pageSize).ToList();
                }

                if (productList.Count() > 0)
                {
                    // get total page count
                    var totalPageCount = 1;
                    var pageCount = productList.Count;
                    if (pageCount > 0 && pageCount >= pageSize)
                    {
                        totalPageCount = (int)Math.Ceiling((double)pageCount / pageSize);
                    }
                    aResp = new APIResponse
                    {
                        Message = "Tous les éléments ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = new
                        {
                            ProductList = productList,
                            PageCount = totalPageCount
                        },
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp.Message = "Aucun article n'a été récupéré.";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("PopulateProductBySort");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse ImportProduct(ImportProductParamModel model)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ImportProduct - " + model);
            try
            {
                if (model.File != null && model.File.Length > 0)
                {
                    List<ImportProductErrorModel> errorModel = new List<ImportProductErrorModel>();
                    List<Product> productList = new List<Product>();
                    int totalProduct = 0;
                    using (var stream = new MemoryStream())
                    {

                        model.File.CopyToAsync(stream);
                        ISheet sheet;
                        string sFileExtension = Path.GetExtension(model.File.FileName).ToLower();
                        stream.Position = 0;

                        if(sFileExtension != ".xls" && sFileExtension != ".xlsx")
                        {
                            aResp = new APIResponse
                            {
                                Message = "Veuillez télécharger un fichier .xls ou .xlsx uniquement..",
                                Status = "Échec",                                
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                            return aResp;
                        }

                        if (sFileExtension == ".xls")
                        {
                            HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                            sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  
                        }
                        else
                        {
                            XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                            sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   
                        }

                        IRow headerRow = sheet.GetRow(0); //Get Header Row
                        int cellCount = headerRow.LastCellNum;
                        LogManager.LogInfo("cellCount - " + cellCount);
                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                        {
                            LogManager.LogInfo("InsideLoop - " + i);
                            Product product = new Product();
                            IRow row = sheet.GetRow(i);

                            if (row == null) continue;
                            if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                           
                            string productName = null;
                            totalProduct = totalProduct + 1;
                            //Product Name
                            if (row.GetCell(0) != null && row.GetCell(0).ToString() != "")
                            {
                                productName = row.GetCell(0).ToString().Trim();
                                product.ProductName = productName;
                            }
                            else
                            {
                                RequiredValidation(i, "Name", "Product Name", errorModel);
                                continue;
                            }
                            LogManager.LogInfo("ProductName - " + productName);

                            //Product category Id
                            if (row.GetCell(1) != null && row.GetCell(1).ToString() != "")
                            {
                                var categoryId = row.GetCell(1).ToString().Trim();
                                LogManager.LogInfo("CategoryId - " + categoryId);
                                if (CheckValidExcelData(i, categoryId, "Numeric", productName, "Product category", errorModel))
                                {
                                    product.ProductCategoryId = Convert.ToInt32(categoryId);
                                }
                                else continue;                               
                            }
                            else
                            {
                                RequiredValidation(i, productName, "Product category", errorModel);
                                continue;
                            }

                            
                            //Product Description
                            if (row.GetCell(2) != null && row.GetCell(2).ToString() != "")
                            {
                                product.ProductDesc = row.GetCell(2).ToString().Trim();
                                LogManager.LogInfo("ProductDesc - " + product.ProductDesc);
                            }
                            else
                            {
                                RequiredValidation(i, productName, "Product Description", errorModel);
                                continue;
                            }

                            //Product Price
                            if (row.GetCell(3) != null && row.GetCell(3).ToString() != "")
                            {
                                var price = row.GetCell(3).ToString().Trim();
                                LogManager.LogInfo("Price - " + price);
                                if (CheckValidExcelData(i, price, "Decimal", productName, "Price", errorModel))
                                {
                                    product.ProductPrice = Convert.ToDecimal(price);
                                }
                                else continue;
                            }
                            else
                            {
                                RequiredValidation(i, productName, "Price", errorModel);
                                continue;
                            }

                            //Sale Price
                            if (row.GetCell(4) != null && row.GetCell(4).ToString() != "")
                            {
                                var salePrice = row.GetCell(4).ToString().Trim();
                                LogManager.LogInfo("SalePrice - " + salePrice);
                                if (CheckValidExcelData(i, salePrice, "Decimal", productName, "Sale Price", errorModel))
                                {
                                    product.SalePrice = Convert.ToDecimal(salePrice);
                                }
                                else continue;
                            }
                            else
                            {
                                RequiredValidation(i, productName, "Sale Price", errorModel);
                                continue;
                            }

                            //Unit
                            if (row.GetCell(5) != null && row.GetCell(5).ToString() != "")
                            {
                                product.ProductUnit = row.GetCell(5).ToString().Trim();
                                LogManager.LogInfo("ProductUnit - " + product.ProductUnit);
                            }
                            else
                            {
                                RequiredValidation(i, productName, "Unit", errorModel);
                                continue;
                            }

                            //Tax
                            if (row.GetCell(6) != null && row.GetCell(6).ToString() != "")
                            {
                                var tax = row.GetCell(6).ToString().Trim();
                                LogManager.LogInfo("tax - " + tax);
                                if (CheckValidExcelData(i, tax, "Decimal", productName, "Tax", errorModel))
                                {
                                    product.ProductTaxValue = Convert.ToDecimal(tax);
                                }
                                else continue;
                            }
                            else
                            {
                                RequiredValidation(i, productName, "Tax", errorModel);
                                continue;
                            }

                            //IsSale
                            if (row.GetCell(7) != null && row.GetCell(7).ToString() != "")
                            {
                                var isSale = row.GetCell(7).ToString().Trim();
                                LogManager.LogInfo("IsSale - " + isSale);
                                if (CheckValidExcelData(i, isSale, "Bool", productName, "IsSale", errorModel))
                                {
                                    //product.IsSale = Convert.ToBoolean(isSale);
                                    LogManager.LogInfo("IsSale Inside" + isSale);
                                    bool isSaleValue = false;
                                    if(isSale == "1")
                                    {
                                        isSaleValue = true;
                                    }
                                    product.IsSale = isSaleValue;
                                }
                                else continue;
                            }
                            else
                            {
                                RequiredValidation(i, productName, "IsSale", errorModel);
                                continue;
                            }

                            //IsFragile
                            if (row.GetCell(8) != null && row.GetCell(8).ToString() != "")
                            {
                                var isFragile = row.GetCell(8).ToString().Trim();
                                LogManager.LogInfo("IsFragile - " + isFragile);
                                if (CheckValidExcelData(i, isFragile, "Bool", productName, "IsFragile", errorModel))
                                {
                                    LogManager.LogInfo("IsFragile Inside- " + isFragile);
                                    bool isFragileValue = false;
                                    if(isFragile == "1")
                                    {
                                        isFragileValue = true;
                                    }
                                    product.IsFragile = isFragileValue;
                                }
                                else continue;
                            }
                            else
                            {
                                RequiredValidation(i, productName, "IsFragile", errorModel);
                                continue;
                            }

                            //IsProductFeature
                            if (row.GetCell(9) != null && row.GetCell(9).ToString() != "")
                            {
                                var isProductFeature = row.GetCell(9).ToString().Trim();
                                LogManager.LogInfo("IsProductFeature - " + isProductFeature);
                                if (CheckValidExcelData(i, isProductFeature, "Bool", productName, "IsProductFeature", errorModel))
                                {
                                    LogManager.LogInfo("IsProductFeature Inside- " + isProductFeature);
                                    bool isProductFeatureValue = false;
                                    if (isProductFeature == "1")
                                    {
                                        isProductFeatureValue = true;
                                    }
                                    product.IsProductFeature = isProductFeatureValue;
                                }
                                else continue;
                            }
                            else
                            {
                                RequiredValidation(i, productName, "IsProductFeature", errorModel);
                                continue;
                            }

                            //ProductImage
                            if (row.GetCell(10) != null && row.GetCell(10).ToString() != "")
                            {
                                product.ProductIcon = row.GetCell(10).ToString().Trim();
                                LogManager.LogInfo("ProductIcon - " + product.ProductIcon);
                            }
                            else
                            {
                                product.ProductIcon = APIConfig.WebAPILink.Replace("api/", "") + "resources/Icons/default-img.png";
                            }
                            
                            DateTime NowDate = DateTime.Now;
                            product.ProductStatus = ProductStatus.ACTIVE;
                            product.ShopId = model.ShopId;
                            product.ProductId = Guid.NewGuid();

                            product.ProductPricePerKG = 0;
                            product.CreatedBy = model.AdminId;                            
                            product.IsEnabled = true;
                            product.IsEnabledBy = model.AdminId;
                            product.CreatedDate = NowDate;
                            product.DateEnabled = NowDate;
                            LogManager.LogInfo("Product - " + product);
                            productList.Add(product);
                        }
                    }
                    ProductErrorModel productErrorModel = new ProductErrorModel();
                    productErrorModel.TotalProduct = totalProduct;
                    productErrorModel.errorModel = errorModel;
                    if (productList.Count() > 0)
                    {
                        LogManager.LogInfo("Add productList - " + productList);
                        DbContext.Products.AddRange(productList);
                        DbContext.SaveChanges(); 
                        aResp = new APIResponse
                        {
                            Message = productList.Count() + " Produits importés avec succès.",
                            Status = "Succès!",
                            Payload = productErrorModel,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Aucun produit importé.",
                            Status = "Succès!",
                            Payload = productErrorModel,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Veuillez télécharger le fichier.",
                        Status = "Échec",                        
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError(ex.StackTrace);
                LogManager.LogError(ex.InnerException.ToString());
                LogManager.LogInfo("ImportProduct - " + model);
                LogManager.LogError(ex.InnerException.Message);               
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetProductsForPrescription(Guid shopId, int productRecordId)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetProductsForPrescription");

            try
            {
                if (productRecordId == 0)
                {
                   var products = DbContext.Products
                        .Join(DbContext.ProductCategories, i => i.ProductCategoryId, c => c.ProductCategoryId, (i, c) => new { i, c })
                        .Where(w => w.i.ShopId == shopId && w.i.ProductStatus == ProductStatus.ACTIVE)
                        .Select(p => new 
                        {
                            p.i.ProductRecordId,
                            p.i.ProductIcon,
                            p.i.ProductName,
                            p.i.ProductPrice,
                            p.c.ProductCategoryName,
                            p.i.ProductStatus
                        });

                    aResp = new APIResponse
                    {
                        Message = "Tous les produits en service ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = products,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {                   
                    var product = DbContext.Products.FirstOrDefault(i => i.ProductRecordId == productRecordId);
                    aResp = new APIResponse
                    {
                        Message = "Produit actif récupéré.",
                        Status = "Succès!",
                        Payload = product,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetProductsForPrescription");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        private bool CheckValidExcelData(int rowNum, string value, string dataType, string productName, string columnName, List<ImportProductErrorModel> errorModel)
        {
            if(dataType == "Numeric")
            {
                if(!int.TryParse(value, out int n))
                {
                    errorModel.Add(new ImportProductErrorModel
                    {
                        Row = "Row " + rowNum,
                        ProductName = productName,
                        Error = "Please enter valid data in " + columnName + " column."
                    }); ;
                    return false;
                }

                var productCategoryExist = DbContext.ProductCategories.FirstOrDefault(s => s.ProductCategoryId == Convert.ToInt32(value));
                if(productCategoryExist == null)
                {
                    errorModel.Add(new ImportProductErrorModel
                    {
                        Row = "Row " + rowNum,
                        ProductName = productName,
                        Error = "Please enter a valid product category id in " + columnName + " column."
                    });
                    return false;
                }
            }
            else if(dataType == "Decimal")
            {
                if(!decimal.TryParse(value, out decimal n))
                {
                    errorModel.Add(new ImportProductErrorModel
                    {
                        Row = "Row " + rowNum,
                        ProductName = productName,
                        Error = "Please enter valid data in " + columnName + " column."
                    });
                    return false;
                }
            }
            else if (dataType == "Bool")
            {
                //if (!bool.TryParse(value, out bool n))
                //{
                //    errorModel.Add(new ImportProductErrorModel
                //    {
                //        Row = "Row " + rowNum,
                //        ProductName = productName,
                //        Erreur = "Please enter True/False data in " + columnName + " column."
                //    });
                //    return false;
                //}

                if (!int.TryParse(value, out int n))
                {
                    if(value != "1" && value != "0")
                    {
                        errorModel.Add(new ImportProductErrorModel
                        {
                            Row = "Row " + rowNum,
                            ProductName = productName,
                            Error = "Please enter 1=True/0=False data in " + columnName + " column."
                        }); ;
                        return false;
                    }                   
                }

                //if (value.ToLower() != "true" && value.ToLower() != "false")
                //{
                //    errorModel.Add(new ImportProductErrorModel
                //    {
                //        Row = "Row " + rowNum,
                //        ProductName = productName,
                //        Erreur = "Please enter True/False data in " + columnName + " column."
                //    });
                //    return false;
                //}
            }
            return true;
        }
        private void  RequiredValidation(int rowNum, string productName , string columnName, List<ImportProductErrorModel> errorModel)
        {
            errorModel.Add(new ImportProductErrorModel
            {
                Row = "Row " + rowNum,
                ProductName = productName,
                Error = columnName + " is reuired."
            });
        }

		public APIResponse AddProductFromExtern(int productRecordId)
		{
			throw new NotImplementedException();
		}
	} 
}
