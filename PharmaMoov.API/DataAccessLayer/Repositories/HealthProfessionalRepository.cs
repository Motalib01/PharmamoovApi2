using Microsoft.EntityFrameworkCore;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using PharmaMoov.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class HealthProfessionalRepository : APIBaseRepo, IHealthProfessionalRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private readonly LocalizationService localization;

        public HealthProfessionalRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, LocalizationService _localization)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            localization = _localization;
        }

        public APIResponse GetHealthProfessionals(string Authorization, int HealthProfID)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetHealthProfessionals");
            LogManager.LogDebugObject("User Id: " + HealthProfID.ToString());

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == Authorization && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("GetHealthProfessionals SAdminID: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);
                    if (HealthProfID > 0)
                    {
                        aResp.Message = "Recherche d'un dossier de professionnel de santé avec ID: " + HealthProfID.ToString();
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        aResp.Payload = DbContext.Users.FirstOrDefault(u => u.UserRecordID == HealthProfID);
                    }
                    else
                    {
                        aResp.Message = "Récupérer tous les dossiers des professionnels de la santé";
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        aResp.Payload = DbContext.Users.Where(u => u.AccountType == AccountTypes.HEALTHPROFESSIONAL).AsNoTracking().ToList();
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
                LogManager.LogInfo("GetHealthProfessionals");
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
