using Fabrica.Identity;
using Fabrica.Watch;

namespace Fabrica.Api.Support.Identity.Token;

public class GatewayAccessTokenSource: IAccessTokenSource
{

    public GatewayAccessTokenSource(IGatewayTokenEncoder encoder, IClaimSet claims)
    {
        Encoder = encoder;
        Claims  = claims;

    }
    
    private IGatewayTokenEncoder Encoder { get; }
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