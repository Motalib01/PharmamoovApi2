using PharmaMoov.Models;
using PharmaMoov.Models.User;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using PharmaMoov.Models.Admin;
using MangoPay.SDK.Entities.GET;
using MangoPay.SDK.Entities.POST;
using MangoPay.SDK;
using MangoPay.SDK.Entities;
using MangoPay.SDK.Core.Enumerations;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class UserRepository : APIBaseRepo, IUserRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig;
        ILoggerManager LogManager { get; }
        IPaymentRepository PaymentRepo { get; }
        public UserRepository(IWebHostEnvironment env,APIDBContext _dbCtxt, IPaymentRepository _payRepo, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            PaymentRepo = _payRepo;
        }

        public APIResponse RegisterUserViaEmail(UserRegistrationViaEmail _user)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("RegisterUserViaEmail: " + _user.Email + " Platform: " + _user.DevicePlatform);
            LogManager.LogDebugObject(_user);

            try
            {
                //Check if the email exist in the database
                User FoundUser = DbContext.Users.AsNoTracking().Where(u => u.Email == _user.Email).FirstOrDefault();
                if (FoundUser == null)
                {
                    Guid UserGuidID = Guid.NewGuid();
                    DateTime RegistrationDate = DateTime.Now;

                    User NewUser = new User
                    {
                        CreatedBy = UserGuidID,
                        CreatedDate = RegistrationDate,
                        IsEnabledBy = UserGuidID,
                        DateEnabled = RegistrationDate,
                        IsEnabled = false,

                        UserId = UserGuidID,
                        AccountType = _user.AccountType,
                        Email = _user.Email,
                        FirstName = _user.FirstName,
                        LastName = _user.LastName,
                        RegistrationCode = null,
                        LockedDateTime = RegistrationDate,
                        IsLocked = true, // = Unverified

                        MobileNumber = null,
                        DateOfBirth = null,
                        Gender = 0,
                        ImageUrl = null,

                        RegistrationPlatform = _user.DevicePlatform
                    };
                    NewUser.Password = NewUser.HashP(_user.Password.ToString(), APIConfig.TokenKeys.Key);

                    DbContext.Add(NewUser);
                    DbContext.SaveChanges();

                    //check for unverified account


                    LogManager.LogInfo("-- New User Added --");
                    LogManager.LogDebugObject(NewUser);

                    apiResp = new APIResponse
                    {
                        Message = "Compte créé avec succès.",
                        Status = "Succès",
                        Payload = NewUser,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    if (FoundUser.IsLocked == true)
                    {
                        LogManager.LogError("RegisterUserViaEmail >> Compte utilisateur non vérifié." + _user.Email);
                        apiResp = new APIResponse
                        {
                            Message = "L'adresse email a été enregistrée mais non verifiée",
                            Status = "Échec",
                            StatusCode = System.Net.HttpStatusCode.BadRequest,
                            ResponseCode = APIResponseCode.UserIsNotYetVerified
                        };
                    }
                    else
                    {
                        LogManager.LogError("RegisterUserViaEmail >> L'email existe déjà." + _user.Email);
                        apiResp = new APIResponse
                        {
                            Message = "L'email existe déjà.",
                            Status = "Échec",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError("RegisterUserViaEmail: " + _user.Email + " Platform: " + _user.DevicePlatform);
                LogManager.LogDebugObject(ex);
                apiResp = new APIResponse
                {
                    Message = "Server Erreur!",
                    Status = "Erreur de serveur interne!",
                    StatusCode = System.Net.HttpStatusCode.BadRequest

                };
            }

            return apiResp;
        }

        public APIResponse VerifyUserMobileOrEmail(VerifyMobileOrEmail _user)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("VerifyMobileOrEmail: " + _user.UserId + " Platform: " + _user.DevicePlatform);

            try
            {
                IsEmailOrMobileNumberValid returnValidation = new IsEmailOrMobileNumberValid();

                var FoundUserPhone = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.MobileNumber == _user.MobileNumber && u.UserId != _user.UserId && u.IsLocked == false);
                if (FoundUserPhone == null)
                {
                    apiResp = new APIResponse
                    {
                        Message = "Le numéro de téléphone est valide.",
                        Status = "Succès",
                        Payload = returnValidation.IsMobileNumberValid = true,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    LogManager.LogError("VerifyUserMobileOrEmail >> Le numéro de téléphone n'est pas valide." + _user.MobileNumber);
                    apiResp = new APIResponse
                    {
                        Message = "Le numéro de téléphone n'est pas valide.",
                        Status = "Échec",
                        Payload = returnValidation.IsMobileNumberValid = false,
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("VerifyMobileOrEmail");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                apiResp.Message = "Quelque chose s'est mal passé !";
                apiResp.Status = "Erreur de serveur interne";
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                apiResp.ModelError = GetStackError(ex.InnerException);
            }
            return apiResp;
        }

        public APIResponse RegisterUserViaMobileNumber(UserRegistrationViaMobileNumber _user, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("RegisterUserViaMobileNumber: " + _user.UserId + " Platform: " + _user.DevicePlatform);
            LogManager.LogDebugObject(_user);

            try
            {
                //Check if the mobile number exist in the database
                User FoundDuplicateRecord = DbContext.Users.AsNoTracking().Where(u => u.MobileNumber == _user.MobileNumber && u.UserId != _user.UserId).FirstOrDefault();
                if (FoundDuplicateRecord == null)
                {
                    DateTime RegistrationDate = DateTime.Now;
                    string registrationCode = GenerateUniqueCodeForUser(DbContext);

                    User FoundUser = DbContext.Users.Where(u => u.UserId == _user.UserId).FirstOrDefault();
                    if (FoundUser != null)
                    {
                        //send registration code via email
                        var emailBody = String.Format(APIConfig.MsgConfigs.RegisterMobileUser, FoundUser.FirstName, registrationCode);
                        var sendStatus = SendVerificationCodeViaEmailOrSMS(FoundUser.Email, emailBody, "PharmaMoov: Registration Code", _conf);

                        if (sendStatus == 0)
                        {
                            FoundUser.LastEditedBy = FoundUser.UserId;
                            FoundUser.LastEditedDate = RegistrationDate;
                            FoundUser.MobileNumber = _user.MobileNumber;
                            FoundUser.RegistrationCode = registrationCode;

                            DbContext.Update(FoundUser);
                            DbContext.SaveChanges();

                            apiResp = new APIResponse
                            {
                                Message = "Le code d'enregistrement a été envoyé avec succès",
                                Status = "Succès",
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            LogManager.LogInfo("Sending Registration Code");
                            LogManager.LogError("Sending Échec >> " + FoundUser.Email);

                            apiResp = new APIResponse
                            {
                                Message = "L'envoi du code d'enregistrement a échoué",
                                Status = "Échec",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }
                }
                else
                {
                    LogManager.LogError("RegisterUserViaMobileNumber >> " + _user.MobileNumber);
                    apiResp = new APIResponse
                    {
                        Message = "Le numéro de mobile existe déjà.",
                        Status = "Échec",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError("RegisterUserViaMobileNumber: " + _user.UserId + " Platform: " + _user.DevicePlatform);
                LogManager.LogDebugObject(ex);
                apiResp = new APIResponse
                {
                    Message = "Server Erreur!",
                    Status = "Erreur de serveur interne!",
                    StatusCode = System.Net.HttpStatusCode.BadRequest

                };
            }

            return apiResp;
        }

        public APIResponse VerifyUserCode(UserVerifyCode _vCode)
        {
            APIResponse apiResp = new APIResponse();

            User FoundUserRegistrationCode = new User();
            UserVerificationCode FoundUserVerificationCode = new UserVerificationCode();

            try
            {
                if (_vCode.VerificationType == VerificationTypes.RegisterAccount)
                {
                    FoundUserRegistrationCode = DbContext.Users.AsNoTracking().Where(e => e.RegistrationCode == _vCode.VerificationCode && e.Email == _vCode.Email).FirstOrDefault();
                    if (FoundUserRegistrationCode != null)
                    {
                        LogManager.LogInfo("VerifyUserCode > RegisterAccount: " + FoundUserRegistrationCode.UserId + " Platform: " + _vCode.DevicePlatform);

                        FoundUserRegistrationCode.LastEditedBy = FoundUserRegistrationCode.UserId;
                        FoundUserRegistrationCode.LastEditedDate = DateTime.Now;
                        FoundUserRegistrationCode.RegistrationCode = null;
                        FoundUserRegistrationCode.DateEnabled = DateTime.Now;
                        FoundUserRegistrationCode.IsEnabledBy = FoundUserRegistrationCode.UserId;
                        FoundUserRegistrationCode.IsEnabled = true;
                        FoundUserRegistrationCode.LockedDateTime = DateTime.Now;
                        FoundUserRegistrationCode.IsLocked = false;

                        DbContext.Users.Update(FoundUserRegistrationCode);
                        DbContext.SaveChanges();

                        apiResp.Message = "Vérification réussie.";
                        apiResp.Status = "Succès!";
                        apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        LogManager.LogError("VerifyUserCode >> Échec de la vérification." + _vCode);
                        apiResp.Message = "Échec de la vérification.";
                        apiResp.Status = "Échec";
                        apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    }
                }
                else if (_vCode.VerificationType == VerificationTypes.ChangePhoneNumber)
                {
                    FoundUserVerificationCode = DbContext.UserVerificationCodes.AsNoTracking().Where(e => e.UserId == _vCode.UserId && e.VerificationCode == _vCode.VerificationCode).FirstOrDefault();
                    LogManager.LogInfo("VerifyUserCode > ChangePhoneNumber/UpdateProfile: " + FoundUserVerificationCode.UserId + " Platform: " + _vCode.DevicePlatform);
                    if (FoundUserVerificationCode != null)
                    {
                        FoundUserVerificationCode.LastEditedBy = FoundUserVerificationCode.UserId;
                        FoundUserVerificationCode.LastEditedDate = DateTime.Now;
                        FoundUserVerificationCode.DateEnabled = DateTime.Now;
                        FoundUserVerificationCode.IsEnabledBy = FoundUserVerificationCode.UserId;
                        FoundUserVerificationCode.IsEnabled = false;

                        DbContext.UserVerificationCodes.Update(FoundUserVerificationCode);
                        DbContext.SaveChanges();

                        apiResp.Message = "Vérification réussie.";
                        apiResp.Status = "Succès!";
                        apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        LogManager.LogError("VerifyUserCode >> Échec de la vérification." + _vCode);
                        apiResp.Message = "Échec de la vérification.";
                        apiResp.Status = "Échec";
                        apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    LogManager.LogError("VerifyUserCode >> Type de vérification non valide." + _vCode);
                    apiResp.Message = "Type de vérification non valide.";
                    apiResp.Status = "Échec";
                    apiResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError("VerifyUserCode");
                LogManager.LogDebugObject(ex);
                apiResp = new APIResponse
                {
                    Message = "Server Erreur!",
                    Status = "Erreur de serveur interne!",
                    StatusCode = System.Net.HttpStatusCode.BadRequest

                };
            }

            return apiResp;
        }

        public APIResponse SendUserVerificationCode(UserVerifyCode _vCode, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("SendUserVerificationCode: " + _vCode);

            User FoundUser = new User();
            UserVerificationCode FoundUserVerificationCode = new UserVerificationCode();

            try
            {
                string NewVerificationCode = GenerateUniqeCode(4, true, false);
                if (_vCode.VerificationType == VerificationTypes.RegisterAccount)
                {
                    FoundUser = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.Email == _vCode.Email);
                    if (FoundUser != null)
                    {
                        string NewRegistrationCode = GenerateUniqueCodeForUser(DbContext);

                        FoundUser.RegistrationCode = NewRegistrationCode;
                        FoundUser.IsLocked = false;
                        FoundUser.LockedDateTime = DateTime.Now;
                        FoundUser.LastEditedDate = DateTime.Now;
                        FoundUser.LastEditedBy = FoundUser.UserId;

                        DbContext.Users.Update(FoundUser);
                        DbContext.SaveChanges();

                        //send verification code
                        var emailBody = String.Format(APIConfig.MsgConfigs.RegisterMobileUser, FoundUser.FirstName, NewRegistrationCode);
                        var sendStatus = SendVerificationCodeViaEmailOrSMS(FoundUser.Email, emailBody, "PharmaMoov: New Registration Code",  _conf);
                        //APIConfig.SmsConfig.To = FoundUser.MobileNumber;
                        //SendCodeViaSMS(APIConfig.SmsConfig, FoundUser.MobileNumber, NewVerificationCode);
                        if (sendStatus == 0)
                        {
                            apiResp = new APIResponse
                            {
                                Message = "Code de vérification envoyé avec succès",
                                Status = "Succès",
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            LogManager.LogInfo("Sending Verification Code");
                            LogManager.LogError("Sending Échec >> " + FoundUser.Email);

                            apiResp = new APIResponse
                            {
                                Message = "L'envoi du code de vérification a échoué",
                                Status = "Échec",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }
                    else
                    {
                        apiResp.Message = "Enregistrement de l'utilisateur non trouvé.";
                        apiResp.Status = "Échec!";
                        apiResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                    }
                }
                else if (_vCode.VerificationType == VerificationTypes.ForgotPassword)
                {
                    //for verified users only
                    FoundUser = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.Email == _vCode.Email);
                    if (FoundUser != null)
                    {                       
                        var emailBody = String.Format(APIConfig.MsgConfigs.CustomerForgotPassword, FoundUser.FirstName, NewVerificationCode);
                        var sendStatus = SendVerificationCodeViaEmailOrSMS(FoundUser.Email, emailBody, "PharmaMoov: Forgot Password", _conf);
                        //APIConfig.SmsConfig.To = FoundUser.MobileNumber;
                        //SendCodeViaSMS(APIConfig.SmsConfig, FoundUser.MobileNumber, NewVerificationCode);
                        if (sendStatus == 0)
                        {
                            FoundUser.ForgotPasswordCode = NewVerificationCode;
                            FoundUser.LastEditedBy = FoundUser.UserId;
                            FoundUser.LastEditedDate = DateTime.Now;

                            DbContext.Users.Update(FoundUser);
                            DbContext.SaveChanges();

                            apiResp = new APIResponse
                            {
                                Message = "Code de vérification envoyé avec succès",
                                Status = "Succès",
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            LogManager.LogInfo("Sending verification code for Forgot Password");
                            LogManager.LogError("Sending Échec >> " + FoundUser.Email);

                            apiResp = new APIResponse
                            {
                                Message = "L'envoi du nouveau mot de passe a échoué",
                                Status = "Échec",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }
                    else
                    {     
                        apiResp.Message = "Enregistrement de l'utilisateur non trouvé.";
                        apiResp.Status = "Mauvaise demande!";
                        apiResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                    }
                }
                else if (_vCode.VerificationType == VerificationTypes.ChangePhoneNumber)
                {
                    FoundUser = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserId == _vCode.UserId);
                    if (FoundUser != null)
                    {
                        CheckUserVerificationCode(FoundUser, VerificationTypes.ChangePhoneNumber, NewVerificationCode);

                        //send verification code
                        var emailBody = String.Format(APIConfig.MsgConfigs.ChangeNumber, FoundUser.FirstName, NewVerificationCode);
                        var sendStatus = SendVerificationCodeViaEmailOrSMS(FoundUser.Email, emailBody, "PharmaMoov: Change Mobile Number", _conf);
                        //APIConfig.SmsConfig.To = FoundUser.MobileNumber;
                        //SendCodeViaSMS(APIConfig.SmsConfig,FoundUser.MobileNumber,NewVerificationCode);
                        if (sendStatus == 0)
                        {
                            apiResp = new APIResponse
                            {
                                Message = "Code de vérification envoyé avec succès",
                                Status = "Succès",
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            LogManager.LogInfo("Sending Verification Code");
                            LogManager.LogError("Sending Échec >> " + FoundUser.Email);

                            apiResp = new APIResponse
                            {
                                Message = "L'envoi du code de vérification a échoué",
                                Status = "Échec",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                    }
                    else
                    {
                        apiResp.Message = "Enregistrement de l'utilisateur non trouvé.";
                        apiResp.Status = "Mauvaise demande!";
                        apiResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                    }
                }
                else
                {
                    LogManager.LogError("Type de vérification non valide >> " + _vCode.VerificationType);
                    apiResp.Message = "Type de vérification non valide";
                    apiResp.Status = "Échec";
                    apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("SendUserVerificationCode: " + _vCode);
                apiResp.Message = ex.Message;
                apiResp.Status = "Mauvaise demande!";
                apiResp.ModelError = GetStackError(ex.InnerException);
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
            }

            return apiResp;
        }

        public APIResponse ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("SendUserVerificationCode: " + resetPasswordModel.VerificationCode);
            try
            {
                var userReult = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.ForgotPasswordCode == resetPasswordModel.VerificationCode);
                if(userReult != null)
                {
                    var HashedNewPass = userReult.HashP(resetPasswordModel.NewPassword, APIConfig.TokenKeys.Key);
                    if (userReult.Password == HashedNewPass)
                    {
                        apiResp = new APIResponse
                        {
                            Message = "Le mot de passe doit être différent du précédent",
                            Status = "Échec",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                        return apiResp;
                    }
                    userReult.ForgotPasswordCode = null;
                    userReult.Password = HashedNewPass;
                    userReult.LastEditedBy = userReult.UserId;
                    userReult.LastEditedDate = DateTime.Now;

                    DbContext.Users.Update(userReult);
                    DbContext.SaveChanges();

                    apiResp = new APIResponse
                    {
                        Message = "Votre mot de passe a été réinitialisé avec succès.",
                        Status = "Succès",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    apiResp.Message = "Veuillez entrer un code de vérification valide.";
                    apiResp.Status = "Échec!";
                    apiResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("SendUserVerificationCode: " + resetPasswordModel.VerificationCode);
                apiResp.Message = ex.Message;
                apiResp.Status = "Mauvaise demande!";
                apiResp.ModelError = GetStackError(ex.InnerException);
                apiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
            }

            return apiResp;
        }
        private int SendVerificationCodeViaEmailOrSMS(string _email, string _eBody, string _eSubject, APIConfigurationManager _conf = null)
        {
            //send new code via email
            var EmailParam = _conf.MailConfig;
            EmailParam.To = new List<string>() { _email };
            EmailParam.Subject = _eSubject;
            EmailParam.Body = _eBody;

            var RetStatus = SendEmailByEmailAddress(new List<string>() { _email }, EmailParam, LogManager);

            return RetStatus;
        }

        void SendCodeViaSMS(SmsParameter _smsParams, string mobileNumber,string code) 
        {
            MainHttpClient UserHttpClient = new MainHttpClient(APIConfig);
            _smsParams.To = mobileNumber;
            _smsParams.Text = string.Format(_smsParams.Text, code);

            var ParameterBuilder = "?account=" + _smsParams.Action;
            ParameterBuilder += "&login=" + _smsParams.User;
            ParameterBuilder += "&password=" + _smsParams.Password;
            ParameterBuilder += "&from=" + _smsParams.From;
            ParameterBuilder += "&to=" + _smsParams.To;
            ParameterBuilder += "&message=" + Uri.EscapeUriString(_smsParams.Text);

            var apiCall = UserHttpClient.SendSmsHttpClientRequestAsync(ParameterBuilder);
            LogManager.LogInfo("SendCodeViaSMS");
            LogManager.LogDebugObject(apiCall);
        }

        void CheckUserVerificationCode(User _user, VerificationTypes _vTypes, string _vCode)
        {
            LogManager.LogInfo("CheckUserVerificationCode: " + _user);

            UserVerificationCode FoundUserVerificationCode = new UserVerificationCode();

            FoundUserVerificationCode = DbContext.UserVerificationCodes.AsNoTracking()
                              .FirstOrDefault(u => u.UserId == _user.UserId && u.IsEnabled == true && u.VerificationType == _vTypes);

            //has existing update code
            if (FoundUserVerificationCode != null)
            {
                FoundUserVerificationCode.VerificationCode = _vCode;
                FoundUserVerificationCode.LastEditedDate = DateTime.Now;
                FoundUserVerificationCode.LastEditedBy = FoundUserVerificationCode.UserId;

                DbContext.UserVerificationCodes.Update(FoundUserVerificationCode);
                DbContext.SaveChanges();
            }
            else
            {
                UserVerificationCode NewUserVerificationCode = new UserVerificationCode
                {
                    CreatedBy = _user.UserId,
                    CreatedDate = DateTime.Now,
                    IsEnabledBy = _user.UserId,
                    DateEnabled = DateTime.Now,
                    IsEnabled = true,

                    UserId = _user.UserId,
                    Email = _user.Email,
                    MobileNumber = _user.MobileNumber,
                    VerificationType = _vTypes,
                    VerificationCode = _vCode,
                    Device = _user.RegistrationPlatform
                };

                DbContext.Add(NewUserVerificationCode);
                DbContext.SaveChanges();
            }
        }

        public APIResponse MobileLogin(LoginCredentials _user, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            APIResponse ApiResp = new APIResponse();
            LogManager.LogInfo("MobileLogin: " + _user.Email + "Platform: " + _user.Device);
            LogManager.LogDebugObject(_user);

            try
            {
                //Check if user exist
                User foundUser = DbContext.Users.Where(u => u.Email == _user.Email || u.Username == _user.Username).FirstOrDefault();
                if (foundUser != null)
                {
                    //check if the user is verified
                    if (foundUser.IsLocked == true && foundUser.IsEnabled == false)
                    {
                        LogManager.LogError("MobileLogin > Compte utilisateur non vérifié. " + foundUser.Email);
                        ApiResp.Message = "Compte utilisateur non vérifié.";
                        ApiResp.Status = "Échec";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.OK;
                        ApiResp.ResponseCode = APIResponseCode.UserIsNotYetVerified;

                        return ApiResp;
                    }
                    else
                    {
                        //check if password matches
                        LogManager.LogInfo("CheckPassword");
                        if (foundUser.Password == foundUser.HashP(_user.Password.ToString(), APIConfig.TokenKeys.Key))
                        {
                            //check if user has address
                            int hasAddress = DbContext.UserAddresses.AsNoTracking().Where(u => u.UserId == foundUser.UserId && u.IsEnabled == true).Count();
                            if (hasAddress == 0)
                            {
                                ApiResp.ResponseCode = APIResponseCode.UserHasNoAddress;
                            }

                            //Remove hash password value before sending back to user!
                            ApiResp.Payload = new UserContext
                            {
                                UserInfo = foundUser,
                                Tokens = AddLoginTransactionForUser(foundUser)
                            };

                            ApiResp.Message = "Connexion réussie.";
                            ApiResp.Status = "Succès";
                            ApiResp.StatusCode = System.Net.HttpStatusCode.OK;

                            LogManager.LogInfo("Connexion réussie with user :" + _user.Email);
                            LogManager.LogDebugObject(foundUser);
                        }
                        else
                        {
                            ApiResp.Message = "Mot de passe incorrect.";
                            ApiResp.Status = "Échec";
                            ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                            LogManager.LogError("The login details you have provided are not correct. Please try again with valid credentials.");
                        }
                    }
                }
                else
                {
                    LogManager.LogError("MobileLogin > Compte non trouvé. " + _user.Email + "/" + _user.Username);
                    ApiResp.Message = "Compte non trouvé.";
                    ApiResp.Status = "Échec";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("MobileLogin: " + _user.Email + "Platform: " + _user.Device);
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                ApiResp.Message = "Quelque chose s'est mal passé !";
                ApiResp.Status = "Erreur de serveur interne";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }
            return ApiResp;
        }

        UserLoginTransaction AddLoginTransactionForUser(User _user)
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
                               .FirstOrDefault(u => u.UserId == _user.UserId &&
                                           u.Device == _user.RegistrationPlatform &&
                                           u.IsActive == true &&
                                           u.TokenExpiration > DateTime.Now);

                if (ulT != null) // deactivate existing
                {
                    ulT.IsActive = false;
                }

                // create new
                ulT = new UserLoginTransaction
                {
                    UserId = _user.UserId,
                    AccountType = _user.AccountType,
                    Device = _user.RegistrationPlatform,
                    Token = CreateJWTToken(_user, ExpirationDates, APIConfig),
                    TokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp),
                    RefreshToken = Guid.NewGuid().ToString().Replace("-", ""),
                    RefreshTokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp * 2),
                    IsActive = true,
                    DateCreated = DateTime.Now
                };

                DbContext.UserLoginTransactions.Add(ulT);
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("AddLoginTransactionForUser");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
            }

            return ulT;
        }

        public APIResponse SetFCMToken(string UserToken, string FCMToken, DevicePlatforms DeviceType)
        {
            LogManager.LogInfo("SetFCMToken: Platform: " + DeviceType);
            APIResponse ApiResp = new APIResponse();

            try
            {
                UserToken = UserToken.Split(' ')[1];
                var foundUserTrans = DbContext.UserLoginTransactions.AsNoTracking().
                                               OrderByDescending(ult => ult.DateCreated).
                                               FirstOrDefault(ult => ult.Token == UserToken);

                // Check if latest unexpired user login transaction exists
                LogManager.LogInfo("Check if auth token is valid: " + UserToken);
                LogManager.LogDebugObject(foundUserTrans);
                if (foundUserTrans != null && foundUserTrans.TokenExpiration > DateTime.Now)
                {
                    LogManager.LogInfo("Auth token is valid");
                    LogManager.LogInfo("Check user device fcm token");
                    //Old
                    //var foundUserDevice = DbContext.UserDevices.FirstOrDefault(ud => ud.UserId == foundUserTrans.UserId &&
                    //                                                                 ud.DeviceType == DeviceType &&
                    //                                                                 ud.DeviceFCMToken == FCMToken);

                    var foundUserDevice = DbContext.UserDevices.FirstOrDefault(ud => ud.UserId == foundUserTrans.UserId &&
                                                                                     ud.DeviceType == DeviceType);
                    LogManager.LogDebugObject(foundUserDevice);

                    // Check if user device is not yet registered
                    if (foundUserDevice == null)
                    {
                        LogManager.LogInfo("No FCM device found for user id: " + foundUserTrans.UserId + " and FCM token: " + FCMToken);
                        // If non existing, create new record
                        foundUserDevice = new UserDevice
                        {
                            UserId = foundUserTrans.UserId,
                            DeviceFCMToken = FCMToken,
                            DeviceType = DeviceType,
                            CreatedDate = DateTime.Now,
                            CreatedBy = foundUserTrans.UserId,
                            LastEditedDate = DateTime.Now,
                            LastEditedBy = foundUserTrans.UserId,
                            IsEnabled = true,
                            IsEnabledBy = foundUserTrans.UserId,
                            DateEnabled = DateTime.Now,
                            IsLocked = false
                        };
                        DbContext.Add(foundUserDevice);
                        DbContext.SaveChanges();
                        LogManager.LogInfo("Added new device : ");
                        LogManager.LogDebugObject(foundUserDevice);
                        LogManager.LogInfo("Added FCM Token Succès");
                    }
                    else
                    {
                        // Re-enable token; probably user has logged out and logged in again using the same fcm
                        LogManager.LogInfo("Old fcm token found");
                        LogManager.LogDebugObject(foundUserDevice);
                        foundUserDevice.DeviceFCMToken = FCMToken;
                        foundUserDevice.DeviceType = DeviceType;

                        foundUserDevice.IsEnabled = true;
                        foundUserDevice.LastEditedDate = DateTime.Now;
                        foundUserDevice.LastEditedBy = foundUserTrans.UserId;
                        foundUserDevice.IsEnabledBy = foundUserTrans.UserId;
                        foundUserDevice.DateEnabled = DateTime.Now;
                        DbContext.Update(foundUserDevice);
                        DbContext.SaveChanges();
                        LogManager.LogInfo("Enabled Old fcm token");
                        LogManager.LogInfo("Update FCM Token Succès");
                    }

                    ApiResp.Message = "Update FCM Token Succès!";
                    ApiResp.Status = "Succès";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {

                    ApiResp.Message = "User login was not available";
                    ApiResp.Status = "Transaction de connexion de l'utilisateur non trouvée";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("SetFCMToken: Platform: " + DeviceType);
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                ApiResp.Message = "Quelque chose s'est mal passé !";
                ApiResp.Status = "Erreur de serveur interne";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }
            return ApiResp;
        }

        public APIResponse ReGenerateTokens(UserLoginTransaction _user)
        {
            APIResponse ApiResp = new APIResponse();
            LogManager.LogInfo("ReGenerateTokens");
            try
            {
                LogManager.LogDebugObject(_user);
                //check if the requesting user is valid
                var foundUser = DbContext.Users.FirstOrDefault(u => u.UserId == _user.UserId);
                DateTime ExpirationDates = DateTime.Now;
                UserLoginTransaction ulTransactions = new UserLoginTransaction();
                if (foundUser != null)
                {
                    // user id is valid
                    // check if tokens are valid
                    ulTransactions = DbContext.UserLoginTransactions.FirstOrDefault(ult => ult.Token == _user.Token
                                                                                        && ult.RefreshToken == _user.RefreshToken
                                                                                        && ult.RefreshTokenExpiration > ExpirationDates
                                                                                        && ult.UserId == _user.UserId);
                    if (ulTransactions != null)
                    {
                        // tokens are valid lets make a new one
                        UserLoginTransaction newUserLoginTrans = new UserLoginTransaction
                        {
                            UserId = foundUser.UserId,
                            Device = _user.Device,
                            AccountType = _user.AccountType,
                            Token = CreateJWTToken(foundUser, ExpirationDates, APIConfig),
                            TokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp),
                            RefreshToken = Guid.NewGuid().ToString().Replace("-", ""),
                            RefreshTokenExpiration = ExpirationDates.AddMinutes(APIConfig.TokenKeys.Exp * 2),
                            IsActive = true,
                            DateCreated = DateTime.Now
                        };

                        DbContext.UserLoginTransactions.Add(newUserLoginTrans);
                        DbContext.SaveChanges();

                        ApiResp.Message = "Rafraîchir le code d'accès régénéré";
                        ApiResp.Status = "Succès";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.OK;

                        ApiResp.Payload = new UserContext
                        {
                            UserInfo = foundUser,
                            Tokens = newUserLoginTrans
                        };
                    }
                    else
                    {
                        ApiResp.Message = "Connexion expirée ! Veuillez vous reconnecter !";
                        ApiResp.Status = "Erreur!";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    }
                }
                else
                {
                    ApiResp.Message = "Connexion expirée ! Veuillez vous reconnecter !";
                    ApiResp.Status = "Erreur!";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);

                ApiResp.Message = "Quelque chose s'est mal passé !";
                ApiResp.Status = "Erreur de serveur interne";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }
            return ApiResp;
        }

        public APIResponse MobileLogout(LogoutCredentials _user)
        {
            APIResponse ApiResp = new APIResponse();
            try
            {
                LogManager.LogInfo("MobileLogout");
                LogManager.LogDebugObject(_user);

                //Check if user device exist
                var foundUserDevice = DbContext.UserDevices.FirstOrDefault(ud => ud.DeviceFCMToken == _user.DeviceFCMToken &&
                                                                                 ud.UserId == _user.UserId);

                if (foundUserDevice != null)
                {
                    foundUserDevice.IsEnabled = false;
                    foundUserDevice.LastEditedBy = _user.UserId;
                    foundUserDevice.LastEditedDate = DateTime.Now;
                    DbContext.Update(foundUserDevice);

                    var foundUserLoginTransaction = DbContext.UserLoginTransactions.FirstOrDefault(ult => ult.Token == _user.AuthToken);
                    if (foundUserLoginTransaction != null)
                    {
                        foundUserLoginTransaction.IsActive = false;
                        DbContext.Update(foundUserLoginTransaction);
                    }

                    DbContext.SaveChanges();

                    ApiResp.Message = "Code FCM désactivé";
                    ApiResp.Status = "Déconnexion réussie!";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.OK;

                }
                else
                {
                    ApiResp.Message = "Dispositif utilisateur non trouvé";
                    ApiResp.Status = "Code FCM invalide";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.OK; // it's ok if token is not found, just return ok status
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("MobileLogout");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                ApiResp.Message = "Quelque chose s'est mal passé !";
                ApiResp.Status = "Erreur de serveur interne";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }
            return ApiResp;
        }

        public APIResponse FullUserRegistration(FullUserRegForm _user)
        {
            APIResponse apiResp = new APIResponse();
            LogManager.LogInfo("FullUserRegistration: " + _user.Email + " Platform: " + _user.DevicePlatform);
            LogManager.LogDebugObject(_user);

            try
            {
                //Check if the email exist in the database
                User FoundUser = DbContext.Users.AsNoTracking().Where(u => u.Email == _user.Email || u.MobileNumber == _user.MobileNumber || u.Username == _user.Username).FirstOrDefault();
                if (FoundUser == null)
                {
                    Guid UserGuidID = Guid.NewGuid();
                    DateTime RegistrationDate = DateTime.Now;
                    string registrationCode = GenerateUniqueCodeForUser(DbContext);

                    User NewUser = new User
                    {
                        CreatedBy = UserGuidID,
                        CreatedDate = RegistrationDate,
                        IsEnabledBy = UserGuidID,
                        DateEnabled = RegistrationDate,
                        IsEnabled = false,

                        UserId = UserGuidID,
                        AccountType = _user.AccountType,
                        Email = _user.Email,
                        Username = _user.Username,
                        FirstName = _user.FirstName,
                        LastName = _user.LastName,
                        RegistrationCode = registrationCode,
                        LockedDateTime = RegistrationDate,
                        IsLocked = true, // = Unverified

                        MobileNumber = _user.MobileNumber,
                        DateOfBirth = null,
                        Gender = 0,
                        ImageUrl = null,
                        ProfessionalID = _user.ProfessionalID,
                        KBIS = _user.KBIS,
                        UserFieldID = _user.UserFieldID,
                        HealthNumber = _user.HealthNumber,
                        MethodDelivery = _user.MethodDelivery,
                        RegistrationPlatform = _user.DevicePlatform,
                        IsDecline = false
                    };
                    NewUser.Password = NewUser.HashP(_user.Password.ToString(), APIConfig.TokenKeys.Key);

                    APIResponse MangoPayUserCreation = PaymentRepo.CreateUserNaturalUserInMangoPay(NewUser);

                    DbContext.Add(NewUser);
                    DbContext.SaveChanges();
                   
                    LogManager.LogInfo("-- New User Added --");
                    LogManager.LogDebugObject(NewUser);
                    
                    if(_user.AccountType == AccountTypes.APPUSER)
                    {
                        //send registration code via email
                        var emailBody = String.Format(APIConfig.MsgConfigs.RegisterMobileUser, NewUser.FirstName, registrationCode);
                        var sendStatus = SendVerificationCodeViaEmailOrSMS(NewUser.Email, emailBody, "PharmaMoov: Registration Code", APIConfig);
                    }
                    apiResp = new APIResponse
                    {
                        Message = (_user.AccountType == AccountTypes.HEALTHPROFESSIONAL || _user.AccountType == AccountTypes.COURIER) ? "Votre inscription a bien été prise en compte. Votre compte est en cours d’approbation." : "Compte créé avec succès.",
                        Status = "Succès",
                        Payload = NewUser,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    if (FoundUser.IsLocked == true)
                    {
                        LogManager.LogError("FullUserRegistration >> Compte utilisateur non vérifié." + _user.Email);
                        apiResp = new APIResponse
                        {
                            Message = "L'adresse email/numéro de téléphone a été enregistrée mais non verifiée",
                            Status = "Échec",
                            StatusCode = System.Net.HttpStatusCode.BadRequest,
                            ResponseCode = APIResponseCode.UserIsNotYetVerified
                        };
                    }
                    else
                    {
                        if (FoundUser.Email == _user.Email)
                        {
                            LogManager.LogError("FullUserRegistration >> L'email existe déjà." + _user.Email);
                            apiResp = new APIResponse
                            {
                                Message = "L'email existe déjà.",
                                Status = "Échec",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                        else if (FoundUser.Username == _user.Username)
                        {
                            LogManager.LogError("FullUserRegistration >> Le nom d'utilisateur existe déjà." + _user.Username);
                            apiResp = new APIResponse
                            {
                                Message = "Le nom d'utilisateur existe déjà.",
                                Status = "Échec",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };

                        }
                        else 
                        {
                            LogManager.LogError("FullUserRegistration >> Mobile Number already exists." + _user.MobileNumber);
                            apiResp = new APIResponse
                            {
                                Message = "Mobile Number already exists.",
                                Status = "Échec",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError("FullUserRegistration: " + _user.Email + " Platform: " + _user.DevicePlatform);
                LogManager.LogDebugObject(ex);
                apiResp = new APIResponse
                {
                    Message = "Server Erreur!",
                    Status = "Erreur de serveur interne!",
                    StatusCode = System.Net.HttpStatusCode.BadRequest

                };
            }

            return apiResp;

        }

        public APIResponse LoginEmailOrUsername(LoginEmailUsername _user) 
        {

            APIResponse ApiResp = new APIResponse();
            LogManager.LogInfo("LoginEmailOrUsername: " + _user.LoginName + "Platform: " + _user.Device);
            LogManager.LogDebugObject(_user);

            try
            {
                //Check if user exist
                //Select Customer, COURIER and Health Professional User
                User foundUser = DbContext.Users.Where(u => u.Email == _user.LoginName || u.Username == _user.LoginName).FirstOrDefault(); //&& u.AccountType != AccountTypes.COURIER
                if (foundUser != null)
                {
                    //check if the user is verified
                    if (foundUser.IsLocked == true && foundUser.IsEnabled == false)
                    {
                        LogManager.LogError("LoginEmailOrUsername > Compte utilisateur non vérifié. " + foundUser.Email);
                        ApiResp.Message = "Compte utilisateur non vérifié.";
                        ApiResp.Status = "Échec";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                        ApiResp.ResponseCode = APIResponseCode.UserIsNotYetVerified;
                        ApiResp.Payload = new
                        {
                            UserInfo = foundUser
                        };
                        return ApiResp;
                    }
                    else if (foundUser.IsEnabled == false)
                    {
                        LogManager.LogError("LoginEmailOrUsername > Compte utilisateur non vérifié. " + foundUser.Email);
                        ApiResp.Message = "Votre compte n'est pas approuvé par l'administrateur.";
                        ApiResp.Status = "Échec";
                        ApiResp.StatusCode = System.Net.HttpStatusCode.OK;
                        ApiResp.ResponseCode = APIResponseCode.UserIsNotYetVerified;
                        ApiResp.Payload = new
                        {
                            UserInfo = foundUser
                        };
                        return ApiResp;
                    }
                    else
                    {
                        //check if password matches
                        LogManager.LogInfo("CheckPassword");
                        if (foundUser.Password == foundUser.HashP(_user.Password.ToString(), APIConfig.TokenKeys.Key))
                        {
                            //check if user has address
                            int hasAddress = DbContext.UserAddresses.AsNoTracking().Where(u => u.UserId == foundUser.UserId && u.IsEnabled == true).Count();
                            if (hasAddress == 0)
                            {
                                ApiResp.ResponseCode = APIResponseCode.UserHasNoAddress;
                            }

                            //Remove hash password value before sending back to user!
                            ApiResp.Payload = new UserContext
                            {
                                UserInfo = foundUser,
                                Tokens = AddLoginTransactionForUser(foundUser)
                            };

                            ApiResp.Message = "Connexion réussie.";
                            ApiResp.Status = "Succès";
                            ApiResp.StatusCode = System.Net.HttpStatusCode.OK;

                            LogManager.LogInfo("Connexion réussie with user :" + _user.LoginName);
                            LogManager.LogDebugObject(foundUser);
                        }
                        else
                        {
                            ApiResp.Message = "Mot de passe incorrect.";
                            ApiResp.Status = "Échec";
                            ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                            LogManager.LogError("The login details you have provided are not correct. Please try again with valid credentials.");
                        }
                    }
                }
                else
                {
                    LogManager.LogError("LoginEmailOrUsername > Compte non trouvé. " + _user.LoginName);
                    ApiResp.Message = "Compte non trouvé.";
                    ApiResp.Status = "Échec";
                    ApiResp.StatusCode = System.Net.HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("LoginEmailOrUsername: " + _user.LoginName + "Platform: " + _user.Device);
                LogManager.LogDebugObject(ex);
                ApiResp.Message = "Quelque chose s'est mal passé !";
                ApiResp.Status = "Erreur de serveur interne";
                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                ApiResp.ModelError = GetStackError(ex.InnerException);
            }
            return ApiResp;
        }

        #region Section: For Customer, Courier and Health Prof under Super Admin
        public APIResponse ChangeUserStatus(string Authorization, ChangeRecordStatus _userStat)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ChangeCustomerStatus");
            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == Authorization && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    User foundUser = DbContext.Users.Where(a => a.UserRecordID == _userStat.RecordId).FirstOrDefault();
                    if (foundUser != null)
                    {
                        if((foundUser.AccountType == AccountTypes.COURIER || foundUser.AccountType == AccountTypes.HEALTHPROFESSIONAL) && foundUser.IsLocked == true && _userStat.IsActive == true)
                        {
                            aResp = new APIResponse
                            {
                                Message = "Veuillez d'abord accepter la demande.",
                                Status = "Échec!",
                                StatusCode = System.Net.HttpStatusCode.BadRequest
                            };
                            return aResp;
                        }

                        DateTime NowDate = DateTime.Now;
                        foundUser.LastEditedDate = DateTime.Now;
                        foundUser.LastEditedBy = _userStat.AdminId;
                        foundUser.DateEnabled = DateTime.Now;
                        foundUser.IsEnabledBy = _userStat.AdminId;
                        foundUser.IsEnabled = _userStat.IsActive;

                        DbContext.Users.Update(foundUser);
                        DbContext.SaveChanges();

                        if (_userStat.IsActive == false)
                        {
                            //Deactivate also the token to force logout the user
                            List<UserLoginTransaction> getLoginTrans = DbContext.UserLoginTransactions
                                                      .Where(u => u.UserId == foundUser.UserId && u.IsActive == true).ToList();

                            if (getLoginTrans != null)
                            {
                                foreach (var uTrans in getLoginTrans)
                                {
                                    uTrans.IsActive = false;
                                    DbContext.UserLoginTransactions.Update(uTrans);
                                }

                                DbContext.SaveChanges();
                            }
                        }

                        aResp = new APIResponse
                        {
                            Message = "Utilisateur mis à jour avec succès.",
                            Status = "Succès!",
                            Payload = foundUser,
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
                LogManager.LogInfo("ChangeCustomerStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse EditUserProfile(string Authorization, User _userProfile)
        {
            APIResponse ApiResp = new APIResponse();
            LogManager.LogInfo("EditUserProfile");
            LogManager.LogDebugObject(_userProfile);

            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == Authorization && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    LogManager.LogInfo("EditUserProfile intiated by SAdminID: " + IsUserLoggedIn.UserId + " Platform: " + IsUserLoggedIn.Device);
                    //Check if user exist
                    var foundUser = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserRecordID == _userProfile.UserRecordID);
                    if (foundUser != null)
                    {
                        // if email is changed
                        if (foundUser.Email != _userProfile.Email && _userProfile.Email != string.Empty)
                        {
                            // Check if email already belongs to another user
                            var foundDuplicateEmail = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.Email == _userProfile.Email);
                            if (foundDuplicateEmail != null)
                            {
                                LogManager.LogError("EditUserProfile >> L'adresse email est déjà utilisée" + _userProfile.Email);
                                ApiResp.Message = "L'adresse email est déjà utilisée";
                                ApiResp.Status = "Échec";
                                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;

                                return ApiResp;
                            }
                        }

                        // if phone number is changed
                        if (foundUser.MobileNumber != _userProfile.MobileNumber && _userProfile.MobileNumber != string.Empty)
                        {
                            // Check if phone number already belongs to another user
                            var foundDuplicatePhoneNumber = DbContext.Users.AsNoTracking().FirstOrDefault(u => u.MobileNumber == _userProfile.MobileNumber);
                            if (foundDuplicatePhoneNumber != null)
                            {
                                LogManager.LogError("EditUserProfile >> Le numéro de téléphone est déjà utilisé" + _userProfile.MobileNumber);
                                ApiResp.Message = "Le numéro de téléphone est déjà utilisé";
                                ApiResp.Status = "Échec";
                                ApiResp.StatusCode = System.Net.HttpStatusCode.BadRequest;

                                return ApiResp;
                            }
                        }

                        // This phone number should be verified from the mobile app first
                        foundUser.MobileNumber = _userProfile.MobileNumber != null && _userProfile.MobileNumber.Trim() != string.Empty ? _userProfile.MobileNumber : foundUser.MobileNumber;
                        foundUser.Email = _userProfile.Email != null && _userProfile.Email.Trim() != string.Empty ? _userProfile.Email : foundUser.Email;
                        foundUser.FirstName = _userProfile.FirstName != null && _userProfile.FirstName.Trim() != string.Empty ? _userProfile.FirstName : foundUser.FirstName;
                        foundUser.LastName = _userProfile.LastName != null && _userProfile.LastName.Trim() != string.Empty ? _userProfile.LastName : foundUser.LastName;
                        foundUser.ImageUrl = _userProfile.ImageUrl;
                        foundUser.KBIS = _userProfile.KBIS;
                        foundUser.MethodDelivery = _userProfile.MethodDelivery;
                        foundUser.ProfessionalID = _userProfile.ProfessionalID;
                        foundUser.IsEnabled = _userProfile.IsEnabled;

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
                        LogManager.LogInfo("Enregistrement non trouvé." + _userProfile.UserId);
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
        #endregion
        public APIResponse ChangeAcceptOrDeclineRequest(string Authorization, ChangeAcceptOrDeclineRequestStatus model, IMainHttpClient _mhttp, APIConfigurationManager _conf = null)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ChangeAcceptOrDeclineRequest");
            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == Authorization && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    User foundUser = DbContext.Users.Where(a => a.UserRecordID == model.RecordId).FirstOrDefault();
                    if (foundUser != null)
                    {
                        DateTime NowDate = DateTime.Now;
                        foundUser.LastEditedDate = DateTime.Now;
                        foundUser.LastEditedBy = model.AdminId;
                        foundUser.DateEnabled = DateTime.Now;
                        foundUser.IsEnabledBy = model.AdminId;
                        foundUser.IsEnabled = true;
                        foundUser.IsLocked = model.IsLocked;
                        foundUser.IsDecline = model.IsLocked == true ? true : false;
                        DbContext.Users.Update(foundUser);
                        DbContext.SaveChanges();

                        if(model.IsLocked == false) // Verified account
                        {
                            //Send email to user

                            var emailBody = String.Format(APIConfig.MsgConfigs.ApproveUserAccount, foundUser.FirstName);
                            var sendStatus = SendVerificationCodeViaEmailOrSMS(foundUser.Email, emailBody, "PharmaMoov: Approved account", _conf);
                        }
                        aResp = new APIResponse
                        {
                            Message = "Utilisateur mis à jour avec succès.",
                            Status = "Succès!",
                            Payload = foundUser,
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
                LogManager.LogInfo("ChangeAcceptOrDeclineRequest");
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
