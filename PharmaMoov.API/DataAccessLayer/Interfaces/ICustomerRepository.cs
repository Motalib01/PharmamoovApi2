using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface ICustomerRepository
    {
        APIResponse GetCustomers(string Authorization, int CustomerID);
        //APIResponse ChangeCustomerStatus(string Authorization, ChangeRecordStatus _customerStatus);
        APIResponse EditCustomerProfile(string Authorization, User _customerProfile);
    }
}
