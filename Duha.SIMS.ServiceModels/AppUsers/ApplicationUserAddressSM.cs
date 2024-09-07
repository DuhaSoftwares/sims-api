using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.AppUsers
{
    public class ApplicationUserAddressSM : SIMSServiceModelBase<int>
    {
        public string Country { get; set; }

        public string State { get; set; }

        public string City { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string PinCode { get; set; }

        public int ApplicationUserId { get; set; }
    }
}
