using Duha.SIMS.DomainModels.AppUsers.Login;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Duha.SIMS.DomainModels.AppUsers
{
    [Index(nameof(LoginId), IsUnique = true)]
    [Index(nameof(EmailId), IsUnique = true)]
    public class ApplicationUserDM : LoginUserDM
    {
        public ApplicationUserDM()
        {
        }
        public virtual HashSet<ApplicationUserAddressDM> ApplicationUserAddress { get; set; }
    }
}
