using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.User;
using System;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IAdminRepository
    {
        APIResponse RegisterAdmin(AdminProfile _admin);
        APIResponse AdminLogin(AdminLogin _admin);
        APIResponse ReGenerateTokens(UserLoginTransaction _admin);

        APIResponse GetAllAdmins(Guid _shop, int _admin);
        APIResponse EditAdminProfile(EditAdminProfile _admin);
        APIResponse ChangeAdminStatus(ChangeRecordStatus _admin);

        APIResponse ForgotPassword(AdminForgotPassword _admin, IMainHttpClient _mhttp, APIConfigurationManager _conf = null);

        APIResponse GetAdminList();
    }
}
