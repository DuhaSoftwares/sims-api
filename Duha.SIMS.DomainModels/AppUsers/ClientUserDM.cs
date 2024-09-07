using Duha.SIMS.DomainModels.AppUsers.Login;
using Duha.SIMS.DomainModels.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Duha.SIMS.DomainModels.Client;

namespace Duha.SIMS.DomainModels.AppUsers
{
    public class ClientUserDM : LoginUserDM
    {
        public ClientUserDM()
        {
        }
        public GenderDM? Gender { get; set; }

        [MaxLength(50)]
        [EmailAddress]
        public string PersonalEmailId { get; set; }

        public HashSet<ClientUserAddressDM> ClientUserAddress { get; set;}

        [ForeignKey(nameof(ClientCompanyDetail))]
        public int? ClientCompanyDetailId { get; set; }
        public virtual ClientCompanyDetailDM? ClientCompanyDetail { get; set; }

    }
}
