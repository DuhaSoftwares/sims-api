using Duha.SIMS.BAL.Base;
using Duha.SIMS.ServiceModels.Base;
using Microsoft.EntityFrameworkCore;
using System.Web.Http.OData.Query;

namespace Duha.SIMS.API.Controllers.Root
{
    public abstract class ApiControllerWithOdataRoot<T> : ApiControllerRoot where T : ServiceModelRoot
    {
        private readonly BalOdataRoot<T> _balOdataRoot;

        public ApiControllerWithOdataRoot(BalOdataRoot<T> balOdataRoot)
        {
            _balOdataRoot = balOdataRoot;
        }

        protected async Task<IEnumerable<T>> GetAsEntitiesOdata(ODataQueryOptions<T> oDataOptions)
        {
            return await ((oDataOptions.ApplyTo(await _balOdataRoot.GetServiceModelEntitiesForOdata()) as IQueryable<T>)?.ToListAsync());
        }
    }
}
