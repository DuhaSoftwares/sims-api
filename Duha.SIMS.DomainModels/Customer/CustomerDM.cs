using Duha.SIMS.DomainModels.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Duha.SIMS.DomainModels.Enums;
using Duha.SIMS.DomainModels.Invoice;

namespace Duha.SIMS.DomainModels.Customer
{
    public class CustomerDM : SIMSDomainModelBase<int>
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]        
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        [EmailAddress]
        public string EmailId { get; set; }
        public string PhoneNumber { get; set; }
        public string Country {  get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
        public CustomerGroupDM CustomerGroup { get; set; }

        public ICollection<PurchaseHistoryDM> Purchases { get; set; }
        public ICollection<MoneyTransactionHistoryDM> MoneyTransactionHistories { get; set; }
    }
}
