
// ReSharper disable UnusedMember.Global

using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Fabrica.Identity;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Api.Support.Identity.Gateway;

public class ClaimGatewayTokenPayloadBuilder: IGatewayTokenPayloadBuilder
{

    public ClaimGatewayTokenPayloadBuilder()
    {

        ClaimMap = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(CreateStdMappings()));

    }

    public ClaimGatewayTokenPayloadBuilder( IEnumerable<KeyValuePair<string, string>> mappings )
    {

        var std = CreateStdMappings();
        foreach (var pair in mappings)
            std[pair.Key] = pair.Value;
        
        ClaimMap = new ReadOnlyDictionary<string,string>( new Dictionary<string,string>( std ) );

    }

    private IReadOnlyDictionary<string,string> ClaimMap { get; }

    private Dictionary<string, string> CreateStdMappings()
    {

        var mappings = new Dictionary<string, string>
        {

            [FabricaClaims.FlowClaim]            = FabricaClaims.FlowClaim,
            [FabricaClaims.TenantClaim]          = FabricaClaims.TenantClaim,
            [JwtRegisteredClaimNames.Sub]        = FabricaClaims.SubjectClaim,
            [ClaimTypes.NameIdentifier]          = FabricaClaims.SubjectClaim,
            [FabricaClaims.AltSubjectClaim]      = FabricaClaims.AltSubjectClaim,
            ["preferred_username"]               = FabricaClaims.UserNameClaim,
            [JwtRegisteredClaimNames.GivenName]  = FabricaClaims.GivenNameClaim,
            [JwtRegisteredClaimNames.FamilyName] = FabricaClaims.FamilyNameClaim,
            [JwtRegisteredClaimNames.Name]       = FabricaClaims.NameClaim,
            ["picture-url"]                      = FabricaClaims.PictureClaim,
            [JwtRegisteredClaimNames.Email]      = FabricaClaims.EmailClaim,
            ["role"]                             = FabricaClaims.RoleClaim,

        };

        return mappings;

    }


    public IClaimSet Build(HttpContext context)
    {

        using var logger = this.EnterMethod();

        var set = Build(context.User.Claims);

        return set;

    }

    public IClaimSet Build( IEnumerable<Claim> claims )
    {

        using var logger = this.EnterMethod();

        logger.LogObject(nameof(ClaimMap), ClaimMap);

        var payload = new ClaimSetModel();

        foreach( var claim in claims )
        {

            logger.Inspect(nameof(claim.Type), claim.Type);
            logger.Inspect(nameof(claim.Value), claim.Value);

            if( ClaimMap.TryGetValue(claim.Type, out var mapped) )
            {

                logger.Inspect(nameof(mapped), mapped);

                switch( mapped )
                {
                    case FabricaClaims.FlowClaim:
                        payload.AuthenticationFlow = claim.Value;
                        break;
                    case FabricaClaims.TenantClaim:
                        payload.Tenant = claim.Value;
                        break;
                    case FabricaClaims.SubjectClaim:
                        payload.Subject = claim.Value;
                        break;
                    case FabricaClaims.AltSubjectClaim:
                        payload.AltSubject = claim.Value;
                        break;
                    case FabricaClaims.UserNameClaim:
                        payload.UserName = claim.Value;
                        break;
                    case FabricaClaims.GivenNameClaim:
                        payload.GivenName = claim.Value;
                        break;
                    case FabricaClaims.FamilyNameClaim:
                        payload.FamilyName = claim.Value;
                        break;
                    case FabricaClaims.NameClaim:
                        payload.Name = claim.Value;
                        break;
                    case FabricaClaims.PictureClaim:
                        payload.Picture = claim.Value;
                        break;
                    case FabricaClaims.EmailClaim:
                        payload.Email = claim.Value;
                        break;
                    case nameof(ClaimSetModel.Roles):
                        payload.Roles.Add(claim.Value);
                        break;

                }

            }

        }


        logger.LogObject(nameof(payload), payload);


        return payload;

    }


}