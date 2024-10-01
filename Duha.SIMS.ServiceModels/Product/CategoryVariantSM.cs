using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.Product
{
    public class CategoryVariantSM : SIMSServiceModelBase<int>
    {
        public int? ProductCategoryId { get; set; }
        public int? VariantId { get; set; }
    }
}
