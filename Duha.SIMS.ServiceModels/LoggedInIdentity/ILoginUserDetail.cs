using Duha.SIMS.ServiceModels.Enums;

namespace Duha.SIMS.ServiceModels.LoggedInIdentity
{
    public interface ILoginUserDetail
    {
        public int DbRecordId { get; set; }
        public string LoginId { get; set; }
        public RoleTypeSM UserType { get; set; }
    }

    public class LoginUserDetail : ILoginUserDetail
    {
        public int DbRecordId { get; set; }
        public string LoginId { get; set; }
        public RoleTypeSM UserType { get; set; }
    }

    public class LoginUserDetailWithCompany : ILoginUserDetail
    {
        public int DbRecordId { get; set; }
        public string LoginId { get; set; }
        public int CompanyRecordId { get; set; }
        public string CompanyCode { get; set; }
        public RoleTypeSM UserType { get; set; }
    }
}
