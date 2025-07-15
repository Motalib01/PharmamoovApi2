using Microsoft.AspNetCore.Http;
using PharmaMoov.Models;
using PharmaMoov.Models.User;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IUserHelpRequestRepository
    {
        APIResponse GetUserRequestList(int _id);
        APIResponse AddUserRequest(string _auth, NewUserHelpRequest _uhRequest);
        APIResponse GetOrderNumber(string _auth);
        APIResponse GetUserConcernList(string _auth, int _id);
        APIResponse AddUserConcern(string _auth, NewUserGeneralConcern _ugConcern);
        APIResponse ChangeHelpRequestStatus(string _auth, ChangeHelpRequestStatus _request);
        APIResponse SendInquiry(GeneralInquiry _inquiry);
        APIResponse SendCareer(CareersForm _career);
    }
}
