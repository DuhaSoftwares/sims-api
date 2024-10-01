using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.Product
{
    public class VariantsSM
    {
        public VariantSM? Level1Variant { get; set; }
        public List<VariantSM>? Level2Variants { get; set; }
    }
}
