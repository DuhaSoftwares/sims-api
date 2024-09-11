using Duha.SIMS.DomainModels.Base;
using System.ComponentModel.DataAnnotations;

namespace Duha.SIMS.DomainModels.Product
{
    public class UnitsDM : SIMSDomainModelBase<int>
    {
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Symbol { get; set; }
        public virtual HashSet<ProductDM> Products { get; set; }
    }
}
