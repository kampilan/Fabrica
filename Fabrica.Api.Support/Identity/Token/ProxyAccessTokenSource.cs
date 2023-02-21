using Fabrica.Identity;
using Fabrica.Watch;

namespace Fabrica.Api.Support.Identity.Token;

public class ProxyAccessTokenSource: IAccessTokenSource
{

    public ProxyAccessTokenSource(IProxyTokenEncoder encoder, IClaimSet claims)
    {
        Encoder = encoder;
        Claims  = claims;

    }
    
    private IProxyTokenEncoder Encoder { get; }
    private IClaimSet Claims { get; }

    public string Name { get; set; } = "";
    public bool HasExpired { get; set; }

    public Task<string> GetToken()
    {
        using var logger = this.EnterMethod();

        var token = Encoder.Encode(Claims);
        return Task.FromResult(token);

    }

}