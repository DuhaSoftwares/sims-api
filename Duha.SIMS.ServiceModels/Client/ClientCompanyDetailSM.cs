using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.Client
{
    public class ClientCompanyDetailSM : SIMSServiceModelBase<int>
    {
        public ClientCompanyDetailSM()
        {
        }

        public string CompanyCode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ContactEmail { get; set; }
        public string CompanyMobileNumber { get; set; }
        public string CompanyWebsite { get; set; }
        public string CompanyLogoPath { get; set; }

        //[ConvertFilePathToUri(SourcePropertyName = nameof(CompanyLogoPath))]
        public DateTime CompanyDateOfEstablishment { get; set; }
        public int ClientCompanyAddressId { get; set; }
    }
}
