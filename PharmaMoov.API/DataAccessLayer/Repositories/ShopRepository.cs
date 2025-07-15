using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using System;
using System.Linq;
using PharmaMoov.Models.Shop;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.User;
using PharmaMoov.Models.Orders;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class ShopRepository : APIBaseRepo, IShopRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        IPaymentRepository PaymentRepo { get; }

        public ShopRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, IPaymentRepository _payRepo, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            PaymentRepo = _payRepo;
        }

        public APIResponse RegisterShop(ShopProfile _shop, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("RegisterShop: " + _shop.Email);
            LogManager.LogDebugObject(_shop);

            try
            {
                //Check Duplicate Record 
                Shop FoundShop = DbContext.Shops.Where(u => u.ShopName == _shop.ShopName).FirstOrDefault();
                if (FoundShop == null)
                {
                    Guid ShopGuidID = Guid.NewGuid();
                    DateTime RegistrationDate = DateTime.Now;

                    Shop NewShop = new Shop
                    {
                        ShopId = ShopGuidID,
                        //AccountType = AccountTypes.SHOPADMINUSER,
                        ShopName = _shop.ShopName,
                        ShopLegalName = _shop.ShopLegalName,
                        ShopTags = _shop.ShopTags,
                        Description = _shop.Description,
                        ShopIcon = _shop.ShopIcon,
                       // ShopCategoryId = _shop.ShopCategoryId,
                        OwnerName = _shop.OwnerName,
                        Email = _shop.Email,
                        MobileNumber = _shop.MobileNumber,
                        TelephoneNumber = _shop.TelephoneNumber,
                        Address = _shop.Address,
                        SuiteAddress = _shop.SuiteAddress,
                        HeadquarterAddress = _shop.HeadquarterAddress,
                        PostalCode = _shop.PostalCode,
                        City = _shop.City,
                        Latitude = _shop.Latitude,
                        Longitude = _shop.Longitude,
                        TradeLicenseNo = _shop.TradeLicenseNo,
                        VATNumber = _shop.VATNumber,
                        OwnerFirstName = _shop.OwnerFirstName,
                        OwnerLastName = _shop.OwnerLastName,
                        KbisNumber = _shop.KbisNumber,

                        HasOffers = false,
                        DeliveryMethod = OrderDeliveryType.ALL,
                        PreparationTime = null,
                        DeliveryCommission = _shop.DeliveryCommission,
                        PickupCommission = _shop.PickupCommission,

                        LastEditedBy = ShopGuidID,
                        LastEditedDate = RegistrationDate,
                        CreatedBy = ShopGuidID,
                        CreatedDate = RegistrationDate,
                        IsEnabledBy = ShopGuidID,
                        IsEnabled = false,
                        DateEnabled = RegistrationDate,
                        IsLocked = false,
                        LockedDateTime = null,
                        RegistrationStatus = RegistrationStatus.PENDING,
                    };

                    DbContext.Shops.Add(NewShop);
                    DbContext.SaveChanges();

                    //create shop owner's admin account
                    string initialPassword = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8).ToUpper();
                    var resultStatus = CreateShopAdminAccount(NewShop, initialPassword);
                    if (resultStatus == 0)
                    {
                        //send initial password
                        var EmailParam = _conf.MailConfig;
                        EmailParam.To = new List<string>() { NewShop.Email };
                        EmailParam.Subject = "PharmaMoov : Enregistrement de la boutique";
                        EmailParam.Body = String.Format(APIConfig.MsgConfigs.RegisterShopUser, NewShop.ShopName, initialPassword); ;

                        var sendStatus = SendEmailByEmailAddress(new List<string>() { NewShop.Email }, EmailParam, LogManager);
                        if (sendStatus == 0)
                        {

                            //create default values of shop opening hours
                            AddShopHours(ShopGuidID);

                            apiResp = new APIResponse
                            {
                                Message = "Un compte administrateur a bien été créé via l'adresse email. Suivez les étapes envoyées pour vous connecter et modifier votre mot de passe.",
                                Status = "Succès",
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            LogManager.LogInfo("Sending Verification Code");
                            LogManager.LogError("Sending Échec >> " + NewShop.Email);

                            apiResp = new APIResponse
                            {
                                Message = "L'envoi du mot de passe initial a échoué",
                                Status = "Échec",
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                    }
                }
                else
                {
                    LogManager.LogWarn("Shop:" + FoundShop.ShopName + " already registered!");
                    LogManager.LogDebugObject(FoundShop);
                    apiResp = new APIResponse
                    {
                        Message = "Commerçant déjà enregistré ",
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

        int CreateShopAdminAccount(Shop _admin, string _initpass)
        {
            LogManager.LogInfo("CreateShopAdminAccount: Start >" + _admin);

            int status = 0;

            try
            {
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
                        MobileNumber = _admin.MobileNumber,
                        FirstName = _admin.OwnerName,
                        LastName = _admin.OwnerName,
                        UserTypeId = UserTypes.SHOPADMIN,
                        //AccountType = AccountTypes.SHOPADMINUSER,
                        IsVerified = false,

                        LastEditedBy = AdminGuidID,
                        LastEditedDate = RegistrationDate,
                        CreatedBy = AdminGuidID,
                        CreatedDate = RegistrationDate,
                        IsEnabledBy = AdminGuidID,
                        IsEnabled = true,
                        DateEnabled = RegistrationDate,
                        IsLocked = false,
                        LockedDateTime = null
                    };
                    NewUser.Password = NewUser.HashP(_initpass.ToString(), APIConfig.TokenKeys.Key);

                    DbContext.Admins.Add(NewUser);
                    DbContext.SaveChanges();

                    LogManager.LogInfo("Admin account created successfully.");
                }
                else
                {
                    LogManager.LogInfo("Duplicat d'enregistrement trouvé.");
                    status = 1;
                }
            }
            catch (Exception ex)
            {
                status = 1;
                LogManager.LogError(ex.Message + "\n---------------\n" + ex.StackTrace);
                LogManager.LogError("Échec to create admin account.");
            }

            return status;
        }

        void AddShopHours(Guid shopId)
        {
            LogManager.LogInfo("AddShopHours: Start >" + shopId);

            try
            {
                ShopOpeningHour hasShopHours = DbContext.ShopOpeningHours.Where(u => u.ShopId == shopId).FirstOrDefault();
                if (hasShopHours == null)
                {
                    List<ShopOpeningHour> shopOpeningHours = new List<ShopOpeningHour>();
                    for (int ctr = 0; ctr < 7; ctr++)
                    {
                        ShopOpeningHour openingHour = new ShopOpeningHour
                        {
                            CreatedBy = shopId,
                            CreatedDate = DateTime.Now,
                            DateEnabled = DateTime.Now,
                            IsEnabled = true,
                            IsEnabledBy = shopId,
                            IsLocked = false,
                            LastEditedBy = shopId,
                            LastEditedDate = DateTime.Now,

                            ShopId = shopId,
                            DayOfWeek = (DayOfWeek)ctr,
                            StartTimeAM = "06:00",
                            EndTimeAM = "12:00",
                            StartTimePM = "12:00",
                            EndTimePM = "18:00",
                            StartTimeEvening = "18:00",
                            EndTimeEvening = "23:30"
                        };
                        shopOpeningHours.Add(openingHour);
                    }

                    DbContext.ShopOpeningHours.AddRange(shopOpeningHours);
                    DbContext.SaveChanges();

                    LogManager.LogInfo("Shop opening hours created successfully.");
                }
                else
                {
                    LogManager.LogInfo("Existing shop hours found.");
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError(ex.Message + "\n---------------\n" + ex.StackTrace);
                LogManager.LogError("Échec to shop opening hours.");
            }
        }

        public APIResponse AddShopRequest(ShopRequestDTO _shop)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("AddShopRequest");
            LogManager.LogDebugObject(_shop);

            try
            {
                ShopRequest foundDuplicate = DbContext.ShopRequests.Where(c => c.Email == _shop.Email).FirstOrDefault();
                if (foundDuplicate == null)
                {
                    ShopRequest shopRequest = new ShopRequest()
                    {
                        CompanyName = _shop.CompanyName,
                        FirstName = _shop.FirstName,
                        LastName = _shop.LastName,
                        Email = _shop.Email,
                        MobileNumber = _shop.MobileNumber,
                        Address = _shop.Address,
                        SuiteAddress = _shop.SuiteAddress,
                        City = _shop.City,
                        PostalCode = _shop.PostalCode,
                        KbisNumber = _shop.KbisNumber,
                        KbisDocument = _shop.KbisDocument,
                        RegistrationStatus = RegistrationStatus.PENDING,

                        IsEnabled = false,
                        IsEnabledBy = null,
                        DateEnabled = DateTime.Now,
                        CreatedBy = null,
                        CreatedDate = DateTime.Now,
                        IsLocked = false,
                        LockedDateTime = DateTime.Now,
                        LastEditedBy = null,
                        LastEditedDate = DateTime.Now
                    };

                    DbContext.ShopRequests.Add(shopRequest);
                    DbContext.SaveChanges();

                    aResp.Message = "Votre demande a été envoyée! ";
                    aResp.Status = "Succès";
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Duplicat d'enregistrement trouvé.",
                        Status = "Échec",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AddShopRequest");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse GetShopRequests()
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopRequests");

            try
            {
                aResp = new APIResponse
                {
                    Message = "Tous les articles ont été récupérés avec succès.",
                    Status = "Succès",
                    Payload = DbContext.ShopRequests.AsNoTracking(),
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetShopRequests");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse GetShopProfile(Guid _shop)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopProfile: " + _shop);

            try
            {
                aResp = new APIResponse
                {
                    Message = "Tous les éléments ont été récupérés avec succès.",
                    Status = "Succès!",
                    Payload = DbContext.Shops.AsNoTracking().FirstOrDefault(x => x.ShopId == _shop),
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetShopProfile");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetShopOpeningHours(Guid _shop)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopOpeningHours: " + _shop);

            try
            {
                IEnumerable<ShopOpeningHourDTO> openingHours = null;
                openingHours = DbContext.ShopOpeningHours.AsNoTracking().Where(o => o.ShopId == _shop)
                                            .Select(p => new ShopOpeningHourDTO
                                            {
                                                ShopOpeningHourID = p.ShopOpeningHourID,
                                                ShopId = p.ShopId,
                                                DayOfWeek = p.DayOfWeek,
                                                StartTimeAM = TimeSpan.Parse(p.StartTimeAM),
                                                StartTimePM = TimeSpan.Parse(p.StartTimePM),
                                                EndTimeAM = TimeSpan.Parse(p.EndTimeAM),
                                                EndTimePM = TimeSpan.Parse(p.EndTimePM),
                                                StartTimeEvening = TimeSpan.Parse(p.StartTimeEvening),
                                                EndTimeEvening = TimeSpan.Parse(p.EndTimeEvening),
                                                NowOpen = p.IsEnabled ?? false
                                            }).ToList();

                if (openingHours != null)
                {
                    aResp.Message = "Tous les éléments ont été récupérés avec succès.";
                    aResp.Status = "Succès";
                    aResp.Payload = openingHours;
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
                LogManager.LogInfo("GetShopOpeningHours: " + _shop);
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse EditShopOpeningHours(ShopHourList _shop)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("EditShopOpeningHours");
            LogManager.LogDebugObject(_shop);

            try
            {
                DateTime NowDate = DateTime.Now;
                foreach (var item in _shop.OpeningHours)
                {
                    ShopOpeningHour openingHour = DbContext.ShopOpeningHours.FirstOrDefault(i => i.ShopOpeningHourID == item.ShopOpeningHourID);

                    openingHour.LastEditedBy = _shop.AdminId;
                    openingHour.LastEditedDate = NowDate;
                    openingHour.IsEnabledBy = _shop.AdminId;
                    openingHour.DateEnabled = NowDate;

                    openingHour.StartTimeAM = item.StartTimeAM.ToString();
                    openingHour.EndTimeAM = item.EndTimeAM.ToString();
                    openingHour.StartTimePM = item.StartTimePM.ToString();
                    openingHour.EndTimePM = item.EndTimePM.ToString();
                    openingHour.StartTimeEvening = item.StartTimeEvening.ToString();
                    openingHour.EndTimeEvening = item.EndTimeEvening.ToString();
                    openingHour.IsEnabled = item.NowOpen;

                    DbContext.ShopOpeningHours.Update(openingHour);
                    DbContext.SaveChanges();
                }

                aResp = new APIResponse
                {
                    Message = "Les horaires d'ouvertures ont été mises à jour avec succès",
                    Status = "Succès!",
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("EditShopOpeningHours");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse EditShopProfile(Shop _shop)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("EditShopProfile");
            LogManager.LogDebugObject(_shop);

            try
            {
                Shop updateShop = DbContext.Shops.Where(a => a.ShopRecordID == _shop.ShopRecordID).FirstOrDefault();
                if (updateShop != null)
                {
                    // check duplicate record 
                    var foundDuplicate = DbContext.Shops.AsNoTracking().Where(c => c.ShopName == _shop.ShopName && c.ShopCategoryId == updateShop.ShopCategoryId);
                    if (foundDuplicate.FirstOrDefault(d => d.ShopRecordID != _shop.ShopRecordID) == null)
                    {
                        DateTime NowDate = DateTime.Now;
                        updateShop.IsEnabled = updateShop.IsEnabled;
                        updateShop.IsEnabledBy = updateShop.IsEnabledBy;
                        updateShop.DateEnabled = updateShop.DateEnabled;
                        updateShop.CreatedBy = updateShop.CreatedBy;
                        updateShop.CreatedDate = updateShop.CreatedDate;
                        updateShop.IsLocked = updateShop.IsLocked;
                        updateShop.LockedDateTime = updateShop.LockedDateTime;

                        updateShop.LastEditedBy = _shop.LastEditedBy;
                        updateShop.LastEditedDate = _shop.LastEditedDate;

                        //updateShop.ShopCategoryId = _shop.ShopCategoryId;
                        updateShop.ShopName = _shop.ShopName;
                        //updateShop.ShopLegalName = _shop.ShopLegalName;
                        //updateShop.ShopTags = _shop.ShopTags;
                        //updateShop.OwnerName = _shop.OwnerName;
                        updateShop.Description = _shop.Description;
                        updateShop.ShopIcon = _shop.ShopIcon;
                        updateShop.Email = _shop.Email;
                        updateShop.Website = _shop.Website;
                        updateShop.TelephoneNumber = _shop.TelephoneNumber;
                        updateShop.MobileNumber = _shop.MobileNumber;
                        //updateShop.TradeLicenseNo = _shop.TradeLicenseNo;
                        //updateShop.VATNumber = _shop.VATNumber;
                        updateShop.Address = _shop.Address;
                        updateShop.SuiteAddress = _shop.SuiteAddress;
                        //updateShop.HeadquarterAddress = _shop.HeadquarterAddress;
                        updateShop.PostalCode = _shop.PostalCode;
                        updateShop.City = _shop.City;
                        updateShop.Latitude = _shop.Latitude;
                        updateShop.Longitude = _shop.Longitude;
                        updateShop.ShopBanner = _shop.ShopBanner;
                        updateShop.OwnerFirstName = _shop.OwnerFirstName;
                        updateShop.OwnerLastName = _shop.OwnerLastName;
                        updateShop.KbisNumber = _shop.KbisNumber;
                        updateShop.IsPopularPharmacy = _shop.IsPopularPharmacy;

                        DbContext.Shops.Update(updateShop);
                        DbContext.SaveChanges();
                        aResp = new APIResponse
                        {
                            Message = "Le profil commerçant a été mis à jour avec succès",
                            Status = "Succès!",
                            Payload = updateShop,
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

        public APIResponse SetShopConfigurations(ShopConfigs _shop)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("SetShopConfigurations");
            LogManager.LogDebugObject(_shop);

            try
            {
                Shop updateShop = DbContext.Shops.Where(a => a.ShopId == _shop.ShopId).FirstOrDefault();
                if (updateShop != null)
                {
                    DateTime NowDate = DateTime.Now;
                    updateShop.LastEditedBy = _shop.AdminId;
                    updateShop.LastEditedDate = NowDate;

                    if (_shop.PreparationTime != null)
                    {
                        updateShop.PreparationTime = _shop.PreparationTime;
                        aResp.Message = "Le temps de préparation a été mis à jour avec succès";
                    }

                    if (_shop.DeliveryMethod != null)
                    {
                        updateShop.DeliveryMethod = _shop.DeliveryMethod ?? 0;
                        aResp.Message = "La méthode de livraison a été mis à jour avec succès";
                    }

                    DbContext.Shops.Update(updateShop);
                    DbContext.SaveChanges();

                    aResp.Status = "Succès";
                    aResp.Payload = updateShop;
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
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
                LogManager.LogInfo("SetShopConfigurations");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse AddShopDocument(string _auth, ShopDocumentDTO _shop)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("AddShopDocument: " + _auth);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("AddShopDocument: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    ShopDocument foundDuplicate = DbContext.ShopDocuments.Where(c => c.FileName == _shop.FileName).FirstOrDefault();
                    if (foundDuplicate == null)
                    {
                        ShopDocument shopDocument = new ShopDocument()
                        {
                            ShopId = _shop.ShopId,
                            FileName = _shop.FileName,
                            FileType = _shop.FileType,
                            FileSize = _shop.FileSize,
                            FilePath = _shop.FilePath,

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

                        DbContext.ShopDocuments.Add(shopDocument);
                        DbContext.SaveChanges();

                        aResp.Message = "Les documents commerçants ont été soumis avec succès";
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Duplicat d'enregistrement trouvé.",
                            Status = "Échec",
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
                LogManager.LogInfo("AddShopDocument");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse GetShopDocuments(string _auth, Guid _shop)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("GetShopDocuments: " + _auth);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("GetShopDocuments: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    IEnumerable<ShopDocument> documents = null;

                    if (_shop == Guid.Empty)
                    {
                        documents = DbContext.ShopDocuments.AsNoTracking().Where(x => x.IsEnabled == true).ToList();
                    }
                    else
                    {
                        documents = DbContext.ShopDocuments.AsNoTracking().Where(x => x.ShopId == _shop && x.IsEnabled == true).ToList();
                    }

                    aResp = new APIResponse
                    {
                        Message = "Tous les éléments ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = documents,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetShopDocuments");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse DeleteShopDocuments(string _auth, int _shop)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("DeleteShopDocuments: " + _auth);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("DeleteShopDocuments: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    ShopDocument foundDoc = DbContext.ShopDocuments.FirstOrDefault(d => d.ShopDocumentID == _shop);
                    if (foundDoc != null)
                    {
                        foundDoc.IsEnabled = false;
                        foundDoc.LastEditedBy = IsUserLoggedIn.UserId;
                        foundDoc.LastEditedDate = DateTime.Now;

                        DbContext.ShopDocuments.Update(foundDoc);
                        DbContext.SaveChanges();

                        aResp = new APIResponse
                        {
                            Message = "Les documents commerçants ont été supprimés avec succès",
                            Status = "Succès!",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Aucun enregistrement trouvé.",
                            Status = "Échec!",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("DeleteShopDocuments");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse GetShopList()
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopList");

            try
            {
                IEnumerable<ShopList> shopList = null;
                shopList = DbContext.Shops.AsNoTracking()
                                //.Join(DbContext.ShopCategories, s => s.ShopCategoryId, c => c.ShopCategoryID, (s, c) => new { s, c })
                                .Where(w => w.RegistrationStatus == RegistrationStatus.APPROVE)
                                .Select(p => new ShopList
                                {
                                    ShopRecordID = p.ShopRecordID,
                                    ShopId = p.ShopId,
                                    Name = p.ShopName,
                                    Email = p.Email,
                                   // Category = p.Name,
                                    Owner = p.OwnerFirstName + " " + p.OwnerLastName,
                                    DateCreated = p.CreatedDate.GetValueOrDefault(),
                                    Status = p.IsEnabled ?? false,
                                    RegistrationStatus = p.RegistrationStatus,
                                    IsPopularPharmacy = p.IsPopularPharmacy,
                                    ContactNumber = p.TelephoneNumber,
                                    ShopIcon = p.ShopIcon
                                }).ToList();

                if (shopList != null)
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
                LogManager.LogInfo("GetShopList:");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse ChangeShopStatus(string _auth, ChangeRecordStatus _shop)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ChangeShopStatus");
            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    Shop foundShop = DbContext.Shops.Where(a => a.ShopRecordID == _shop.RecordId).FirstOrDefault();
                    if (foundShop != null)
                    {
                        DateTime NowDate = DateTime.Now;
                        foundShop.LastEditedDate = DateTime.Now;
                        foundShop.LastEditedBy = _shop.AdminId;
                        foundShop.DateEnabled = DateTime.Now;
                        foundShop.IsEnabledBy = _shop.AdminId;
                        foundShop.IsEnabled = _shop.IsActive;

                        DbContext.Shops.Update(foundShop);
                        DbContext.SaveChanges();
                        aResp = new APIResponse
                        {
                            Message = "La boutique a été mise à jour avec succès.",
                            Status = "Succès!",
                            Payload = foundShop,
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
                LogManager.LogInfo("ChangeShopStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse ShopCommisionInvoice(ShopCommissionDateRange _range, string _auth)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("ShopCommisionInvoice : " + _auth);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("ShopCommisionInvoice : " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    DateTime dtFrom = (DateTime)_range.dateFrom;
                    DateTime dtTO = (DateTime)_range.dateTo;
                    List<ShopComissionDTO> comissionListing = new List<ShopComissionDTO>();
                    // get all shops
                    List<Shop> ShopList = DbContext.Shops.ToList();
                    foreach (Shop sShop in ShopList)
                    {
                        // check if the shop
                        // has an oder in the given date range
                        List<Order> ShopOrder = DbContext.Orders.Where(so => so.ShopId == sShop.ShopId
                                                                   && so.OrderProgressStatus == OrderProgressStatus.COMPLETED
                                                                   && (so.CreatedDate > dtFrom && so.CreatedDate < dtTO)).ToList();
                        if (ShopOrder.Count > 0)
                        {
                            //decimal PharmaMoovSetCommission = DbContext.Shops.FirstOrDefault(comm => comm.ShopRecordID == sShop.ShopRecordID).DeliveryCommission;
                            // -- replace with fixed 15%
                            //decimal PharmaMoovSetCommission = (sShop.DeliveryCommission + sShop.PickupCommission) / 100;
                            decimal PharmaMoovSetCommission = 0.15m;
                            decimal ShopOrdersTotalAmount = ShopOrder.Sum(so => so.OrderGrossAmount);
                            ShopComissionDTO comissionOnOneShop = new ShopComissionDTO
                            {
                                ShopRecordId = sShop.ShopRecordID,
                                ShopName = sShop.ShopName,
                                TotalOrder = ShopOrder.Count,
                                TotalSales = ShopOrdersTotalAmount,
                                PharmaMoovCommission = PharmaMoovSetCommission,
                                PharmaMoovAmount = ShopOrdersTotalAmount * PharmaMoovSetCommission
                            };
                            comissionOnOneShop.ShopAmount = ShopOrdersTotalAmount - comissionOnOneShop.PharmaMoovAmount;
                            // add to the listing
                            comissionListing.Add(comissionOnOneShop);
                        }
                        else
                        {
                            // move to the next shop
                        }
                    }

                    aResp = new APIResponse
                    {
                        Message = "Succès",
                        Status = "Succès",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = comissionListing
                    };
                }
                else
                {
                    aResp.Message = "Échec";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                    aResp.Payload = new List<ShopComissionDTO>();
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("ShopCommisionInvoice");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse ComissionInvoiceView(SingleShopComissionInvoiceParameters _sParams, string _auth)
        {

            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("ComissionInvoiceView : " + _auth);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("ShopCommisionInvoice : " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);
                    Shop ShopInfo = DbContext.Shops.FirstOrDefault(s => s.ShopRecordID == _sParams.ShopRecordId);
                    List<Order> ShopOrder = DbContext.Orders.Where(s => s.ShopId == ShopInfo.ShopId
                                                            && s.OrderProgressStatus == OrderProgressStatus.COMPLETED
                                                            && (s.CreatedDate > _sParams.DateFrom && s.CreatedDate < _sParams.DateTo)).ToList();
                    List<SingleShopComissionInvoiceTransaction> ShopTransactionss = new List<SingleShopComissionInvoiceTransaction>();
                    ShopTransactionss.Add(BuildComissionsTransactions(ShopInfo, ShopOrder.Where(so => so.OrderDeliveryType == OrderDeliveryType.FORDELIVERY).ToList()));
                    ShopTransactionss.Add(BuildComissionsTransactions(ShopInfo, ShopOrder.Where(so => so.OrderDeliveryType == OrderDeliveryType.FORPICKUP).ToList()));

                    SingleShopComissionInvoice NewShopCommssionInvoice = new SingleShopComissionInvoice
                    {
                        InvoiceDate = DateTime.Now,
                        ShopRecordId = ShopInfo.ShopRecordID,
                        ShopName = ShopInfo.ShopName,
                        ShopAddress = ShopInfo.Address,
                        InvoiceDateRangeFrom = _sParams.DateFrom,
                        InvoiceDateRangeTo = _sParams.DateTo,
                        InvoiceNumber = DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Year.ToString(),
                        Transactions = ShopTransactionss
                    };

                    NewShopCommssionInvoice.TotalAmountWithTax = NewShopCommssionInvoice.Transactions.Sum(s => s.TotalAmountWithtax);
                    NewShopCommssionInvoice.VatAmount = NewShopCommssionInvoice.Transactions.Sum(s => s.GeneralTaxAmount);
                    NewShopCommssionInvoice.NetAmount = NewShopCommssionInvoice.Transactions.Sum(s => s.CommissionAmount);

                    aResp = new APIResponse
                    {
                        Message = "Succès",
                        Status = "Succès",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = NewShopCommssionInvoice
                    };
                }
                else
                {
                    aResp.Message = "Échec";
                    aResp.Status = "Échec";
                    aResp.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                    aResp.Payload = new List<ShopComissionDTO>();
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("ShopCommisionInvoice");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;

        }

        SingleShopComissionInvoiceTransaction BuildComissionsTransactions(Shop shopInfo, List<Order> ordersList)
        {
            SingleShopComissionInvoiceTransaction DeliveryTrans = new SingleShopComissionInvoiceTransaction();

            if (ordersList.Count > 0)
            {
                decimal PharmaMoovSetCommission = 0.15m;

                decimal CommissionRate = shopInfo.DeliveryCommission + shopInfo.PickupCommission;
                decimal TotalAmountOfSale = ordersList.Sum(tao => tao.OrderGrossAmount);
                decimal FixTax = 20m;

                DeliveryTrans = new SingleShopComissionInvoiceTransaction
                {
                    OrderType = ordersList.FirstOrDefault().OrderDeliveryType,
                    TotalNumberOfOrder = ordersList.Count(),
                    TotalAmountOfSales = TotalAmountOfSale,
                    CommissionRate = Convert.ToInt32(PharmaMoovSetCommission * 100),
                    //CommissionAmount = TotalAmountOfSale * (CommissionRate / 100),
                    CommissionAmount = TotalAmountOfSale * PharmaMoovSetCommission,
                    FixTax = Convert.ToInt32(FixTax),
                    GeneralTaxAmount = TotalAmountOfSale * (FixTax / 100)
                };
                DeliveryTrans.TotalAmountWithtax = DeliveryTrans.TotalAmountWithtax + TotalAmountOfSale;

                return DeliveryTrans;
            }
            else
            {
                return DeliveryTrans;
            }

        }

        public APIResponse ChangeRegistrationStatus(string _auth, ChangeRegistrationStatus _shop)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ChangeRegistrationStatus");
            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    Shop foundRequest = DbContext.Shops.Where(a => a.ShopRecordID == _shop.ShopRecordID).FirstOrDefault();

                    if (foundRequest != null)
                    {
                        DateTime NowDate = DateTime.Now;
                        foundRequest.LastEditedDate = DateTime.Now;
                        foundRequest.LastEditedBy = _shop.AdminId;
                        foundRequest.DateEnabled = DateTime.Now;
                        foundRequest.IsEnabledBy = _shop.AdminId;
                        foundRequest.RegistrationStatus = _shop.RegistrationStatus;

                        if (_shop.RegistrationStatus == RegistrationStatus.APPROVE)
                        {
                            User NewUser = new User();
                            NewUser.FirstName = foundRequest.OwnerFirstName;
                            NewUser.LastName = foundRequest.OwnerLastName;
                            NewUser.Email = foundRequest.Email;
                            //APIResponse MangoPayUserCreation = PaymentRepo.CreateUserNaturalUserInMangoPay(NewUser);
                            APIResponse MangoPayUserCreation = PaymentRepo.CreateUserLegalUserInMangoPay(foundRequest);

                            if(MangoPayUserCreation.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                DbContext.Shops.Update(foundRequest);
                                DbContext.SaveChanges();
                                aResp = new APIResponse
                                {
                                    Message = "Le statut d'enregistrement a été mis à jour avec succès.",
                                    Status = "Succès!",
                                    Payload = foundRequest,
                                    StatusCode = System.Net.HttpStatusCode.OK
                                };
                            }
                            else
                            {
                                aResp = new APIResponse
                                {
                                    Message = "Pas de compte Mangopay.",
                                    Status = "Échec!",
                                    StatusCode = System.Net.HttpStatusCode.NotFound
                                };
                            }
                        }
                        else
                        {
                            DbContext.Shops.Update(foundRequest);
                            DbContext.SaveChanges();
                            aResp = new APIResponse
                            {
                                Message = "Le statut d'enregistrement a été mis à jour avec succès.",
                                Status = "Succès!",
                                Payload = foundRequest,
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
                LogManager.LogInfo("ChangeRegistrationStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetShopRegistrationRequest(int _shopId)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopRegistrationRequest: " + _shopId);

            try
            {
                aResp = new APIResponse
                {
                    Message = "Tous les éléments ont été récupérés avec succès.",
                    Status = "Succès!",
                    Payload = DbContext.ShopRequests.AsNoTracking().FirstOrDefault(x => x.ShopRequestID == _shopId),
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetShopRegistrationRequest");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetRequestList()
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetRequestList");

            try
            {
                IEnumerable<ShopList> shopList = null;
                shopList = DbContext.Shops.AsNoTracking()
                                //.Join(DbContext.ShopCategories, s => s.ShopCategoryId, c => c.ShopCategoryID, (s, c) => new { s, c })
                                .Where(w => w.RegistrationStatus != RegistrationStatus.APPROVE)
                                .Select(p => new ShopList
                                {
                                    ShopRecordID = p.ShopRecordID,
                                    ShopId = p.ShopId,
                                    Name = p.ShopName,
                                    Email = p.Email,
                                    //Category = p.c.Name,
                                    ContactNumber = p.TelephoneNumber,
                                    Owner = p.OwnerFirstName + " " + p.OwnerLastName,
                                    DateCreated = p.CreatedDate.GetValueOrDefault(),
                                    Status = p.IsEnabled ?? false,
                                    RegistrationStatus = p.RegistrationStatus
                                }).ToList();

                if (shopList != null)
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
                LogManager.LogInfo("GetRequestList:");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse ChangePopularStatus(string _auth, ChangeRecordStatus _shop)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ChangePopularStatus");
            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    Shop foundShop = DbContext.Shops.Where(a => a.ShopRecordID == _shop.RecordId).FirstOrDefault();
                    if (foundShop != null)
                    {
                        foundShop.LastEditedDate = DateTime.Now;
                        foundShop.LastEditedBy = _shop.AdminId;
                        foundShop.IsPopularPharmacy = _shop.IsActive;

                        DbContext.Shops.Update(foundShop);
                        DbContext.SaveChanges();
                        aResp = new APIResponse
                        {
                            Message = "La boutique a été mise à jour avec succès.",
                            Status = "Succès!",
                            Payload = foundShop,
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
                LogManager.LogInfo("ChangePopularStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetShopListForAutocomplete(string _searchKey)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetShopListForAutocomplete");

            try
            {
                var shopList = DbContext.Shops.AsNoTracking()
                                .Where(s => s.RegistrationStatus == RegistrationStatus.APPROVE && s.IsEnabled == true && s.ShopName.ToLower().Contains(_searchKey.ToLower()))
                                .Select(p => new ShopList
                                {
                                    ShopRecordID = p.ShopRecordID,
                                    ShopId = p.ShopId,
                                    Name = p.ShopName,
                                    Latitude = p.Latitude,
                                    Longitude = p.Longitude
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
                LogManager.LogInfo("GetShopListForAutocomplete:");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetPharmacyOwner(int shopRecordId)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetPharmacyOwner: " + shopRecordId);
            try
            {
                aResp = new APIResponse
                {
                    Message = "Tous les éléments ont été récupérés avec succès.",
                    Status = "Succès!",
                    Payload = DbContext.Shops.AsNoTracking().FirstOrDefault(x => x.ShopRecordID == shopRecordId),
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetPharmacyOwner");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse UpdatePharmacyOwner(PharmacyOwner model)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("UpdatePharmacyOwner");
            LogManager.LogDebugObject(model);

            try
            {
                Shop updateShop = DbContext.Shops.Where(a => a.ShopRecordID == model.ShopRecordID).FirstOrDefault();
                if (updateShop != null)
                {

                    updateShop.LastEditedBy = model.AdminID;
                    updateShop.LastEditedDate = DateTime.Now;
                    updateShop.ShopIcon = model.ShopIcon;
                    updateShop.Email = model.Email;             
                    updateShop.OwnerFirstName = model.OwnerFirstName;
                    updateShop.OwnerLastName = model.OwnerLastName;
                    updateShop.MobileNumber = model.MobileNumber;
                    updateShop.IsEnabled = model.IsEnabled;

                    DbContext.Shops.Update(updateShop);
                    DbContext.SaveChanges();
                    aResp = new APIResponse
                    {
                        Message = "Le profil commerçant a été mis à jour avec succès",
                        Status = "Succès!",
                        Payload = updateShop,
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
                LogManager.LogInfo("UpdatePharmacyOwner");
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
