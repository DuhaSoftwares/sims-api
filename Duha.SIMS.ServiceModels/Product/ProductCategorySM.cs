using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.Enums;

namespace Duha.SIMS.ServiceModels.Product
{
    public class ProductCategorySM : SIMSServiceModelBase<int>
    {
        public string Name { get; set; }
        public int LevelId { get; set; }
        public CategoryLevelSM Level { get; set; }
        public bool Status { get; set; }
        public int? ProductCount { get; set; }
    }
}
