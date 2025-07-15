using System.Collections.Generic;

namespace PharmaMoov.Models.External.Medipim
{
    public class MedipimCategoryDto
    {
        public int Id { get; set; }
        public Dictionary<string, string> Name { get; set; }
        public int? Parent { get; set; }
        public int Order { get; set; }
    }
}