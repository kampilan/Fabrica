using System.Runtime.CompilerServices;
using System.Security.Claims;
using Fabrica.Exceptions;
using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabrica.Api.Support.Endpoints;

public abstract class BaseEndpointHandler
{

    static BaseEndpointHandler()
    {

        Settings = new JsonSerializerSettings
        {
            ContractResolver = new ModelContractResolver(),
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

    }

    protected static JsonSerializerSettings Settings { get; }



    [FromServices]
    public ICorrelation Correlation { get; set; } = null!;

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


    protected virtual bool TryValidate(BaseCriteria? criteria, out IResult error)
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(criteria), criteria);

        error = null!;


        if (criteria is null)
            throw new BadRequestException($"Errors occurred while parsing criteria for {Request.Method} at {Request.Path}");


        if (criteria.IsOverposted())
            throw new BadRequestException($"The following properties were not found: ({string.Join(',', criteria.GetOverpostNames())})")
                .WithErrorCode("DisallowedProperties");

        return true;

    }


    protected virtual bool TryValidate(BaseDelta? delta, out IResult error)
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(delta), delta);

        error = null!;


        if (delta is null)
            throw new BadRequestException($"Errors occurred while parsing model for {Request.Method} at {Request.Path}");


        if (delta.IsOverposted())
            throw new BadRequestException($"The following properties were not found or are not mutable: ({string.Join(',', delta.GetOverpostNames())})")
                .WithErrorCode("DisallowedProperties");

        return true;

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

        var serializer = JsonSerializer.Create(Settings);

        using var reader = new StreamReader(Request.Body);
        await using var jreader = new JsonTextReader(reader);

        var token = await JToken.LoadAsync(jreader);
        if (token is null)
            throw new BadRequestException($"Could not parse JSON body in {GetType().FullName}.FromBody<>");

        var target = token.ToObject<TTarget>(serializer);
        if (target is null)
            throw new BadRequestException($"Could not parse Token in {GetType().FullName}.FromBody<>");


        return target;

    }


}