
// ReSharper disable UnusedMember.Global

using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sprache;

namespace Fabrica.Api.Support.Filters;

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


public class ApiKeyEndpointFilter : IEndpointFilter
{

    public ApiKeyEndpointFilter(IApiKeyValidator validator, ICorrelation correlation)
    {
        _validator = validator;
        _correlation = correlation;
    }


    private readonly IApiKeyValidator _validator;
    private readonly ICorrelation _correlation;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {

        using var logger = _correlation.EnterMethod();

        var header = context.HttpContext.Request.Headers["x-api-key"];
        var key = header.FirstOrDefault();

        if( string.IsNullOrWhiteSpace(key) )
        {
            logger.Warning("API key not present");
            return Results.BadRequest();
        }
        
        if( !_validator.IsValid(key) )
        {
            logger.Warning("API key not Valid");
            return Results.Unauthorized();
        }

        return await next(context);


    }

}
