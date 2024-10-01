using Duha.SIMS.DomainModels.Base;
using Duha.SIMS.DomainModels.Product;

public class CategoryVariantDM : SIMSDomainModelBase<int>
{
    public int ProductCategoryId { get; set; }

    // Navigation property for ProductCategory
    public virtual ProductCategoryDM ProductCategory { get; set; } // Change to single navigation property

    // Foreign key for Variant
    public int VariantId { get; set; }

    // Navigation property for Variant
    public virtual VariantDM Variant { get; set; } // Change to single navigation property
}
