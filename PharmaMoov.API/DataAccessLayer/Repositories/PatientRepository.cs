using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using System;
using PharmaMoov.Models.Patient;
using PharmaMoov.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using PharmaMoov.Models.Orders;
using PharmaMoov.Models.Prescription;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class PatientRepository : APIBaseRepo, IPatientRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public PatientRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public APIResponse GetAllPatients(string _auth, int _pageNo, string _searchKey)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("GetAllPatients: " + _pageNo);

            try
            {
                if (IsUserLoggedIn != null)
                {
                    var pageSize = 10;
                    List<PatientListModel> patientList = new List<PatientListModel>();
                    if (!string.IsNullOrEmpty(_searchKey))
                    {
                        patientList = DbContext.Patients.AsNoTracking().Where(s => s.UserId == IsUserLoggedIn.UserId && (s.FirstName.ToLower().Contains(_searchKey.ToLower()) || s.LastName.ToLower().Contains(_searchKey.ToLower())))
                                        .Select(p => new PatientListModel
                                        {
                                            PatientRecordId = p.PatientRecordId,
                                            PatientId = p.PatientId,
                                            FirstName = p.FirstName,
                                            LastName = p.LastName,
                                            IsEnabled = p.IsEnabled ?? false
                                        }).ToList();
                    }
                    else
                    {
                        patientList = DbContext.Patients.AsNoTracking().Where(s => s.UserId == IsUserLoggedIn.UserId)
                                        .Select(p => new PatientListModel
                                        {
                                            PatientRecordId = p.PatientRecordId,
                                            PatientId = p.PatientId,
                                            FirstName = p.FirstName,
                                            LastName = p.LastName,
                                            IsEnabled = p.IsEnabled ?? false,
                                            MobileNumber = p.MobileNumber,
                                            CompleteAddress = p.Street + ", " + p.City + ", " + (p.POBox ?? "")
                                        }).ToList();
                    }

                   var paggedList = patientList.Where(w => w.PatientRecordId != 0, _pageNo, pageSize).ToList();

                    if (paggedList.Count > 0)
                    {
                        // get total page count
                        var totalPageCount = 1;
                        var pageCount = patientList.Count;
                        if (pageCount > 0 && pageCount >= pageSize)
                        {
                            totalPageCount = (int)Math.Ceiling((double)pageCount / pageSize);
                        }
                        aResp = new APIResponse
                        {
                            Message = "Tous les éléments ont été récupérés avec succès.",
                            Status = "Succès!",
                            Payload = new
                            {
                                PatientList = paggedList,
                                PageCount = totalPageCount
                            },
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Aucun article n'a été récupéré.",
                            Status = "Échec!",
                            StatusCode = System.Net.HttpStatusCode.OK
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
                LogManager.LogInfo("GetAllPatients");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse AddPatient(string _auth, PatientDTO _patient)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("AddPatient");
            try
            {
                if (IsUserLoggedIn != null)
                {
                    Guid PatientGuidID = Guid.NewGuid();
                    DateTime RegistrationDate = DateTime.Now;

                    Patient Patient = new Patient
                    {
                        PatientId = PatientGuidID,
                        UserId = IsUserLoggedIn.UserId,

                        FirstName = _patient.FirstName,
                        LastName = _patient.LastName,
                        MobileNumber = _patient.MobileNumber,
                        AddressName = _patient.AddressName,
                        Street = _patient.Street,
                        POBox = _patient.POBox,
                        City = _patient.City,
                        AddressNote = _patient.AddressNote,
                        Latitude = _patient.Latitude,
                        Longitude = _patient.Longitude,

                        CreatedDate = RegistrationDate,
                        CreatedBy = IsUserLoggedIn.UserId,
                        IsEnabled = _patient.IsActive,
                        IsEnabledBy = IsUserLoggedIn.UserId,
                        DateEnabled = RegistrationDate,
                    };

                    DbContext.Patients.Add(Patient);
                    DbContext.SaveChanges();
                    aResp = new APIResponse
                    {
                        Message = "La Patient a bien été ajoutée",
                        Status = "Succès!",
                        Payload = _patient,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
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
                LogManager.LogInfo("AddPatient");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse ViewPatient(string _auth, int _patientRecordId)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("ViewPatient");
            try
            {
                if (IsUserLoggedIn != null)
                {
                    var patientDetail = DbContext.Patients.AsNoTracking().Where(s => s.PatientRecordId == _patientRecordId)
                                        .Select(p => new PatientDTO
                                        {
                                            PatientRecordId = p.PatientRecordId,
                                            PatientId = p.PatientId,
                                            FirstName = p.FirstName,
                                            LastName = p.LastName,
                                            MobileNumber = p.MobileNumber,
                                            IsActive = p.IsEnabled ?? false,
                                            Address = (p.Street ?? "") + ", " + (p.City ?? "") + ", " + (p.POBox ?? ""),
                                            AddressNote = p.AddressNote,
                                            City = p.City,
                                            POBox = p.POBox,
                                            Street = p.Street,
                                            AddressName = p.AddressName
                                        }).FirstOrDefault();

                    if (patientDetail != null)
                    {
                        var patientOrderList = DbContext.Orders.AsNoTracking().Where(s => s.PatientId == patientDetail.PatientId && (s.OrderProgressStatus == OrderProgressStatus.COMPLETED || s.OrderProgressStatus == OrderProgressStatus.REJECTED || s.OrderProgressStatus == OrderProgressStatus.PLACED))
                            .Select(o=> new UserOrderList
                            {
                                OrderID = o.OrderID,
                                OrderReferenceID = o.OrderReferenceID,
                                ShopName = o.ShopName,
                                DateAdded = o.CreatedDate.GetValueOrDefault(),
                                ScheduleDate = o.DeliveryDate,
                                OrderProgressStatus = o.OrderProgressStatus,
                                ShopIcon = DbContext.Shops.FirstOrDefault(s => s.ShopId == o.ShopId).ShopIcon,
                                DeliveryAddressId = o.DeliveryAddressId,
                                OrderGrossAmount = o.OrderGrossAmount
                            }).OrderByDescending(o => o.OrderProgressStatus).ThenBy(o => o.ScheduleDate).ToList();

                        //get the delivery address of each order
                        foreach (var item in patientOrderList)
                        {
                            UserAddresses getAddress = DbContext.UserAddresses.FirstOrDefault(a => a.UserAddressID == item.DeliveryAddressId);
                            if (getAddress != null)
                            {
                                item.CompleteDeliveryAddress = getAddress.Street + ", " + getAddress.City + ", " + getAddress.PostalCode;
                            }
                        };

                        //NEW: JUL-13-21 Get Patient's Prescription List
                        var prescriptionList = DbContext.Prescriptions.AsNoTracking()
                                                .Join(DbContext.Shops, p => p.ShopId, s => s.ShopId, (p, s) => new { p, s })
                                                .Where(x => x.p.PatientId == patientDetail.PatientId && x.p.PrescriptionStatus == PrescriptionStatus.APPROVED)
                                                .Select(x => new PrescriptionList
                                                {
                                                    PrescriptionRecordId = x.p.PrescriptionRecordId,
                                                    ShopId = x.p.ShopId,
                                                    MedicineDescription = x.p.MedicineDescription,
                                                    DoctorName = x.p.DoctorName,
                                                    PrescriptionIcon = x.p.PrescriptionIcon,
                                                    UserId = x.p.UserId,
                                                    PatientId = x.p.PatientId,
                                                    PrescriptionStatus = x.p.PrescriptionStatus,
                                                    CreatedDate = x.p.CreatedDate,
                                                    ShopIcon = x.s.ShopIcon,
                                                    ShopName = x.s.ShopName
                                                }).OrderByDescending(o => o.PrescriptionRecordId).ToList();

                        aResp = new APIResponse
                        {
                            Message = "Tous les éléments ont été récupérés avec succès.",
                            Status = "Succès!",
                            Payload = new
                            {
                                PatientDetail = patientDetail,
                                PatientOrderList = patientOrderList,
                                PrescriptionList = prescriptionList
                            },
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Aucun article n'a été récupéré.",
                            Status = "Échec!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
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
                LogManager.LogInfo("ViewPatient");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse PatientDetail(string _auth, int _patientRecordId, int patientDetailType)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("PatientDetail");
            try
            {
                if (IsUserLoggedIn != null)
                {
                    dynamic patientDetail = null;
                    if (patientDetailType == 1)
                    {
                        patientDetail = DbContext.Patients.AsNoTracking().Where(s => s.PatientRecordId == _patientRecordId)
                                       .Select(p => new 
                                       {
                                           p.PatientRecordId,
                                           p.PatientId,
                                           p.FirstName,
                                           p.LastName,
                                           p.MobileNumber,
                                           IsEnabled = p.IsEnabled ?? false,
                                       }).FirstOrDefault();
                    }
                    else if(patientDetailType == 2)
                    {
                        patientDetail = DbContext.Patients.AsNoTracking().Where(s => s.PatientRecordId == _patientRecordId)
                                        .Select(p => new 
                                        {
                                            p.PatientRecordId,
                                            p.PatientId,
                                            p.AddressName,
                                            p.Street,
                                            p.POBox,
                                            p.City,
                                            p.AddressNote,
                                            p.Latitude,
                                            p.Longitude,
                                            p.MobileNumber
                                        }).FirstOrDefault();
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Veuillez fournir des détails valides sur le patient (profil=1, adresse=2).",
                            Status = "Échec!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }

                    if (patientDetail != null)
                    {
                        aResp = new APIResponse
                        {
                            Message = "Tous les éléments ont été récupérés avec succès.",
                            Status = "Succès!",
                            Payload = new
                            {
                                PatientDetail = patientDetail
                            },
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Aucun article n'a été récupéré.",
                            Status = "Échec!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
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
                LogManager.LogInfo("PatientDetail");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse UpdatePatientProfile(string _auth, PatientProfileModel _patientProfile)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("UpdatePatientProfile");
            try
            {
                if (IsUserLoggedIn != null)
                {
                    Patient updatePatient = DbContext.Patients.FirstOrDefault(d => d.PatientRecordId == _patientProfile.PatientRecordId);
                    if (updatePatient != null)
                    {
                        updatePatient.FirstName = _patientProfile.FirstName;
                        updatePatient.LastName = _patientProfile.LastName;
                        updatePatient.MobileNumber = _patientProfile.MobileNumber;
                        updatePatient.IsEnabled = _patientProfile.IsActive;
                        updatePatient.LastEditedDate = DateTime.Now;
                        updatePatient.LastEditedBy = IsUserLoggedIn.UserId;

                        DbContext.Patients.Update(updatePatient);
                        DbContext.SaveChanges();

                        aResp.Message = "Profil du patient mis à jour avec succès";
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        aResp.Message = "Aucun enregistrement trouvé.";
                        aResp.Status = "Échec";
                        aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
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
                LogManager.LogInfo("UpdatePatientProfile");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse UpdatePatientAddress(string _auth, PatientAddressModel _patientAddress)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("UpdatePatientAddress");
            try
            {
                if (IsUserLoggedIn != null)
                {
                    Patient updatePatientAddress = DbContext.Patients.FirstOrDefault(d => d.PatientRecordId == _patientAddress.PatientRecordId);
                    if (updatePatientAddress != null)
                    {
                        updatePatientAddress.AddressName = _patientAddress.AddressName;
                        updatePatientAddress.Street = _patientAddress.Street;
                        updatePatientAddress.POBox = _patientAddress.POBox;
                        updatePatientAddress.City = _patientAddress.City;
                        updatePatientAddress.AddressNote = _patientAddress.AddressNote;
                        updatePatientAddress.Latitude = _patientAddress.Latitude;
                        updatePatientAddress.Longitude = _patientAddress.Longitude;

                        updatePatientAddress.LastEditedDate = DateTime.Now;
                        updatePatientAddress.LastEditedBy = IsUserLoggedIn.UserId;

                        DbContext.Patients.Update(updatePatientAddress);
                        DbContext.SaveChanges();

                        aResp.Message = "L'adresse du patient a été mise à jour avec succès";
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        aResp.Message = "Aucun enregistrement trouvé.";
                        aResp.Status = "Échec";
                        aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
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
                LogManager.LogInfo("UpdatePatientAddress");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }

        public APIResponse UpdatePatientDetails(string _auth, PatientDTO _patient)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("UpdatePatientProfile");
            try
            {
                if (IsUserLoggedIn != null)
                {
                    Patient updatePatient = DbContext.Patients.FirstOrDefault(d => d.PatientRecordId == _patient.PatientRecordId);
                    if (updatePatient != null)
                    {
                        updatePatient.FirstName = _patient.FirstName;
                        updatePatient.LastName = _patient.LastName;
                        updatePatient.MobileNumber = _patient.MobileNumber;
                        updatePatient.AddressName = _patient.AddressName;
                        updatePatient.Street = _patient.Street;
                        updatePatient.POBox = _patient.POBox;
                        updatePatient.City = _patient.City;
                        updatePatient.AddressNote = _patient.AddressNote;
                        updatePatient.IsEnabled = _patient.IsActive;

                        updatePatient.LastEditedDate = DateTime.Now;
                        updatePatient.LastEditedBy = IsUserLoggedIn.UserId;

                        DbContext.Patients.Update(updatePatient);
                        DbContext.SaveChanges();

                        aResp.Message = "Profil du patient mis à jour avec succès";
                        aResp.Status = "Succès";
                        aResp.StatusCode = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        aResp.Message = "Aucun enregistrement trouvé.";
                        aResp.Status = "Échec";
                        aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
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
                LogManager.LogInfo("UpdatePatientDetails");
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
