namespace PharmaMoov.Models.External.Medipim
{
    public class GetMedipimProductsRequest
    {
        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 100;
        public long UpdatedSince { get; set; } = 1640991600;
        public int? PublicCategoryId { get; set; }
    }

}