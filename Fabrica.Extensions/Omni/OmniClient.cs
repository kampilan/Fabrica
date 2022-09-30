using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Fabrica.Watch;
using Newtonsoft.Json;

namespace Fabrica.Omni;

public class OmniClient
{

    public static OutboundEmail Create()
    {
        return new OutboundEmail();
    }

    public OmniClient(IHttpClientFactory factory)
    {

        Factory = factory;

    }

    private IHttpClientFactory Factory { get; }


    public async Task<SendResult> Send( OutboundEmail model )
    {

        using var logger = this.EnterMethod();


        var json = JsonConvert.SerializeObject(model);

        using var client = Factory.CreateClient("Fabrica.Omni.Email");

        var res = await client.PostAsJsonAsync("/mail/send", json);

        var resJson = await res.Content.ReadAsStringAsync();

        var result = JsonConvert.DeserializeObject<SendResult>(resJson);

        return result;

    }


}

