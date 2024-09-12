using Duha.SIMS.ServiceModels.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace Duha.SIMS.ServiceModels.Product
{
    public class ProductSM : SIMSServiceModelBase<int>
    {
        public string Name { get; set; }
        public string? Code { get; set; }
        public int? CategoryId { get; set; }
        public int BrandId { get; set; }
        public int UnitId { get; set; }        
        public string Variant { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public bool Status { get; set; }
    }
}
