using Duha.SIMS.DomainModels.Base;
using Duha.SIMS.DomainModels.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Duha.SIMS.DomainModels.Product
{
    public class ProductCategoryDM : SIMSDomainModelBase<int>
    {
        [StringLength(200)]
        public string Name { get; set; }

        [MaxLength(int.MaxValue)]
<<<<<<< HEAD
        public string? Description { get; set; }

        public CategoryStatusDM Status { get; set; }

=======
        public string? ImagePath { get; set; }

        public LevelTypeDM Level { get; set; }

        [ForeignKey("LevelId")]
        public int? LevelId { get; set; }
>>>>>>> d7e1872a1de90f935ce092d442553b14f49297e8
        public virtual HashSet<ProductDM> Products { get; set; }
    }
}
