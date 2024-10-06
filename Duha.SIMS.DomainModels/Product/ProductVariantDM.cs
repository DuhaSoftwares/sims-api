using Duha.SIMS.DomainModels.Base;
using Duha.SIMS.DomainModels.Product;

public class ProductVariantDM : SIMSDomainModelBase<int>
{
    public int ProductId { get; set; }
    public ProductDM Product { get; set; }

    public int VariantLevel1Id { get; set; }
    public VariantDM VariantLevel1 { get; set; } 

    public int VariantLevel2Id { get; set; }
    public VariantDM VariantLevel2 { get; set; } 
}

