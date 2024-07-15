
// ReSharper disable UnusedMember.Global

using System.Text;
using System.Text.Json;


namespace Fabrica.Http;

public class HttpRequestBuilder
{


    private HttpRequestBuilder()
    {
    }


    public static HttpRequestBuilder Get( string httpClientName="Api" )
    {
        var builder = new HttpRequestBuilder
        {
            HttpClientName = httpClientName,
            Method = HttpMethod.Get
        };

        return builder;
    }

    public static HttpRequestBuilder Post(string httpClientName = "Api")
    {
        var builder = new HttpRequestBuilder
        {
            HttpClientName = httpClientName,
            Method         = HttpMethod.Post
        };

        return builder;
    }

    public static HttpRequestBuilder Put(string httpClientName = "Api")
    {
        var builder = new HttpRequestBuilder
        {
            HttpClientName = httpClientName,
            Method = HttpMethod.Put
        };

        return builder;
    }

    public static HttpRequestBuilder Patch(string httpClientName = "Api")
    {
        var builder = new HttpRequestBuilder
        {
            HttpClientName = httpClientName,
            Method = new HttpMethod("PATCH")
        };

        return builder;
    }


    public static HttpRequestBuilder Delete(string httpClientName = "Api")
    {
        var builder = new HttpRequestBuilder
        {
            HttpClientName = httpClientName,
            Method = HttpMethod.Delete
        };

        return builder;
    }


    public static implicit operator string( HttpRequestBuilder builder )
    {

        var path = builder.ToString();

        return path;

    }


    public static implicit operator HttpRequest( HttpRequestBuilder builder )
    {

        var request = new HttpRequest
        {
            DebugMode      = builder.DebugMode,
            HttpClientName = builder.HttpClientName,
            Method         = builder.Method,
            Path           = builder.ToString(),
        };


        if (builder.DebugMode)
            request.CustomHeaders["Fabrica-Watch-Debug"] = "1";


        if( string.IsNullOrWhiteSpace(builder.Json) && builder.Body != null )
            request.ToBody(builder.Body, builder.Options);
        else if( !string.IsNullOrWhiteSpace(builder.Json) )
            request.ToBody( builder.Json );
        else if( builder.BodyStream != null )
        {

            var content = new MemoryStream();
            builder.BodyStream.CopyTo( content );
            content.Seek(0, SeekOrigin.Begin);

            request.BodyContent = new StreamContent( content );

        }


        return request;

    }


    private HttpMethod Method { get; init; } = HttpMethod.Get;

    private bool DebugMode { get; set; }


    private string HttpClientName { get; init; } = "";

    private bool AtRoot { get; set; }
    private string Path { get; set; } = "";
    private string Uid { get; set; } = "";
    private string SubResource { get; set; } = "";
    private string SubUid { get; set; } = "";
    private List<string> Rql { get; } = new ();

    private List<KeyValuePair<string,object>> QueryParameters { get; set; } = new ();


    private JsonSerializerOptions Options { get; set; } = null!;

    private string? Json { get; set; }

    private object? Body { get; set; }

    private Stream? BodyStream { get; set; }


    public bool AddHost { get; set; } = true;


    public HttpRequestBuilder InDebugMode()
    {
        DebugMode = true;
        return this;
    }


    public HttpRequestBuilder ForPath( params string[] segments )
    {

        Path = string.Join("/", segments );

        Uid         = "";
        SubResource = "";
        SubUid      = "";

        return this;

    }


    public HttpRequestBuilder ForResource( string resource, bool atRoot=false )
    {

        Path   = resource;
        AtRoot = atRoot;

        return this;

    }


    public HttpRequestBuilder WithIdentifier( string uid )
    {
        Uid = uid;
        return this;

    }


    public HttpRequestBuilder WithSubResource( string sub )
    {
        SubResource = sub;
        return this;

    }


    public HttpRequestBuilder WithSubIdentifier(string uid)
    {
        SubUid = uid;
        return this;

    }



    public HttpRequestBuilder WithRql( params string[] filters )
    {
        Rql.AddRange( filters );
        return this;
    }



    public HttpRequestBuilder WithRql(  IEnumerable<string> filters )
    {
        Rql.AddRange(filters);
        return this;
    }


    public HttpRequestBuilder AddParameter( string name, object value )
    {
        QueryParameters.Add( new KeyValuePair<string,object>(name, value) );
        return this;
    }


    public HttpRequestBuilder WithParameters( IEnumerable<KeyValuePair<string,object>> parameters )
    {
        QueryParameters = [..parameters];
        return this;
    }


    public HttpRequestBuilder WithJson( string json )
    {
        Json = json;
        return this;
    }


    public HttpRequestBuilder WithBody( object body, JsonSerializerOptions? options=null )
    {
        Body = body;
        Options = options;
        return this;
    }


    public HttpRequestBuilder WithStream( Stream body )
    {
        BodyStream  = body;
        return this;
    }



    public HttpRequest ToRequest()
    {
        return this;
    }

    public override string ToString()
    {

        var builder = new StringBuilder();


        if ( !string.IsNullOrWhiteSpace(Path) )
        {
            if( AtRoot && !Path.StartsWith("/") )
                builder.Append("/");
            builder.Append(Path);
        }

        if( !string.IsNullOrWhiteSpace(Uid) )
        {
            builder.Append("/");
            builder.Append(Uid);
        }

        if( !string.IsNullOrWhiteSpace(SubResource) )
        {
            if (!SubResource.StartsWith("/"))
                builder.Append("/");
            builder.Append(SubResource);
        }

        if( !string.IsNullOrWhiteSpace(SubUid) && !string.IsNullOrWhiteSpace(SubResource) )
        {
            builder.Append("/");
            builder.Append(SubUid);
        }

        if( Rql.Count > 0 && QueryParameters.Count == 0 )
        {
            var join = string.Join( "&", Rql.Select(r => $"rql={r}") );
            builder.Append("?");
            builder.Append( join );
        }

        if( QueryParameters.Count > 0 )
        {

            string MakeValue( object value )
            {
                switch (value)
                {
                    case string:
                        return $"{value}";
                    case short:
                    case int:
                    case long:
                    case double:
                    case decimal:
                    case bool:
                        return $"{value}";
                    case DateTime dt:
                        return $"{dt:o}";
                    default:
                        return $"'{value}'";
                }
            }

            builder.Append("?");

            var list  = QueryParameters.Select(p => $"{p.Key}={MakeValue(p.Value)}");
            var query = string.Join("&", list);

            builder.Append(query);

        }


        var path = builder.ToString();


        return path;


    }


}