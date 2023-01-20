
// ReSharper disable UnusedMember.Global

using Fabrica.Watch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;


namespace Fabrica.Api.Support.Conventions;

public class ApiKeyAuthorizationConvention<TAuthorizationFilter> : IApplicationModelConvention where TAuthorizationFilter : class, IAuthorizationFilter, new()
{


    public void Apply( ApplicationModel application )
    {

        var logger = this.GetLogger();

        try
        {

            logger.EnterMethod();

            logger.Inspect("AuthorizationFilter type", typeof(TAuthorizationFilter).FullName);


            foreach( var controller in application.Controllers )
            {


                logger.Inspect(nameof(controller.ControllerName), controller.ControllerName);


                // *****************************************************************
                logger.Debug("Attempting to check for Authorize attributes");
                var attributes = controller.ControllerType.GetCustomAttributes(typeof(AuthorizeAttribute), false);

                if (logger.IsDebugEnabled)
                {
                    logger.Inspect(nameof(attributes.Length), attributes.Length);
                    var policies = attributes.Cast<AuthorizeAttribute>().Select(a => a.Policy).ToList();
                    logger.LogObject(nameof(policies), policies);
                }

                if( attributes.Length == 0 )
                    continue;


                controller.Filters.Add(new TAuthorizationFilter());

            }


        }
        finally
        {
            logger.LeaveMethod();
        }


    }




}