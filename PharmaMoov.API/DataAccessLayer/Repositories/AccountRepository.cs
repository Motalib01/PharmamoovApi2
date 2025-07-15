using PharmaMoov.Models;
using PharmaMoov.Models.User;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using PharmaMoov.Models.Admin;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class AccountRepository : APIBaseRepo, IAccountRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private readonly LocalizationService localization;

        public AccountRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, LocalizationService _localization)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            localization = _localization;
        }

        public APIResponse GetUserProfile(string _auth)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("GetUserProfile: " + _auth);

            try
            {
                User FoundUser = new User();
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("GetUserProfile: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    FoundUser = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserId == IsUserLoggedIn.UserId);
                    if (FoundUser != null)
                    {
                        //Remove password and verification code before returning
                        FoundUser.Password = string.Empty;
                        FoundUser.RegistrationCode = string.Empty;

                        aResp.Message = "Enregistrement trouvé.";
                        aResp.Status = "Succès";
                        aResp.Payload = FoundUser;
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        aResp.Message = "Enregistrement non trouvé.";
                        aResp.Status = "Échec";
                        aResp.StatusCode = System.Net.HttpStatusCode.NotFound;
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
                LogManager.LogInfo("GetUserProfile: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse EditUserProfile(UserProfile _user)
        {
            APIResponse ApiResp = new APIResponse();
            LogManager.LogInfo("User Update Profile: " + _user.UserId + "Platform: " + _user.Device);
            LogManager.LogDebugObject(_user);

            try
            {
                //Check if user exist
                var foundUser = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserId == _user.UserId);
                if (foundUser != null)
                {
                    var sendSms = false;

                    // if username is changed
                    if (foundUser.Username != _user.Username && _user.Username != string.Empty)
                    {
                        // Check if email already belongs to another user
                        var foundDuplicateUsername = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.Username == _user.Username);
                        if (foundDuplicateUsername != null)
                        {
                            LogManager.LogError("EditUserProfile >> Le nom d'utilisateur est déjà pris " + _user.Username);
                            ApiResp.Message = "Le nom d'utilisateur est déjà pris";
                            ApiResp.Status = "Échec";
                            ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;

                            return ApiResp;
                        }
                    }

                    // if email is changed
                    if (foundUser.Email != _user.Email && _user.Email != string.Empty)
                    {
                        // Check if email already belongs to another user
                        var foundDuplicateEmail = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.Email == _user.Email);
                        if (foundDuplicateEmail != null)
                        {
                            LogManager.LogError("EditUserProfile >> L'adresse email est déjà utilisée" + _user.Email);
                            ApiResp.Message = "L'adresse email est déjà utilisée";
                            ApiResp.Status = "Échec";
                            ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;

                            return ApiResp;
                        }
                    }

                    // if phone number is changed
                    if (foundUser.MobileNumber != _user.MobileNumber && _user.MobileNumber != string.Empty)
                    {
                        // Check if phone number already belongs to another user
                        var foundDuplicatePhoneNumber = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.MobileNumber == _user.MobileNumber);
                        if (foundDuplicatePhoneNumber != null)
                        {
                            LogManager.LogError("EditUserProfile >> Le numéro de téléphone est déjà utilisé" + _user.MobileNumber);
                            ApiResp.Message = "Le numéro de téléphone est déjà utilisé";
                            ApiResp.Status = "Échec";
                            ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;

                            return ApiResp;
                        }

                        sendSms = true;
                        //foundUser.IsLocked = true;
                        //foundUser.IsEnabled = false;
                    }

                    // This phone number should be verified from the mobile app first
                    foundUser.MobileNumber = _user.MobileNumber != null && _user.MobileNumber.Trim() != string.Empty ? _user.MobileNumber : foundUser.MobileNumber;
                    foundUser.Email = _user.Email != null && _user.Email.Trim() != string.Empty ? _user.Email : foundUser.Email;
                    foundUser.FirstName = _user.FirstName != null && _user.FirstName.Trim() != string.Empty ? _user.FirstName : foundUser.FirstName;
                    foundUser.LastName = _user.LastName != null && _user.LastName.Trim() != string.Empty ? _user.LastName : foundUser.LastName;
                    foundUser.ImageUrl = _user.ImageUrl;
                    foundUser.Username = _user.Username != null && _user.Username.Trim() != string.Empty ? _user.Username : foundUser.Username;
                    foundUser.HealthNumber = _user.HealthNumber != null && _user.HealthNumber.Trim() != string.Empty ? _user.HealthNumber : foundUser.HealthNumber;
                    foundUser.ProfessionalID = _user.ProfessionalID != null && _user.ProfessionalID.Trim() != string.Empty ? _user.ProfessionalID : foundUser.ProfessionalID;

                    foundUser.LastEditedBy = _user.UserId;
                    foundUser.LastEditedDate = DateTime.Now;

                    DbContext.Update(foundUser);
                    DbContext.SaveChanges();

                    // if phone number is updated
                    if (sendSms)
                    {
                        // Resend verification code
                    }

                    ApiResp.Message = "Profil mis à jour avec succès";
                    ApiResp.Status = "Succès";
                    ApiResp.Payload = foundUser;
                    ApiResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    LogManager.LogInfo("Enregistrement non trouvé." + _user.UserId);
                    ApiResp.Message = "Enregistrement non trouvé.";
                    ApiResp.Status = "Échec";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("EditUserProfile");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                ApiResp.Message = "Quelque chose s'est mal passé !";
                ApiResp.Status = "Erreur de serveur interne";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }

            return ApiResp;
        }

        //public APIResponse ChangeUserPassword(UserChangePassword _user)
        //{
        //    APIResponse apiResp = new APIResponse();
        //    UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.UserId == _user.UserId && ult.IsActive == true);
        //    LogManager.LogInfo("ChangeUserPassword: ");

        //    try
        //    {
        //        if (IsUserLoggedIn != null)
        //        {
        //            LogManager.LogInfo("ChangeUserPassword: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);
        //            int hasError = 0;

        //            User FoundUser = DbContext.Users.FirstOrDefault(u => u.UserId == IsUserLoggedIn.UserId);
        //            string HashedCurrPass = FoundUser.HashP(_user.CurrentPassword.ToString(), APIConfig.TokenKeys.Key);
        //            string HashedNewPass = FoundUser.HashP(_user.NewPassword.ToString(), APIConfig.TokenKeys.Key);

        //            if (FoundUser.Password == HashedNewPass) 
        //            {
        //                LogManager.LogInfo("New password is same as old password! Terminating Change password process!");
        //                return new APIResponse {
        //                    Message = "Le nouveau mot de passe ne peut pas être le même que l'ancien mot de passe!",
        //                    StatusCode = System.Net.HttpStatusCode.BadRequest
        //                };
        //            }

        //            if (FoundUser.Password == HashedCurrPass)
        //            {
        //                FoundUser.Password = HashedNewPass;
        //                FoundUser.LastEditedBy = FoundUser.UserId;
        //                FoundUser.LastEditedDate = DateTime.Now;

        //                DbContext.Update(FoundUser);
        //                DbContext.SaveChanges();
        //            }
        //            else
        //            {
        //                hasError = 1;
        //            }


        //            if (hasError == 0)
        //            {
        //                apiResp = new APIResponse
        //                {
        //                    Message = "Mot de passe mis à jour avec succès",
        //                    Status = "Succès!",
        //                    StatusCode = System.Net.HttpStatusCode.OK
        //                };
        //            }
        //            else
        //            {
        //                LogManager.LogInfo("Le mot de passe actuel est invalide ! L'utilisateur doit changer son mot de passe: " + _user.UserId + " Password: " + _user.CurrentPassword);
        //                apiResp = new APIResponse
        //                {
        //                    Message = "Mot de passe actuel incorrect ",
        //                    Status = "Échec",
        //                    StatusCode = System.Net.HttpStatusCode.BadRequest
        //                };
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.LogInfo("ChangeUserPassword: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);
        //        LogManager.LogError(ex.InnerException.Message);
        //        LogManager.LogError(ex.StackTrace.ToString());
        //        apiResp = new APIResponse
        //        {
        //            Message = ex.Message,
        //            Status = "Mauvaise demande",
        //            ModelError = GetStackError(ex),
        //            StatusCode = System.Net.HttpStatusCode.BadRequest
        //        };
        //    }
        //    return apiResp;
        //}

        public APIResponse ChangeUserPassword(UserChangePassword _user)
        {
            APIResponse apiResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.UserId == _user.UserId && ult.IsActive == true);
            LogManager.LogInfo("ChangeUserPassword: ");

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("ChangeUserPassword: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);
                    int hasError = 0;

                    //if (IsUserLoggedIn.AccountType == AccountTypes.APPUSER || IsUserLoggedIn.AccountType == AccountTypes.COURIER || IsUserLoggedIn.AccountType == AccountTypes.HEALTHPROFESSIONAL)
                    if(_user.UserTypeId != 0)
                    {
                        //Super Admin and all Admins Change Password
                        Admin FoundUser = DbContext.Admins.FirstOrDefault(u => u.AdminId == IsUserLoggedIn.UserId);
                        string HashedCurrPass = FoundUser.HashP(_user.CurrentPassword.ToString(), APIConfig.TokenKeys.Key);
                        string HashedNewPass = FoundUser.HashP(_user.NewPassword.ToString(), APIConfig.TokenKeys.Key);
                       
                        if(FoundUser.Password == HashedNewPass)
                        {
                            apiResp = new APIResponse
                            {
                                Message = "Le mot de passe doit être différent du précédent",
                                Status = "Échec",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                            return apiResp;
                        }

                        if (FoundUser.Password == HashedCurrPass)
                        {
                            FoundUser.Password = HashedNewPass;
                            FoundUser.LastEditedBy = FoundUser.AdminId;
                            FoundUser.LastEditedDate = DateTime.Now;
                            FoundUser.IsVerified = true;

                            DbContext.Update(FoundUser);
                            DbContext.SaveChanges();
                        }
                        else
                        {
                            hasError = 1;
                        }
                    }
                    else
                    {
                        //Customer, Courier and Health Prof (Users) Change Password
                        User FoundUser = DbContext.Users.FirstOrDefault(u => u.UserId == IsUserLoggedIn.UserId);
                        string HashedCurrPass = FoundUser.HashP(_user.CurrentPassword.ToString(), APIConfig.TokenKeys.Key);
                        string HashedNewPass = FoundUser.HashP(_user.NewPassword.ToString(), APIConfig.TokenKeys.Key);

                        if (FoundUser.Password == HashedNewPass)
                        {
                            apiResp = new APIResponse
                            {
                                Message = "Le mot de passe doit être différent du précédent",
                                Status = "Échec",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                            return apiResp;
                        }

                        if (FoundUser.Password == HashedCurrPass)
                        {
                            FoundUser.Password = HashedNewPass;
                            FoundUser.LastEditedBy = FoundUser.UserId;
                            FoundUser.LastEditedDate = DateTime.Now;

                            DbContext.Update(FoundUser);
                            DbContext.SaveChanges();
                        }
                        else
                        {
                            hasError = 1;
                        }
                    }

                    if (hasError == 0)
                    {
                        apiResp = new APIResponse
                        {
                            Message = "Mot de passe mis à jour avec succès",
                            Status = "Succès!",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        LogManager.LogInfo("Le mot de passe actuel est invalide ! L'utilisateur doit changer son mot de passe: " + _user.UserId + " Password: " + _user.CurrentPassword);
                        apiResp = new APIResponse
                        {
                            Message = "Mot de passe actuel incorrect ",
                            Status = "Échec",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("ChangeUserPassword: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace.ToString());
                apiResp = new APIResponse
                {
                    Message = ex.Message,
                    Status = "Mauvaise demande",
                    ModelError = GetStackError(ex),
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
            return apiResp;
        }

        public APIResponse GetDeliveryAddressBook(string _auth, int _address)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("GetDeliveryAddressBook: " + _auth);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("GetDeliveryAddressBook: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    if (_address == 0) //all per user
                    {
                        IEnumerable<UserAddresses> addresses = null;
                        addresses = DbContext.UserAddresses.AsNoTracking().Where(d => d.IsEnabled == true & d.UserId == IsUserLoggedIn.UserId).ToList();

                        List<UserAddressBook> addressBook = new List<UserAddressBook>();
                        foreach (var item in addresses)
                        {
                            UserAddressBook address = new UserAddressBook()
                            {
                                UserAddressID = item.UserAddressID,
                                AddressName = item.IsCurrentAddress == true ? item.AddressName + " (Current)" : item.AddressName,
                                //CompleteAddress = item.Street + ", " + item.Building + ", " + item.Area + ", " + item.City,
                                CompleteAddress = item.Street + ", " + item.City + ", " + item.PostalCode,
                                IsCurrentAddress = item.IsCurrentAddress,
                                Latitude = item.Latitude,
                                Longitude = item.Longitude,
                                PostalCode = item.PostalCode
                            };
                            addressBook.Add(address);
                        }

                        aResp = new APIResponse
                        {
                            Message = "Toutes les adresses de livraison actives ont été récupérées.",
                            Status = "Succès!",
                            Payload = addressBook,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        UserAddresses getAddress = DbContext.UserAddresses.AsNoTracking().FirstOrDefault(d => d.UserAddressID == _address);
                        if (getAddress != null)
                        {
                            string compeleteAddress = getAddress.Street + ", " + getAddress.City + "," + getAddress.PostalCode;
                            getAddress.CompleteAddress = compeleteAddress;
                        }

                        aResp = new APIResponse
                        {
                            Message = "Adresse complète recherchée !",
                            Status = "Succès!",
                            Payload = getAddress,
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
                LogManager.LogInfo("GetUserDeliveryAdress");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse AddUserDeliveryAddress(string _auth, UserAddress _address)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("AddUserDeliveryAddress");
            LogManager.LogDebugObject(_address);

            try
            {
                DateTime NowDate = DateTime.Now;
                UserAddresses foundDuplicate = DbContext.UserAddresses.Where(c => c.AddressName == _address.AddressName && c.IsEnabled == true && c.UserId == _address.UserId).FirstOrDefault();
                if (foundDuplicate == null)
                {
                    //NEW: Check if this is the 1st ACTIVE Address then set to default
                    int userAddressCtr = DbContext.UserAddresses.Where(c => c.IsEnabled == true && c.UserId == _address.UserId).Count();
                    if (userAddressCtr == 0)
                    {
                        _address.IsCurrentAddress = true;
                    }

                    //update current address
                    UserAddresses hasCurrentAddress = DbContext.UserAddresses.FirstOrDefault(def => def.UserId == _address.UserId && def.IsCurrentAddress == true);
                    if (hasCurrentAddress != null && _address.IsCurrentAddress == true)
                    {
                        hasCurrentAddress.IsCurrentAddress = false;
                        hasCurrentAddress.LastEditedDate = NowDate;

                        DbContext.UserAddresses.Update(hasCurrentAddress);
                        DbContext.SaveChanges();
                    }

                    UserAddresses NewAddress = new UserAddresses
                    {
                        UserId = _address.UserId,
                        AddressName = _address.AddressName,
                        Street = _address.Street,
                        City = _address.City,
                        Area = _address.Area,
                        Building = _address.Building,
                        AddressNote = _address.AddressNote,
                        Latitude = _address.Latitude,
                        Longitude = _address.Longitude,
                        PostalCode = _address.PostalCode,

                        CreatedDate = NowDate,
                        CreatedBy = _address.UserId,
                        IsEnabled = true,
                        IsEnabledBy = _address.UserId,
                        DateEnabled = NowDate,
                        IsCurrentAddress = _address.IsCurrentAddress
                    };

                    DbContext.UserAddresses.Add(NewAddress);
                    DbContext.SaveChanges();

                    aResp = new APIResponse
                    {
                        Message = "La nouvelle adresse a été ajoutée avec succès.",
                        Status = "Succès!",
                        Payload = _address,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    LogManager.LogError("Nom de l'adresse existe déjà" + _address.AddressName);
                    aResp = new APIResponse
                    {
                        Message = "Nom de l'adresse existe déjà",
                        Status = "Échec",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AddUserDeliveryAdress");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse EditUserDeliveryAddress(UserAddress _address)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("EditUserDeliveryAddress");
            LogManager.LogDebugObject(_address);

            try
            {
                DateTime NowDate = DateTime.Now;
                UserAddresses updateAddress = DbContext.UserAddresses.FirstOrDefault(d => d.UserAddressID == _address.UserAddressID);
                if (updateAddress != null)
                {
                    // check duplicate record 
                    UserAddresses foundDuplicate = DbContext.UserAddresses.Where(c => c.AddressName == _address.AddressName && c.IsEnabled == true && c.UserId == _address.UserId).FirstOrDefault();

                    if (foundDuplicate != null && foundDuplicate.UserAddressID != updateAddress.UserAddressID)
                    {
                        LogManager.LogError("Nom de l'adresse existe déjà" + _address.AddressName);
                        aResp = new APIResponse
                        {
                            Message = "Nom de l'adresse existe déjà",
                            Status = "Échec",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                    else
                    {
                        //update current address
                        UserAddresses hasCurrentAddress = DbContext.UserAddresses.FirstOrDefault(def => def.UserId == _address.UserId && def.IsCurrentAddress == true);
                        if (hasCurrentAddress != null && _address.IsCurrentAddress == true)
                        {
                            hasCurrentAddress.IsCurrentAddress = false;
                            hasCurrentAddress.LastEditedDate = NowDate;

                            DbContext.UserAddresses.Update(hasCurrentAddress);
                            DbContext.SaveChanges();
                        }

                        //update address
                        updateAddress.LastEditedDate = NowDate;
                        updateAddress.LastEditedBy = _address.UserId;
                        updateAddress.AddressName = _address.AddressName;
                        updateAddress.Street = _address.Street;
                        updateAddress.City = _address.City;
                        updateAddress.Area = _address.Area;
                        updateAddress.Building = _address.Building;
                        updateAddress.AddressNote = _address.AddressNote;
                        updateAddress.IsCurrentAddress = _address.IsCurrentAddress;
                        updateAddress.Longitude = _address.Longitude;
                        updateAddress.Latitude = _address.Latitude;
                        updateAddress.PostalCode = _address.PostalCode;

                        DbContext.UserAddresses.Update(updateAddress);
                        DbContext.SaveChanges();

                        //NEW: Check if there is no active Default Address and set the first one
                        int userAddressCtr = DbContext.UserAddresses.Where(c => c.IsEnabled == true && c.IsCurrentAddress == true && c.UserId == _address.UserId).Count();
                        if (userAddressCtr == 0)
                        {
                            UserAddresses getFirstAddress = DbContext.UserAddresses.FirstOrDefault(c => c.IsEnabled == true && c.UserId == _address.UserId);
                            getFirstAddress.IsCurrentAddress = true;

                            DbContext.UserAddresses.Update(getFirstAddress);
                            DbContext.SaveChanges();
                        }

                        aResp = new APIResponse
                        {
                            Message = "Mis à jour avec succès",
                            Status = "Succès!",
                            Payload = updateAddress,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
                else
                {
                    LogManager.LogError("Enregistrement non trouvé." + _address.AddressName);
                    aResp = new APIResponse
                    {
                        Message = "Enregistrement non trouvé.",
                        Status = "Erreur!",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("EditUserDeliveryAdress");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse DeleteUserDeliveryAddress(UserAddressToDel _address)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("DeleteUserDeliveryAddress");
            LogManager.LogDebugObject(_address);

            try
            {
                DateTime NowDate = DateTime.Now;
                UserAddresses updateAddress = DbContext.UserAddresses.FirstOrDefault(d => d.UserAddressID == _address.UserAddressID);
                if (updateAddress != null)
                {
                    updateAddress.LastEditedDate = NowDate;
                    updateAddress.LastEditedBy = _address.UserId;
                    updateAddress.DateEnabled = NowDate;
                    updateAddress.IsEnabled = false;
                    updateAddress.IsEnabledBy = _address.UserId;

                    DbContext.UserAddresses.Update(updateAddress);
                    DbContext.SaveChanges();

                    aResp = new APIResponse
                    {
                        Message = "L'adresse a été supprimée avec succès",
                        Status = "Succès!",
                        Payload = updateAddress,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    LogManager.LogError("Enregistrement non trouvé." + _address.UserAddressID);
                    aResp = new APIResponse
                    {
                        Message = "Enregistrement non trouvé.",
                        Status = "Erreur!",
                        StatusCode = System.Net.HttpStatusCode.NotFound
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("DeleteUserDeliveryAddress");
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
