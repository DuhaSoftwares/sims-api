using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.Product
{
    public class CategoriesSM
    {
        public ProductCategorySM? Level1Category { get; set; }
        public List<ProductCategorySM>? Level2Categories { get; set; }
    }
}
