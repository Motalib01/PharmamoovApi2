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
    public class CourierRepository : APIBaseRepo, ICourierRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        private readonly LocalizationService localization;

        public CourierRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon, LocalizationService _localization)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            localization = _localization;
        }

        public APIResponse GetCouriers(string Authorization, int CourierID)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetCouriers");
            LogManager.LogDebugObject("Courier Id: " + CourierID.ToString());

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == Authorization && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("GetCouriers SAdminID: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);
                    if (CourierID > 0)
                    {
                        aResp.Message = "Récupération d'un enregistrement de courrier avec identité.: " + CourierID.ToString();
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        aResp.Payload = DbContext.Users.FirstOrDefault(u => u.UserRecordID == CourierID);
                    }
                    else
                    {
                        aResp.Message = "Récupérer tous les enregistrements de courrier";
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                        aResp.Payload = DbContext.Users.Where(u => u.AccountType == AccountTypes.COURIER).AsNoTracking().ToList();
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
                LogManager.LogInfo("GetCouriers");
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
