using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using System;
using System.Linq;
using PharmaMoov.Models.Shop;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PharmaMoov.Models.Campaign;
using PharmaMoov.Models.Product;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class CatalogueRepository : APIBaseRepo, ICatalogueRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public CatalogueRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public APIResponse GetMainCatalogue(FilterMain _filter)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetMainCatalogue");

            try
            {
                ShopCatalogue shopCatalogue = new ShopCatalogue();

                #region Section 1: ACTIVE BANNERS

                var campaigns = new List<CampaignDTO>();
                campaigns = DbContext.Campaigns.AsNoTracking().Where(c => c.IsEnabled == true)
                            .Select(p => new CampaignDTO
                            {
                                CampaignRecordID = p.CampaignRecordID,
                                ShopId = p.ShopId,
                                Name = p.Name,
                                Description = p.Description,
                                ImageUrl = p.ImageUrl
                            }).ToList();

                if (campaigns.Count() > 0)
                {
                    shopCatalogue.CampaignBanners = campaigns;
                }

                #endregion

                #region Section 2: ACTIVE SHOPS WITH OFFER ITEMS

                IEnumerable<Shop> filterResult = null;
                IEnumerable<ShopOpeningHourDTO> filterShopHours = null;

                //filter active shops with offer items
                filterResult = DbContext.Shops.AsNoTracking().Where(s => s.IsEnabled == true && s.HasOffers == true);

                //filter by delivery method 
                if (_filter.DeliveryMethod == OrderDeliveryType.FORDELIVERY)
                {
                    filterResult = filterResult.Where(i => i.DeliveryMethod != OrderDeliveryType.FORPICKUP);
                }
                else
                {
                    filterResult = filterResult.Where(i => i.DeliveryMethod == OrderDeliveryType.FORPICKUP);
                }

                //filter shops within working hours
                DateTime dateNow = DateTime.Now;
                if (_filter.OpeningDay == null || _filter.OpeningHour == null || _filter.ClosingHour == null) //set default
                {
                    filterShopHours = FilterShopOpeningHours(dateNow.DayOfWeek, TimeSpan.Parse(dateNow.ToString()), TimeSpan.Parse(dateNow.ToString())).ToList();
                }
                else
                {
                    filterShopHours = FilterShopOpeningHours(_filter.OpeningDay, TimeSpan.Parse(_filter.OpeningHour), TimeSpan.Parse(_filter.ClosingHour)).ToList();
                }

                //remodel
                var shopsWithOffers = new List<ShopsWithOffers>();
                shopsWithOffers = filterResult
                    .Join(filterShopHours, s => s.ShopId, h => h.ShopId, (s, h) => new { s, h })
                     .Select(w => new ShopsWithOffers
                     {
                         ShopId = w.s.ShopId,
                         Name = w.s.ShopName,
                         Description = w.s.Description.Substring(0, 100),
                         ImageUrl = w.s.ShopIcon,
                         Latitude = w.s.Latitude,
                         Longitude = w.s.Longitude
                     }).Take(10).ToList();

                //filter by nearest location > calculate distance
                foreach (var shop in shopsWithOffers)
                {
                    var distance = new Coordinates(_filter.Latitude, _filter.Longitude)
                        .DistanceTo(
                            new Coordinates((double)shop.Latitude, (double)shop.Longitude)
                        );

                    shop.Distance = decimal.Round((decimal)distance, 2, MidpointRounding.AwayFromZero);
                }

                if (shopsWithOffers.Count() > 0)
                {
                    //filter by Maximum KM per Delivery Method
                    decimal IsWithinDistance = GetMaximumKMDistance(_filter.DeliveryMethod);

                    shopCatalogue.ShopWithOffers = shopsWithOffers.Where(d => d.Distance <= IsWithinDistance).OrderBy(x => x.Distance).ToList();
                }

                #endregion

                #region Section 3: ACTIVE SHOPS CATEGORIES WITH ACTIVE SHOPS
                var shopCategories = new List<ShopCategoriesDTO>();
                var listCategories = new List<ShopCategory>();
                listCategories = DbContext.ShopCategories.AsNoTracking().Where(i => i.IsEnabled == true).ToList();

                for (int i = 0; i < listCategories.Count(); i++)
                {
                    int shops = DbContext.Shops.AsNoTracking().Where(p => p.IsEnabled == true && p.ShopCategoryId == listCategories[i].ShopCategoryID).Count();
                    if (shops > 0)
                    {
                        var newList = listCategories.Where(c => c.ShopCategoryID == listCategories[i].ShopCategoryID)
                                                            .Select(p => new ShopCategoriesDTO
                                                            {
                                                                ShopCategoryId = p.ShopCategoryID,
                                                                Name = p.Name,
                                                                Description = p.Description,
                                                                ImageUrl = p.ImageUrl,
                                                                IsActive = p.IsEnabled ?? false
                                                            }).FirstOrDefault();

                        shopCategories.Add(newList);
                    }
                }

                if (shopCategories.Count() > 0)
                {
                    shopCatalogue.ShopCategories = shopCategories.Take(9).ToList();
                }

                #endregion

                if (shopCatalogue.ShopCategories.Count > 0)
                {
                    aResp = new APIResponse
                    {
                        Message = "Tous les éléments ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = shopCatalogue,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun article n'a été récupéré.",
                        Status = "Échec!",
                        Payload = shopCategories,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetMainCatalogue");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetAllShopsCategories(FilterShopCategoriesModel _filterShopCategories)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetAllShopsCategories: " + _filterShopCategories._pageNo);

            try
            {
                var shopCategories = new List<ShopCategoriesListModel>();
                var listCategories = new List<ShopCategory>();
                listCategories = DbContext.ShopCategories.AsNoTracking().Where(i => i.IsEnabled == true).ToList();

                for (int i = 0; i < listCategories.Count(); i++)
                {
                    int shops = DbContext.Shops.AsNoTracking().Where(p => p.IsEnabled == true && p.ShopCategoryId == listCategories[i].ShopCategoryID).Count();
                    if (shops > 0)
                    {
                        if (!string.IsNullOrEmpty(_filterShopCategories._searchKey))
                        {
                            //search by will be performed by "pharmayc name and products"
                            var shopIdList = DbContext.Shops.Where(p => p.IsEnabled == true && p.ShopCategoryId == listCategories[i].ShopCategoryID && p.ShopName.ToLower().Contains(_filterShopCategories._searchKey.ToLower())).Select(s => s.ShopId).ToList();
                            if(shopIdList.Count() > 0) // If the search key is match with the shop name then get the category
                            {
                                var newList = listCategories.Where(c => c.ShopCategoryID == listCategories[i].ShopCategoryID)
                                                            .Select(p => new ShopCategoriesListModel
                                                            {
                                                                ShopCategoryId = p.ShopCategoryID,
                                                                Name = p.Name,
                                                                ImageUrl = p.ImageUrl,
                                                            }).FirstOrDefault();
                                shopCategories.Add(newList);
                            }
                            else
                            {
                                var productShopIdList = DbContext.Products.Where(p => p.IsEnabled == true  && p.ProductName.ToLower().Contains(_filterShopCategories._searchKey.ToLower())).Select(s => s.ShopId).ToList();
                                var shopListCount = DbContext.Shops.Where(p => p.IsEnabled == true && productShopIdList.Contains(p.ShopId) && p.ShopCategoryId == listCategories[i].ShopCategoryID).Count();

                                if (shopListCount > 0) // If the search key is match with the product name then get the category
                                {
                                    var newList = listCategories.Where(c => c.ShopCategoryID == listCategories[i].ShopCategoryID)
                                                                .Select(p => new ShopCategoriesListModel
                                                                {
                                                                    ShopCategoryId = p.ShopCategoryID,
                                                                    Name = p.Name,
                                                                    ImageUrl = p.ImageUrl,
                                                                }).FirstOrDefault();
                                    shopCategories.Add(newList);
                                }
                            }                          
                                                  
                        }
                        else
                        {
                            var newList = listCategories.Where(c => c.ShopCategoryID == listCategories[i].ShopCategoryID)
                                                           .Select(p => new ShopCategoriesListModel
                                                           {
                                                               ShopCategoryId = p.ShopCategoryID,
                                                               Name = p.Name,
                                                               ImageUrl = p.ImageUrl,
                                                           }).FirstOrDefault();

                            shopCategories.Add(newList);
                        }                       
                    }                  
                }

                var pageSize = 10;
                IEnumerable<ShopCategoriesListModel> pagedResult = null;
               
                pagedResult = shopCategories.Where(w => w.ShopCategoryId != 0, _filterShopCategories._pageNo, pageSize).ToList();
                

                if (pagedResult.Count() > 0)
                {

                    // get total page count
                    var totalPageCount = 1;
                    var pageCount = shopCategories.Count();
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
                            CategoryList = pagedResult,
                            PageCount = totalPageCount
                        },
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun article n'a été récupéré.",
                        Status = "Échec!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetShopsProductCategories");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse FilterShops(FilterShops _filter)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("FilterShops: " + _filter);

            try
            {
                IEnumerable<Shop> filterResult = null;
                IEnumerable<ShopOpeningHourDTO> filterShopHours = null;
                IEnumerable<FilteredShops> pagedResult = null;
                IEnumerable<FilteredShops> finalResults = null;

                var CategoryName = string.Empty;

                //get active shops and filter by delivery method
                filterResult = DbContext.Shops.AsNoTracking().Where(i => i.IsEnabled == true);

                if (filterResult != null)
                {
                    //filter by delivery method 
                    if (_filter.DeliveryMethod != OrderDeliveryType.ALL)
                    {
                        if (_filter.DeliveryMethod == OrderDeliveryType.FORDELIVERY)
                        {
                            filterResult = filterResult.Where(i => i.DeliveryMethod != OrderDeliveryType.FORPICKUP);
                        }

                        if (_filter.DeliveryMethod == OrderDeliveryType.FORPICKUP)
                        {
                            filterResult = filterResult.Where(i => i.DeliveryMethod == OrderDeliveryType.FORPICKUP);
                        }
                    }

                    //filter by with offers
                    if (_filter.ShopsWithOffers == true)
                    {
                        filterResult = filterResult.Where(i => i.HasOffers == true);
                    }

                    //filter by categories
                    if (_filter.ShopCatergoryId > 0)
                    {
                        filterResult = filterResult.Where(i => i.ShopCategoryId == _filter.ShopCatergoryId);
                    }

                    //filter shops within working hours
                    DateTime dateNow = DateTime.Now;
                    if (_filter.OpeningDay == null || _filter.OpeningHour == null || _filter.ClosingHour == null) //set default
                    {
                        filterShopHours = FilterShopOpeningHours(dateNow.DayOfWeek, TimeSpan.Parse(dateNow.ToString()), TimeSpan.Parse(dateNow.ToString())).ToList();
                    }
                    else
                    {
                        filterShopHours = FilterShopOpeningHours(_filter.OpeningDay, TimeSpan.Parse(_filter.OpeningHour), TimeSpan.Parse(_filter.ClosingHour)).ToList();
                    }

                    //filter by Search Key (name and desc)
                    if (_filter.SearchKey != null && _filter.SearchKey != string.Empty)
                    {
                        filterResult = filterResult.Where(i => (i.ShopName ?? string.Empty).Contains(_filter.SearchKey, StringComparison.OrdinalIgnoreCase)
                            || (i.Description ?? string.Empty).Contains(_filter.SearchKey, StringComparison.OrdinalIgnoreCase));
                    }

                    //remodel
                    pagedResult = filterResult
                         .Join(filterShopHours, s => s.ShopId, h => h.ShopId, (s, h) => new { s, h })
                         .Select(w => new FilteredShops
                         {
                             ShopId = w.s.ShopId,
                             ShopCategoryID = w.s.ShopCategoryId,
                             ImageUrl = w.s.ShopIcon,
                             Name = w.s.ShopName,
                             Tags = w.s.ShopTags,
                             Address = w.s.Address,
                             //PreparationTime = w.s.PreparationTime,
                             DeliveryMethod = w.s.DeliveryMethod,
                             Description = w.s.Description,
                             Latitude = w.s.Latitude,
                             Longitude = w.s.Longitude
                         }).ToList();

                    //filter by nearest location > calculate distance
                    foreach (var shop in pagedResult)
                    {
                        //append delivery note
                        if (shop.DeliveryMethod == OrderDeliveryType.FORPICKUP)
                        {
                            shop.DeliveryNote = "Pas de livraison possible";
                        }
                        else if (shop.DeliveryMethod == OrderDeliveryType.FORDELIVERY)
                        {
                            shop.DeliveryNote = "Pas de click&collect possible";
                        }
                        else
                        {
                            shop.DeliveryNote = string.Empty;
                        }

                        //get category name
                        CategoryName = DbContext.ShopCategories.AsNoTracking().Where(c => c.ShopCategoryID == shop.ShopCategoryID).FirstOrDefault().Name;
                        shop.Category = CategoryName;

                        //calculate distance
                        var distance = new Coordinates(_filter.Latitude, _filter.Longitude)
                            .DistanceTo(
                                new Coordinates((double)shop.Latitude, (double)shop.Longitude)
                            );

                        shop.Distance = decimal.Round((decimal)distance, 2, MidpointRounding.AwayFromZero);
                    }
                }

                finalResults = pagedResult;

                //NULL check
                if (pagedResult.Count() > 0)
                {
                    //when user is logged in append favorite items
                    if (_filter.UserId != null)
                    {
                        var listFavorites = new List<UserShopFavorite>();
                        listFavorites = DbContext.UserShopFavorites.AsNoTracking().Where(x => x.UserId == _filter.UserId && x.IsEnabled == true).ToList();

                        for (int i = 0; i < listFavorites.Count(); i++)
                        {
                            foreach (var item in finalResults)
                            {
                                if (item.ShopId == listFavorites[i].ShopId)
                                {
                                    item.IsFavorite = true;
                                }
                            }
                        }

                        //filter by Favorite
                        if (_filter.IsFavorite == true)
                        {
                            var userFavs = DbContext.UserShopFavorites.AsNoTracking().Where(x => x.UserId == _filter.UserId);
                            finalResults = (from i in pagedResult
                                            join c in userFavs on i.ShopId equals c.ShopId
                                            where c.IsEnabled == true
                                            select i);

                            foreach (var item in finalResults)
                            {
                                item.IsFavorite = true;
                            };
                        }
                    }
                }

                var pageSize = 10;
                if (pagedResult.Count() > 0)
                {
                    //filter by Minimum KM per Delivery Method
                    decimal IsWithinDistance = GetMaximumKMDistance(_filter.DeliveryMethod);

                    // get all products per page
                    finalResults = finalResults.Where(w => w.Distance <= IsWithinDistance, _filter.PageNumber, pageSize)
                        .OrderBy(x => x.Distance).ToList();
                }

                if (finalResults != null && finalResults.Count() > 0)
                {

                    // get total page count
                    var totalPageCount = 1;
                    var pageCount = finalResults.Count();
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
                            ShopList = finalResults,
                            PageCount = totalPageCount
                        },
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun article n'a été récupéré.",
                        Status = "Échec!",
                        Payload = null,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("FilterShops");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetShopsProductCategories(Guid _shop)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopsProductCategories");

            try
            {
                var productCategories = new List<ProductCategoriesDTO>();
                var listCategories = new List<ProductCategory>();
                listCategories = DbContext.ProductCategories.AsNoTracking().Where(i => i.IsEnabled == true).ToList();

                for (int i = 0; i < listCategories.Count(); i++)
                {
                    int shops = DbContext.Products.AsNoTracking().Where(p => p.ProductStatus != ProductStatus.INACTIVE && p.ProductCategoryId == listCategories[i].ProductCategoryId && p.ShopId == _shop).Count();
                    if (shops > 0)
                    {
                        var newList = listCategories.Where(c => c.ProductCategoryId == listCategories[i].ProductCategoryId)
                                                            .Select(p => new ProductCategoriesDTO
                                                            {
                                                                ShopId = _shop,
                                                                ProductCategoryId = p.ProductCategoryId,
                                                                ProductCategoryName = p.ProductCategoryName,
                                                                ProductCategoryDesc = p.ProductCategoryDesc,
                                                                ProductCategoryImage = p.ProductCategoryImage,
                                                                IsActive = p.IsEnabled ?? false
                                                            }).FirstOrDefault();

                        productCategories.Add(newList);
                    }
                }

                if (productCategories.Count() > 0)
                {
                    aResp = new APIResponse
                    {
                        Message = "Tous les éléments ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = productCategories,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun article n'a été récupéré.",
                        Status = "Échec!",
                        Payload = productCategories,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetShopsProductCategories");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetShopDetails(Guid _shop)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopDetails :" + _shop);

            try
            {
                ShopDetails shopDetails = new ShopDetails();
                shopDetails = DbContext.Shops.AsNoTracking()
                            .Where(w => w.ShopId == _shop)
                            .Select(i => new ShopDetails
                            {
                                ShopId = i.ShopId,
                                ImageUrl = i.ShopIcon,
                                ShopBanner = i.ShopBanner,
                                Name = i.ShopName,
                                Tags = i.ShopTags,
                                Address = i.Address,
                                PreparationTime = i.PreparationTime,
                                DeliveryMethod = i.DeliveryMethod,
                                Description = i.Description,
                                MobileNumber = i.MobileNumber,
                                TelephoneNumber = i.TelephoneNumber
                            }).FirstOrDefault();
               
                //append delivery note
                if (shopDetails.DeliveryMethod == OrderDeliveryType.FORPICKUP)
                {
                    shopDetails.DeliveryNote = "Pas de livraison possible";
                }
                else if (shopDetails.DeliveryMethod == OrderDeliveryType.FORDELIVERY)
                {
                    shopDetails.DeliveryNote = "Pas de click&collect possible";
                }
                else
                {
                    shopDetails.DeliveryNote = string.Empty;
                }

                //append total reviews
                int totalRws = DbContext.ShopReviewRatings.AsNoTracking().Where(r => r.ShopId == shopDetails.ShopId && r.IsEnabled == true).Count();
                shopDetails.TotalReviews = totalRws == 0 ? "0 notes" : totalRws + " notes";

                //append average ratings
                decimal average_rate = GetAverageRating(_shop);
                if (average_rate == 0)
                {
                    shopDetails.AverageRating = average_rate;
                }

                if (shopDetails != null)
                {
                    shopDetails.ShopStatus = GetPharmacyOpenOrClose(shopDetails.ShopId);
                    IEnumerable<ShopOpeningHourDTO> openingHours = null;
                    openingHours = DbContext.ShopOpeningHours.AsNoTracking().Where(h => h.ShopId == shopDetails.ShopId)
                          .Select(p => new ShopOpeningHourDTO
                          {
                              ShopOpeningHourID = p.ShopOpeningHourID,
                              ShopId = p.ShopId,
                              DayOfWeek = p.DayOfWeek,
                              StartTimeAM = TimeSpan.Parse(p.StartTimeAM),
                              EndTimeAM = TimeSpan.Parse(p.EndTimeAM),
                              StartTimePM = TimeSpan.Parse(p.StartTimePM),
                              EndTimePM = TimeSpan.Parse(p.EndTimePM),
                              StartTimeEvening = TimeSpan.Parse(p.StartTimeEvening),
                              EndTimeEvening = TimeSpan.Parse(p.EndTimeEvening),
                              NowOpen = p.IsEnabled ?? false
                          }).OrderBy(x => x.DayOfWeek).ToList();

                    if (openingHours != null)
                    {
                        shopDetails.WorkingHours = openingHours;

                        aResp = new APIResponse
                        {
                            Message = "Enregistrement trouvé.",
                            Status = "Succès!",
                            Payload = shopDetails,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun enregistrement trouvé.",
                        Status = "Échec!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetShopDetails");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse FilterShopsProducts(FilterProducts _filter)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("FilterShopsProducts: " + _filter);

            try
            {
                IEnumerable<FilteredProducts> pagedResult = null;
                IQueryable<Product> filterResult = null;

                //filter items with active categories only, active product status, and by shop
                filterResult = (from i in DbContext.Products
                                join c in DbContext.ProductCategories on i.ProductCategoryId equals c.ProductCategoryId
                                where c.IsEnabled == true && i.ProductStatus != ProductStatus.INACTIVE && i.ShopId == _filter.ShopId
                                select i);

                if (filterResult != null)
                {
                    //filter by Category
                    if (_filter.ProductCategoryId != 0)
                    {
                        filterResult = filterResult.Where(i => i.ProductCategoryId == _filter.ProductCategoryId);
                    }

                    //filter by Search Key (name and desc)
                    if (_filter.SearchKey != null)
                    {
                        filterResult = filterResult.Where(i => (i.ProductName ?? string.Empty).Contains(_filter.SearchKey, StringComparison.OrdinalIgnoreCase)
                            || (i.ProductDesc ?? string.Empty).Contains(_filter.SearchKey, StringComparison.OrdinalIgnoreCase));

                    }
                }

                // get all products per page
                if (_filter.PageSize == 0)
                {
                    _filter.PageSize = 12; //default 
                }

                pagedResult = filterResult
                    .Join(DbContext.Shops, p => p.ShopId, s => s.ShopId, (p, s) => new { p, s })
                    .Where(w => w.p.ProductRecordId != 0, _filter.PageNumber, _filter.PageSize)
                    .Select(i => new FilteredProducts
                    {
                        ShopId = i.p.ShopId,
                        ShopName = i.s.ShopName,
                        ShopAddress = i.s.Address,
                        ProductRecordId = i.p.ProductRecordId,
                        ProductId = i.p.ProductId,
                        ProductName = i.p.ProductName,
                        ProductUnit = i.p.ProductUnit,
                        ProductPrice = i.p.ProductPrice,
                        ProductPricePerKG = i.p.ProductPricePerKG,
                        ProductTaxValue = i.p.ProductTaxValue,
                        ProductTaxAmount = (i.p.ProductTaxValue / 100) * (1 * i.p.ProductPrice),
                        ProductIcon = i.p.ProductIcon,
                        ProductStatus = i.p.ProductStatus, //i.p.ProductStatus == ProductStatus.OUTOFSTOCK ? "Stock épuisé" : string.Empty,
                        SalePrice = i.p.SalePrice,
                        IsSale = i.p.IsSale,
                        IsFragile = i.p.IsFragile,
                        IsUnsold = i.p.IsUnsold,
                        PriceHolder = i.p.PriceHolder
                    }).ToList();

                if (_filter.SortBy == (int)ProductSortEnum.PriceHighToLow || _filter.SortBy == (int)ProductSortEnum.PriceLowToHigh)
                {
                    foreach (var item in pagedResult)
                    {
                        if (item.SalePrice != 0)
                        {
                            item.PriceHolder = item.SalePrice;
                        }
                        else
                        {
                            item.PriceHolder = item.ProductPrice;
                        }
                    }

                    if (_filter.SortBy == (int)ProductSortEnum.PriceHighToLow)
                    {
                        pagedResult = pagedResult.OrderByDescending(s => s.PriceHolder);
                    }
                    else if (_filter.SortBy == (int)ProductSortEnum.PriceLowToHigh)
                    {
                        pagedResult = pagedResult.OrderBy(s => s.PriceHolder);
                    }
                }
                else if (_filter.SortBy == (int)ProductSortEnum.AscendingProduct)
                {
                    pagedResult = pagedResult.OrderBy(s => s.ProductName);
                }
                else if (_filter.SortBy == (int)ProductSortEnum.DescendingProduct)
                {
                    pagedResult = pagedResult.OrderByDescending(s => s.ProductName);
                }


                if (pagedResult.Count() > 0)
                {
                    
                    // get total page count
                    var totalPageCount = 1;
                    var pageCount = filterResult.Count();
                    if (pageCount > 0 && pageCount >= _filter.PageSize)
                    {
                        totalPageCount = (int)Math.Ceiling((double)pageCount / _filter.PageSize);
                    }

                    aResp = new APIResponse
                    {
                        Message = "Tous les éléments ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = new
                        {
                            ProductList = pagedResult,
                            PageCount = totalPageCount
                        },
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun article n'a été récupéré.",
                        Status = "Échec!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("FilterShopsProducts");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetShopsProductDetails(Guid _product)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopsProductDetails :" + _product);

            try
            {
                ProductDetails productDetails = new ProductDetails();
                productDetails = DbContext.Products.AsNoTracking()
                            .Join(DbContext.Shops, p => p.ShopId, s => s.ShopId, (p, s) => new { p, s })
                            .Where(w => w.p.IsEnabled == true && w.s.IsEnabled == true && w.p.ProductId == _product)
                            .Select(i => new ProductDetails
                            {
                                ProductRecordId = i.p.ProductRecordId,
                                ProductId = i.p.ProductId,
                                ProductName = i.p.ProductName,
                                ProductUnit = i.p.ProductUnit,
                                ProductPrice = i.p.ProductPrice,
                                ProductPricePerKG = i.p.ProductPricePerKG,
                                ProductTaxValue = i.p.ProductTaxValue,
                                ProductTaxAmount = (i.p.ProductTaxValue / 100) * (1 * i.p.ProductPrice),
                                ProductIcon = i.p.ProductIcon,
                                ProductStatus = i.p.ProductStatus, //i.p.ProductStatus == ProductStatus.OUTOFSTOCK ? "Stock épuisé" : string.Empty,
                                ProductDesc = i.p.ProductDesc,
                                SalePrice = i.p.SalePrice,
                                ShopId = i.s.ShopId,
                                ShopName = i.s.ShopName,
                                ShopAddress = i.s.Address
                            }).FirstOrDefault();

                if (productDetails != null)
                {
                    //IQueryable<Product> filterResult = null;
                    //filterResult = (from i in DbContext.Products
                    //                join c in DbContext.ProductCategories on i.ProductCategoryId equals c.ProductCategoryId
                    //                where c.IsEnabled == true && i.IsEnabled == true && i.ShopId == productDetails.ShopId
                    //                select i);

                    //IEnumerable<FilteredProducts> recomProducts = null;
                    //recomProducts = filterResult.AsNoTracking()
                    //       .Join(DbContext.Shops, p => p.ShopId, s => s.ShopId, (p, s) => new { p, s })
                    //       .Where(w => w.p.IsEnabled == true && w.p.ShopId == productDetails.ShopId)
                    //       .Select(i => new FilteredProducts
                    //       {
                    //           ShopId = i.p.ShopId,
                    //           ShopName = i.s.ShopName,
                    //           ShopAddress = i.s.Address,
                    //           ProductRecordId = i.p.ProductRecordId,
                    //           ProductId = i.p.ProductId,
                    //           ProductName = i.p.ProductName,
                    //           ProductUnit = i.p.ProductUnit,
                    //           ProductPrice = i.p.ProductPrice,
                    //           ProductPricePerKG = i.p.ProductPricePerKG,
                    //           ProductTaxValue = i.p.ProductTaxValue,
                    //           ProductTaxAmount = (i.p.ProductTaxValue / 100) * (1 * i.p.ProductPrice),
                    //           ProductIcon = i.p.ProductIcon,
                    //           ProductStatus = i.p.ProductStatus == ProductStatus.OUTOFSTOCK ? "Stock épuisé" : string.Empty,
                    //           IsSale = i.p.IsSale,
                    //           IsFragile = i.p.IsFragile,
                    //           IsUnsold = i.p.IsUnsold
                    //       }).OrderBy(x => Guid.NewGuid()).Take(4).ToList();

                    //if (recomProducts != null)
                    //{
                        //productDetails.RecomProducts = recomProducts;

                    aResp = new APIResponse
                    {
                        Message = "L'article a été récupéré avec succès.",
                        Status = "Succès!",
                        Payload = productDetails,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                    //}
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun élément n'a été récupéré.",
                        Status = "Échec!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetShopsProductDetails");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse CatalougeForRegularCustomer() 
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("CatalougeForRegularCustomer");

            try
            {
                RegularCustomerCatalogue Catalouge = new RegularCustomerCatalogue();

                #region Section 1: Home Page Banners

                var campaigns = new List<CampaignDTO>();
                campaigns = DbContext.Campaigns.AsNoTracking().Where(c => c.IsEnabled == true && c.IsProductOfferBanner == false)
                            .Select(p => new CampaignDTO
                            {
                                CampaignRecordID = p.CampaignRecordID,
                                ShopId = null,
                                Name = p.Name,
                                Description = p.Description,
                                ImageUrl = p.ImageUrl,
                                IsProductOfferBanner = p.IsProductOfferBanner
                            }).ToList();

                if (campaigns.Count() > 0)
                {
                    Catalouge.HomePageBanners = campaigns;
                }

                #endregion

                #region Section 2: Popular Pharmacies

                List<PopularPharmaciesDTO> popPharmacies = DbContext.Shops.Where(s => s.IsPopularPharmacy == true).Select(s => new PopularPharmaciesDTO
                {
                    ShopAddress = s.Address + " " + s.SuiteAddress + " " + s.PostalCode +  " " + s.City,
                    ShopDescription = s.Description,
                    ShopIcon = s.ShopIcon,
                    ShopID = s.ShopId,
                    ShopName = s.ShopName,
                    ShopRecordId = s.ShopRecordID,
                    ShopTelephoneNumber = s.TelephoneNumber,                    
                }).ToList();

                if (popPharmacies.Count > 0) 
                {
                    popPharmacies = popPharmacies.Select(s=> 
                    {
                        s.ShopStatus = GetPharmacyOpenOrClose(s.ShopID);
                        return s;                        
                     }).ToList();
                    Catalouge.PopularPharmacies = popPharmacies;
                }

                #endregion

                #region Section 3: Banners for Offers 
                List<CampaignDTO> BannersForOffers = DbContext.Campaigns.AsNoTracking().Where(c => c.IsEnabled == true && c.IsProductOfferBanner == true)
                            .Select(p => new CampaignDTO
                            {
                                CampaignRecordID = p.CampaignRecordID,
                                ShopId = null,
                                Name = p.Name,
                                Description = p.Description,
                                ImageUrl = p.ImageUrl,
                                IsProductOfferBanner = p.IsProductOfferBanner
                            }).ToList();

                if (BannersForOffers.Count() > 0)
                {
                    Catalouge.BannersForOffers = BannersForOffers;
                }
                #endregion

                #region Section 4: Featured Products
                List<ProductCategory> ProductCatList = DbContext.ProductCategories.Where(pc => pc.IsEnabled == true).ToList();
                List<ProductList> FeatureProducts = DbContext.Products
                    .Join(DbContext.Shops, p => p.ShopId, s => s.ShopId, (p, s) => new { p, s })
                    .Where(w => w.p.IsProductFeature == true && w.p.IsEnabled == true)
                    .Select(pl => new ProductList {
                        ProductId = pl.p.ProductId,
                        ProductIcon = pl.p.ProductIcon,
                        ProductName = pl.p.ProductName,
                        ProductPrice = pl.p.ProductPrice,
                        SalePrice = pl.p.SalePrice,
                        ProductRecordId = pl.p.ProductRecordId,
                        ProductStatus = pl.p.ProductStatus,
                        ProductCategoryId = pl.p.ProductCategoryId,
                        ShopId = pl.s.ShopId,
                        ShopName = pl.s.ShopName,
                        ShopAddress = pl.s.Address,
                        ShopIcon = pl.s.ShopIcon
                    }).ToList();

                if (FeatureProducts.Count > 0) 
                {
                    foreach (var product in FeatureProducts)
                    {
                        var hasCategory = ProductCatList.FirstOrDefault(pci => pci.ProductCategoryId == product.ProductCategoryId);
                        if(hasCategory != null)
                        {
                            product.ProductCategory = ProductCatList.FirstOrDefault(pci => pci.ProductCategoryId == product.ProductCategoryId).ProductCategoryName;
                        }
                    }

                    Catalouge.FeaturedProducts = FeatureProducts;
                }
                #endregion

                #region Section 5: Get all Product Categories
                List<ProductCategoriesDTO> getAllCategories = DbContext.ProductCategories
                        .Where(s => s.IsEnabled == true)
                        .Select(s => new ProductCategoriesDTO
                        {
                            ProductCategoryId = s.ProductCategoryId,
                            ProductCategoryName = s.ProductCategoryName,
                            ProductCategoryImage = s.ProductCategoryImage,
                            IsCategoryFeatured = s.IsCategoryFeatured
                        }).ToList();

                if (getAllCategories.Count > 0)
                {
                    Catalouge.ProductCategories = getAllCategories;
                }
                #endregion

                #region Section 6: Top Categories of Featured Products
                if (ProductCatList.Count > 0)
                {
                    ProductCatList = ProductCatList.Where(pcl => pcl.IsCategoryFeatured).OrderByDescending(pc => pc.LastEditedDate).Take(2).ToList();
                    if (ProductCatList.Count == 2) 
                    {
                        // set the first feature categorie product list
                        List<ProductList> FirstFeatureProductsList = DbContext.Products
                            .Where(p => p.IsProductFeature == true && p.IsEnabled == true && p.ProductCategoryId == ProductCatList[0].ProductCategoryId)
                            .Select(pl => new ProductList
                            {
                                ProductCategory = ProductCatList[0].ProductCategoryName,
                                ProductId = pl.ProductId,
                                SalePrice = pl.SalePrice,
                                ProductIcon = pl.ProductIcon,
                                ProductName = pl.ProductName,
                                ProductPrice = pl.ProductPrice,
                                ProductRecordId = pl.ProductRecordId,
                                ProductStatus = pl.ProductStatus,
                                ProductCategoryId = pl.ProductCategoryId,
                                ShopId = pl.ShopId
                            }).ToList();

                        Catalouge.FeaturedProductCategoriesOne = FirstFeatureProductsList;

                        List<ProductList> SecondFeatureProductsList = DbContext.Products
                            .Where(p => p.IsProductFeature == true && p.IsEnabled == true && p.ProductCategoryId == ProductCatList[1].ProductCategoryId)
                            .Select(pl => new ProductList
                            {
                                ProductCategory = ProductCatList[1].ProductCategoryName,
                                ProductId = pl.ProductId,
                                SalePrice = pl.SalePrice,
                                ProductIcon = pl.ProductIcon,
                                ProductName = pl.ProductName,
                                ProductPrice = pl.ProductPrice,
                                ProductRecordId = pl.ProductRecordId,
                                ProductStatus = pl.ProductStatus,
                                ProductCategoryId = pl.ProductCategoryId,
                                ShopId = pl.ShopId
                            }).ToList();

                        Catalouge.FeaturedProductCategoriesTwo = SecondFeatureProductsList;
                    }
                }

                #endregion

                aResp = new APIResponse
                {
                    Message = "Tous les éléments ont été récupérés avec succès.",
                    Status = "Succès!",
                    Payload = Catalouge,
                    StatusCode = System.Net.HttpStatusCode.OK
                };

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("CatalougeForRegularCustomer");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;

        }

        public APIResponse GetShopAddressDetailsForMap(FilterShopAddress filterShopAddress)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopAddressDetailsForMap: " + filterShopAddress);
            try
            {
                //IEnumerable<ShopOpeningHourDTO> filterShopHours = null;
                var shopList = DbContext.Shops.AsNoTracking().Where(i => i.IsEnabled == true).Select(s=> new ShopAddressModel { 
                    ShopId = s.ShopId,
                    ShopName = s.ShopName,
                    ShopIcon = s.ShopIcon,
                    Description = s.Description,
                    MobileNumber = s.MobileNumber,
                    TelephoneNumber = s.TelephoneNumber,
                    Address = s.Address,
                    SuiteAddress = s.SuiteAddress,
                    PostalCode = s.PostalCode,
                    City = s.City,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude
                }).ToList();

                if (!string.IsNullOrEmpty(filterShopAddress.SearchKey))
                {
                    shopList = shopList.Where(s => s.ShopName.ToLower().Contains(filterShopAddress.SearchKey.ToLower())).ToList();
                }

                foreach (var item in shopList)
                {
                    //calculate distance
                    var distance = new Coordinates(filterShopAddress.Latitude, filterShopAddress.Longitude)
                        .DistanceTo(
                            new Coordinates((double)item.Latitude, (double)item.Longitude)
                        );

                    item.Distance = decimal.Round((decimal)distance, 2, MidpointRounding.AwayFromZero);
                    item.ShopStatus = GetPharmacyOpenOrClose(item.ShopId);
                }

                //filter shops within working hours
                //DateTime dateNow = DateTime.Now;
                //filterShopHours = FilterShopOpeningHours(dateNow.DayOfWeek, dateNow.TimeOfDay, dateNow.TimeOfDay).ToList();

                decimal IsWithinDistance = 10;// display shops within 10 km 

                shopList = shopList
                    .Where(w => w.Distance <= IsWithinDistance).OrderBy(x => x.Distance)
                    .Take(10).ToList(); // display nearest 10 shops

                if (shopList.Count() > 0)
                {
                    aResp.Message = "Tous les éléments ont été récupérés avec succès.";
                    aResp.Status = "Succès";
                    aResp.Payload = shopList;
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
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
                LogManager.LogInfo("GetShopAddressDetailsForMap");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetAllShop(ShopListParamModel model)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopList");
            try
            {
                var pageSize = 10;
                List<ShopListModel> shopList = new List<ShopListModel>();
                
                if (model.SortBy != (int)PharmacySortEnum.Ascending && model.SortBy != (int)PharmacySortEnum.Descending)
                {
                    aResp.Message = "Fournissez une valeur de tri valide.";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return aResp;
                }

                if (!string.IsNullOrEmpty(model.SearchKey))
                {
                    shopList = DbContext.Shops.AsNoTracking().Where(s => s.IsEnabled == true && s.ShopName.ToLower().Contains(model.SearchKey.ToLower()))
                                        .Select(s => new ShopListModel
                                        {
                                            ShopRecordId = s.ShopRecordID,
                                            ShopId = s.ShopId,
                                            ShopName = s.ShopName,
                                            ShopIcon = s.ShopIcon,
                                            Description = s.Description,
                                            MobileNumber = s.MobileNumber,
                                            TelephoneNumber = s.TelephoneNumber,
                                            Address = s.Address,
                                            PostalCode = s.PostalCode,
                                            City = s.City,
                                            Latitude = s.Latitude,
                                            Longitude = s.Longitude
                                        }).ToList();
                }
                else
                {
                    shopList = DbContext.Shops.AsNoTracking().Where(s => s.IsEnabled == true)
                                .Select(s=>new ShopListModel {
                                    ShopRecordId = s.ShopRecordID,
                                    ShopId = s.ShopId,
                                    ShopName = s.ShopName,
                                    ShopIcon = s.ShopIcon,
                                    Description = s.Description,
                                    MobileNumber = s.MobileNumber,
                                    TelephoneNumber = s.TelephoneNumber,
                                    Address = s.Address,
                                    PostalCode = s.PostalCode,
                                    City = s.City,
                                    Latitude = s.Latitude,
                                    Longitude = s.Longitude
                                }).ToList();

                }

                var paggedList = shopList.Where(w => w.ShopRecordId != 0, model.PageNo, pageSize).ToList();
                
                foreach (var item in paggedList)
                {
                    item.ShopStatus = GetPharmacyOpenOrClose(item.ShopId);
                }

                if (paggedList.Count > 0)
                {
                    if (model.SortBy == (int)PharmacySortEnum.Ascending)
                    {
                        paggedList = paggedList.OrderBy(s => s.ShopName).ToList();
                    }
                    else if (model.SortBy == (int)PharmacySortEnum.Descending)
                    {
                        paggedList = paggedList.OrderByDescending(s => s.ShopName).ToList();
                    }
                    // get total page count
                    var totalPageCount = 1;
                    var pageCount = shopList.Count;
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
                            ShopList = paggedList,
                            PageCount = totalPageCount
                        },
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun article n'a été récupéré.",
                        Status = "Échec!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetShopList");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        IEnumerable<ShopOpeningHourDTO> FilterShopOpeningHours(DayOfWeek? dayOfWeek, TimeSpan openHour, TimeSpan closeHour)
        {
            IEnumerable<ShopOpeningHourDTO> filteredShopHours = null;
            IEnumerable<ShopOpeningHourDTO> shopHours = null;

            try
            {
                shopHours = DbContext.ShopOpeningHours.AsNoTracking().Where(h => h.IsEnabled == true && h.DayOfWeek == dayOfWeek)
                                            .Select(p => new ShopOpeningHourDTO
                                            {
                                                ShopOpeningHourID = p.ShopOpeningHourID,
                                                ShopId = p.ShopId,
                                                DayOfWeek = p.DayOfWeek,
                                                StartTimeAM = TimeSpan.Parse(p.StartTimeAM),
                                                EndTimeAM = TimeSpan.Parse(p.EndTimeAM),
                                                StartTimePM = TimeSpan.Parse(p.StartTimePM),
                                                EndTimePM = TimeSpan.Parse(p.EndTimePM),
                                                StartTimeEvening = TimeSpan.Parse(p.StartTimeEvening),
                                                EndTimeEvening = TimeSpan.Parse(p.EndTimeEvening)
                                            });

                if (shopHours != null)
                {
                    filteredShopHours = shopHours
                        .Where(x => x.StartTimeAM >= openHour || x.EndTimeAM <= closeHour
                                    || x.StartTimePM >= openHour || x.EndTimePM <= closeHour
                                    || x.StartTimeEvening >= openHour || x.EndTimeEvening <= closeHour
                                    ).ToList();
                }

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetShopOpeningHours: ");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
            }

            return filteredShopHours;
        }

        public decimal GetMaximumKMDistance(OrderDeliveryType _deliveryType)
        {
            decimal maxKM = 0;
            if (_deliveryType == OrderDeliveryType.FORDELIVERY || _deliveryType == OrderDeliveryType.ALL)
            {
                var getMaxKM = DbContext.OrderConfigurations.AsNoTracking().FirstOrDefault(m => m.IsEnabled == true && m.ConfigType == OrderConfigType.MAXDELIVERYDISTANCE);
                if (getMaxKM != null) { maxKM = getMaxKM.ConfigDecValue; }
            }

            if (_deliveryType == OrderDeliveryType.FORPICKUP)
            {
                var getMaxKM = DbContext.OrderConfigurations.AsNoTracking().FirstOrDefault(m => m.IsEnabled == true && m.ConfigType == OrderConfigType.MAXPICKUPDISTANCE);
                if (getMaxKM != null) { maxKM = getMaxKM.ConfigDecValue; }
            }

            return maxKM;
        }

        decimal GetAverageRating(Guid ShopId)
        {
            var getTotalRatings = DbContext.ShopRatings
                .FromSqlRaw("SELECT ShopId, sum(Product) AS 'TotalRates', sum(CountStars) AS 'TotalVotes', " +
                "CAST(ROUND(sum(Product) * 1.0 / sum(CountStars), 2) AS DEC(10, 1)) AS 'FinalRates' " +
                "FROM (SELECT ShopId, ShopRating, Count(ShopRating) CountStars, " +
                "Count(ShopRating) * ShopRating AS Product FROM ShopReviewRatings " +
                "WHERE ShopId = {0} AND IsEnabled = 1 " +
                "GROUP BY ShopRating, ShopId) a GROUP BY ShopId", ShopId).FirstOrDefault();

            if (getTotalRatings == null)
            {
                return 0;
            }
            else
            {
                return getTotalRatings.FinalRates;
            }
        }
        private string GetPharmacyOpenOrClose(Guid shopId)
        {
            var today = DateTime.Now;
            DayOfWeek dayOfweek = today.DayOfWeek;
            var shopOpeningHours = DbContext.ShopOpeningHours.AsNoTracking().Where(s => s.ShopId == shopId && s.IsEnabled == true && s.DayOfWeek == dayOfweek).FirstOrDefault();
            if (shopOpeningHours != null)
            {
                if (TimeSpan.Parse(shopOpeningHours.StartTimeAM) <= today.TimeOfDay && today.TimeOfDay <= TimeSpan.Parse(shopOpeningHours.EndTimeAM))
                {
                    return "Ouvert";
                }
                else if (TimeSpan.Parse(shopOpeningHours.StartTimePM) <= today.TimeOfDay && today.TimeOfDay <= TimeSpan.Parse(shopOpeningHours.EndTimePM))
                {
                    return "Ouvert";
                }
                else if (TimeSpan.Parse(shopOpeningHours.StartTimeEvening) <= today.TimeOfDay && today.TimeOfDay <= TimeSpan.Parse(shopOpeningHours.EndTimeEvening))
                {
                    return "Ouvert";
                }
                else
                {
                    return "Fermer";
                }
                //if (shopOpeningHours.StartTimePM == "00:00:00" || shopOpeningHours.EndTimePM == "00:00:00")
                //{
                //    return "Closed";
                //}
                //else if (shopOpeningHours.StartTimeEvening == "00:00:00" || shopOpeningHours.EndTimeEvening == "00:00:00")
                //{
                //    return "Closed";
                //}
                //return "Open";
            }
            return "Fermer";
        }
        private string GetPharmacyWorkingAtNight(Guid shopId)
        {
            var today = DateTime.Now;
            var dayOfweek = today.DayOfWeek;
            var shopOpeningHours = DbContext.ShopOpeningHours.AsNoTracking().Where(s => s.ShopId == shopId && s.IsEnabled == true && s.DayOfWeek == dayOfweek && s.StartTimeEvening != null && s.EndTimeEvening != null).Count();
            if (shopOpeningHours > 0)
            {
                return "Open";
            }
            return "Closed";
        }

        public APIResponse GetTopShops()
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetTopShops");
            try
            {
                List<PopularPharmaciesDTO> TopPharmacies = DbContext.Shops
                                .Where(s => s.IsPopularPharmacy == true)
                                .Select(s => new PopularPharmaciesDTO
                                {
                                    ShopAddress = s.Address + " " + s.SuiteAddress + " " + s.PostalCode + " " + s.City,
                                    ShopDescription = s.Description,
                                    ShopIcon = s.ShopIcon,
                                    ShopID = s.ShopId,
                                    ShopName = s.ShopName,
                                    ShopRecordId = s.ShopRecordID,
                                    ShopTelephoneNumber = s.TelephoneNumber,
                                }).ToList();

                aResp = new APIResponse
                {
                    Message = "Tous les éléments ont été récupérés avec succès.",
                    Status = "Succès!",
                    Payload = TopPharmacies,
                    StatusCode = System.Net.HttpStatusCode.OK
                };

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetTopShops");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
    }
}
