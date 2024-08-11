using Microsoft.AspNetCore.Authentication;

namespace Duha.SIMS.BAL.Security
{
    public class DuhaAuthenticationSchemeOptions : AuthenticationSchemeOptions
    {
        public string JwtTokenSigningKey { get; set; }
    }
}
