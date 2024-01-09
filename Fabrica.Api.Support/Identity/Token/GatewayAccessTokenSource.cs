using System.Text.Json;
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

        var json = JsonSerializer.Serialize(Claims);
        var cc = JsonSerializer.Deserialize<ClaimSetModel>(json)??new ClaimSetModel();

        var token = Encoder.Encode(cc);
        return Task.FromResult(token);

    }

}