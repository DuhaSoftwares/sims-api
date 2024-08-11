using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.DAL.Contexts
{
    public abstract class EfCoreContextRoot : DbContext, IEfCoreContextRoot
    {
        public EfCoreContextRoot(DbContextOptions options)
            : base(options)
        {
        }
    }
}
