using Duha.SIMS.DomainModels.Base;
using System.ComponentModel.DataAnnotations;

namespace Duha.SIMS.DomainModels.Product
{
    public class BrandDM : SIMSDomainModelBase<int>
    {
        [StringLength(200)]
        public string Name { get; set; }
        [MaxLength(Int32.MaxValue)]
        public string ImagePath { get; set; }
        public virtual HashSet<ProductDM> Products { get; set; }
    }
}
