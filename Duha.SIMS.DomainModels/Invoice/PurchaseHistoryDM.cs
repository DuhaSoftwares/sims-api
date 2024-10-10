using Duha.SIMS.DomainModels.Base;
using Duha.SIMS.DomainModels.Product;
using Duha.SIMS.DomainModels.Customer;
namespace Duha.SIMS.DomainModels.Invoice
{
    /// <summary>
    /// Purchase History of particular Products
    /// </summary>
    public class PurchaseHistoryDM : SIMSDomainModelBase<int>
    {
        public int CustomerId { get; set; }
        public int ProductDetailsId { get; set; }
        public int Quantity { get; set; } 
        public DateTime PurchaseDate { get; set; } 
        public decimal TotalPrice { get; set; }
        public decimal MoneyPaid { get; set; }
        public CustomerDM Customer { get; set; } 
        public ProductDetailsDM ProductDetails { get; set; } 
    }
}
