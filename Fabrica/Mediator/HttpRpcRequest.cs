using System.Reflection;
using MediatR;
using Newtonsoft.Json;

namespace Fabrica.Mediator;

public interface IHttpRpcRequest
{
    string HttpClientName { get; }
    string Path { get; }

}


public class HttpRpcRequest<TResponse> : IHttpRpcRequest, IRequest<Response<TResponse>>  where TResponse : class
{

    public HttpRpcRequest()
    {
    }

    public HttpRpcRequest( object body )
    {

        var attr = body.GetType().GetCustomAttribute<HttpRpcRequestAttribute>();
        if (attr != null)
        {
            Method = attr.Method;
            Path = attr.Path;
        }

        ToBody(body);

    }


    public string HttpClientName { get; set; } = "default";

    private string _baseAddress = "";
    public string BaseAddress
    {
        get => _baseAddress;
        set
        {
            if( !string.IsNullOrWhiteSpace(value) && value != _baseAddress )
            {
                HttpClientName = "";
                _baseAddress = value;
            }
        }
    }


    public Dictionary<string, string> CustomHeaders { get; } = new();

    public HttpMethod Method { get; set; } = HttpMethod.Post;
    public string Path { get; set; } = "";

    public string BodyContent { get; set; } = "{}";

    public void ToBody(object source)
    {
        var json = JsonConvert.SerializeObject(source);
        BodyContent = json;
    }




}