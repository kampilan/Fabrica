using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Identity;
using Fabrica.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Repository;

public static class AutofacExtensions
{


    public static ContainerBuilder UseRepositoryClient(this ContainerBuilder builder)
    {


        builder.Register(c =>
            {

                var factory = c.Resolve<IHttpClientFactory>();
                var source  = c.Resolve<IAccessTokenSource>();

                var comp = new ObjectRepository(factory, source, ServiceEndpoints.Repository);

                return comp;

            })
            .As<IObjectRepository>()
            .InstancePerDependency();


        return builder;


    }


    public static ContainerBuilder UseRepositoryRemoteClient( this ContainerBuilder builder, string url )
    {


        var address = url.EndsWith("/") ? url : $"{url}/";
        var uri = new Uri(address);

        var sc = new ServiceCollection();
        sc.AddHttpClient( ServiceEndpoints.Repository, c => c.BaseAddress = uri );

        builder.Populate(sc);


        builder.Register(c =>
            {

                var factory = c.Resolve<IHttpClientFactory>();

                var comp = new ObjectRepository( factory, ServiceEndpoints.Repository );

                return comp;

            })
            .As<IObjectRepository>()
            .InstancePerDependency();


        return builder;


    }



}