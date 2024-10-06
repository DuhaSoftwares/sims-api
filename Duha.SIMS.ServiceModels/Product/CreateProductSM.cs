using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.Product
{
    public class CreateProductSM 
    {
        public ProductSM Product { get; set; }
        public List<ProductVariantDetailsSM> ProductVariantDetails { get; set; }
    }
}
