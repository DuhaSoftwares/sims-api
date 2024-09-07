using Duha.SIMS.ServiceModels.Base;
using Duha.SIMS.ServiceModels.Enums;

namespace Duha.SIMS.ServiceModels.Product
{
    public class ProductCategorySM : SIMSServiceModelBase<int>
    {
        public string Name { get; set; }
<<<<<<< HEAD
        public string? Description { get; set; }

        public CategoryStatusSM Status { get; set; }
=======
        public string? ImagePath { get; set; }

        public LevelTypeSM Level { get; set; }

        public long? LevelId { get; set; }

>>>>>>> d7e1872a1de90f935ce092d442553b14f49297e8
        public int? ProductCount { get; set; }
    }
}
