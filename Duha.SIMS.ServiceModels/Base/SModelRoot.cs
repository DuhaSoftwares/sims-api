namespace Duha.SIMS.ServiceModels.Base
{
    public abstract class SModelRoot<T> : ServiceModelRoot
    {
        public T Id { get; set; }

        public string CreatedBy { get; set; }

        public string? LastModifiedBy { get; set; }
    }
}
