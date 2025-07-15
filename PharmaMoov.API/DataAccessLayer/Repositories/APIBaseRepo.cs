using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.User;
using PharmaMoov.API.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using PharmaMoov.PushNotification;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class APIBaseRepo
    {
        public string GenerateUniqueCodeForUser(APIDBContext _dbCtxt)
        {
            bool IsCodeUnique = false;
            //
            // generate random code
            //
            string code = GenerateUniqeCode(4, true, false);
            while (IsCodeUnique == false)
            {
                //check if random code is Uniqie
                if (_dbCtxt.Users.Where(u => u.RegistrationCode == code).ToList().Count == 0)
                {
                    IsCodeUnique = true;
                }
                else
                {
                    IsCodeUnique = false;
                    code = GenerateUniqeCode(8, true, false);
                }
            }
            return code;
        }

        public string GenerateUniqeCode(int _requiredLength, bool _requireDigit, bool _requireUppercase)
        {

            //int requiredUniqueChars = -1;
            //bool requireDigit = true;
            //bool requireNonAlphanumeric = false;
            //bool requireUppercase = true;
            //bool requireLowercase = false;

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "0123456789"                 // digits
            };
            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (_requireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (_requireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            var theBit = _requireUppercase ? 0 : 1;
            for (int i = chars.Count; i < _requiredLength; i++)
            {
                string rcs = randomChars[rand.Next(theBit, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        public string CreateJWTToken(User _logindetails, DateTime _notBefore, APIConfigurationManager _apiConf)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, _logindetails.Email),
                new Claim(JwtRegisteredClaimNames.Sub, _logindetails.AccountType.ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, _apiConf.TokenKeys.Issuer),
                new Claim(JwtRegisteredClaimNames.Aud, _apiConf.TokenKeys.Audience),
                new Claim(JwtRegisteredClaimNames.Exp, _notBefore.AddMinutes(_apiConf.TokenKeys.Exp).ToString()),
                new Claim(JwtRegisteredClaimNames.Sid, _logindetails.UserId.ToString())
            };
            SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_apiConf.TokenKeys.Key));
            var jwt = new JwtSecurityToken(
                issuer: _apiConf.TokenKeys.Issuer,
                audience: _apiConf.TokenKeys.Audience,
                claims: claims,
                notBefore: _notBefore,
                expires: _notBefore.AddMinutes(_apiConf.TokenKeys.Exp),
                signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        public string CreateJWTTokenForAdmin(Admin _logindetails, DateTime _notBefore, APIConfigurationManager _apiConf)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, _logindetails.Email),
                new Claim(JwtRegisteredClaimNames.Sub, _logindetails.UserTypeId.ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, _apiConf.TokenKeys.Issuer),
                new Claim(JwtRegisteredClaimNames.Aud, _apiConf.TokenKeys.Audience),
                new Claim(JwtRegisteredClaimNames.Exp, _notBefore.AddMinutes(_apiConf.TokenKeys.Exp).ToString()),
                new Claim(JwtRegisteredClaimNames.Sid, _logindetails.AdminId.ToString())
            };
            SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_apiConf.TokenKeys.Key));
            var jwt = new JwtSecurityToken(
                issuer: _apiConf.TokenKeys.Issuer,
                audience: _apiConf.TokenKeys.Audience,
                claims: claims,
                notBefore: _notBefore,
                expires: _notBefore.AddMinutes(_apiConf.TokenKeys.Exp),
                signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        public IEnumerable<KeyValuePair<string, string[]>> GetStackError(Exception ex)
        {
            return new[] { new KeyValuePair<string, string[]>(ex.Message, SplitStackTrace(ex.StackTrace)) };
        }

        public List<FCMReturnData> SendPushNotification(ILoggerManager LogManager, PushNotificationData _pushNotifData, NonSilentNotification _silentNotifData, MainHttpClient _httpc, APIDBContext _dbCtxt, bool _isAsyncCall = false)
        {
            if (_pushNotifData.Platform == 2)
            {
                //for Android
                _silentNotifData.icon = "ic_chp_logo";
                _silentNotifData.sound = "";
                _silentNotifData.android_channel_id = "CHP_CHANNEL_ID";
            }

            if (_pushNotifData.Platform != null && _pushNotifData.Platform != 0)
            {
                List<UserDevice> FCMTokens = GetFCMToken(_pushNotifData.UserId, _dbCtxt);
                List<FCMReturnData> returnData = new List<FCMReturnData>();

                foreach (UserDevice ud in FCMTokens)
                {
                    if (ud.IsEnabled == true)
                    {
                        // Ensure content has no null values
                        _pushNotifData.Subtitle = _pushNotifData.Subtitle == null ? string.Empty : _pushNotifData.Subtitle;
                        _pushNotifData.Header = _pushNotifData.Header == null ? string.Empty : _pushNotifData.Header;
                        _pushNotifData.Description = _pushNotifData.Description == null ? string.Empty : _pushNotifData.Description;
                        _pushNotifData.Platform = (int)ud.DeviceType;

                        PushNotificationPayload pnPayload = new PushNotificationPayload
                        {
                            content_available = false,
                            priority = "high",
                            notification = _silentNotifData,
                            data = new PushNotificationData
                            {
                                Subtitle = Regex.Replace(_pushNotifData.Subtitle, @"<(.|\n)*?>", string.Empty),
                                Header = Regex.Replace(_pushNotifData.Header, @"<(.|\n)*?>", string.Empty),
                                Description = Regex.Replace(_pushNotifData.Description, @"<(.|\n)*?>", string.Empty),
                                orderId = _pushNotifData.orderId,
                                UserId = _pushNotifData.UserId,
                                IsPaymentSuccessful = _pushNotifData.IsPaymentSuccessful
                            },
                            sound = _silentNotifData.sound,
                            to = ud.DeviceFCMToken
                        };

                        LogManager.LogInfo("-- Pushing to this device --");
                        LogManager.LogDebugObject(ud);

                        // If call is synchronous
                        if (!_isAsyncCall)
                        {
                            FCMReturnData FCMResponse = JsonConvert.DeserializeObject<FCMReturnData>(_httpc.PostHttpClientRequest("", pnPayload, _pushNotifData.Platform));
                            FCMResponse.results[0].info = "uid:" + _pushNotifData.UserId + " fcmtoken:" + ud.DeviceFCMToken;
                            returnData.Add(FCMResponse);
                        }
                        else // else if async (for bulk sending)
                        {
                            var returnDataN = _httpc.PostHttpClientRequestAsync("", pnPayload, _pushNotifData.Platform);
                        }
                    }
                }
                return returnData;
            }
            return null;
        }

        List<UserDevice> GetFCMToken(Guid UserId, APIDBContext _dbCtxt)
        {
            List<UserDevice> foundUDevice = _dbCtxt.UserDevices.Where(ud => ud.UserId == UserId && ud.IsEnabled == true && ud.DeviceFCMToken != null).ToList();
            if (foundUDevice != null) { return foundUDevice; } else { return null; }
        }

        public string[] SplitStackTrace(string stackT)
        {
            string[] stackTraceTemp = stackT.Split("\r\n");
            List<string> returnStack = new List<string>();
            foreach (var stack in stackTraceTemp)
            {
                returnStack.Add(stack);
            }
            return returnStack.ToArray();
        }

        public string SendSmsAsync(MainHttpClient _httpc, SmsParameter _smsConfig)
        {
            var ParameterBuilder = "?account=" + _smsConfig.Action;
            ParameterBuilder += "&login=" + _smsConfig.User;
            ParameterBuilder += "&password=" + _smsConfig.Password;
            ParameterBuilder += "&from=" + _smsConfig.From;
            ParameterBuilder += "&to=" + _smsConfig.To;
            ParameterBuilder += "&message=" + Uri.EscapeUriString(_smsConfig.Text);

            var apiCall = _httpc.SendSmsHttpClientRequestAsync(ParameterBuilder);

            return "Sending SMS Asynchronously.. to:" + _smsConfig.To + " msg: " + _smsConfig.Text;

        }

        /// <summary>
        /// Base push notification
        /// </summary>
        /// <param name="_pushNotifData"></param>
        /// <param name="_httpc"></param>
        /// <param name="_dbCtxt"></param>
        /// <param name="_isAsyncCall"></param>
        /// <returns></returns>
        //public List<FCMReturnData> PushNotification(ILoggerManager LogManager, PushNotificationData _pushNotifData, NonSilentNotification _silentNotifData, MainHttpClient _httpc, APIDBContext _dbCtxt, bool _isAsyncCall = false)
        //{
        //    if (_pushNotifData.Platform == 2)
        //    {
        //        //for Android
        //        _silentNotifData.icon = "ic_alz_logo";
        //        _silentNotifData.sound = "";
        //        _silentNotifData.android_channel_id = "ALZ_CHANNEL_ID";
        //    }

        //    if (_pushNotifData.Platform != null && _pushNotifData.Platform != 0)
        //    {
        //        List<UserDevice> FCMTokens = GetFCMToken(_pushNotifData.UserId, (DevicePlatforms)_pushNotifData.Platform, _dbCtxt);
        //        List<FCMReturnData> returnData = new List<FCMReturnData>();

        //        Dictionary<Guid, int> UnreadCountCache = new Dictionary<Guid, int>();

        //        foreach (UserDevice ud in FCMTokens)
        //        {
        //            if (ud.UserDeviceFCMToken != string.Empty && ud.IsEnabled == true)
        //            {

        //                var UnReadCount = 0;
        //                if (UnreadCountCache.ContainsKey(ud.UserDeviceOwnerID))
        //                {
        //                    UnReadCount = UnreadCountCache[ud.UserDeviceOwnerID];
        //                }
        //                else
        //                {
        //                    // Get all unread push messages
        //                    var Unread = _dbCtxt.PushNotificationsPerUser.Where(pn => pn.IsEnabled == true &&
        //                                                                              pn.UserId == ud.UserDeviceOwnerID &&
        //                                                                              pn.CreatedDate > DateTime.Now.AddMonths(-2) &&
        //                                                                              pn.IsRead == false);

        //                    // add orders ready for pickup
        //                    int countRFPorders = 0;
        //                    countRFPorders = _dbCtxt.Orders.Where(ro => ro.CustomerID == _pushNotifData.UserId && (ro.OrderProgressStatus == OrderProgressStatus.READYFORPICKUP
        //                         || ro.OrderProgressStatus == OrderProgressStatus.OUTFORDELIVERY)).Count();

        //                    if (Unread != null)
        //                    {
        //                        UnReadCount = Unread.Count() + countRFPorders;
        //                    }

        //                    UnreadCountCache.Add(ud.UserDeviceOwnerID, UnReadCount);
        //                }

        //                // Ensure content has no null values
        //                _pushNotifData.Subtitle = _pushNotifData.Subtitle == null ? string.Empty : _pushNotifData.Subtitle;
        //                _pushNotifData.Header = _pushNotifData.Header == null ? string.Empty : _pushNotifData.Header;
        //                _pushNotifData.Description = _pushNotifData.Description == null ? string.Empty : _pushNotifData.Description;
        //                _pushNotifData.Platform = (int)ud.DeviceType;
        //                _silentNotifData.badge = UnReadCount;
        //                PushNotificationPayload pnPayload = new PushNotificationPayload
        //                {
        //                    content_available = false,
        //                    priority = "high",
        //                    notification = _silentNotifData,
        //                    data = new PushNotificationData
        //                    {
        //                        Subtitle = Regex.Replace(_pushNotifData.Subtitle, @"<(.|\n)*?>", string.Empty),
        //                        Header = Regex.Replace(_pushNotifData.Header, @"<(.|\n)*?>", string.Empty),
        //                        Description = Regex.Replace(_pushNotifData.Description, @"<(.|\n)*?>", string.Empty),
        //                        TransactionType = _pushNotifData.TransactionType,
        //                        Badge = UnReadCount,
        //                        OrderId = _pushNotifData.OrderId
        //                    },
        //                    sound = _silentNotifData.sound,
        //                    to = ud.UserDeviceFCMToken
        //                };

        //                LogManager.LogInfo("-- Pushing to this device --");
        //                LogManager.LogDebugObject(ud);

        //                // If call is synchronous
        //                if (!_isAsyncCall)
        //                {
        //                    FCMReturnData FCMResponse = JsonConvert.DeserializeObject<FCMReturnData>(_httpc.PostHttpClientRequest("", pnPayload, _pushNotifData.Platform));
        //                    FCMResponse.results[0].info = "uid:" + _pushNotifData.UserId + " fcmtoken:" + ud.UserDeviceFCMToken;
        //                    returnData.Add(FCMResponse);

        //                }
        //                else // else if async (for bulk sending)
        //                {
        //                    var returnDataN = _httpc.PostHttpClientRequestAsync("", pnPayload, _pushNotifData.Platform);
        //                }
        //            }
        //        }
        //        return returnData;
        //    }
        //    return null;
        //}

        List<UserDevice> GetFCMToken(Guid UserId, DevicePlatforms DeviceType, APIDBContext _dbCtxt)
        {
            List<UserDevice> foundUDevice = _dbCtxt.UserDevices.Where(ud => ud.UserId == UserId && ud.IsEnabled == true).ToList();
            if (foundUDevice != null) { return foundUDevice; } else { return null; }
        }

        /// <summary>
        /// et auto push notification content
        /// </summary>
        /// <param name="_notifData"></param>
        /// <param name="_dbCtxt"></param>
        /// <returns></returns>
        //public int SavePushNotification(PushNotificationBulk _notifData, APIDBContext _dbCtxt, PushNotificationType _notifType = PushNotificationType.Auto)
        //{
        //    PushNotificationModel NotifModel = new PushNotificationModel()
        //    {
        //        CreatedDate = DateTime.Now,
        //        CreatedBy = _notifData.CreatedBy,
        //        DateEnabled = DateTime.Now,
        //        IsEnabledBy = _notifData.CreatedBy,
        //        IsEnabled = true,
        //        LastEditedDate = DateTime.Now,
        //        LastEditedBy = _notifData.CreatedBy,
        //        IsLocked = false,
        //        Header = _notifData.Header,
        //        Subtitle = _notifData.Subtitle,
        //        Description = _notifData.Description,
        //        IsAlertOn = _notifData.IsAlertOn,
        //        DeliveryType = DeliveryType.Immediate,
        //        NotificationType = _notifType,
        //        FilterType = _notifData.FilterType,
        //        Gender = _notifData.Gender,
        //        FromBirthdate = _notifData.FromBirthdate,
        //        ToBirthdate = _notifData.ToBirthdate,
        //        Nationality = _notifData.Nationality,
        //        FromAge = _notifData.FromAge,
        //        ToAge = _notifData.ToAge
        //    };

        //    _dbCtxt.PushNotifications.Add(NotifModel);
        //    _dbCtxt.SaveChanges();

        //    return NotifModel.NotificationID;
        //}

        /// <summary>
        /// Set auto push notification for user
        /// </summary>
        /// <param name="_notifData"></param>
        /// <param name="_recipient"></param>
        /// <param name="_dbCtxt"></param>
        /// <returns></returns>
        //public PushNotificationPerUser SaveUserPushNotification(PushNotificationBulk _notifData, Guid _recipient, APIDBContext _dbCtxt, PushNotificationType _notifType = PushNotificationType.Auto)
        //{
        //    var notifId = SavePushNotification(_notifData, _dbCtxt, _notifType);

        //    var NotifData = new PushNotificationPerUser()
        //    {
        //        UserId = _recipient,
        //        NotificationID = notifId,
        //        Platform = (int)DevicePlatforms.Others,
        //        IsDelivered = false,
        //        CreatedDate = DateTime.Now,
        //        CreatedBy = _recipient,
        //        DateEnabled = DateTime.Now,
        //        IsEnabledBy = _recipient,
        //        IsEnabled = true,
        //        LastEditedDate = DateTime.Now,
        //        LastEditedBy = _recipient,
        //        IsLocked = false,
        //        IsRead = false
        //    };

        //    _dbCtxt.Add(NotifData);
        //    _dbCtxt.SaveChanges();

        //    return NotifData;
        //}MainHttpClient _httpc, 

        public async Task<string> SendEmailAsync(SMTPConfig _smtpConfig, ILoggerManager _logMgr)
        {
            SmtpClient client = new SmtpClient(_smtpConfig.Server, _smtpConfig.Port);
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(_smtpConfig.Username, _smtpConfig.Password);

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(_smtpConfig.Username);
            mailMessage.IsBodyHtml = true;

            foreach (var toAddress in _smtpConfig.To)
            {
                mailMessage.Bcc.Add(toAddress);
            }

            mailMessage.Body = _smtpConfig.Body;
            mailMessage.Subject = _smtpConfig.Subject;

            try
            {
                // Note : Must set https://www.google.com/settings/security/lesssecureapps to allow mail sending
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                _logMgr.LogInfo("SendEmailAsync");
                _logMgr.LogError(ex.Message);
                _logMgr.LogError(ex.StackTrace);
            }

            return "Sending Email Asynchronously.. to:" + string.Join(", ", _smtpConfig.To) + " subj: " + _smtpConfig.Subject;

        }

        //by single address
        public int SendEmailByEmailAddress(IEnumerable<string> _recipients, SMTPConfig _smtpConfig, ILoggerManager _logMgr)
        {
            _logMgr.LogInfo("SendEmailByEmail Start");
            _logMgr.LogDebugObject(_recipients);
            _logMgr.LogDebugObject(_smtpConfig);

            var retStatus = 0;

            try
            {
                // initialize email client
                SmtpClient client = new SmtpClient(_smtpConfig.Server, _smtpConfig.Port);
                client.UseDefaultCredentials = false;
                client.EnableSsl = _smtpConfig.EnableSSL;
                client.Credentials = new NetworkCredential(_smtpConfig.Username, _smtpConfig.Password);

                // initialize message
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_smtpConfig.Username);
                mailMessage.IsBodyHtml = true;

                // one recipient
                if (_recipients.Count() < 2)
                {
                    mailMessage.To.Add(_recipients.First());
                    _logMgr.LogInfo("Sending inquiry to one SA account.");
                }
                else // bulk
                {
                    // Sending Inquiry Email from Contact US
                    if (_smtpConfig.From != null)
                    {
                        _logMgr.LogInfo("Sending inquiry to multiple SA account.");
                       
                        foreach (var toAddress in _recipients)
                        {
                            mailMessage.To.Add(toAddress);
                        }
                    }
                    else
                    {
                        // set this because email sending does not seem to work without a 'to'
                        mailMessage.To.Add(_smtpConfig.Username);
                    }
                    // Sending Inquiry Email from Contact US

                    // set recipients to bcc
                    foreach (var toAddress in _recipients)
                    {
                        mailMessage.Bcc.Add(toAddress);
                    }
                }

                // body and subject
                mailMessage.Body = _smtpConfig.Body;
                mailMessage.Subject = _smtpConfig.Subject;

                // Note : Must set https://www.google.com/settings/security/lesssecureapps to allow mail sending
                client.Send(mailMessage);
            }
            catch (Exception e)
            {
                retStatus = -1;
                _logMgr.LogInfo("La nouvelle adresse a été ajoutée avec succès. in SendEmailByEmail");
                _logMgr.LogDebugObject(e);
            }

            _logMgr.LogInfo("SendEmailByEmail End");

            return retStatus;
        }

        //public int SavePushNotificationForPOS(PushNotificationData _notifData, APIDBContext _dbCtxt)
        //{
        //    PushNotificationModel NotifModel = new PushNotificationModel()
        //    {
        //        CreatedDate = DateTime.Now,
        //        CreatedBy = _notifData.UserId,
        //        DateEnabled = DateTime.Now,
        //        IsEnabledBy = _notifData.UserId,
        //        IsEnabled = true,
        //        LastEditedDate = DateTime.Now,
        //        LastEditedBy = _notifData.UserId,
        //        IsLocked = false,
        //        Header = _notifData.Header,
        //        Subtitle = _notifData.Subtitle,
        //        Description = _notifData.Description,
        //        IsAlertOn = true,
        //        DeliveryType = DeliveryType.Immediate,
        //        NotificationType = PushNotificationType.Auto,
        //        FilterType = NotificationTargetFilter.All
        //    };

        //    _dbCtxt.PushNotifications.Add(NotifModel);
        //    _dbCtxt.SaveChanges();

        //    return NotifModel.NotificationID;
        //}

        public static Byte[] BitmapToBytesCode(Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public string TranslateDoW(DayOfWeek dayOfWeek)
        {
            string strDayOfWk;

            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    strDayOfWk = "Lundi";
                    break;
                case DayOfWeek.Tuesday:
                    strDayOfWk = "Mardi";
                    break;
                case DayOfWeek.Wednesday:
                    strDayOfWk = "Mercredi";
                    break;
                case DayOfWeek.Thursday:
                    strDayOfWk = "Jeudi";
                    break;
                case DayOfWeek.Friday:
                    strDayOfWk = "Vendredi";
                    break;
                case DayOfWeek.Saturday:
                    strDayOfWk = "Samedi";
                    break;
                default:
                    strDayOfWk = "Dimanche";
                    break;
            }

            return strDayOfWk;
        }

        public string TranslateMonth(DateTime dt)
        {
            string strDate;

            switch (dt.Month)
            {
                case 1:
                    strDate = "Janvier";
                    break;
                case 2:
                    strDate = "Février";
                    break;
                case 3:
                    strDate = "Mars";
                    break;
                case 4:
                    strDate = "Avril";
                    break;
                case 5:
                    strDate = "Mai";
                    break;
                case 6:
                    strDate = "Juin";
                    break;
                case 7:
                    strDate = "Juillet";
                    break;
                case 8:
                    strDate = "Août";
                    break;
                case 9:
                    strDate = "Septembre";
                    break;
                case 10:
                    strDate = "Octobre";
                    break;
                case 11:
                    strDate = "Novembre";
                    break;
                default:
                    strDate = "Décembre";
                    break;
            }

            return strDate;
        }
    }
}
