using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Duha.SIMS.DomainModels.Base;

namespace Duha.SIMS.DomainModels.Client
{
    public class ClientCompanyAddressDM : SIMSDomainModelBase<int>
    {
        public ClientCompanyAddressDM()
        {
        }

        [StringLength(50, MinimumLength = 0)]
        public string? Country { get; set; }

        [StringLength(50, MinimumLength = 0)]
        public string? State { get; set; }

        [StringLength(50, MinimumLength = 0)]
        public string? City { get; set; }

        [StringLength(50, MinimumLength = 0)]
        public string? Address1 { get; set; }

        [StringLength(50, MinimumLength = 0)]
        public string? Address2 { get; set; }

        [StringLength(20, MinimumLength = 0)]
        [RegularExpression(@"^\d{6}$")]
        public string? PinCode { get; set; }

        [ForeignKey(nameof(ClientCompanyDetail))]
        public int? ClientCompanyDetailId { get; set; }
        public virtual ClientCompanyDetailDM? ClientCompanyDetail { get; set; }
    }
}