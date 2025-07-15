using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using System;
using PharmaMoov.Models.Prescription;
using PharmaMoov.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.Patient;
using System.Collections.Generic;
using PharmaMoov.Models.Cart;
using PharmaMoov.Models.Product;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class PrescriptionRepository : APIBaseRepo, IPrescriptionRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }

        public PrescriptionRepository(APIDBContext _dbCtxt, ILoggerManager _logManager, APIConfigurationManager _apiCon)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
        }

        public APIResponse PopulatePrescriptions(int _prescriptionRecordId, int IsActive, Guid ShopId)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("PopulatePrescriptions: " + _prescriptionRecordId);

            try
            {
                IEnumerable<PrescriptionDetail> filterResult = null;
                filterResult = DbContext.Prescriptions.AsNoTracking()
                                        .Join(DbContext.Users, p => p.UserId, u => u.UserId, (p, u) => new { p, u })
                                        .Select(p => new PrescriptionDetail
                                        {
                                            PrescriptionRecordId = p.p.PrescriptionRecordId,
                                            ShopId = p.p.ShopId,
                                            MedicineDescription = p.p.MedicineDescription,
                                            CustomerName = p.u.FirstName + " " + p.u.LastName,
                                            CustomerEmail = p.u.Email,
                                            DoctorName = p.p.DoctorName,
                                            PrescriptionIcon = p.p.PrescriptionIcon,
                                            DateCreated = p.p.CreatedDate.GetValueOrDefault(),
                                            PrescriptionStatus = p.p.PrescriptionStatus
                                        }).OrderByDescending(p => p.PrescriptionRecordId);

                if (filterResult != null)
                {
                    if (_prescriptionRecordId > 0)
                    {
                        //Get the Prescription details 
                        PrescriptionDetail prescription = filterResult.Where(c => c.PrescriptionRecordId == _prescriptionRecordId).FirstOrDefault();

                        aResp = new APIResponse
                        {
                            Message = "Enregistrement récupéré avec succès.",
                            Status = "Succès!",
                            Payload = prescription,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else if (ShopId != Guid.Empty)
                    {
                        //Get all the Prescriptions of the selected Pharmacy/Shop
                        //NEW: should only fetch prescriptions less than 7 days old 
                        DateTime last7Days = DateTime.Now.AddDays(-7);
                        filterResult = filterResult.Where(i => i.ShopId == ShopId && i.DateCreated >= last7Days);

                        aResp = new APIResponse
                        {
                            Message = "Tous les éléments ont été récupérés avec succès.",
                            Status = "Succès!",
                            Payload = filterResult,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        if (IsActive > 0)
                        {
                            aResp = new APIResponse
                            {
                                Message = "Tous les éléments ont été récupérés avec succès.",
                                Status = "Succès!",
                                Payload = filterResult.Where(i => i.IsActive == true),
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                        else
                        {
                            aResp = new APIResponse
                            {
                                Message = "Tous les éléments ont été récupérés avec succès.",
                                Status = "Succès!",
                                Payload = filterResult,
                                StatusCode = System.Net.HttpStatusCode.OK
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("PopulatePrescriptions");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse AddPrescription(string _auth, PrescriptionDetail _prescription)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("AddPrescription");
            try
            {
                if (IsUserLoggedIn != null)
                {
                    Guid PrescriptionGuidID = Guid.NewGuid();
                    DateTime RegistrationDate = DateTime.Now;

                    Prescription prescription = new Prescription
                    {
                        PrescriptionId = PrescriptionGuidID,
                        UserId = IsUserLoggedIn.UserId,
                        ShopId = _prescription.ShopId,
                        MedicineDescription = _prescription.MedicineDescription,
                        DoctorName = _prescription.DoctorName,
                        PatientId = (_prescription.PatientId != null && _prescription.PatientId != Guid.Empty) ? _prescription.PatientId : Guid.Empty,
                        PrescriptionIcon = _prescription.PrescriptionIcon,
                        PrescriptionStatus = PrescriptionStatus.PENDING,

                        CreatedDate = RegistrationDate,
                        CreatedBy = IsUserLoggedIn.UserId,
                        IsEnabled = true,
                        IsEnabledBy = IsUserLoggedIn.UserId,
                        DateEnabled = RegistrationDate,
                    };

                    DbContext.Prescriptions.Add(prescription);
                    DbContext.SaveChanges();
                    aResp = new APIResponse
                    {
                        Message = "La prescription a bien été ajoutée",
                        Status = "Succès!",
                        Payload = _prescription,
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
                LogManager.LogInfo("AddPrescription");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse EditPrescription(string _auth, PrescriptionDetail _prescription)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("EditPrescription");
            try
            {
                if (IsUserLoggedIn != null)
                {
                    Prescription updatePrescription = DbContext.Prescriptions.FirstOrDefault(d => d.PrescriptionRecordId == _prescription.PrescriptionRecordId);
                    if (updatePrescription != null)
                    {
                        updatePrescription.MedicineDescription = _prescription.MedicineDescription;
                        updatePrescription.DoctorName = _prescription.DoctorName;
                        updatePrescription.PrescriptionIcon = _prescription.PrescriptionIcon;

                        updatePrescription.LastEditedDate = DateTime.Now;
                        updatePrescription.LastEditedBy = IsUserLoggedIn.UserId;
                        updatePrescription.DateEnabled = DateTime.Now;
                        updatePrescription.IsEnabled = _prescription.IsActive;
                        updatePrescription.IsEnabledBy = IsUserLoggedIn.UserId;

                        DbContext.Prescriptions.Update(updatePrescription);
                        DbContext.SaveChanges();

                        aResp.Message = "Prescription mise à jour avec succès";
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
                LogManager.LogInfo("EditPrescription");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse ChangePrescriptionStatus(string _auth, ChangeRecordStatus _prescription)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("ChangePrescriptionStatus");
            try
            {
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    Prescription foundPrescription = DbContext.Prescriptions.Where(a => a.PrescriptionRecordId == _prescription.RecordId).FirstOrDefault();
                    if (foundPrescription != null)
                    {
                        DateTime NowDate = DateTime.Now;
                        foundPrescription.LastEditedDate = DateTime.Now;
                        foundPrescription.LastEditedBy = _prescription.AdminId;
                        foundPrescription.PrescriptionStatus = PrescriptionStatus.APPROVED; //Approve Prescription

                        DbContext.Prescriptions.Update(foundPrescription);
                        DbContext.SaveChanges();
                        aResp = new APIResponse
                        {
                            Message = "Prescription successfully updated.",
                            Status = "Succès!",
                            Payload = foundPrescription,
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
                LogManager.LogInfo("ChangePrescriptionStatus");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetPrescription(int prescriptionRecordId)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetPrescription");
            try
            {
                
                PrescriptionDetailAndProducts prescriptionData = new PrescriptionDetailAndProducts();
                List<ProductList> productList = new List<ProductList>();

                prescriptionData.PrescriptionDetail = DbContext.Prescriptions.AsNoTracking()
                                        .Where(s => s.PrescriptionRecordId == prescriptionRecordId)
                                        .Select(s => new PrescriptionDetail
                                        {
                                            PrescriptionRecordId = s.PrescriptionRecordId,
                                            ShopId = s.ShopId,
                                            MedicineDescription = s.MedicineDescription,
                                            DoctorName = s.DoctorName,
                                            PrescriptionIcon = s.PrescriptionIcon,
                                            UserId = s.UserId,
                                            PatientId = s.PatientId,
                                            PrescriptionStatus = s.PrescriptionStatus
                                        }).FirstOrDefault();

                //Get list of prescribed products added by the pharmacy admin
                List<PrescriptionProduct> getPrescriptionProducts = DbContext.PrescriptionProducts.AsNoTracking()
                            .Where(p => p.PrescriptionRecordId == prescriptionData.PrescriptionDetail.PrescriptionRecordId).ToList();

                foreach (var item in getPrescriptionProducts)
                {
                    var getProduct = DbContext.Products
                           .Join(DbContext.ProductCategories, i => i.ProductCategoryId, c => c.ProductCategoryId, (i, c) => new { i, c })
                           .Where(w => w.i.ProductRecordId == item.ProductRecordId)
                           .Select(p => new ProductList
                           {
                               ProductRecordId = p.i.ProductRecordId,
                               ProductIcon = p.i.ProductIcon,
                               ProductName = p.i.ProductName,
                               ProductPrice = p.i.ProductPrice,
                               ProductCategory = p.c.ProductCategoryName,
                               ProductStatus = p.i.ProductStatus
                           }).FirstOrDefault();

                    productList.Add(getProduct);
                }
                prescriptionData.PrescriptionProducts = productList;
                if (prescriptionData.PrescriptionDetail.PatientId != Guid.Empty)
                {
                    //Check for patient
                    Patient pDetails = DbContext.Patients.FirstOrDefault(p => p.PatientId == prescriptionData.PrescriptionDetail.PatientId);
                    prescriptionData.PrescriptionDetail.CustomerName = pDetails.FirstName + " " + pDetails.LastName;
                }
                else
                {
                    //Check for customer
                    User uDetails = DbContext.Users.FirstOrDefault(u => u.UserId == prescriptionData.PrescriptionDetail.UserId);
                    prescriptionData.PrescriptionDetail.CustomerName = uDetails.FirstName + " " + uDetails.LastName;
                    prescriptionData.PrescriptionDetail.CustomerEmail = uDetails.Email;
                }

                if (prescriptionData != null)
                {
                    aResp = new APIResponse
                    {
                        Message = "Les données de l'ordonnance ont été extraites avec succès.",
                        Status = "Succès!",
                        Payload = prescriptionData,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun enregistrement trouvé.",
                        Status = "Échec!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetPrescription");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetPrescriptionDetails(int prescriptionRecordId)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetPrescription");
            try
            {
                var productList = DbContext.PrescriptionProducts
                    .Join(DbContext.Products, i => i.ProductRecordId, c => c.ProductRecordId, (i, c) => new { i, c })
                    .Where(s => s.i.PrescriptionRecordId == prescriptionRecordId).ToList()
                    .Select(p => new PrescriptionProductList
                    {
                        productRecordId = p.i.ProductRecordId.ToString(),
                        productIcon = p.c.ProductIcon,
                        productName = p.c.ProductName,
                        productPrice = p.c.ProductPrice.ToString(),
                        customPrice = p.i.ProductPrice.ToString(),
                        quantity = p.i.ProductQuantity.ToString(),
                        subTotal = p.i.SubTotal.ToString(),
                        action = p.i.ProductRecordId.ToString(),
                    });

                //foreach (var item in getPrescriptionProducts)
                //{
                //    var getProduct = DbContext.Products
                //           .Join(DbContext.ProductCategories, i => i.ProductCategoryId, c => c.ProductCategoryId, (i, c) => new { i, c })
                //           .Where(w => w.i.ProductRecordId == item.ProductRecordId)
                //           .Select(p => new ProductList
                //           {
                //               ProductRecordId = p.i.ProductRecordId,
                //               ProductIcon = p.i.ProductIcon,
                //               ProductName = p.i.ProductName,
                //               ProductPrice = p.i.ProductPrice,
                //               ProductCategory = p.c.ProductCategoryName,
                //               ProductStatus = p.i.ProductStatus
                //           }).FirstOrDefault();
                //}

                if (productList != null)
                {
                    aResp = new APIResponse
                    {
                        Message = "Récupération réussie des données détaillées de l'ordonnance.",
                        Status = "Succès!",
                        Payload = productList,
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun enregistrement trouvé.",
                        Status = "Échec!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }

            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetPrescription");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse AddPrescriptionProducts(string _auth, List<PrescriptionProductsParamModel> pModel)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("AddPrescriptionProducts");
            try
            {
                List<PrescriptionProduct> prescriptionProductList = new List<PrescriptionProduct>();
                UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
                if (IsUserLoggedIn != null)
                {
                    var PrescriptionId = pModel.FirstOrDefault().PrescriptionRecordId;

                    foreach (var item in pModel)
                    {
                        var product = DbContext.Products.FirstOrDefault(p => p.ProductRecordId == item.ProductRecordId);
                        PrescriptionProduct prescriptionProduct = new PrescriptionProduct
                        {
                            PrescriptionRecordId = PrescriptionId,
                            ProductRecordId = item.ProductRecordId,
                            ProductQuantity = item.ProductQuantity, //Convert.ToDecimal(string.Format("{0:F1}", item.ProductQuantity)),
                            ProductPrice = item.ProductCustomPrice,
                            ProductTaxValue = product.ProductTaxValue,
                            ProductTaxAmount = (product.ProductTaxValue / 100) * (item.ProductQuantity * item.ProductCustomPrice),
                            ProductUnit = product.ProductUnit,
                            PrescriptionProductStatus = PrescriptionProductStatus.New,

                            CreatedBy = IsUserLoggedIn.UserId,
                            CreatedDate = DateTime.Now,
                            DateEnabled = DateTime.Now,
                            IsEnabled = true,
                            IsEnabledBy = IsUserLoggedIn.UserId,
                            IsLocked = false
                        };

                        prescriptionProduct.SubTotal = (prescriptionProduct.ProductPrice * item.ProductQuantity) + prescriptionProduct.ProductTaxAmount;
                        prescriptionProductList.Add(prescriptionProduct);
                    }

                    DbContext.AddRange(prescriptionProductList);

                    var getPRecord = DbContext.Prescriptions.FirstOrDefault(p => p.PrescriptionRecordId == PrescriptionId);
                    getPRecord.PrescriptionStatus = PrescriptionStatus.APPROVED;

                    DbContext.Prescriptions.Update(getPRecord);
                    DbContext.SaveChanges();

                    aResp = new APIResponse
                    {
                        Message = "Des produits d'ordonnance ont été créés.",
                        Status = "Succès!",
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
                LogManager.LogInfo("AddPrescriptionProducts");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetInvoicePrescriptionCount(Guid userId)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetPrescriptionCount");
            try
            {
                var prescriptionCount = (from p in DbContext.Prescriptions
                                         join pp in DbContext.PrescriptionProducts on p.PrescriptionRecordId equals pp.PrescriptionRecordId
                                         where p.UserId == userId && p.PrescriptionStatus == PrescriptionStatus.APPROVED && pp.PrescriptionProductStatus == PrescriptionProductStatus.New && p.PatientId == Guid.Empty
                                         select p.PrescriptionRecordId
                                         ).Distinct().Count();
                aResp = new APIResponse
                {
                    Message = "Nombre d'ordonnances extraites avec succès.",
                    Status = "Succès!",
                    Payload = new { prescriptionCount = prescriptionCount },
                    StatusCode = System.Net.HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetPrescriptionCount");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetInvoicePrescriptionList(string _auth, Guid userId)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("GetInvoicePrescriptionList");

            try
            {
                if (IsUserLoggedIn != null)
                {
                    var user = DbContext.Users.Where(x => x.UserId == userId).FirstOrDefault();

                    if(user.AccountType == AccountTypes.HEALTHPROFESSIONAL)
                    {
                        //Get APPROVED Prescriptions
                        var prescriptionRecordIdList = (from p in DbContext.Prescriptions
                                                        join pp in DbContext.PrescriptionProducts on p.PrescriptionRecordId equals pp.PrescriptionRecordId
                                                        where p.CreatedBy == userId && p.PrescriptionStatus == PrescriptionStatus.APPROVED && p.PatientId == Guid.Empty // && pp.PrescriptionProductStatus == PrescriptionProductStatus.New
                                                        select p.PrescriptionRecordId
                                                   ).Distinct().ToList();

                        var invoicePrescriptionList = (from p in DbContext.Prescriptions
                                                       where prescriptionRecordIdList.Contains(p.PrescriptionRecordId)
                                                       select new InvoicePrescriptionListModel
                                                       {
                                                           PrescriptionRecordId = p.PrescriptionRecordId,
                                                           PrescriptionStatus = p.PrescriptionStatus,
                                                           DateAdded = p.CreatedDate.GetValueOrDefault(),
                                                           ShopId = p.ShopId,
                                                           ShopName = DbContext.Shops.FirstOrDefault(s => s.ShopId == p.ShopId).ShopName,
                                                           ShopIcon = DbContext.Shops.FirstOrDefault(s => s.ShopId == p.ShopId).ShopIcon,
                                                       }).ToList();

                        aResp = new APIResponse
                        {
                            Message = "Enregistrement récupéré avec succès.",
                            Status = "Succès!",
                            Payload = invoicePrescriptionList,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        //Get APPROVED Prescriptions
                        var prescriptionRecordIdList = (from p in DbContext.Prescriptions
                                                        join pp in DbContext.PrescriptionProducts on p.PrescriptionRecordId equals pp.PrescriptionRecordId
                                                        where p.CreatedBy == userId && p.PrescriptionStatus == PrescriptionStatus.APPROVED // && pp.PrescriptionProductStatus == PrescriptionProductStatus.New
                                                        select p.PrescriptionRecordId
                                                   ).Distinct().ToList();

                        var invoicePrescriptionList = (from p in DbContext.Prescriptions
                                                       where prescriptionRecordIdList.Contains(p.PrescriptionRecordId)
                                                       select new InvoicePrescriptionListModel
                                                       {
                                                           PrescriptionRecordId = p.PrescriptionRecordId,
                                                           PrescriptionStatus = p.PrescriptionStatus,
                                                           DateAdded = p.CreatedDate.GetValueOrDefault(),
                                                           ShopId = p.ShopId,
                                                           ShopName = DbContext.Shops.FirstOrDefault(s => s.ShopId == p.ShopId).ShopName,
                                                           ShopIcon = DbContext.Shops.FirstOrDefault(s => s.ShopId == p.ShopId).ShopIcon,
                                                       }).ToList();

                        aResp = new APIResponse
                        {
                            Message = "Enregistrement récupéré avec succès.",
                            Status = "Succès!",
                            Payload = invoicePrescriptionList,
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
                LogManager.LogInfo("GetInvoicePrescriptionList");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetPrescriptionProductList(string _auth, PrescriptionProductListParamModel model)
        {
            APIResponse aResp = new APIResponse();
            UserLoginTransaction IsUserLoggedIn = DbContext.UserLoginTransactions.AsNoTracking().FirstOrDefault(ult => ult.Token == _auth && ult.IsActive == true);
            LogManager.LogInfo("GetPrescriptionProductList");

            try
            {
                if (IsUserLoggedIn != null)
                {
                    var shopDetails = DbContext.Shops.AsNoTracking().FirstOrDefault(s => s.ShopId == model.ShopId);

                    var userPrescriptionList = (from pp in DbContext.PrescriptionProducts
                                                join p in DbContext.Products on pp.ProductRecordId equals p.ProductRecordId
                                                where pp.PrescriptionRecordId == model.PrescriptionRecordId && pp.PrescriptionProductStatus == PrescriptionProductStatus.New
                                                select new UserCartItem
                                                {
                                                    ShopId = shopDetails.ShopId,
                                                    ShopName = shopDetails.ShopName,
                                                    ShopAddress = shopDetails.Address,
                                                    ShopIcon = shopDetails.ShopIcon,
                                                    ProductIcon = p.ProductIcon,
                                                    SalePrice = p.SalePrice,
                                                    ProductRecordId = pp.ProductRecordId,
                                                    PrescriptionRecordId = pp.PrescriptionRecordId,
                                                    ProductName = p.ProductName,
                                                    ProductQuantity = pp.ProductQuantity,
                                                    ProductPrice = pp.ProductPrice,
                                                    ProductTaxValue = pp.ProductTaxValue,
                                                    ProductTaxAmount = pp.ProductTaxAmount
                                                }).ToList();

                    userPrescriptionList = userPrescriptionList.Select(s =>
                    {
                        s.ShopStatus = GetPharmacyOpenOrClose(s.ShopId);
                        s.TotalAmount = (s.ProductQuantity * s.ProductPrice) + s.ProductTaxAmount;
                        return s;
                    }).ToList();

                    if (userPrescriptionList.Count() > 0)
                    {
                        //var cartItemListForPrescription = DbContext.CartItems.AsNoTracking().Where(s => s.PrescriptionRecordId == model.PrescriptionRecordId && s.UserId == model.UserId).Count();
                        //if(cartItemListForPrescription == 0) //Check if prescription is added or not
                        //{
                        //    foreach (var item in userPrescriptionList)
                        //    {
                        //        CartItem NewCartItem = new CartItem
                        //        {
                        //            ShopId = item.ShopId,
                        //            UserId = model.UserId,
                        //            ProductRecordId = item.ProductRecordId,
                        //            ProductQuantity = item.ProductQuantity,
                        //            PrescriptionRecordId = model.PrescriptionRecordId,

                        //            CreatedBy = model.UserId,
                        //            CreatedDate = DateTime.Now,
                        //            DateEnabled = DateTime.Now,
                        //            IsEnabled = true,
                        //            IsEnabledBy = model.UserId,
                        //            LastEditedBy = model.UserId,
                        //            LastEditedDate = DateTime.Now
                        //        };
                        //        DbContext.Add(NewCartItem);
                        //    }
                        //    DbContext.SaveChanges();
                        //}

                        aResp = new APIResponse
                        {
                            Message = "Récupération de tous les articles avec succès.",
                            Status = "Succès!",
                            Payload = userPrescriptionList,
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                    else
                    {
                        aResp = new APIResponse
                        {
                            Message = "Aucun enregistrement trouvé.",
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
                LogManager.LogInfo("GetPrescriptionProductList");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse CancelInvoice(InvoiceParamModel model)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("CancelInvoice");
            try
            {
                var prescriptionList = DbContext.PrescriptionProducts.Where(s => s.PrescriptionRecordId == model.PrescriptionRecordId).ToList();
                if(prescriptionList.Count() > 0)
                {
                    foreach (var item in prescriptionList)
                    {
                        item.PrescriptionProductStatus = PrescriptionProductStatus.Cancelled;
                        item.LastEditedDate = DateTime.Now;
                        item.LastEditedBy = model.UserId;

                        DbContext.PrescriptionProducts.Update(item);
                        DbContext.SaveChanges();
                    }

                    var currentCartItems = DbContext.CartItems.Where(cci => cci.UserId == model.UserId && cci.PrescriptionRecordId == model.PrescriptionRecordId).ToList();
                    if (currentCartItems.Count() > 0) //list not null
                    {
                        DbContext.RemoveRange(currentCartItems);
                        DbContext.SaveChanges();
                    }

                    aResp = new APIResponse
                    {
                        Message = "Cette commande a été annulée.",
                        Status = "Succès!",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    aResp = new APIResponse
                    {
                        Message = "Aucun enregistrement trouvé.",
                        Status = "Échec!",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("CancelInvoice");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        public APIResponse GetPrescriptionRecordId(PrescriptionParamModel model)
        {
            APIResponse aResp = new APIResponse();
            LogManager.LogInfo("GetPrescriptionRecordId");
            try
            {
                int? prescriptionRecordId = null;
                var cartList = DbContext.CartItems.AsNoTracking().Where(s => s.UserId == model.UserId && s.ShopId == model.ShopId && s.PrescriptionRecordId != 0).ToList();
                if(cartList.Count() > 0)
                {
                    aResp.Message = "Tous les éléments ont été récupérés avec succès.";
                    aResp.Status = "Succès";
                    aResp.Payload = new
                    {
                        prescriptionRecordId = cartList.FirstOrDefault().PrescriptionRecordId
                    };
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
                else
                {
                    aResp.Message = "Aucun enregistrement trouvé.";
                    aResp.Status = "Succès";
                    aResp.Payload = new
                    {
                        prescriptionRecordId = prescriptionRecordId
                    };
                    aResp.StatusCode = System.Net.HttpStatusCode.OK;
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo("GetPrescriptionRecordId");
                LogManager.LogError(ex.InnerException.Message);
                LogManager.LogError(ex.StackTrace);
                aResp.Message = "Quelque chose s'est mal passé !";
                aResp.Status = "Erreur de serveur interne";
                aResp.StatusCode = System.Net.HttpStatusCode.BadRequest;
                aResp.ModelError = GetStackError(ex.InnerException);
            }
            return aResp;
        }
        private string GetPharmacyOpenOrClose(Guid shopId)
        {
            var today = DateTime.Now;
            var dayOfweek = today.DayOfWeek;
            var shopOpeningHours = DbContext.ShopOpeningHours.AsNoTracking().Where(s => s.ShopId == shopId && s.IsEnabled == true && s.DayOfWeek == dayOfweek).FirstOrDefault();
            if (shopOpeningHours != null)
            {
                if (shopOpeningHours.StartTimePM == "00:00:00" || shopOpeningHours.EndTimePM == "00:00:00")
                {
                    return "Fermer";
                }
                else if (shopOpeningHours.StartTimeEvening == "00:00:00" || shopOpeningHours.EndTimeEvening == "00:00:00")
                {
                    return "Fermer";
                }
                return "Ouvert";
            }
            return "Fermer";
        }
    }
}
