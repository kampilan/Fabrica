namespace Fabrica.Mediator;

[AttributeUsage(AttributeTargets.Class)]
public class HttpRpcRequestAttribute: Attribute
{

    public HttpRpcRequestAttribute( string path )
    {
        Path = path;
    }        

    public string Path { get; }
    public HttpMethod Method { get; set; } = HttpMethod.Post;


}