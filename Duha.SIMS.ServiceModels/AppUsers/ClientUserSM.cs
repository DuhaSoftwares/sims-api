using Duha.SIMS.ServiceModels.AppUsers.Login;
using Duha.SIMS.ServiceModels.Enums;

namespace Duha.SIMS.ServiceModels.AppUsers
{
    public class ClientUserSM : LoginUserSM
    {
        public ClientUserSM()
        {
        }
        public GenderSM Gender { get; set; }
        public string PersonalEmailId { get; set; }
        public int? ClientCompanyDetailId { get; set; }


    }
}
