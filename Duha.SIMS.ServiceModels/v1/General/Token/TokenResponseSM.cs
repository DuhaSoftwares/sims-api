using Duha.SIMS.ServiceModels.AppUsers.Login;

namespace Duha.SIMS.ServiceModels.v1.General.Token
{
    public class TokenResponseSM : TokenResponseRoot
    {
        public LoginUserSM LoginUserDetails { get; set; }
        public int ClientCompanyId { get; set; }
    }
}
