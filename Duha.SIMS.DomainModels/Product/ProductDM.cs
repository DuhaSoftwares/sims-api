using Duha.SIMS.DomainModels.Base;
using Duha.SIMS.DomainModels.Client;
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

        [StringLength(100)]
        public string? Code { get; set; }
        [ForeignKey(nameof(Category))]
        public int? CategoryId { get; set; }
        public virtual ProductCategoryDM? Category { get; set; }

        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }
        public virtual WarehouseDM Warehouse { get; set; }


        [ForeignKey(nameof(Brand))]
        public int BrandId { get; set; }

        public virtual BrandDM Brand { get; set; }

        [ForeignKey(nameof(Unit))]
        public int UnitId { get; set; }
        public virtual UnitsDM Unit { get; set; }

        [StringLength(100)]
        public string? Variant { get; set; }
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Image { get; set; }

        public bool Status { get; set; }
    }
}
