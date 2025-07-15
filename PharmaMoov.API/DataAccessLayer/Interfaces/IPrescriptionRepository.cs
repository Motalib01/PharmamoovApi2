using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.Prescription;
using System;
using System.Collections.Generic;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IPrescriptionRepository
    {
        APIResponse PopulatePrescriptions(int _prescriptionRecordId, int IsActive, Guid _shopId);
        APIResponse AddPrescription(string _auth, PrescriptionDetail _prescription);
        APIResponse EditPrescription(string _auth, PrescriptionDetail _prescription);
        APIResponse ChangePrescriptionStatus(string _auth, ChangeRecordStatus _prescription);
        APIResponse GetPrescription(int prescriptionRecordId);
        APIResponse GetPrescriptionDetails(int prescriptionRecordId);
        APIResponse AddPrescriptionProducts(string _auth, List<PrescriptionProductsParamModel> pModel);
        APIResponse GetInvoicePrescriptionCount(Guid userId);
        APIResponse GetInvoicePrescriptionList(string _auth, Guid userId);
        APIResponse GetPrescriptionProductList(string _auth, PrescriptionProductListParamModel model);
        APIResponse CancelInvoice(InvoiceParamModel model);
        APIResponse GetPrescriptionRecordId(PrescriptionParamModel model);
    }
}
