using Fabrica.Identity;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Api.Support.Identity.Gateway
{


    public interface IGatewayTokenPayloadBuilder
    {

        IClaimSet Build( HttpContext context );

    }


}
