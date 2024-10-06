using Duha.SIMS.DomainModels.Base;
using Duha.SIMS.DomainModels.Client;
using Duha.SIMS.DomainModels.Customer;
using Duha.SIMS.DomainModels.Warehouse;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Duha.SIMS.DomainModels.Product
{
    public class ProductDM : SIMSDomainModelBase<int>
    {
        [StringLength(200)]
        public string Name { get; set; }
        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }
        public virtual ProductCategoryDM? Category { get; set; }

        [ForeignKey(nameof(Brand))]
        public int BrandId { get; set; }

        public virtual BrandDM Brand { get; set; }

        [ForeignKey(nameof(Unit))]
        public int UnitId { get; set; }
        public virtual UnitsDM Unit { get; set; }
        public ICollection<ProductDetailsDM> ProductDetails { get; set; }
        public ICollection<ProductVariantDM> ProductVariants { get; set; }
    }
}
