using System.Linq;
using System.Threading.Tasks;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fabrica.Api.Support.Filters
{

    
    public class ApiKeyActionFilter: IAsyncActionFilter
    {

        public ApiKeyActionFilter( IApiKeyValidator validator, ICorrelation correlation )
        {
            Validator   = validator;
            Correlation = correlation;
        }


        private IApiKeyValidator Validator { get; }
        private ICorrelation Correlation { get; }


        public void OnAuthorization( AuthorizationFilterContext context )
        {

        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {

            var logger = Correlation.GetLogger(this);

            try
            {

                logger.EnterMethod();

                var header = context.HttpContext.Request.Headers["x-api-key"];
                var key    = header.FirstOrDefault();

                if (string.IsNullOrWhiteSpace(key))
                {
                    logger.Warning("API key not present");
                    context.Result = new StatusCodeResult(401);
                    return;
                }
                else if (!Validator.IsValid(key))
                {
                    logger.Warning("API key not Valid");
                    context.Result = new StatusCodeResult(401);
                    return;
                }

                await next();

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }


}
