using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.Base.AutoIgnoreProperty;
using Duha.SIMS.ServiceModels.Enums;

namespace Duha.SIMS.ServiceModels.AppUsers.Login
{
    public class LoginUserSM : SIMSServiceModelBase<int>
    {
        public LoginUserSM()
        {
        }
        public RoleTypeSM RoleType { get; set; }
        public string LoginId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }

        [IgnorePropertyOnWrite(AutoMapConversionType.Dm2SmOnly)]
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePicturePath { get; set; }

        public bool IsPhoneNumberConfirmed { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public LoginStatusSM LoginStatus { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
