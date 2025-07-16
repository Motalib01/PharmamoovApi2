using System;

namespace PharmaMoov.Models.External.Medipim
{
    public class AddMedipimProductToCartRequest
    {
        public Guid ShopId { get; set; }
        public Guid PatientId { get; set; }
        public string MedipimProductId { get; set; }
        public decimal Quantity { get; set; }
        public int PrescriptionRecordId { get; set; }
    }
}