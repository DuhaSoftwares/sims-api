using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.Product
{
    public class ProductSM : SIMSServiceModelBase<int>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public string ProductColor { get; set; }
        public DateTime LastViewed { get; set; }
        public int ViewCount { get; set; }
        public decimal Discount { get; set; }
        /*public string ManufacturerPartNumber { get; set; }
        public string EAN { get; set; }
        public string UPC { get; set; }*/
        public int? ProductCategoryId { get; set; }
        public int? BrandId { get; set; }
        public int ProductId { get; set; }
        public int? SellerId { get; set; }
        public decimal? DiscountPrice { get; set; }
        //public int ProductGroupId { get; set; }
        public double? AverageRating { get; set; }
        public int? TotalRating { get; set; }
    }
}
