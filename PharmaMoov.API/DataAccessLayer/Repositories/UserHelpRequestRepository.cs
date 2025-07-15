using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using PharmaMoov.Models.User;
using PharmaMoov.Models.Admin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class UserHelpRequestRepository: APIBaseRepo, IUserHelpRequestRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; } 

        public UserHelpRequestRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon; 
        }

        public APIResponse GetUserRequestList(int _id)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetUserRequestList"); 
            try
            {
                if (_id == 0)
                {
                    List<UserHelpRequestView> uhrViewList = DbContext.UserHelpRequests
                        .Join(DbContext.Users, uhr => uhr.UserId, u => u.UserId, (uhr, u) => new { uhr, u })
                        .Select( uhrvl => new UserHelpRequestView { 
                            UserHelpRequestRecordID = uhrvl.uhr.UserHelpRequestRecordID,
                            UserId = uhrvl.uhr.UserId,
                            DateCreated = uhrvl.uhr.CreatedDate,
                            Description = uhrvl.uhr.Description,
                            OrderId = uhrvl.uhr.OrderId,
                            UserEmail = uhrvl.u.Email,
                            UserFullName = uhrvl.u.FirstName + " " + uhrvl.u.LastName,
                            UserMobileNumber = uhrvl.u.MobileNumber,
                            RequestStatus = uhrvl.uhr.RequestStatus
                        }).ToList();

                        aResp = new APIResponse
                        {
                            Message = "Tous les éléments ont été récupérés avec succès.",
                            Status = "Succès!",
                            Payload = uhrViewList,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Tous les éléments ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = DbContext.UserHelpRequests
                        .Join(DbContext.Users, uhr => uhr.UserId, u => u.UserId, (uhr, u) => new { uhr, u })
                        .Select(uhrvl => new UserHelpRequestView
                        {
                            UserHelpRequestRecordID = uhrvl.uhr.UserHelpRequestRecordID,
                            UserId = uhrvl.uhr.UserId,
                            DateCreated = uhrvl.uhr.CreatedDate,
                            Description = uhrvl.uhr.Description,
                            OrderId = uhrvl.uhr.OrderId,
                            UserEmail = uhrvl.u.Email,
                            UserFullName = uhrvl.u.FirstName + " " + uhrvl.u.LastName,
                            UserMobileNumber = uhrvl.u.MobileNumber,
                            RequestStatus = uhrvl.uhr.RequestStatus
                        }).FirstOrDefault(uhrv => uhrv.UserHelpRequestRecordID == _id),
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetUserRequestList");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse GetOrderNumber(string _auth)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("GetOrderNumber: " + _auth);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("GetOrderNumber: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    return aResp = new APIResponse
                    {
                        Message = "Tous les éléments ont été récupérés avec succès.",
                        Status = "Succès!",
                        Payload = DbContext.Orders.AsNoTracking()
                                       .Where(o => o.UserId == IsUserLoggedIn.UserId)
                                       .Select(x => new OrderNumber
                                       {
                                           OrderId = x.OrderID,
                                           OrderReferenceID = x.OrderReferenceID
                                       }),
                       StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetOrderNumber");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse AddUserRequest(string _auth, NewUserHelpRequest _uhRequest)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("AddUserRequest");
            LogManager.LogDebugObject(_uhRequest);

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("AddUserRequest by: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    UserHelpRequest uhelpReq = new UserHelpRequest()
                    {
                        Description = _uhRequest.Description,
                        OrderId = _uhRequest.OrderId,
                        UserId = IsUserLoggedIn.UserId,
                        RequestStatus = HelpRequestStatus.PENDING,
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

                    DbContext.UserHelpRequests.Add(uhelpReq);
                    DbContext.SaveChanges();

                    aResp.Message = "La demande d'aide a été soumise avec succès";
                    aResp.Status = "Succès";
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                    aResp.Payload = uhelpReq; 
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
                LogManager.LogInfo("AddUserRequest");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse GetUserConcernList(string _auth, int _id)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetUserConcernList");

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("GetUserConcernList: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    if (_id == 0)
                    {
                        List<UserGeneralConcernView> ugcViewList = new List<UserGeneralConcernView>();
                        List<UserGeneralConcernView> admViewList = new List<UserGeneralConcernView>();
                        if (IsUserLoggedIn.AccountType == AccountTypes.APPUSER)
                        {
                            ugcViewList = DbContext.UserGeneralConcerns
                                .Join(DbContext.Users, ugc => ugc.UserId, u => u.UserId, (ugc, u) => new { ugc, u })
                                .Select(ugcvl => new UserGeneralConcernView
                                {
                                    UserConcernRecordID = ugcvl.ugc.UserConcernRecordID,
                                    UserId = ugcvl.ugc.UserId,
                                    DateCreated = ugcvl.ugc.CreatedDate,
                                    Subject = ugcvl.ugc.Subject,
                                    Description = ugcvl.ugc.Description,
                                    UserEmail = ugcvl.u.Email,
                                    UserFullName = ugcvl.u.FirstName + " " + ugcvl.u.LastName,
                                    UserMobileNumber = ugcvl.u.MobileNumber
                                }).ToList();
                        }
                        else
                        {
                            admViewList = DbContext.UserGeneralConcerns
                               .Join(DbContext.Admins, ugc => ugc.UserId, u => u.AdminId, (ugc, u) => new { ugc, u })
                               .Select(ugcvl => new UserGeneralConcernView
                               {
                                   UserConcernRecordID = ugcvl.ugc.UserConcernRecordID,
                                   UserId = ugcvl.ugc.UserId,
                                   DateCreated = ugcvl.ugc.CreatedDate,
                                   Subject = ugcvl.ugc.Subject,
                                   Description = ugcvl.ugc.Description,
                                   UserEmail = ugcvl.u.Email,
                                   UserFullName = ugcvl.u.FirstName + " " + ugcvl.u.LastName,
                                   UserMobileNumber = ugcvl.u.MobileNumber
                               }).ToList();
                        }

                        aResp = new APIResponse
                        {
                            Message = "Tous les éléments ont été récupérés avec succès.",
                            Status = "Succès!",
                            Payload = ugcViewList.Union(admViewList),
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Tous les éléments ont été récupérés avec succès.",
                            Status = "Succès!",
                            Payload = DbContext.UserGeneralConcerns
                            .Join(DbContext.Users, ugc => ugc.UserId, u => u.UserId, (ugc, u) => new { ugc, u })
                            .Select(ugcvl => new UserGeneralConcernView
                            {
                                UserId = ugcvl.ugc.UserId,
                                DateCreated = ugcvl.ugc.CreatedDate,
                                Subject = ugcvl.ugc.Subject,
                                Description = ugcvl.ugc.Description,
                                UserEmail = ugcvl.u.Email,
                                UserFullName = ugcvl.u.FirstName + " " + ugcvl.u.LastName,
                                UserMobileNumber = ugcvl.u.MobileNumber
                            }).FirstOrDefault(uhrv => uhrv.UserConcernRecordID == _id),
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetUserConcernList");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse AddUserConcern(string _auth, NewUserGeneralConcern _ugConcern)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("AddUserConcern");
            LogManager.LogDebugObject(_ugConcern);

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("AddUserConcern by: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);

                    UserGeneralConcern uConcern = new UserGeneralConcern()
                    {
                        Subject = _ugConcern.Subject,
                        Description = _ugConcern.Description,
                        UserId = IsUserLoggedIn.UserId,
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

                    DbContext.UserGeneralConcerns.Add(uConcern);
                    DbContext.SaveChanges();

                    aResp.Message = "Merci pour votre message. Nous allons traiter l'information et revenir vers vous rapidement";
                    aResp.Status = "Succès";
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                    aResp.Payload = uConcern;
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
                LogManager.LogInfo("AddUseConcern");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }

            return aResp;
        }

        public APIResponse ChangeHelpRequestStatus(string _auth, ChangeHelpRequestStatus _request)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ChangeHelpRequestStatus");
            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    UserHelpRequest foundRequest = DbContext.UserHelpRequests.Where(a => a.UserHelpRequestRecordID == _request.RecordId).FirstOrDefault();
                    if (foundRequest != null)
                    {
                        foundRequest.LastEditedDate = DateTime.Now;
                        foundRequest.LastEditedBy = _request.AdminId;
                        foundRequest.RequestStatus = _request.HelpRequestStatus;

                        DbContext.UserHelpRequests.Update(foundRequest);
                        DbContext.SaveChanges();
                        aResp = new APIResponse
                        {
                            Message = "Demande d'aide résolue avec succès.",
                            Status = "Succès!",
                            Payload = foundRequest,
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
                LogManager.LogInfo("ChangeHelpRequestStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse SendInquiry(GeneralInquiry _inquiry)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("SendInquiry");
            try
            {
                List<string> adminSA = DbContext.Admins.Where(ult => ult.UserTypeId == UserTypes.SUPERADMIN)
                                        .Select(a => a.Email).ToList();

                var EmailParam = APIConfig.MailConfig;
                EmailParam.From = _inquiry.Email;
                EmailParam.Subject = "PharmaMoov : Demande d'information générale";

                var emailBody = String.Format("From: " + _inquiry.Email + "<br/><br/>" + _inquiry.InquiryMessage);
                EmailParam.Body = emailBody;
             
                var RetStatus = SendEmailByEmailAddress(adminSA, EmailParam, LogManager);

                if (RetStatus == 0)
                {
                    aResp = new APIResponse
                    {
                        Message = "Le message a été envoyé avec succès aux administrateurs.",
                        Status = "Succès!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Le message n'a pas été envoyé!",
                        Status = "BadRequest",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
                

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("SendInquiry");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse SendCareer(CareersForm _career)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("SendCareer");
            try
            {
                List<string> adminSA = DbContext.Admins.Where(ult => ult.UserTypeId == UserTypes.SUPERADMIN)
                                        .Select(a => a.Email).ToList();


                var EmailParam = APIConfig.MailConfig;
                EmailParam.From = _career.Email;
                EmailParam.Subject = "PharmaMoov : Demande d'emploi";

                var emailBody = String.Format("First Name: " + _career.FirstName + 
                                            "<br/> Last Name: " + _career.LastName +
                                            "<br/> Email: " + _career.Email + 
                                            "<br/> Phone Number: " + _career.PhoneNumber + 
                                            "<br/> Position: " + _career.Position + 
                                            "<br/><br/> Cover Letter: " + _career.CoverLetter);
                
                EmailParam.Body = emailBody;

                var RetStatus = SendEmailByEmailAddress(adminSA, EmailParam, LogManager);

                if (RetStatus == 0)
                {
                    aResp = new APIResponse
                    {
                        Message = "Le message a été envoyé avec succès aux administrateurs.",
                        Status = "Succès!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Le message n'a pas été envoyé!",
                        Status = "BadRequest",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }


            }
            catch (Exception ex)
            {
                LogManager.LogInfo("SendCareer");
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
