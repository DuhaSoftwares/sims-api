using Microsoft.AspNetCore.Mvc.Filters;

namespace Duha.SIMS.BAL.Exceptions
{
    public abstract class APIExceptionFilterRoot : ExceptionFilterAttribute
    {
        public APIExceptionFilterRoot()
        {
        }

        public override void OnException(ExceptionContext context)
        {
            base.OnException(context);
        }
    }
}