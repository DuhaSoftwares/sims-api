using Duha.SIMS.ServiceModels.Enums;
using Microsoft.AspNetCore.Identity;

namespace Duha.SIMS.ServiceModels.AppUsers.AutheticUser
{
    public class AuthenticUserSM : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public RoleTypeSM Role { get; set; }
    }
}
