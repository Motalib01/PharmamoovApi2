using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using PharmaMoov.Models.User;
using PharmaMoov.Models.Promo;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.Campaign;
using Microsoft.AspNetCore.Http;
using PharmaMoov.Models.Cart;
using PharmaMoov.Models.Shop;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class PromoRepository : APIBaseRepo, IPromoRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private readonly LocalizationService localization;
        private IHttpContextAccessor accessor;
        private IMainHttpClient MainHttpClient { get; }

        public PromoRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, IHttpContextAccessor _accessor, LocalizationService _localization, IMainHttpClient _mhttpc)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            localization = _localization;
            accessor = _accessor;
            MainHttpClient = _mhttpc;
        }

        public APIResponse AddPromo(string _auth, PromoDTO _promo)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("AddPromo");
            LogManager.LogDebugObject(_promo);

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("AddPromo: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    Promo foundDuplicate = DbContext.Promos.AsNoTracking().Where(c => c.PromoCode == _promo.PromoCode && c.IsEnabled == true).FirstOrDefault();
                    if (foundDuplicate == null)
                    {
                        Promo shopPromo = new Promo()
                        {
                            PromoCode = _promo.PromoCode,
                            PromoDescription = _promo.PromoDescription,
                            PromoValue = _promo.PromoValue,
                            ValidityPeriod = _promo.ValidityPeriod,
                            ValidityDate = _promo.ValidityDate,
                            PromoName = _promo.PromoName,
                            ValidFrom = _promo.ValidFrom,
                            ValidTo = _promo.ValidTo,
                            PType = _promo.PType,
                            IsEnabled = _promo.IsEnabled,
                            IsEnabledBy = IsUserLoggedIn.UserId,
                            DateEnabled = DateTime.Now,
                            CreatedBy = IsUserLoggedIn.UserId,
                            CreatedDate = DateTime.Now,
                            IsLocked = false,
                            LockedDateTime = DateTime.Now,
                            LastEditedBy = IsUserLoggedIn.UserId,
                            LastEditedDate = DateTime.Now
                        };

                        DbContext.Promos.Add(shopPromo);
                        DbContext.SaveChanges();

                        aResp.Message = "Nouvelle Promo ajoutée avec succès.";
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        aResp.Payload = _promo;
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Le code promo existe déjà",
                            Status = "Échec",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
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
                LogManager.LogInfo("AddPromo");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse EditPromo(string _auth, PromoDTO _promo)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("EditPromo");
            LogManager.LogDebugObject(_promo);

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("EditPromo: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    Promo updatePromo = DbContext.Promos.FirstOrDefault(d => d.PromoRecordID == _promo.PromoRecordID);
                    if (updatePromo != null)
                    {
                        // check duplicate record 
                        Promo foundDuplicate = DbContext.Promos.Where(c => c.PromoCode == _promo.PromoCode && c.IsEnabled == true).FirstOrDefault();
                        if (foundDuplicate != null && foundDuplicate.PromoRecordID != updatePromo.PromoRecordID)
                        {
                            aResp = new APIResponse
                            {
                                Message = "Le code promo existe déjà",
                                Status = "Échec",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                        else
                        {
                            updatePromo.PromoName = _promo.PromoName;
                            updatePromo.PromoCode = _promo.PromoCode;
                            updatePromo.PromoDescription = _promo.PromoDescription;
                            updatePromo.PromoValue = _promo.PromoValue;
                            updatePromo.PType = _promo.PType;
                            updatePromo.ValidityPeriod = _promo.ValidityPeriod;
                            updatePromo.ValidityDate = _promo.ValidityDate;
                            updatePromo.ValidFrom = _promo.ValidFrom;
                            updatePromo.ValidTo = _promo.ValidTo; 

                            updatePromo.LastEditedDate = DateTime.Now;
                            updatePromo.LastEditedBy = IsUserLoggedIn.UserId;
                            updatePromo.DateEnabled = DateTime.Now;
                            updatePromo.IsEnabled = _promo.IsEnabled;
                            updatePromo.IsEnabledBy = IsUserLoggedIn.UserId;

                            DbContext.Update(updatePromo);
                            DbContext.SaveChanges();

                            aResp.Message = "Promo mise à jour avec succès.";
                            aResp.Status = "Succès";
                            aResp.StatusCode = System.Net.HttpStatusCode.OK;
                            aResp.Payload = updatePromo;
                        }
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
                LogManager.LogInfo("EditPromo");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse GetPromoValue(string _auth, string _code)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetPromoValue");
            LogManager.LogDebugObject(_code);

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    Promo foundPromo = DbContext.Promos.AsNoTracking().FirstOrDefault(i => i.PromoCode == _code);
                    if (foundPromo != null)
                    {
                        if (foundPromo.IsEnabled == false)
                        {
                            aResp = new APIResponse
                            {
                                Message = "Le code promo n'est plus valide",
                                Status = "Échec!",
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            List<CartItemsDTO> getCartItems = DbContext.CartItems.Where(c => c.UserId == IsUserLoggedIn.UserId)
                                .Select(i => new CartItemsDTO { 
                                    ProductQuantity = i.ProductQuantity,
                                    ProductRecordId = i.ProductRecordId
                                })
                                .ToList();

                            //var cartRepo = new CartRepository(DbContext,  LogManager, APIConfig, accessor, localization, MainHttpClient);

                            aResp = new APIResponse
                            {
                                Message = "Le code promotionnel est disponible.",
                                Status = "Succès!",  
                                //Payload = cartRepo.GetPromoAmount(_code, getCartItems),
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Code promo non trouvé.",
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
                LogManager.LogInfo("GetPromoValue");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse GetPromoList(int _id)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetPromoList");

            try
            {
                if (_id == 0)
                {
                    IEnumerable<PromoDTO> promos = null;
                    promos = DbContext.Promos.AsNoTracking()
                        .Select(i => new PromoDTO
                        {
                            PromoRecordID = i.PromoRecordID,
                            PromoCode = i.PromoCode,
                            PromoDescription = i.PromoDescription,
                            PromoValue = i.PromoValue,
                            ValidityPeriod = i.ValidityPeriod,
                            ValidityDate = i.ValidityDate,
                            IsEnabled = i.IsEnabled,
                            PromoName = i.PromoName,
                            PType = i.PType,
                            ValidFrom = i.ValidFrom,
                            ValidTo = i.ValidTo,
                        });
                     
                    if (promos.Count() > 0)
                    {
                        aResp = new APIResponse
                        {
                            Message = "Tous les éléments ont été récupérés avec succès.",
                            Status = "Succès!",
                            Payload = promos,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Aucun article n'a été récupéré.",
                            Status = "Échec!",
                            Payload = new List<PromoDTO>(),
                            StatusCode = System.Net.HttpStatusCode.NotFound
                        };
                    }
                }
                else 
                {
                    aResp = new APIResponse
                    {
                        Message = "Tous les éléments ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = DbContext.Promos.Where(p => p.PromoRecordID == _id)
                        .AsNoTracking()
                        .Select(i => new PromoDTO
                        {
                            PromoRecordID = i.PromoRecordID,
                            PromoCode = i.PromoCode,
                            PromoDescription = i.PromoDescription,
                            PromoValue = i.PromoValue,
                            ValidityPeriod = i.ValidityPeriod,
                            ValidityDate = i.ValidityDate,
                            IsEnabled = i.IsEnabled,
                            PromoName = i.PromoName,
                            PType = i.PType,
                            ValidFrom = i.ValidFrom,
                            ValidTo = i.ValidTo,
                        }).FirstOrDefault(),
                        StatusCode = System.Net.HttpStatusCode.OK
                    }; 
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetPromoList");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetCampaignByShopId(string _auth, Guid _cId) 
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetCampaignByShopId");
            LogManager.LogDebugObject("Campaign ID: " + _cId.ToString());

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("Get Campaign by ID UserId: " + IsUserLoggedIn.UserId);

                    aResp.Message = "Campagne récupérée avec succès.";
                    aResp.Status = "Succès";
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                    aResp.Payload = DbContext.Campaigns.Where(c => c.ShopId == _cId).FirstOrDefault();
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
                LogManager.LogInfo("GetCampaignByShopId");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;

        }

        public APIResponse ChangePromoStatus(ChangeRecordStatus _admin)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ChangeRecordStatus");

            try
            {
                Promo foundAdmin = DbContext.Promos.Where(a => a.PromoRecordID == _admin.RecordId).FirstOrDefault();
                if (foundAdmin != null)
                {
                    DateTime NowDate = DateTime.Now;
                    foundAdmin.LastEditedDate = DateTime.Now;
                    foundAdmin.LastEditedBy = _admin.AdminId;
                    foundAdmin.DateEnabled = DateTime.Now;
                    foundAdmin.IsEnabledBy = _admin.AdminId;
                    foundAdmin.IsEnabled = _admin.IsActive;

                    DbContext.Promos.Update(foundAdmin);
                    DbContext.SaveChanges();
                    aResp = new APIResponse
                    {
                        Message = "Mise à jour de la promotion réussie.",
                        Status = "Succès!",
                        Payload = foundAdmin,
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
                LogManager.LogInfo("ChangeRecordStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse AddBanner(string _auth, Campaign _campaign) 
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("AddBanner");
            LogManager.LogDebugObject(_campaign);

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("Add shop Banner: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);
                    Campaign foundDuplicate = DbContext.Campaigns.AsNoTracking().Where(c => c.CampaignRecordID == _campaign.CampaignRecordID).FirstOrDefault();
                    if (foundDuplicate == null)
                    {
                        _campaign.CreatedBy = IsUserLoggedIn.UserId;
                        _campaign.CreatedDate = DateTime.Now;
                        _campaign.DateEnabled = DateTime.Now;
                        _campaign.IsEnabled = true;
                        _campaign.IsEnabledBy = IsUserLoggedIn.UserId;
                        _campaign.IsLocked = false;
                        _campaign.LastEditedBy = IsUserLoggedIn.UserId;
                        _campaign.LastEditedDate = DateTime.Now;

                        DbContext.Campaigns.Add(_campaign);
                        DbContext.SaveChanges();

                        aResp.Message = "Nouvelle bannière ajoutée avec succès";
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        aResp.Payload = _campaign;
                    }
                    else
                    {   
                        _campaign.LastEditedBy = IsUserLoggedIn.UserId;
                        _campaign.LastEditedDate = DateTime.Now;

                        DbContext.Update(_campaign);
                        DbContext.SaveChanges();

                        aResp.Message = "Bannière mise à jour avec succès";
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        aResp.Payload = _campaign;
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
                LogManager.LogInfo("AddBanner");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;

        }

        public APIResponse GetShopBanners()
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopBanners");

            try
            {
                IEnumerable<CampaignDTO> campaignBanners = null;

                campaignBanners = DbContext.Campaigns.AsNoTracking()
                    .Where(w => w.IsEnabled == true)
                    .Select(c => new CampaignDTO
                    {
                        CampaignRecordID = c.CampaignRecordID,
                        ShopId = c.ShopId,
                        ImageUrl = c.ImageUrl,
                        IsProductOfferBanner = c.IsProductOfferBanner
                    }).ToList();

                foreach (var banner in campaignBanners)
                {
                    var getShopName = DbContext.Shops.AsNoTracking().FirstOrDefault(s => s.ShopId == banner.ShopId);

                    if (getShopName != null)
                    {
                        banner.RelatedShopName = getShopName.ShopName;
                    }
                }

                if (campaignBanners.Count() > 0)
                {
                    aResp = new APIResponse
                    {
                        Message = "Tous les éléments ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = campaignBanners,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun article n'a été récupéré.",
                        Status = "Échec!",
                        Payload = new List<CampaignDTO>(),
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
                
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetShopBanners");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetCampaignByBannerId(string _auth, int _id)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetCampaignByBannerId");
            LogManager.LogDebugObject("CampaignRecord ID: " + _id.ToString());

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("Get Campaign by ID UserId: " + IsUserLoggedIn.UserId);

                    aResp.Message = "Campagne récupérée avec succès.";
                    aResp.Status = "Succès";
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                    aResp.Payload = DbContext.Campaigns.Where(c => c.CampaignRecordID == _id).FirstOrDefault();
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
                LogManager.LogInfo("GetCampaignByBannerId");
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
