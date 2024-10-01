using Duha.SIMS.DomainModels.Base;
using Duha.SIMS.DomainModels.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class VariantDM : SIMSDomainModelBase<int>
{
    [StringLength(200)]
    public string Name { get; set; }

    public VariantLevelDM VariantLevel { get; set; }
    public int? VariantId { get; set; } 

    public virtual ICollection<CategoryVariantDM> CategoryVariants { get; set; }
}
