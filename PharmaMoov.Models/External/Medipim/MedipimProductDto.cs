using System.Collections.Generic;

namespace PharmaMoov.Models.External.Medipim
{
    public class MedipimProductDto
    {
        public string Id { get; set; }
        public Dictionary<string, string> Name { get; set; }
        public List<PhotoDto> Photos { get; set; }
        public List<MedipimCategoryDto> PublicCategories { get; set; }
        public List<BrandDto> Brands { get; set; }
        public string Status { get; set; }
        public decimal? PharmacistPrice { get; set; }
        public decimal? PublicPrice { get; set; }
        public List<string> Ean { get; set; }
        public string SupplierReference { get; set; }
    }

    public class PhotoDto
    {
        public int Id { get; set; }
        public Dictionary<string, string> Formats { get; set; }
    }

    public class BrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}