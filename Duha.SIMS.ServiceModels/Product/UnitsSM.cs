using Duha.SIMS.ServiceModels.Base;

namespace Duha.SIMS.ServiceModels.Product
{
    public class UnitsSM : SIMSServiceModelBase<int>
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
    }
}
