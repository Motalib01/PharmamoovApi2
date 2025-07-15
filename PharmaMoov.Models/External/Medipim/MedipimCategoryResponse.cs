using System.Collections.Generic;

namespace PharmaMoov.Models.External.Medipim
{
    public class MedipimCategoryResponse
    {
        public Meta Meta { get; set; }
        public List<MedipimCategoryDto> Results { get; set; }
    }

    public class Meta
    {
        public int Total { get; set; }
    }
}