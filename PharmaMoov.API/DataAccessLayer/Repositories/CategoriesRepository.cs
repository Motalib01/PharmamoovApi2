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
using PharmaMoov.Models.User;
using PharmaMoov.Models.Admin;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class CategoriesRepository : APIBaseRepo, ICategoriesRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public CategoriesRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public APIResponse PopulateShopCategories(int _category, int IsActive) 
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("PopulateShopCategories: " + _category);

            try
            {
                ShopCategoriesDTO shopCategory = new ShopCategoriesDTO();
                IQueryable<ShopCategoriesDTO> filterResult = null;
                filterResult =  DbContext.ShopCategories.AsNoTracking()
                                        .Select(p => new ShopCategoriesDTO
                                        {
                                            ShopCategoryId = p.ShopCategoryID,
                                            Name = p.Name,
                                            Description = p.Description,
                                            ImageUrl = p.ImageUrl,
                                            DateCreated = p.CreatedDate.GetValueOrDefault(),
                                            IsActive = p.IsEnabled ?? false,
                                            
                                        });

                if (filterResult != null)
                {
                    if (_category > 0)
                    {
                        shopCategory = filterResult.Where(c => c.ShopCategoryId == _category).FirstOrDefault();

                        aResp = new APIResponse
                        {
                            Message = "Enregistrement récupéré avec succès.",
                            Status = "Succès!",
                            Payload = shopCategory,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        if (IsActive > 0)
                        {
                            aResp = new APIResponse
                            {
                                Message = "Tous les éléments ont été récupérés avec succès.",
                                Status = "Succès!",
                                Payload = filterResult.Where(i => i.IsActive == true),
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            aResp = new APIResponse
                            {
                                Message = "Tous les éléments ont été récupérés avec succès.",
                                Status = "Succès!",
                                Payload = filterResult,
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("PopulateShopCategories");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse AddShopCategory(string _auth, ShopCategoriesDTO _category)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("AddShopCategory: " + _category);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    ShopCategory foundDuplicate = DbContext.ShopCategories.AsNoTracking().Where(c => c.Name == _category.Name).FirstOrDefault();
                    if (foundDuplicate == null)
                    {
                        ShopCategory shopCategory = new ShopCategory()
                        {
                            Name = _category.Name,
                            Description = _category.Description,
                            ImageUrl = _category.ImageUrl,
                            
                            IsEnabled = _category.IsActive,
                            IsEnabledBy = IsUserLoggedIn.UserId,
                            DateEnabled = DateTime.Now,
                            CreatedBy = IsUserLoggedIn.UserId,
                            CreatedDate = DateTime.Now,
                            IsLocked = false,
                            LockedDateTime = DateTime.Now,
                            LastEditedBy = IsUserLoggedIn.UserId,
                            LastEditedDate = DateTime.Now
                        };

                        DbContext.ShopCategories.Add(shopCategory);
                        DbContext.SaveChanges();

                        aResp.Message = "Catégorie ajoutée avec succès";
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        aResp.Message = "Duplicat d'enregistrement trouvé.";
                        aResp.Status = "Échec";
                        aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AddShopCategory");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse EditShopCategory(string _auth, ShopCategoriesDTO _category)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("EditShopCategory: " + _category);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    ShopCategory updateCategory = DbContext.ShopCategories.FirstOrDefault(d => d.ShopCategoryID == _category.ShopCategoryId);
                    if (updateCategory != null)
                    {
                        // check duplicate record 
                        ShopCategory foundDuplicate = DbContext.ShopCategories.Where(c => c.Name == _category.Name).FirstOrDefault();
                        if (foundDuplicate != null && foundDuplicate.ShopCategoryID != updateCategory.ShopCategoryID)
                        {
                            aResp.Message = "Duplicat d'enregistrement trouvé.";
                            aResp.Status = "Échec";
                            aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;

                            return aResp;
                        }
                        else
                        {
                            updateCategory.Name = _category.Name;
                            updateCategory.Description = _category.Description;
                            updateCategory.ImageUrl = _category.ImageUrl;

                            updateCategory.LastEditedDate = DateTime.Now;
                            updateCategory.LastEditedBy = IsUserLoggedIn.UserId;
                            updateCategory.DateEnabled = DateTime.Now;
                            updateCategory.IsEnabled = _category.IsActive;
                            updateCategory.IsEnabledBy = IsUserLoggedIn.UserId;

                            DbContext.ShopCategories.Update(updateCategory);
                            DbContext.SaveChanges();

                            aResp.Message = "Catégorie de commerçant mis à jour avec succès";
                            aResp.Status = "Succès";
                            aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        }
                    }
                    else
                    {
                        aResp.Message = "Aucun enregistrement trouvé.";
                        aResp.Status = "Échec";
                        aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("EditShopCategory");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse ChangeShopCategoryStatus(string _auth, ChangeRecordStatus _category)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ChangeShopCategoryStatus");
            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    ShopCategory foundCategory = DbContext.ShopCategories.Where(a => a.ShopCategoryID == _category.RecordId).FirstOrDefault();
                    if (foundCategory != null)
                    {
                        DateTime NowDate = DateTime.Now;
                        foundCategory.LastEditedDate = DateTime.Now;
                        foundCategory.LastEditedBy = _category.AdminId;
                        foundCategory.DateEnabled = DateTime.Now;
                        foundCategory.IsEnabledBy = _category.AdminId;
                        foundCategory.IsEnabled = _category.IsActive;

                        DbContext.ShopCategories.Update(foundCategory);
                        DbContext.SaveChanges();
                        aResp = new APIResponse
                        {
                            Message = "La catégorie de la boutique a été mise à jour avec succès.",
                            Status = "Succès!",
                            Payload = foundCategory,
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
                else
                {
                    aResp.Message = "Utilisateur non connecté";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("ChangeShopCategoryStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse PopulateProductCategories(Guid _shopId, int _category, int IsActive)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("PopulateProductCategories: " + _shopId);

            try
            {
                ProductCategoriesDTO productCategory = new ProductCategoriesDTO();
                IQueryable<ProductCategoriesDTO> filterResult = null;
                filterResult = DbContext.ProductCategories.AsNoTracking()
                                         //.Where(c => c.ShopId == _shopId)
                                         .Select(p => new ProductCategoriesDTO
                                         {
                                             ShopId = p.ShopId,
                                             ProductCategoryId = p.ProductCategoryId,
                                             ProductCategoryName = p.ProductCategoryName,
                                             ProductCategoryDesc = p.ProductCategoryDesc,
                                             ProductCategoryImage = p.ProductCategoryImage,
                                             DateCreated = p.CreatedDate.GetValueOrDefault(),
                                             IsActive = p.IsEnabled ?? false,
                                             IsCategoryFeatured = p.IsCategoryFeatured
                                         });

                if (filterResult != null)
                {
                    
                    if (_category > 0)
                    {
                        productCategory = filterResult.Where(c => c.ProductCategoryId == _category).FirstOrDefault();

                        aResp = new APIResponse
                        {
                            Message = "Enregistrement récupéré avec succès.",
                            Status = "Succès!",
                            Payload = productCategory,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        if (IsActive > 0)
                        {
                            aResp = new APIResponse
                            {
                                Message = "Tous les éléments ont été récupérés avec succès.",
                                Status = "Succès!",
                                Payload = filterResult.Where(i => i.IsActive == true),
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            aResp = new APIResponse
                            {
                                Message = "Tous les éléments ont été récupérés avec succès.",
                                Status = "Succès!",
                                Payload = filterResult,
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("PopulateProductCategories");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse AddProductCategory(string _auth, ProductCategoriesDTO _category)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("AddProductCategory: " + _category);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    ProductCategory foundDuplicate = DbContext.ProductCategories.AsNoTracking().Where(c => c.ProductCategoryName == _category.ProductCategoryName /*&& c.ShopId == _category.ShopId*/).FirstOrDefault();
                    if (foundDuplicate == null)
                    {
                        ProductCategory productCategory = new ProductCategory()
                        {
                            //ShopId = _category.ShopId,
                            ProductCategoryName = _category.ProductCategoryName,
                            ProductCategoryDesc = _category.ProductCategoryDesc,
                            ProductCategoryImage = _category.ProductCategoryImage,
                            IsCategoryFeatured = _category.IsCategoryFeatured,

                            IsEnabled = _category.IsActive,
                            IsEnabledBy = IsUserLoggedIn.UserId,
                            DateEnabled = DateTime.Now,
                            CreatedBy = IsUserLoggedIn.UserId,
                            CreatedDate = DateTime.Now,
                            IsLocked = false,
                            LockedDateTime = DateTime.Now,
                            LastEditedBy = IsUserLoggedIn.UserId,
                            LastEditedDate = DateTime.Now
                        };

                        DbContext.ProductCategories.Add(productCategory);
                        DbContext.SaveChanges();

                        aResp.Message = "L'enregistrement a été ajouté avec succès.";
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        aResp.Message = "Duplicat d'enregistrement trouvé.";
                        aResp.Status = "Échec";
                        aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AddProductCategory");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse EditProductCategory(string _auth, ProductCategoriesDTO _category)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("EditProductCategory: " + _category);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    ProductCategory updateCategory = DbContext.ProductCategories.FirstOrDefault(d => d.ProductCategoryId == _category.ProductCategoryId);
                    if (updateCategory != null)
                    {
                        // check duplicate record 
                        ProductCategory foundDuplicate = DbContext.ProductCategories.Where(c => c.ProductCategoryName == _category.ProductCategoryName /* && c.ShopId == _category.ShopId*/).FirstOrDefault();
                        if (foundDuplicate != null && foundDuplicate.ProductCategoryId != updateCategory.ProductCategoryId)
                        {
                            aResp.Message = "Duplicat d'enregistrement trouvé.";
                            aResp.Status = "Échec";
                            aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;

                            return aResp;
                        }
                        else
                        {
                            updateCategory.ProductCategoryName = _category.ProductCategoryName;
                            updateCategory.ProductCategoryDesc = updateCategory.ProductCategoryDesc;
                            updateCategory.ProductCategoryImage = _category.ProductCategoryImage;
                            updateCategory.IsCategoryFeatured = _category.IsCategoryFeatured;

                            updateCategory.LastEditedDate = DateTime.Now;
                            updateCategory.LastEditedBy = IsUserLoggedIn.UserId;
                            updateCategory.DateEnabled = DateTime.Now;
                            updateCategory.IsEnabled = _category.IsActive;
                            updateCategory.IsEnabledBy = IsUserLoggedIn.UserId;

                            DbContext.ProductCategories.Update(updateCategory);
                            DbContext.SaveChanges();

                            aResp.Message = "L'enregistrement a été mis à jour avec succès.";
                            aResp.Status = "Succès";
                            aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        }
                    }
                    else
                    {
                        aResp.Message = "Aucun enregistrement trouvé.";
                        aResp.Status = "Échec";
                        aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("EditProductCategory");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }
        public APIResponse FilterProductCategories(string _searchKey)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("FilterProductCategories");

            try
            {                
                var shopList = DbContext.ProductCategories.AsNoTracking()
                                .Where(s => s.IsEnabled == true && s.ProductCategoryName.ToLower().Contains(_searchKey.ToLower()))
                                .Select(p => new 
                                {
                                    p.ProductCategoryId,
                                    p.ProductCategoryName
                                }).ToList();

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
                LogManager.LogInfo("FilterProductCategories:");
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
