using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fabrica.Api.Support.Identity.Proxy
{

    public class TokenAuthorizationFilter : IAuthorizationFilter
    {


        public void OnAuthorization( AuthorizationFilterContext context )
        {

            using var logger = this.EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to dig out Identity from HttpContext");
            var identity = context.HttpContext.User.Identity?? new NullUser();

            logger.Inspect(nameof(identity.IsAuthenticated), identity.IsAuthenticated);

            if ( identity.IsAuthenticated )
            {
                logger.Debug("Authorized");
                return;
            }


            // *****************************************************************
            logger.Debug("Not Authorized");
            context.Result = new StatusCodeResult(401);


        }


    }


}
