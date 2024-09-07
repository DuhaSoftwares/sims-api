using Duha.SIMS.DomainModels.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Duha.SIMS.DomainModels.Product
{
    public class ProductDM : SIMSDomainModelBase<int>
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [ForeignKey(nameof(Brand))]
        public int? BrandId { get; set; }
        public virtual BrandDM Brand { get; set; }

        [ForeignKey(nameof(ProductCategory))]
        public int ProductCategoryId { get; set; }
        public virtual ProductCategoryDM ProductCategory { get; set; }
    }
}
