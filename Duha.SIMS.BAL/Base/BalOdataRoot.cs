namespace Duha.SIMS.BAL.Base
{
    public abstract class BalOdataRoot<T> : BalRoot
    {
        public abstract Task<IQueryable<T>> GetServiceModelEntitiesForOdata();
    }
}
