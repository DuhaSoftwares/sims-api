using Duha.SIMS.DomainModels.Base;
using Duha.SIMS.DomainModels.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Duha.SIMS.DomainModels.Product
{
    public class ProductCategoryDM : SIMSDomainModelBase<int>
    {
        [StringLength(200)]
        public string Name { get; set; }
        public int? LevelId { get; set; }

        public CategoryLevelDM Level {  get; set; }
         
        public virtual HashSet<ProductDM> Products { get; set; }
    }
}
