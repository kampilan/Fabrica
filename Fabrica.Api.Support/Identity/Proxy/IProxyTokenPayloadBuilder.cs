using Fabrica.Identity;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Api.Support.Identity.Proxy
{


    public interface IProxyTokenPayloadBuilder
    {

        IClaimSet Build( HttpContext context );

    }


}
