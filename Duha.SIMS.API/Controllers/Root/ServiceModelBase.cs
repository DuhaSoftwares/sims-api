namespace Duha.SIMS.API.Controllers.Root
{
    public abstract class ServiceModelBase
    {
        public DateTime CreatedOnUTC { get; set; }

        public DateTime? LastModifiedOnUTC { get; set; }

        protected ServiceModelBase()
        {
            CreatedOnUTC = DateTime.UtcNow;
        }
    }
}