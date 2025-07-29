using System;

namespace PharmaMoov.API.Services.CartExternal
{
    public class ExternalCartItem
    {
        public Guid ShopId { get; set; }
        public string MedipimProductId { get; set; }
        public Guid PatientId { get; set; }
        public int PrescriptionRecordId { get; set; }
        public decimal Quantity { get; set; }
    }

}