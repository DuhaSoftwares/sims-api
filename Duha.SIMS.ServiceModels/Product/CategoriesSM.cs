using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.Product
{
    public class CategoriesSM
    {
        public ProductCategorySM? Level1Category { get; set; }
        public List<Level2CategoriesSM>? AssociatedLevels { get; set; }
    }
}
