using System.Threading.Tasks;
using Fabrica.Watch;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Fabrica.One.Work.Processor.Parsers;

public class WorkMessageBodyParser: IMessageBodyParser
{


    public Task<(bool ok, WorkRequest request)> Parse(string body)
    {

        using var logger = this.EnterMethod();

        var request = JsonConvert.DeserializeObject<WorkRequest>( body );

        return Task.FromResult((true, request));

    }

}