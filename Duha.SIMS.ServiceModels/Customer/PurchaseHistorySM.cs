using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.Customer
{
    public class PurchaseHistorySM : SIMSServiceModelBase<int>
    {
        public int CustomerId { get; set; }
        public int ProductDetailsId { get; set; }
        public int Quantity { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal MoneyPaid { get; set; }
    }
}
