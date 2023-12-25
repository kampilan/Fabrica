using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fabrica.Make.Sdk;

public class MakeExecuteRequest: HttpRequestMessage
{

    public MakeExecuteRequest( string scenarioId, object? body=null )
    {

        Method = HttpMethod.Post;

        RequestUri = new Uri($"/scenarios/{scenarioId}/run", UriKind.Relative);

        if( body is null ) 
            return;

        var input = new MakeExecuteInput {Data = body};

        var json = JsonSerializer.Serialize(input);
        Content = new StringContent(json, Encoding.UTF8, "application/json");

    }


}

public class MakeExecuteInput
{

    [JsonPropertyName("data")] 
    public object Data { get; set; } = null!;

}