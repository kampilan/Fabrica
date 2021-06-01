using System.Threading;
using System.Threading.Tasks;
using Microsoft.ReverseProxy.Abstractions;
using Microsoft.ReverseProxy.Abstractions.Config;
using Microsoft.ReverseProxy.Service;

namespace Fabrica.Proxy.Appliance
{

    public class AuthHeaderProxyConfigFilter: IProxyConfigFilter
    {


        public ValueTask<Cluster> ConfigureClusterAsync(Cluster cluster, CancellationToken cancel)
        {
            return ValueTask.FromResult(cluster);
        }

        public ValueTask<ProxyRoute> ConfigureRouteAsync(ProxyRoute route, CancellationToken cancel)
        {

            route.WithTransformRequestHeader("Cookies", "", false);
            route.WithTransformRequestHeader("Authorization", "", false);

            return ValueTask.FromResult(route);
        }


    }

}
