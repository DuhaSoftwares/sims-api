using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.Product
{
    public class Level2CategoriesSM 
    {
        public ProductCategorySM? Level2Category { get;set; }
        public List<ProductCategorySM>? Level3Categories { get; set; }      
    }
}
