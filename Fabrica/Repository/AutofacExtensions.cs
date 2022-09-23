using System;
using System.Net.Http;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Repository;

public static class AutofacExtensions
{

    private static string DefaultRepositoryClientName => "Fabrica.Repository.Default";

    public static ContainerBuilder UseRepositoryClient(this ContainerBuilder builder, string repositoryAddress, string repositoryClientName="" )
    {

        var client  = string.IsNullOrEmpty( repositoryClientName ) ? DefaultRepositoryClientName : repositoryClientName;
        var address = repositoryAddress.EndsWith("/") ? repositoryAddress : $"{repositoryAddress}/";
        var uri     = new Uri(address);

        var sc = new ServiceCollection();
        sc.AddHttpClient( client, c => c.BaseAddress = uri );

        builder.Populate(sc);

        builder.Register(c =>
            {

                var factory = c.Resolve<IHttpClientFactory>();

                var comp = new ObjectRepository( factory, client );

                return comp;

            })
            .As<IObjectRepository>()
            .InstancePerDependency();


        return builder;


    }


}