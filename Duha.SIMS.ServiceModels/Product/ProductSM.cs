using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.Product
{
    public class ProductSM : SIMSServiceModelBase<int>
    {
        public string Name { get; set; }
        public string? Code { get; set; }
        public int CategoryId { get; set; }
        public int WarehouseId { get; set; }
        public int SupplierId { get; set; }
        public int BrandId { get; set; }
        public int UnitId { get; set; }     
        public int ProductDetailId { get; set; }     
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
    }
}
