using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Duha.SIMS.DomainModels.Base;

namespace Duha.SIMS.DomainModels.AppUsers
{
    public class ClientUserAddressDM : SIMSDomainModelBase<int>
    {
        [StringLength(100, MinimumLength = 0)]
        public string? Country { get; set; }

        [StringLength(100, MinimumLength = 0)]
        public string? State { get; set; }

        [StringLength(100, MinimumLength = 0)]
        public string? City { get; set; }

        [StringLength(100, MinimumLength = 0)]
        public string? Address1 { get; set; }

        [StringLength(100, MinimumLength = 0)]
        public string? Address2 { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 0)]
        [RegularExpression(@"^\d{6}$")]
        public string? PinCode { get; set; }

        [ForeignKey(nameof(ClientUser))]
        public int? ClientUserId { get; set; }
        public virtual ClientUserDM? ClientUser { get; set; }
    }
}