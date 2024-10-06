using Duha.SIMS.DomainModels.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Duha.SIMS.DomainModels.Enums;
using Duha.SIMS.DomainModels.Product;

namespace Duha.SIMS.DomainModels.Customer
{
    public class SupplierDM : SIMSDomainModelBase<int>
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
        public string CompanyName { get; set; }

        public ICollection<ProductDetailsDM> ProductDetails { get; set; }
    }
}
