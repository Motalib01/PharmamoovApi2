using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using System;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IDeliveryJobRepository
    {
        string GetShopAddressById(int iShopId);
        string GetCustomerAddressById(int iCustomerAddressId);
        JobInput GetJobModel(JobModel iJobModel);
        DeliveryJob CreateDeliveryJob(string iAssignmentCode, JobModel iJobModel, string iJobParameterObj, string iAuthToken);
        DeliveryJob UpdateDeliveryJob(string iAssignmentCode, string iStatus);
        JobInput GetJobModelForMobile(JobModelForMobile iJobModel);
    }
}
