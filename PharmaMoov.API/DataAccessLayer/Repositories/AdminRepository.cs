using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.User;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using PharmaMoov.Models.Shop;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class AdminRepository : APIBaseRepo, IAdminRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public AdminRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public APIResponse RegisterAdmin(AdminProfile _admin)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("RegisterAdmin: " + _admin.Email);

            try
            {
                //Check Duplicate Record 
                Admin FoundUser = DbContext.Admins.Where(u => u.Email == _admin.Email).FirstOrDefault();
                if (FoundUser == null)
                {
                    Guid AdminGuidID = Guid.NewGuid();
                    DateTime RegistrationDate = DateTime.Now;

                    Admin NewUser = new Admin
                    {
                        AdminId = AdminGuidID,
                        ShopId = _admin.ShopId,
                        Email = _admin.Email,
                        FirstName = _admin.FirstName,
                        LastName = _admin.LastName,
                        UserTypeId = _admin.UserTypeId,
                        AccountType = _admin.AccountType,
                        ImageUrl = _admin.ImageUrl,
                        IsVerified = false,

                        LastEditedBy = _admin.AdminId,
                        LastEditedDate = RegistrationDate,
                        CreatedBy = _admin.AdminId,
                        CreatedDate = RegistrationDate,
                        IsEnabledBy = _admin.AdminId,
                        IsEnabled = _admin.IsEnabled,
                        DateEnabled = RegistrationDate,
                        IsLocked = false,
                        LockedDateTime = null
                    };
                    NewUser.Password = NewUser.HashP(_admin.Password.ToString(), APIConfig.TokenKeys.Key);

                    DbContext.Add(NewUser);
                    DbContext.SaveChanges();

                    apiResp.Message = "Nouvel utilisateur ajouté avec succès";
                    apiResp.Status = "Succès!";
                    apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                    apiResp.Payload = NewUser;
                }
                else
                {
                    LogManager.LogWarn("L'email Address:" + FoundUser.Email + " est déjà utilisé !");
                    FoundUser.Password = "";
                    LogManager.LogDebugObject(FoundUser);
                    apiResp = new APIResponse
                    {
                        Message = "Email déjà utilisé",
                        Status = "Erreur",
                        StatusCode = System.Net.HttpStatusCode.Found
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError(ex.Message + "\n---------------\n" + ex.StackTrace);
                apiResp = new APIResponse
                {
                    Message = "Server Erreur!",
                    Status = "Erreur de serveur interne!",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }

            return apiResp;
        }

        public APIResponse AdminLogin(AdminLogin _admin)
        {
            APIResponse ApiResp = new APIResponse();
            LogManager.LogInfo("AdminLogin: " + _admin.Email);
            LogManager.LogDebugObject(_admin);

            try
            {
                //Check if user exist
                Admin foundAdmin = DbContext.Admins.Where(a => a.Email == _admin.Email).FirstOrDefault();
                if (foundAdmin != null)
                {
                    if(foundAdmin.UserTypeId == UserTypes.SHOPADMIN)
                    {
                        Shop foundShop = DbContext.Shops.Where(a => a.ShopId == foundAdmin.ShopId).FirstOrDefault();

                        if(foundShop!= null && foundShop.IsEnabled != true)
                        {
                            ApiResp.Message = "Inactiver le compte administrateur.";
                            ApiResp.Status = "Échec";
                            ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                            return ApiResp;
                        }
                    }
                    else
                    {
                        //check if status is disabled or enabled
                        if (foundAdmin.IsEnabled != true)
                        {
                            ApiResp.Message = "Inactiver le compte administrateur.";
                            ApiResp.Status = "Échec";
                            ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                            return ApiResp;
                        }
                    }
                   

                    // check pw
                    if (foundAdmin.Password == foundAdmin.HashP(_admin.Password.ToString(), APIConfig.TokenKeys.Key))
                    {
                        if (foundAdmin.IsVerified != true)
                        {
                            ApiResp.Message = "Merci de changer le mot de passe";
                            ApiResp.Status = "Erreur";
                            ApiResp.StatusCode = System.Net.HttpStatusCode.Redirect;

                            ApiResp.Payload = new AdminUserContext
                            {
                                AdminInfo = foundAdmin,
                                Tokens = AddLoginTransactionForAdminUser(foundAdmin)
                            };

                            return ApiResp;
                        }
                        else
                        {
                            ApiResp.Message = "Connexion réussie!";
                            ApiResp.Status = "Succès";
                            ApiResp.StatusCode = System.Net.HttpStatusCode.OK;

                            //Remove hash password value before sending back to user!
                            ApiResp.Payload = new AdminUserContext
                            {
                                AdminInfo = foundAdmin,
                                Tokens = AddLoginTransactionForAdminUser(foundAdmin)
                            };
                        }
                    }
                    else
                    {
                        ApiResp.Message = "Mot de passe incorrect";
                        ApiResp.Status = "Identifiants invalides";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    ApiResp.Message = "E-mail not found.";
                    ApiResp.Status = "Identifiants invalides";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AdminWebLogin");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                ApiResp.Message = "La nouvelle adresse a été ajoutée avec succès.!";
                ApiResp.Status = "Erreur de serveur interne";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }
            return ApiResp;
        }

        public UserLoginTransaction AddLoginTransactionForAdminUser(Admin _admin)
        {
            DateTime ExpirationDates = DateTime.Now;
            UserLoginTransaction ulT = new UserLoginTransaction();
            //
            // check if there is an existing login transaction that is not yet expired 
            // for the same platform
            //
            try
            {
                ulT = DbContext.UserLoginTransactions
                               .Where(u => u.UserId == _admin.AdminId &&
                                           u.Device == DevicePlatforms.Web &&
                                           u.IsActive == true &&
                                           u.TokenExpiration > DateTime.Now)
                               .OrderByDescending(u => u.DateCreated)
                               .FirstOrDefault();
                if (ulT == null)
                {
                    //
                    // there was no active login transaction
                    //
                    ulT = new UserLoginTransaction
                    {
                        UserId = _admin.AdminId,
                        Device = DevicePlatforms.Web,
                        AccountType = _admin.AccountType,
                        Token = CreateJWTTokenForAdmin(_admin, ExpirationDates, APIConfig),
                        TokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp),
                        RefreshToken = Guid.NewGuid().ToString().Replace("-", ""),
                        RefreshTokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp * 2),
                        IsActive = true,
                        DateCreated = DateTime.Now
                    };

                    // Invalidate existing active admin login transactions (cleanup)
                    var InvalidTransactions = DbContext.UserLoginTransactions
                                                .Where(u => u.UserId == _admin.AdminId &&
                                                            u.Device == DevicePlatforms.Web &&
                                                            u.IsActive == true);
                    foreach (var InvalidTrans in InvalidTransactions)
                    {
                        InvalidTrans.IsActive = false;
                        DbContext.UserLoginTransactions.Update(InvalidTrans);
                    }

                    DbContext.Add(ulT);
                    DbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AdminWebLogin");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
            }
            return ulT;
        }

        public APIResponse ReGenerateTokens(UserLoginTransaction _loginParam)
        {
            APIResponse ApiResp = new APIResponse();
            try
            {
                LogManager.LogDebugObject(_loginParam);

                Admin foundUser = DbContext.Admins.Where(u => u.AdminId == _loginParam.UserId).FirstOrDefault();
                DateTime ExpirationDates = DateTime.Now;

                UserLoginTransaction returnUser = new UserLoginTransaction();

                if (foundUser != null)
                {
                    // Get latest login transaction by date to get latest user token
                    var FoundTransaction = DbContext.UserLoginTransactions.Where(u => u.UserId == _loginParam.UserId && u.IsActive == true).OrderByDescending(u => u.DateCreated).FirstOrDefault();

                    if (FoundTransaction != null && ExpirationDates < FoundTransaction.RefreshTokenExpiration)   //found user && RT not yet expired)
                    {

                        FoundTransaction.IsActive = false;
                        DbContext.Update(FoundTransaction);

                        returnUser = new UserLoginTransaction
                        {
                            UserId = FoundTransaction.UserId,
                            AccountType = FoundTransaction.AccountType,
                            Token = CreateJWTTokenForAdmin(foundUser, ExpirationDates, APIConfig),
                            TokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp),
                            RefreshToken = Guid.NewGuid().ToString().Replace("-", ""),
                            RefreshTokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp * 2),
                            Device = FoundTransaction.Device,
                            IsActive = true,
                            DateCreated = ExpirationDates
                        };

                        ApiResp.Message = "Succès";
                        ApiResp.Status = "Activé!";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.OK;

                        DbContext.Add(returnUser);
                        DbContext.SaveChanges();
                    }
                    else
                    {
                        // Deactivate user login transaction
                        FoundTransaction.IsActive = false;

                        ApiResp.Message = "Succès";
                        ApiResp.Status = "Désactivé!";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.OK;

                        DbContext.Update(FoundTransaction);
                        DbContext.SaveChanges();

                        returnUser = FoundTransaction;
                    }
                }

                ApiResp.Payload = new AdminUserContext
                {
                    AdminInfo = foundUser,
                    Tokens = returnUser
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("ReGenerateTokens");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
            }
            return ApiResp;
        }

        public APIResponse GetAllAdmins(Guid _shop, int _admin)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetAllAdmins: " + _shop);

            try
            {
                if (_admin == 0)
                {
                    IEnumerable<AdminList> admins = null;
                    admins = DbContext.Admins.AsNoTracking().Where(a => a.ShopId == _shop)
                        .Select(i => new AdminList
                        {
                            AdminRecordID = i.AdminRecordID,
                            AdminId = i.AdminId,
                            ShopId = i.ShopId,
                            ImageUrl = i.ImageUrl,
                            FullName = i.FirstName + " " + i.LastName,
                            UserType = i.UserTypeId,
                            IsActive = i.IsEnabled
                        });

                    if (admins.Count() > 0)
                    {
                        aResp = new APIResponse
                        {
                            Message = "Tous les éléments ont été récupérés avec succès.",
                            Status = "Succès!",
                            Payload = admins,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Enregistrement trouvé.",
                        Status = "Succès!",
                        Payload = DbContext.Admins.AsNoTracking().FirstOrDefault(u => u.AdminRecordID == _admin),
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetAllAdmins");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse EditAdminProfile(EditAdminProfile _admin)
        {
            APIResponse ApiResp = new APIResponse();
            LogManager.LogInfo("EditAdminProfile: " + _admin.AdminId);
            LogManager.LogDebugObject(_admin);

            try
            {
                //Check if user exist
                var foundAdmin = DbContext.Admins.AsNoTracking().FirstOrDefault(u => u.AdminId == _admin.AdminId);
                if (foundAdmin != null)
                {
                    foundAdmin.LastEditedDate = DateTime.Now;
                    foundAdmin.LastEditedBy = _admin.AdminId;
                    foundAdmin.DateEnabled = DateTime.Now;
                    foundAdmin.IsEnabled = _admin.IsEnabled;
                    foundAdmin.IsEnabledBy = _admin.AdminId;

                    foundAdmin.FirstName = _admin.FirstName;
                    foundAdmin.LastName = _admin.LastName;
                    if(_admin.Password != null)
                    {
                        foundAdmin.Password = foundAdmin.HashP(_admin.Password.ToString(), APIConfig.TokenKeys.Key);
                    }
                    foundAdmin.UserTypeId = _admin.UserTypeId;
                    foundAdmin.AccountType = _admin.AccountType;
                    foundAdmin.ImageUrl = _admin.ImageUrl;

                    DbContext.Admins.Update(foundAdmin);
                    DbContext.SaveChanges();

                    ApiResp.Message = "Le profil utilisateur a été mis à jour avec succès";
                    ApiResp.Status = "Succès";
                    ApiResp.Payload = foundAdmin;
                    ApiResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    LogManager.LogInfo("Enregistrement non trouvé." + _admin.AdminId);
                    ApiResp.Message = "Enregistrement non trouvé.";
                    ApiResp.Status = "Échec";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("EditAdminProfile");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                ApiResp.Message = "Quelque chose s'est mal passé !";
                ApiResp.Status = "Erreur de serveur interne";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }

            return ApiResp;
        }

        public APIResponse ChangeAdminStatus(ChangeRecordStatus _admin)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ChangeAdminStatus");

            try
            {
                Admin foundAdmin = DbContext.Admins.Where(a => a.AdminRecordID == _admin.RecordId).FirstOrDefault();
                if (foundAdmin != null)
                {
                    DateTime NowDate = DateTime.Now;
                    foundAdmin.LastEditedDate = DateTime.Now;
                    foundAdmin.LastEditedBy = _admin.AdminId;
                    foundAdmin.DateEnabled = DateTime.Now;
                    foundAdmin.IsEnabledBy = _admin.AdminId;
                    foundAdmin.IsEnabled = _admin.IsActive;

                    DbContext.Admins.Update(foundAdmin);
                    DbContext.SaveChanges();
                    aResp = new APIResponse
                    {
                        Message = "Compte mis à jour avec succès",
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
                LogManager.LogInfo("ChangeAdminStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse ForgotPassword(AdminForgotPassword _admin, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("ForgotPassword: " + _admin);

            Admin FoundAdmin = new Admin();

            try
            {
                //for verified admins only
                FoundAdmin = DbContext.Admins.AsNoTracking().FirstOrDefault(u => u.Email == _admin.Email && u.IsVerified == true);
                if (FoundAdmin != null)
                {
                    if (FoundAdmin.IsEnabled == false)
                    {
                        apiResp = new APIResponse
                        {
                            Message = "Votre compte est inactif. Contactez le support pour activer votre compte",
                            Status = "Échec",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };

                        return apiResp;
                    }
                    else
                    {
                        //send new password
                        var sysGenPassword = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);
                        var emailBody = String.Format(APIConfig.MsgConfigs.ForgotPassword, FoundAdmin.FirstName, sysGenPassword);

                        var EmailParam = _conf.MailConfig;
                        EmailParam.To = new List<string>() { FoundAdmin.Email };
                        EmailParam.Subject = "PharmaMoov Administrateur : Mot de passe oublié";
                        EmailParam.Body = emailBody;

                        var sendStatus = SendEmailByEmailAddress(new List<string>() { FoundAdmin.Email }, EmailParam, LogManager);

                        if (sendStatus == 0)
                        {
                            FoundAdmin.Password = FoundAdmin.HashP(sysGenPassword.ToString(), APIConfig.TokenKeys.Key);
                            FoundAdmin.LastEditedBy = FoundAdmin.AdminId;
                            FoundAdmin.LastEditedDate = DateTime.Now;

                            DbContext.Admins.Update(FoundAdmin);
                            DbContext.SaveChanges();

                            apiResp = new APIResponse
                            {
                                Message = "Vous allez recevoir un email contenant votre nouveau mot de passe",
                                Status = "Succès",
                                // Payload = FoundAdmin.AccountType,
                                Payload = FoundAdmin.UserTypeId,
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            LogManager.LogInfo("Sending New Password for Forgot Password");
                            LogManager.LogError("Sending Échec >> " + FoundAdmin.Email);

                            apiResp = new APIResponse
                            {
                                Message = "L'envoi du nouveau mot de passe a échoué",
                                Status = "Échec",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }
                }
                else
                {
                    apiResp.Message = "Emain non trouvé.";
                    apiResp.Status = "Mauvaise demande!";
                    apiResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("ForgotPassword: " + _admin);
                apiResp.Message = ex.Message;
                apiResp.Status = "Mauvaise demande!";
                apiResp.ModelError = GetStackError(ex.InnerException);
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
            }

            return apiResp;
        }

        public APIResponse GetAdminList()
        {
            //GetAdminList for Super Admin
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetAdminList");

            try
            {
                aResp = new APIResponse
                {
                    Message = "Récupéré avec succès.",
                    Status = "Succès!",
                    Payload = DbContext.Admins.AsNoTracking().Where(a => a.UserTypeId == UserTypes.SHOPADMIN).ToList(),
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetAllAdmins");
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
