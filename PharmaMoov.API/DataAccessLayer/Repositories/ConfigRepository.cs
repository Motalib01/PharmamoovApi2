using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using PharmaMoov.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class ConfigRepository : APIBaseRepo, IConfigRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public ConfigRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public APIResponse GetAllConfigurations() 
        {
            APIResponse aResp = new APIResponse();
            try
            { 
                aResp = new APIResponse
                {
                    Message = "Toutes les configurations ont été récupérées avec succès.",
                    Status = "Succès!",
                    Payload = DbContext.OrderConfigurations.ToList(),
                    StatusCode = System.Net.HttpStatusCode.OK
                };
                // return data
                return aResp;
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetAllConfigurations");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse UpdateOrderConfig(List<OrderConfiguration> _configs) 
        {
            APIResponse aResp = new APIResponse(); 
            try
            {
                DbContext.UpdateRange(_configs);
                DbContext.SaveChanges();
                aResp = new APIResponse
                {
                    Message = "Toutes les configurations ont été récupérées avec succès.",
                    Status = "Succès!",
                    Payload = DbContext.OrderConfigurations.ToList(),
                    StatusCode = System.Net.HttpStatusCode.OK
                };
                return aResp;
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("UpdateOrderConfig");
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
