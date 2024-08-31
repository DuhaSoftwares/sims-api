using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.Product
{
    public class BrandSM  : SIMSServiceModelBase<int>
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public int ProductCount { get; set; }
    }
}
