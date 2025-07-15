using PharmaMoov.Models;
using PharmaMoov.Models.User;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IAccountRepository
    {
        APIResponse ChangeUserPassword(UserChangePassword _user);
        APIResponse GetUserProfile(string _auth);
        APIResponse EditUserProfile(UserProfile _user);

        APIResponse GetDeliveryAddressBook(string _auth, int _address);
        APIResponse AddUserDeliveryAddress(string _auth, UserAddress _address);
        APIResponse EditUserDeliveryAddress(UserAddress _address);
        APIResponse DeleteUserDeliveryAddress(UserAddressToDel _address);
    }
}
