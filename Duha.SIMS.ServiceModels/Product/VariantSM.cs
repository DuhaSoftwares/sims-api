using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Duha.SIMS.ServiceModels.Product
{
    public class VariantSM : SIMSServiceModelBase<int>
    {
        public string Name { get; set; }
        public VariantLevelSM VariantLevel { get; set; }
        public int? VariantId { get; set; }
    }
}
