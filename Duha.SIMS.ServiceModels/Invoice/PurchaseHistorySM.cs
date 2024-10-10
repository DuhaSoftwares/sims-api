using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.Enums;

namespace Duha.SIMS.ServiceModels.Invoice
{
    public class PurchaseHistorySM :SIMSServiceModelBase<int>
    {
        public int CustomerId { get; set; }
        public int ProductDetailsId { get; set; }
        public int Quantity { get; set; }
        public decimal MoneyPaid { get; set; } = 0;
        public PaymentMethodTypeSM PaymentMethod { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
