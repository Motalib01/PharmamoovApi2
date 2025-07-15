using PharmaMoov.Models;
using PharmaMoov.Models.Attachment;
using PharmaMoov.Models.User;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using System;
using System.IO;
using System.Linq;
using PharmaMoov.Models.Shop;
using Microsoft.EntityFrameworkCore;
using PharmaMoov.Models.Admin;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class AttachmentRepository : APIBaseRepo, IAttachmentRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public AttachmentRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public APIResponse AddAttachmentRecord(string _authToken, string _uploadedFileName, string _serverPhyPath, string _urlRoot, UploadTypes _ut)
        {
            string fileFolder = "";
            switch (_ut)
            {
                case UploadTypes.UploadBanner:
                    fileFolder = "Banners";
                    break;
                case UploadTypes.UploadIcon:
                    fileFolder = "Icons";
                    break;
                case UploadTypes.UploadBackgroundImage:
                    fileFolder = "BackgroundImages";
                    break;
                case UploadTypes.UploadProfileImage:
                    fileFolder = "ProfileImages";
                    break;
                case UploadTypes.UploadDocument:
                    fileFolder = "Documents";
                    break;
                case UploadTypes.UploadPrescription:
                    fileFolder = "Prescription";
                    break;
                default:
                    fileFolder = "";
                    break;
            }
            APIResponse ApiResp = new APIResponse();
            try
            {
                UserLoginTransaction ULT = DbContext.UserLoginTransactions.Where(ult => ult.Token == _authToken && ult.IsActive == true).FirstOrDefault();
                if (ULT != null)
                {
                    string ActualFileName = GetNewFileName(_uploadedFileName, ULT.UserId.ToString().Split('-')[4]);
                    string ActualFileExtension = Path.GetExtension(_uploadedFileName);
                    DateTime CreatedNow = DateTime.Now;
                    Attachment NewAttach = new Attachment
                    {
                        AttachmentExternalUrl = _urlRoot + "/resources/" + fileFolder + "/" + ActualFileName,
                        AttachmentName = ActualFileName,
                        AttachmentFileName = Path.GetFileNameWithoutExtension(ActualFileName),
                        AttachmentServerPhysicalPath = Path.Combine(_serverPhyPath, ActualFileName),
                        AttachmentType = Path.GetExtension(_uploadedFileName),
                        AttachmentUploadedFileName = _uploadedFileName,
                        CreatedBy = ULT.UserId,
                        CreatedDate = CreatedNow,
                        UType = _ut,
                        DateEnabled = CreatedNow,
                        IsEnabled = true,
                        IsEnabledBy = ULT.UserId,
                        LastEditedBy = ULT.UserId,
                        LastEditedDate = CreatedNow
                    };
                    DbContext.Add(NewAttach);
                    DbContext.SaveChanges();
                    LogManager.LogInfo("-- Saved New File --");
                    LogManager.LogInfo("Path: " + NewAttach.AttachmentServerPhysicalPath);
                    LogManager.LogDebugObject(NewAttach);
                    return new APIResponse
                    {
                        Message = "Enregistrement ajouté !",
                        Status = "Succès!",
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Payload = NewAttach
                    };
                }
                else
                {
                    ApiResp = new APIResponse
                    {
                        Message = "Code d'autorisation non valide !",
                        Status = "Code d'accès invalide",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AddAttachmentRecord");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                ApiResp.Message = "Quelque chose s'est mal passé !";
                ApiResp.Status = "Erreur de serveur interne";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
            }
            return ApiResp;
        }

        string GetNewFileName(string _upFName, string _uidPrefx)
        {
            return _uidPrefx + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + Path.GetExtension(_upFName);
        }
    }
}
