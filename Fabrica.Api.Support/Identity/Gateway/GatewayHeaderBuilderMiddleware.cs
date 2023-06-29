using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fabrica.Identity;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Api.Support.Identity.Gateway;

public class GatewayHeaderBuilderMiddleware
{

    public GatewayHeaderBuilderMiddleware(RequestDelegate next)
    {
        Next = next;
    }

    private RequestDelegate Next { get; }



    public async Task Invoke( HttpContext context, ICorrelation correlation, IGatewayTokenPayloadBuilder builder )
    {


        using var logger = correlation.EnterMethod<GatewayTokenBuilderMiddleware>();


        // *****************************************************************
        logger.Debug("Attempting to remove existing gateway header");
        if (context.Request.Headers.ContainsKey(IdentityConstants.IdentityHeaderName))
            context.Request.Headers.Remove(IdentityConstants.IdentityHeaderName);



        // *****************************************************************
        logger.Debug("Attempting to check if current call is authenticated");
        if (context.User.Identity is { IsAuthenticated: false })
        {
            logger.Debug("Not authenticated");
            await Next(context);
            return;
        }



        // *****************************************************************
        logger.Debug("Attempting to build claim set");
        var claims = builder.Build( context );

        var set = new ClaimSet
        {
            AuthenticationType = claims.AuthenticationType,
            AuthenticationFlow = claims.AuthenticationFlow,
            Tenant             = claims.Tenant,
            Subject            = claims.Subject,
            AltSubject         = claims.AltSubject,
            UserName           = claims.UserName,
            GivenName          = claims.GivenName,
            FamilyName         = claims.FamilyName,
            Name               = claims.Name,
            Email              = claims.Email,
            Picture            = claims.Picture
        };

        set.Roles.AddRange(claims.Roles);

        logger.LogObject(nameof(set), set);



        // *****************************************************************
        logger.Debug("Attempting to serialize claims set to json");
        var json = JsonSerializer.Serialize( set );



        // *****************************************************************
        logger.Debug("Attempting to set identity header");
        context.Request.Headers.Add(IdentityConstants.IdentityHeaderName, json);



        // *****************************************************************
        await Next(context);

    }


    private class ClaimSet : IClaimSet
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AuthenticationType { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AuthenticationFlow { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? Expiration { get; set; }
        public void SetExpiration(TimeSpan ttl)
        {
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Tenant { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Subject { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AltSubject { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? UserName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? GivenName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FamilyName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Email { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Picture { get; set; }
        public List<string> Roles { get; set; } = new();

        [JsonIgnore]
        IEnumerable<string> IClaimSet.Roles => Roles;

    }


}

