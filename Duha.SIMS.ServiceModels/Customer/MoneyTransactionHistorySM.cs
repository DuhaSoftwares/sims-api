using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.Enums;
using System.ComponentModel.DataAnnotations;

namespace Duha.SIMS.ServiceModels.Customer
{
    public class MoneyTransactionHistorySM : SIMSServiceModelBase<int>
    {
        public decimal Amount { get; set; }
        public DateTime DateOfTransaction { get; set; }
        public PaymentMethodTypeSM PaymentMethod { get; set; }
        public int CustomerId { get; set; }
    }
}
