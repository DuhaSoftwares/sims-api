using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.Invoice;

namespace Duha.SIMS.ServiceModels.Customer
{
    public class OutstandingBalanceHistorySM : SIMSServiceModelBase<int>
    {
        public string CustomerName { get; set; }
        public decimal OutstandingBalance { get; set; }
        public List<PurchaseHistorySM> Purchases { get; set; }
    }
}
