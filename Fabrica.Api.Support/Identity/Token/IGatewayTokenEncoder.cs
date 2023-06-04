using Fabrica.Identity;

namespace Fabrica.Api.Support.Identity.Token;

public interface IGatewayTokenEncoder
{

    string Encode( IClaimSet claims );
    IClaimSet Decode( string authType, string token, bool validate=true );


}