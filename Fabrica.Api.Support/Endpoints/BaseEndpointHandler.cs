using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json;
using Fabrica.Exceptions;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Api.Support.Endpoints;

public abstract class BaseEndpointHandler
{


    [FromServices]
    public ICorrelation Correlation { get; set; } = null!;

    [FromServices] 
    public JsonSerializerOptions Options { get; set; } = null!;


    protected ILogger EnterMethod([CallerMemberName] string name = "")
    {
        var logger = Correlation.EnterMethod(GetType(), name);
        return logger;
    }


    public HttpContext Context { get; set; } = null!;


    public HttpRequest Request => Context.Request;
    public HttpResponse Response => Context.Response;
    public ClaimsPrincipal User => Context.User;


    public abstract Task<IResult> Handle();


    protected virtual void Validate(BaseCriteria? criteria)
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(criteria), criteria);



        if (criteria is null)
            throw new BadRequestException($"Errors occurred while parsing criteria for {Request.Method} at {Request.Path}");


        if (criteria.IsOverposted())
            throw new BadRequestException($"The following properties were not found: ({string.Join(',', criteria.GetOverpostNames())})")
                .WithErrorCode("DisallowedProperties");


    }


    protected virtual void Validate(BaseDelta? delta)
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(delta), delta);


        if (delta is null)
            throw new BadRequestException($"Errors occurred while parsing model for {Request.Method} at {Request.Path}");


        if (delta.IsOverposted())
            throw new BadRequestException($"The following properties were not found or are not mutable: ({string.Join(',', delta.GetOverpostNames())})")
                .WithErrorCode("DisallowedProperties");

    }



    protected async Task<string> FromBody()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        return body;
    }

    protected async Task<TTarget> FromBody<TTarget>() where TTarget : class
    {

        using var logger = EnterMethod();


        using var reader = new StreamReader(Request.Body);

        var target = await JsonSerializer.DeserializeAsync<TTarget>(Request.Body);
        if (target is null)
            throw new BadRequestException($"Could not parse Token in {GetType().FullName}.FromBody<>");

        return target;

    }


}