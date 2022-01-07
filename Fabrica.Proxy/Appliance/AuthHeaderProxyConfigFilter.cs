using System.Threading;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Fabrica.Proxy.Appliance
{

    public class AuthHeaderProxyConfigFilter: IProxyConfigFilter
    {


        public ValueTask<ClusterConfig> ConfigureClusterAsync(ClusterConfig cluster, CancellationToken cancel)
        {
            return ValueTask.FromResult(cluster);
        }

        public ValueTask<RouteConfig> ConfigureRouteAsync(RouteConfig route, ClusterConfig cluster, CancellationToken cancel)
        {

            route.WithTransformRequestHeader("Cookies", "", false);
            route.WithTransformRequestHeader("Authorization", "", false);

            return ValueTask.FromResult(route);

        }


    }

}
