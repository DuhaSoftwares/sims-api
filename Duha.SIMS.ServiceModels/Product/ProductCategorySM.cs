using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.Enums;

namespace Duha.SIMS.ServiceModels.Product
{
    public class ProductCategorySM : SIMSServiceModelBase<int>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string? ImagePath { get; set; }

        public string? CategoryIcon { get; set; }
        public LevelTypeSM Level { get; set; }

        public long? LevelId { get; set; }

        public int? ProductCount { get; set; }
    }
}
