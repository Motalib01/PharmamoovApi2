using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class CustomerRepository : APIBaseRepo, ICustomerRepository
    { 
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private readonly LocalizationService localization;

        public CustomerRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, LocalizationService _localization)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            localization = _localization;
        }

        public APIResponse GetCustomers(string Authorization, int CustomerID) 
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetCustomers");
            LogManager.LogDebugObject("User Id: " + CustomerID.ToString());

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == Authorization && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("GetCustomers SAdminID: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);
                    if (CustomerID > 0)
                    { 
                        aResp.Message = "Récupérer l'enregistrement du client avec son identité (ID): " + CustomerID.ToString();
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        aResp.Payload = DbContext.Users.FirstOrDefault(u => u.UserRecordID == CustomerID);
                    }
                    else 
                    {
                        aResp.Message = "Fetch All Customer Records";
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        aResp.Payload = DbContext.Users.Where(u => u.AccountType == AccountTypes.APPUSER).AsNoTracking().ToList();
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
                LogManager.LogInfo("GetCustomers");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse EditCustomerProfile(string Authorization, User _customerProfile) 
        {
            APIResponse ApiResp = new APIResponse();
            LogManager.LogInfo("EditCustomerProfile");
            LogManager.LogDebugObject(_customerProfile);

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == Authorization && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("EditCustomerProfile intiated by SAdminID: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);
                    //Check if user exist
                    var foundUser = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserRecordID == _customerProfile.UserRecordID);
                    if (foundUser != null)
                    {
                        // if email is changed
                        if (foundUser.Email != _customerProfile.Email && _customerProfile.Email != string.Empty)
                        {
                            // Check if email already belongs to another user
                            var foundDuplicateEmail = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.Email == _customerProfile.Email);
                            if (foundDuplicateEmail != null)
                            {
                                LogManager.LogError("EditUserProfile >> L'adresse email est déjà utilisée" + _customerProfile.Email);
                                ApiResp.Message = "L'adresse email est déjà utilisée";
                                ApiResp.Status = "Échec";
                                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;

                                return ApiResp;
                            }
                        }

                        // if phone number is changed
                        if (foundUser.MobileNumber != _customerProfile.MobileNumber && _customerProfile.MobileNumber != string.Empty)
                        {
                            // Check if phone number already belongs to another user
                            var foundDuplicatePhoneNumber = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.MobileNumber == _customerProfile.MobileNumber);
                            if (foundDuplicatePhoneNumber != null)
                            {
                                LogManager.LogError("EditUserProfile >> Le numéro de téléphone est déjà utilisé" + _customerProfile.MobileNumber);
                                ApiResp.Message = "Le numéro de téléphone est déjà utilisé";
                                ApiResp.Status = "Échec";
                                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;

                                return ApiResp;
                            }
                        }

                        // This phone number should be verified from the mobile app first
                        foundUser.MobileNumber = _customerProfile.MobileNumber != null && _customerProfile.MobileNumber.Trim() != string.Empty ? _customerProfile.MobileNumber : foundUser.MobileNumber;
                        foundUser.Email = _customerProfile.Email != null && _customerProfile.Email.Trim() != string.Empty ? _customerProfile.Email : foundUser.Email;
                        foundUser.FirstName = _customerProfile.FirstName != null && _customerProfile.FirstName.Trim() != string.Empty ? _customerProfile.FirstName : foundUser.FirstName;
                        foundUser.LastName = _customerProfile.LastName != null && _customerProfile.LastName.Trim() != string.Empty ? _customerProfile.LastName : foundUser.LastName;
                        foundUser.ImageUrl = _customerProfile.ImageUrl; 
                        foundUser.IsEnabled = _customerProfile.IsEnabled;

                        foundUser.LastEditedBy = IsUserLoggedIn.UserId;
                        foundUser.LastEditedDate = DateTime.Now;

                        DbContext.Update(foundUser);
                        DbContext.SaveChanges();

                        ApiResp.Message = "Profil mis à jour avec succès";
                        ApiResp.Status = "Succès";
                        ApiResp.Payload = foundUser;
                        ApiResp.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        LogManager.LogInfo("Enregistrement non trouvé." + _customerProfile.UserId);
                        ApiResp.Message = "Enregistrement non trouvé.";
                        ApiResp.Status = "Échec";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                    }
                }
                else
                {
                    ApiResp.Message = "Utilisateur non connecté";
                    ApiResp.Status = "Échec";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.Unauthorized;
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
    }
}
