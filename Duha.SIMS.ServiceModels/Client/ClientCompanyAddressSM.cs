using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.Client
{
    public class ClientCompanyAddressSM : SIMSServiceModelBase<int>
    {
        public ClientCompanyAddressSM()
        {
        }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string PinCode { get; set; }
        public int ClientCompanyDetailId { get; set; }
    }
}
