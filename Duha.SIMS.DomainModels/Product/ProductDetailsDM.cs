using Duha.SIMS.DomainModels.Base;
using Duha.SIMS.DomainModels.Client;
using Duha.SIMS.DomainModels.Customer;
using Duha.SIMS.DomainModels.Warehouse;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Duha.SIMS.DomainModels.Product
{
    public class ProductDetailsDM : SIMSDomainModelBase<int>
    {
        [ForeignKey(nameof(Products))]
        public int ProductId { get; set; }
        public virtual ProductDM Products { get; set; }

        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }
        public virtual WarehouseDM Warehouse { get; set; }

        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }
        public virtual SupplierDM Supplier { get; set; }

        [StringLength(100)]
        public string? Code { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }

    }
}
