namespace Duha.SIMS.DomainModels.Base
{
    public abstract class DomainModelRoot
    {
        public DateTime CreatedOnUTC { get; set; }

        public DateTime? LastModifiedOnUTC { get; set; }

        protected DomainModelRoot()
        {
            CreatedOnUTC = DateTime.UtcNow;
        }
    }
}
