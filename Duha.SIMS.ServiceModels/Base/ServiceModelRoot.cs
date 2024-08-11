namespace Duha.SIMS.ServiceModels.Base
{
    public class ServiceModelRoot
    {
        public DateTime CreatedOnUTC { get; set; }

        public DateTime? LastModifiedOnUTC { get; set; }

        protected ServiceModelRoot()
        {
            CreatedOnUTC = DateTime.UtcNow;
        }
    }
}
