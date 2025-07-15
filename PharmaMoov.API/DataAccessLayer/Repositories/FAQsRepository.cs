using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using PharmaMoov.Models.User;
using PharmaMoov.Models.Shop;
using PharmaMoov.Models.Admin;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class FAQsRepository : APIBaseRepo, IFAQsRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private readonly LocalizationService localization;
        private IHttpContextAccessor accessor;
        private IMainHttpClient MainHttpClient { get; }

        public FAQsRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, IHttpContextAccessor _accessor, LocalizationService _localization, IMainHttpClient _mhttpc)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            localization = _localization;
            accessor = _accessor;
            MainHttpClient = _mhttpc;
        }

        public APIResponse AddFAQuestion(string _auth, ShopFAQdto _fAQ)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogDebugObject(_fAQ);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("AddFAQuestion: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    ShopFAQ shopFAQ = new ShopFAQ()
                    {
                        Question = _fAQ.Question,
                        Answer = _fAQ.Answer,

                        IsEnabled = true,
                        IsEnabledBy = IsUserLoggedIn.UserId,
                        DateEnabled = DateTime.Now,
                        CreatedBy = IsUserLoggedIn.UserId,
                        CreatedDate = DateTime.Now,
                        IsLocked = false,
                        LockedDateTime = DateTime.Now,
                        LastEditedBy = IsUserLoggedIn.UserId,
                        LastEditedDate = DateTime.Now
                    };

                    DbContext.ShopFAQs.Add(shopFAQ);
                    DbContext.SaveChanges();

                    aResp.Message = "La FAQ du magasin a été ajoutée avec succès.";
                    aResp.Status = "Succès";
                    aResp.Payload = shopFAQ;
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    aResp.Message = "Utilisateur non connecté";
                    aResp.Status = "Échec";
                    aResp.Payload = _fAQ;
                    aResp.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AddFAQuestion");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse EditFAQuestion(string _auth, ShopFAQdto _fAQ)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogDebugObject(_fAQ);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("EditFAQuestion: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    ShopFAQ updatedFAQ = DbContext.ShopFAQs.FirstOrDefault(d => d.ShopFAQID == _fAQ.ShopFAQID);
                    if (updatedFAQ != null)
                    {
                        updatedFAQ.Question = _fAQ.Question;
                        updatedFAQ.Answer = _fAQ.Answer;
                        updatedFAQ.LastEditedDate = DateTime.Now;
                        updatedFAQ.LastEditedBy = IsUserLoggedIn.UserId; 

                        DbContext.ShopFAQs.Update(updatedFAQ);
                        DbContext.SaveChanges();

                        aResp.Message = "La FAQ du magasin a été mise à jour avec succès.";
                        aResp.Status = "Succès";
                        aResp.Payload = updatedFAQ;
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
                LogManager.LogInfo("EditFAQuestion");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse GetFAQs()
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetFAQs"); 
            try
            {
                IEnumerable<ShopFAQdto> shopFAqs = DbContext.ShopFAQs.AsNoTracking().Where(r => r.IsEnabled == true)
                    .Select(i => new ShopFAQdto
                    {
                        ShopFAQID = i.ShopFAQID,
                        Question = i.Question,
                        Answer = i.Answer,
                        IsActive = i.IsEnabled,
                        Order = i.Order
                    });

                if (shopFAqs.Count() > 0)
                {
                    aResp = new APIResponse
                    {
                        Message = "Tous les éléments ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = shopFAqs,
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
                LogManager.LogInfo("GetFAQs");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetFAQsList(string _auth, int _id) 
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogDebugObject("GetFAQsList ID parameter: " + _id.ToString()); 
            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("GetFAQsList AdminID: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);
                    if (_id == 0)
                    {
                        IEnumerable<ShopFAQdto> shopFAQList = null;
                        shopFAQList = DbContext.ShopFAQs.AsNoTracking()
                        .Select(i => new ShopFAQdto
                        {
                            ShopFAQID = i.ShopFAQID,
                            Question = i.Question,
                            Answer = i.Answer,
                            IsActive = i.IsEnabled,
                            Order = i.Order
                        });

                        if (shopFAQList.Count() > 0)
                        {
                            aResp = new APIResponse
                            {
                                Message = "Tous les éléments ont été récupérés avec succès.",
                                Status = "Succès!",
                                Payload = shopFAQList,
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            aResp = new APIResponse
                            {
                                Message = "Aucun article n'a été récupéré.",
                                Status = "Échec!",
                                Payload = shopFAQList,
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
                            Payload = DbContext.ShopFAQs.Where(p => p.ShopFAQID == _id).AsNoTracking().FirstOrDefault(),
                            StatusCode = System.Net.HttpStatusCode.OK
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
                LogManager.LogInfo("GetFAQsList");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            } 
            return aResp; 
        }

        public APIResponse ChangeFAQStatus(ChangeRecordStatus _faqStat)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ChangeFAQStatus");

            try
            {
                ShopFAQ foundShopFAQ = DbContext.ShopFAQs.Where(a => a.ShopFAQID == _faqStat.RecordId).FirstOrDefault();
                if (foundShopFAQ != null)
                {
                    DateTime NowDate = DateTime.Now;
                    foundShopFAQ.LastEditedDate = DateTime.Now;
                    foundShopFAQ.LastEditedBy = _faqStat.AdminId;
                    foundShopFAQ.DateEnabled = DateTime.Now;
                    foundShopFAQ.IsEnabledBy = _faqStat.AdminId;
                    foundShopFAQ.IsEnabled = _faqStat.IsActive;

                    DbContext.ShopFAQs.Update(foundShopFAQ);
                    DbContext.SaveChanges();
                    aResp = new APIResponse
                    {
                        Message = "FAQ mise à jour avec succès.",
                        Status = "Succès!",
                        Payload = foundShopFAQ,
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
                LogManager.LogInfo("ChangeFAQStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetTermsAndConditions() 
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetTermsAndConditions");
            try
            {
                TermsAndCondition TandC = new TermsAndCondition();
                TandC = DbContext.TermsAndConditions.FirstOrDefault();
                aResp = new APIResponse
                {
                    Message = "Tous les éléments ont été récupérés avec succès.",
                    Status = "Succès!",
                    Payload = TandC,
                    StatusCode = System.Net.HttpStatusCode.OK
                }; 
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetTermsAndConditions");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetPrivacyPolicy() 
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetPrivacyPolicy");
            try
            {
                PrivacyPolicy PrivPolicy = new PrivacyPolicy();
                PrivPolicy = DbContext.PrivacyPolicies.FirstOrDefault();
                aResp = new APIResponse
                {
                    Message = "Tous les éléments ont été récupérés avec succès.",
                    Status = "Succès!",
                    Payload = PrivPolicy,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetPrivacyPolicy");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse SaveTermsAndConditions(string Authorization,TermsAndCondition _termAndCondition) 
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == Authorization && ult.IsActive == true);
            LogManager.LogInfo("SaveTermsAndConditions");
            LogManager.LogDebugObject(_termAndCondition);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    //
                    // check the table 
                    // if a current terms and conditions exist
                    TermsAndCondition TandC = DbContext.TermsAndConditions.FirstOrDefault();
                    if (TandC != null) // a terms and condition exist
                    {
                        TandC.TermsAndConditionBody = _termAndCondition.TermsAndConditionBody;
                        TandC.LastEditedBy = IsUserLoggedIn.UserId;
                        TandC.LastEditedDate = DateTime.Now;
                        DbContext.Update(TandC);
                    }
                    else // add a new Terms and Conditions
                    {
                        TandC = new TermsAndCondition()
                        {
                            TermsAndConditionBody = _termAndCondition.TermsAndConditionBody,
                            IsEnabled = true,
                            IsEnabledBy = IsUserLoggedIn.UserId,
                            DateEnabled = DateTime.Now,
                            CreatedBy = IsUserLoggedIn.UserId,
                            CreatedDate = DateTime.Now,
                            IsLocked = false,
                            LockedDateTime = DateTime.Now,
                            LastEditedBy = IsUserLoggedIn.UserId,
                            LastEditedDate = DateTime.Now
                        };
                        DbContext.Add(TandC);
                    }
                    DbContext.SaveChanges();
                    aResp.Message = "La FAQ du magasin a été ajoutée avec succès.";
                    aResp.Status = "Succès";
                    aResp.Payload = _termAndCondition;
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    aResp.Message = "Utilisateur non connecté";
                    aResp.Status = "Échec";
                    aResp.Payload = _termAndCondition;
                    aResp.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("SaveTermsAndConditions");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse SavePrivacyPolicy(string Authorization, PrivacyPolicy _privPolicy)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == Authorization && ult.IsActive == true);
            LogManager.LogInfo("SavePrivacyPolicy");
            LogManager.LogDebugObject(_privPolicy);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    //
                    // check the table 
                    // if a current Privacy Policy exist
                    PrivacyPolicy privPolicy = DbContext.PrivacyPolicies.FirstOrDefault();
                    if (privPolicy != null) // a terms and condition exist
                    {
                        privPolicy.PrivacyPolicyBody = _privPolicy.PrivacyPolicyBody;
                        privPolicy.LastEditedBy = IsUserLoggedIn.UserId;
                        privPolicy.LastEditedDate = DateTime.Now;
                        DbContext.Update(privPolicy);
                    }
                    else // add a new Privacy Polic
                    {
                        privPolicy = new PrivacyPolicy()
                        {
                            PrivacyPolicyBody = _privPolicy.PrivacyPolicyBody,
                            IsEnabled = true,
                            IsEnabledBy = IsUserLoggedIn.UserId,
                            DateEnabled = DateTime.Now,
                            CreatedBy = IsUserLoggedIn.UserId,
                            CreatedDate = DateTime.Now,
                            IsLocked = false,
                            LockedDateTime = DateTime.Now,
                            LastEditedBy = IsUserLoggedIn.UserId,
                            LastEditedDate = DateTime.Now
                        };
                        DbContext.Add(privPolicy);
                    }
                    DbContext.SaveChanges();
                    aResp.Message = "Politique de confidentialité ajoutée avec succès.";
                    aResp.Status = "Succès";
                    aResp.Payload = privPolicy;
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    aResp.Message = "Utilisateur non connecté";
                    aResp.Status = "Échec";
                    aResp.Payload = _privPolicy;
                    aResp.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("SavePrivacyPolicy");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse ShopGetTermsAndConditions() 
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ShopGetTermsAndConditions");
            try
            {
                ShopTermsAndCondition TandC = new ShopTermsAndCondition();
                TandC = DbContext.ShopTermsAndConditions.FirstOrDefault();
                aResp = new APIResponse
                {
                    Message = "Tous les éléments ont été récupérés avec succès.",
                    Status = "Succès!",
                    Payload = TandC,
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("ShopGetTermsAndConditions");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse ShopSaveTermsAndConditions(string Authorization, ShopTermsAndCondition _termAndCondition) 
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == Authorization && ult.IsActive == true);
            LogManager.LogInfo("ShopSaveTermsAndConditions");
            LogManager.LogDebugObject(_termAndCondition);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    //
                    // check the table 
                    // if a current terms and conditions exist
                    ShopTermsAndCondition TandC = DbContext.ShopTermsAndConditions.FirstOrDefault();
                    if (TandC != null) // a terms and condition exist
                    {
                        TandC.ShopTermsAndConditionBody = _termAndCondition.ShopTermsAndConditionBody;
                        TandC.LastEditedBy = IsUserLoggedIn.UserId;
                        TandC.LastEditedDate = DateTime.Now;
                        DbContext.Update(TandC);
                    }
                    else // add a new Terms and Conditions
                    {
                        TandC = new ShopTermsAndCondition()
                        {
                            ShopTermsAndConditionBody = _termAndCondition.ShopTermsAndConditionBody,
                            IsEnabled = true,
                            IsEnabledBy = IsUserLoggedIn.UserId,
                            DateEnabled = DateTime.Now,
                            CreatedBy = IsUserLoggedIn.UserId,
                            CreatedDate = DateTime.Now,
                            IsLocked = false,
                            LockedDateTime = DateTime.Now,
                            LastEditedBy = IsUserLoggedIn.UserId,
                            LastEditedDate = DateTime.Now
                        };
                        DbContext.Add(TandC);
                    }
                    DbContext.SaveChanges();
                    aResp.Message = "La boutique T&A a été ajoutée avec succès.";
                    aResp.Status = "Succès";
                    aResp.Payload = _termAndCondition;
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    aResp.Message = "Utilisateur non connecté";
                    aResp.Status = "Échec";
                    aResp.Payload = _termAndCondition;
                    aResp.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("ShopSaveTermsAndConditions");
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
