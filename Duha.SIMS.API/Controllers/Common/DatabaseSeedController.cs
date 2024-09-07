using Duha.SIMS.API.Controllers.Root;
using Duha.SIMS.BAL.Interface;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DAL.Seeds;
using Microsoft.AspNetCore.Mvc;

namespace Duha.SIMS.API.Controllers.Common
{
    [ApiController]
    [Route("[controller]")]
    public partial class DatabaseSeedController : ApiControllerRoot
    {
        private readonly ApiDbContext _apiDbContext;
        private readonly IPasswordEncryptHelper _passwordEncryptHelper;

        public DatabaseSeedController(ApiDbContext context, IPasswordEncryptHelper passwordEncryptHelper)
        {
            _apiDbContext = context;
            _passwordEncryptHelper = passwordEncryptHelper;
        }

        [HttpGet]
        [Route("Init")]
        public async Task<IActionResult> Get()
        {
            DatabaseSeeder<ApiDbContext> databaseSeeder = new DatabaseSeeder<ApiDbContext>();
            var retVal =  databaseSeeder.SetupDatabaseWithTestData(_apiDbContext, (x) => _passwordEncryptHelper.ProtectAsync<string>(x).Result);
            return Ok(retVal);
        }
    }
}
