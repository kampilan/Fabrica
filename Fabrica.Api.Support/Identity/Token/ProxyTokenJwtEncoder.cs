using Fabrica.Identity;
using Fabrica.Watch;
using Jose;

namespace Fabrica.Api.Support.Identity.Token
{

    public class ProxyTokenJwtEncoder: IProxyTokenEncoder
    {


        public byte[] TokenSigningKey { get; set; }

        public string Encode( IClaimSet claims )
        {

            using var logger = this.EnterMethod();

            logger.LogObject(nameof(claims), claims);

            string token;
            if( TokenSigningKey == null )
                token = JWT.Encode(claims, null, JwsAlgorithm.none);
            else
                token = JWT.Encode( claims, TokenSigningKey, JwsAlgorithm.HS256 );

            logger.Inspect(nameof(token), token);

            return token;

        }


        public IClaimSet Decode( string authType, string token )
        {

            using var logger = this.EnterMethod();

            logger.Inspect(nameof(token), token);


            ClaimSetModel claims;
            if( TokenSigningKey == null )
                claims = JWT.Decode<ClaimSetModel>( token, null, JwsAlgorithm.none );
            else
                claims = JWT.Decode<ClaimSetModel>(token, TokenSigningKey, JwsAlgorithm.HS256 );

            claims.AuthenticationType = authType;

            return claims;

        }


    }


}
