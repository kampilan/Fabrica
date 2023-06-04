using Fabrica.Exceptions;
using Fabrica.Identity;
using Fabrica.Watch;
using Jose;

namespace Fabrica.Api.Support.Identity.Token;

public class GatewayTokenJwtEncoder: IGatewayTokenEncoder
{


    public byte[] TokenSigningKey { get; set; }
    public TimeSpan TokenTimeToLive { get; set; } = TimeSpan.FromSeconds(30);

    public string Encode( IClaimSet claims )
    {

        using var logger = this.EnterMethod();

        if (!claims.Expiration.HasValue)
            claims.SetExpiration(TokenTimeToLive);

        logger.LogObject(nameof(claims), claims);


        var token = TokenSigningKey == null ? JWT.Encode( claims, null, JwsAlgorithm.none ) : JWT.Encode( claims, TokenSigningKey, JwsAlgorithm.HS256 );

        logger.Inspect(nameof(token), token);

        return token;

    }


    public IClaimSet Decode( string authType, string token, bool validate=true )
    {

        using var logger = this.EnterMethod();

        logger.Inspect(nameof(token), token);


        var claims = TokenSigningKey == null
            ? JWT.Decode<ClaimSetModel>(token, null, JwsAlgorithm.none)
            : JWT.Decode<ClaimSetModel>(token, TokenSigningKey, JwsAlgorithm.HS256);


        if( validate && claims.Expiration.HasValue )
        {
            var exp = DateTime.UnixEpoch.AddSeconds(claims.Expiration.Value);
            if( exp < DateTime.UtcNow )
                throw new NotAuthorizedException("Token expired");
        }


        claims.AuthenticationType = authType;

        logger.LogObject(nameof(claims), claims);

        return claims;

    }


}