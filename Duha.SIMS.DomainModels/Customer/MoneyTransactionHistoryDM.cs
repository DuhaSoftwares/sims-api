using Duha.SIMS.DomainModels.Base;
using Duha.SIMS.DomainModels.Enums;

namespace Duha.SIMS.DomainModels.Customer
{
    public class MoneyTransactionHistoryDM : SIMSDomainModelBase<int>
    {
           
        public decimal Amount { get; set; }
        public DateTime DateOfTransaction { get; set; }
        public PaymentMethodTypeDM PaymentMethod { get; set; }
        public int CustomerId { get; set; }
        public CustomerDM Customer { get; set; }
    }
}
