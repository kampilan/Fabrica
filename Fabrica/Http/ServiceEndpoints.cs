using System.Net.Http;

namespace Fabrica.Http;

public static  class ServiceEndpoints
{

    public static string Repository => "Fabrica.Service.Repository";
    public static string Work => "Fabrica.Service.Work";


    public static HttpClient GetRepositoryClient(this IHttpClientFactory factory )
    {
        return factory.CreateClient(Repository);
    }

    public static HttpClient GetWorkClient(this IHttpClientFactory factory)
    {
        return factory.CreateClient(Repository);
    }


}