using AutoMapper;
using Duha.SIMS.DAL.Contexts;

namespace Duha.SIMS.BAL.Base
{
    public abstract class SIMSBalBase : BalRoot
    {
        protected readonly IMapper _mapper;
        protected readonly ApiDbContext _apiDbContext;

        public SIMSBalBase(IMapper mapper, ApiDbContext apiDbContext)
        {
            _mapper = mapper;
            _apiDbContext = apiDbContext;
        }
    }
    public abstract class SIMSBalOdataBase<T> : BalOdataRoot<T>
    {
        protected readonly IMapper _mapper;
        protected readonly ApiDbContext _apiDbContext;

        protected SIMSBalOdataBase(IMapper mapper, ApiDbContext apiDbContext)
        {
            _mapper = mapper;
            _apiDbContext = apiDbContext;
        }
    }
}
