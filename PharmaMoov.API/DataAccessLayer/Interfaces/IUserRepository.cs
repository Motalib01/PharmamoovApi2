using PharmaMoov.Models;
using PharmaMoov.Models.User;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models.Admin;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IUserRepository
    {
        APIResponse RegisterUserViaEmail(UserRegistrationViaEmail _user);
        APIResponse VerifyUserMobileOrEmail(VerifyMobileOrEmail _user);
        APIResponse RegisterUserViaMobileNumber(UserRegistrationViaMobileNumber _user, IMainHttpClient _mhttp, APIConfigurationManager _conf = null);
        APIResponse VerifyUserCode(UserVerifyCode _vCode);
        APIResponse SendUserVerificationCode(UserVerifyCode _vCode, IMainHttpClient _mhttp, APIConfigurationManager _conf = null);
        APIResponse MobileLogin(LoginCredentials _user, IMainHttpClient _mhttp, APIConfigurationManager _conf = null);
        APIResponse SetFCMToken(string UserToken, string FCMToken, DevicePlatforms DeviceType);
        APIResponse ReGenerateTokens(UserLoginTransaction _user);
        APIResponse MobileLogout(LogoutCredentials _user);
        APIResponse FullUserRegistration(FullUserRegForm _user);
        APIResponse LoginEmailOrUsername(LoginEmailUsername _user);
        APIResponse ChangeUserStatus(string Authorization, ChangeRecordStatus _userStat);
        APIResponse EditUserProfile(string Authorization, User _userProfile);
        APIResponse ResetPassword(ResetPasswordModel resetPasswordModel);
        APIResponse ChangeAcceptOrDeclineRequest(string Authorization, ChangeAcceptOrDeclineRequestStatus model, IMainHttpClient _mhttp, APIConfigurationManager _conf = null);
    }
}
