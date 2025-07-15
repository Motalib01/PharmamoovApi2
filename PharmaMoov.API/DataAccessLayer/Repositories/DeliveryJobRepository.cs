using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using PharmaMoov.Models.Orders;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class DeliveryJobRepository : APIBaseRepo, IDeliveryJobRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public DeliveryJobRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public string GetShopAddressById(int iShopId)
        {
            var shop = DbContext.Shops.FirstOrDefault(x => x.ShopRecordID == iShopId);
            return shop.Address;
        }

        public string GetCustomerAddressById(int iCustomerAddressId)
        {
            var customerAddress = DbContext.UserAddresses.FirstOrDefault(x => x.UserAddressID == iCustomerAddressId);
            return customerAddress.Street + ", " + customerAddress.Building + ", " + customerAddress.Area + ", " + customerAddress.City;
        }

        public JobInput GetJobModelForMobile(JobModelForMobile iJobModel)
        {
            var shop = DbContext.Shops.FirstOrDefault(x => x.ShopId == iJobModel.ShopId);
            var customer = DbContext.Users.FirstOrDefault(x => x.UserId == iJobModel.CustomerId);
            var deliveryAddress = GetCustomerAddressById(iJobModel.DeliveryAddressId);
            string assignmentCode = GenerateUniqeCode(8, true, true);

            //var job = new JobInput(assignmentCode, "car"); //default
            var job = new JobInput(assignmentCode, "bike"); //default
            job.pickups.Add(new PickupInput()
            {
                address = shop.Address,
                comment = "commentaire par défaut : prix du travail depuis la caisse",
                contact = new ContactInput()
                {
                    phone = shop.MobileNumber,
                    email = shop.Email,
                    company = shop.ShopName
                }
            });
            job.dropoffs.Add(new DropoffInput()
            {
                package_type = "small", //default
                package_description = "description par défaut : type de petit paquet",
                address = deliveryAddress,
                comment = "commentaire par défaut : prix du travail depuis la caisse",
                contact = new ContactInput()
                {
                    firstname = customer.FirstName,
                    lastname = customer.LastName,
                    phone = customer.MobileNumber,
                    email = customer.Email
                }
            });

            return job;
        }

        public JobInput GetJobModel(JobModel iJobModel)
        {
            var order = DbContext.Orders.FirstOrDefault(x => x.OrderID == iJobModel.OrderId);
            var shop = DbContext.Shops.FirstOrDefault(x => x.ShopId == order.ShopId);
            var customer = DbContext.Users.FirstOrDefault(x => x.UserId == order.UserId);
            var customerAddress = DbContext.UserAddresses.FirstOrDefault(x => x.UserAddressID == order.DeliveryAddressId);
            var deliveryAddress = GetCustomerAddressById(order.DeliveryAddressId);

            var job = new JobInput(order.OrderReferenceID, iJobModel.TransportType);
           // var job = new JobInput(order.OrderReferenceID, "bike");
            job.pickups.Add(new PickupInput()
            {
                address = shop.Address,
                comment = iJobModel.PickupComment,
                contact = new ContactInput()
                {
                    phone = shop.TelephoneNumber,
                    email = shop.Email,
                    company = shop.ShopName
                }
            });
            job.dropoffs.Add(new DropoffInput()
            {
                package_type = iJobModel.PackageType,
                package_description = iJobModel.PackageDescription,
                address = deliveryAddress,
                comment = iJobModel.DeliveryComment,
                contact = new ContactInput()
                {
                    firstname = customer.FirstName,
                    lastname = customer.LastName,
                    phone = customer.MobileNumber,
                    email = customer.Email
                }
            });

            return job;
        }

        public DeliveryJob CreateDeliveryJob(string iAssignmentCode, JobModel iJobModel, string iJobParameterObj, string iAuthToken)
        {
            DeliveryJob deliveryJobModel = null;
            try
            {
                var loginTrx = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(x => x.Token == iAuthToken);
                if(loginTrx != null)
                {
                    deliveryJobModel = new DeliveryJob();
                    deliveryJobModel.OrderId = iJobModel.OrderId;
                    deliveryJobModel.AssignmentCode = iAssignmentCode;
                    deliveryJobModel.Status = EDeliveryStatus.PENDING;
                    deliveryJobModel.CreateParam = iJobParameterObj;

                    deliveryJobModel.CreatedBy = loginTrx.UserId;
                    deliveryJobModel.CreatedDate = DateTime.Now;

                    DbContext.DeliveryJobs.Add(deliveryJobModel);
                    DbContext.SaveChanges();
                }
                else
                {
                    LogManager.LogError("Erreur: CreateDeliveryJob - Login transaction not found");
                }
            }
            catch (Exception e)
            {
                LogManager.LogError("Erreur: CreateDeliveryJob");
                LogManager.LogDebugObject(e);
            }

            return deliveryJobModel;
        }

        public DeliveryJob UpdateDeliveryJob(string iAssignmentCode, string iStatus)
        {
            LogManager.LogInfo("UpdateDeliveryJob with paramaters: iAssignmentCode:" + iAssignmentCode + "  iStatus:" + iStatus);
            DeliveryJob deliveryJobModel = null;
            try
            {
                deliveryJobModel = DbContext.DeliveryJobs.FirstOrDefault(x => x.AssignmentCode == iAssignmentCode);                
                if (deliveryJobModel != null)
                {
                    Order updateOrder = DbContext.Orders.FirstOrDefault(o => o.OrderID == deliveryJobModel.OrderId);
                    if (updateOrder != null)
                    {
                        switch (iStatus)
                        {
                            case "delivered":
                                updateOrder.OrderProgressStatus = OrderProgressStatus.COMPLETED;
                                deliveryJobModel.Status = EDeliveryStatus.FINISHED;
                                break;
                            case "delivering":
                                updateOrder.OrderProgressStatus = OrderProgressStatus.OUTFORDELIVERY;
                                deliveryJobModel.Status = EDeliveryStatus.PENDING;
                                break;
                            default:
                                break;
                        }
                    }
                    deliveryJobModel.LastEditedDate = DateTime.Now;

                    DbContext.SaveChanges();
                }
                else
                {
                    LogManager.LogError("Erreur: UpdateDeliveryJob - Delivery job not found");
                }
            }
            catch (Exception e)
            {
                LogManager.LogError("Erreur: UpdateDeliveryJob");
                LogManager.LogDebugObject(e);
            }

            return deliveryJobModel;
        }

        void UpdateOrderDeliveryStatus(int orderId) 
        {
            Order updateOrder = DbContext.Orders.FirstOrDefault(o => o.OrderID == orderId);
            if (updateOrder != null) 
            {
                updateOrder.OrderProgressStatus = OrderProgressStatus.COMPLETED;
            }
        }
    }
}
