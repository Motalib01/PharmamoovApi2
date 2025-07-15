using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using PharmaMoov.Models.Review;
using PharmaMoov.Models.User;
using PharmaMoov.Models.Shop;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class ReviewsRepository : APIBaseRepo, IReviewRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private readonly LocalizationService localization;
        private IHttpContextAccessor accessor;
        private IMainHttpClient MainHttpClient { get; }

        public ReviewsRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, IHttpContextAccessor _accessor, LocalizationService _localization, IMainHttpClient _mhttpc)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            localization = _localization;
            accessor = _accessor;
            MainHttpClient = _mhttpc;
        }

        public APIResponse AddShopReview(string _auth, ShopReviewRatingDTO _review)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogDebugObject(_review);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("ShopReviewRatingDTO: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    //get user details
                    User userDetails = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserId == IsUserLoggedIn.UserId);

                    ShopReviewRating shopReview = new ShopReviewRating()
                    {
                        ShopId = _review.ShopId,
                        UserId = userDetails.UserId,
                        UserName = userDetails.FirstName + " " + userDetails.LastName,
                        ShopReview = _review.ShopReview,
                        ShopRating = _review.ShopRating,

                        IsEnabled = true,
                        IsEnabledBy = userDetails.UserId,
                        DateEnabled = DateTime.Now,
                        CreatedBy = userDetails.UserId,
                        CreatedDate = DateTime.Now,
                        IsLocked = false,
                        LockedDateTime = DateTime.Now,
                        LastEditedBy = userDetails.UserId,
                        LastEditedDate = DateTime.Now
                    };

                    DbContext.ShopReviewRatings.Add(shopReview);
                    DbContext.SaveChanges();

                    aResp.Message = "Note et commentaire du commerçant ajoutée avec succès";
                    aResp.Status = "Succès";
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
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
                LogManager.LogInfo("AddShopReview");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse GetShopReviews(Guid _shop, int _review)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopReviews: " + _shop);

            try
            {
                if (_review == 0)
                {
                    IEnumerable<ShopReviewList> shopReviews = null;
                    shopReviews = DbContext.ShopReviewRatings.AsNoTracking()
                            .Join(DbContext.Users, r => r.UserId, u => u.UserId, (r, u) => new { r, u })
                            .Where(w => w.r.ShopId == _shop)
                            .Select(i => new ShopReviewList
                            {
                                ShopReviewID = i.r.ShopReviewID,
                                CustomerName = i.u.FirstName + " " + i.u.LastName,
                                Rating = i.r.ShopRating,
                                Review = i.r.ShopReview,
                                ReviewDate = i.r.CreatedDate.GetValueOrDefault()
                            });

                    if (shopReviews.Count() > 0)
                    {
                        aResp = new APIResponse
                        {
                            Message = "Tous les éléments ont été récupérés avec succès.",
                            Status = "Succès!",
                            Payload = shopReviews,
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
                else
                {
                    ShopReviewList shopReview = DbContext.ShopReviewRatings.AsNoTracking()
                            .Join(DbContext.Users, r => r.UserId, u => u.UserId, (r, u) => new { r, u })
                            .Where(w => w.r.ShopReviewID == _review)
                            .Select(i => new ShopReviewList
                            {
                                ShopReviewID = i.r.ShopReviewID,
                                CustomerName = i.u.FirstName + " " + i.u.LastName,
                                Rating = i.r.ShopRating,
                                Review = i.r.ShopReview,
                                ReviewDate = i.r.CreatedDate.GetValueOrDefault()
                            }).FirstOrDefault();

                    if (shopReview != null)
                    {
                        aResp = new APIResponse
                        {
                            Message = "L'article a été récupéré avec succès.",
                            Status = "Succès!",
                            Payload = shopReview,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Aucun élément n'a été récupéré.",
                            Status = "Échec!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetShopReviews: " + _shop);
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse SetShopFavorite(string _auth, ShopFavorite _favorite)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogDebugObject(_favorite);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("SetShopFavorite: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    UserShopFavorite hasFavorite = DbContext.UserShopFavorites.AsNoTracking().Where(s => s.ShopId == _favorite.ShopId && s.UserId == IsUserLoggedIn.UserId).FirstOrDefault();
                    if (hasFavorite == null)
                    {
                        //add new
                        if (_favorite.IsFavorite == true)
                        {
                            UserShopFavorite userShopFavorite = new UserShopFavorite()
                            {
                                UserId = IsUserLoggedIn.UserId,
                                ShopId = _favorite.ShopId,

                                IsEnabled = _favorite.IsFavorite,
                                IsEnabledBy = IsUserLoggedIn.UserId,
                                DateEnabled = DateTime.Now,
                                CreatedBy = IsUserLoggedIn.UserId,
                                CreatedDate = DateTime.Now,
                                IsLocked = false,
                                LockedDateTime = DateTime.Now,
                                LastEditedBy = IsUserLoggedIn.UserId,
                                LastEditedDate = DateTime.Now
                            };

                            DbContext.UserShopFavorites.Add(userShopFavorite);
                            DbContext.SaveChanges();

                            aResp.Message = "Commerçant ajouté à vos favoris";
                            aResp.Status = "Succès";
                            aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        }
                    }
                    else
                    {
                        //update existing
                        hasFavorite.IsEnabled = _favorite.IsFavorite;
                        hasFavorite.LastEditedBy = IsUserLoggedIn.UserId;
                        hasFavorite.LastEditedDate = DateTime.Now;

                        DbContext.UserShopFavorites.Update(hasFavorite);
                        DbContext.SaveChanges();

                        if (_favorite.IsFavorite == true)
                        {
                            aResp.Message = "Le commerçant est déjà ajouté à vos favoris";
                        }
                        else
                        {
                            aResp.Message = "Commerçant retiré de vos favoris avec succès";
                        }
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
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
                LogManager.LogInfo("SetShopFavorite");
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
