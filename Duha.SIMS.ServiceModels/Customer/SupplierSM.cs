using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.Enums;

namespace Duha.SIMS.ServiceModels.Customer
{
    public class SupplierSM : SIMSServiceModelBase<int>
    {        
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }
        public string CompanyName { get; set; }
    }
}
