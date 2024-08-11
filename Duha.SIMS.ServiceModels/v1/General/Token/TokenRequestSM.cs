using Duha.SIMS.ServiceModels.Enums;

namespace Duha.SIMS.ServiceModels.v1.General.Token
{
    public class TokenRequestSM : TokenRequestRoot
    {
        public string CompanyCode { get; set; }
        public RoleTypeSM RoleType { get; set; }
    }
}
