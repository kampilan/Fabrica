﻿using System;
using System.Net.Http;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Repository;

public static class AutofacExtensions
{


    public static ContainerBuilder UseRepositoryClient(this ContainerBuilder builder)
    {


        builder.Register(c =>
            {

                var factory = c.Resolve<IHttpClientFactory>();

                var comp = new ObjectRepository(factory, Http.ServiceEndpoints.Repository);

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
        sc.AddHttpClient( Http.ServiceEndpoints.Repository, c => c.BaseAddress = uri );

        builder.Populate(sc);


        builder.Register(c =>
            {

                var factory = c.Resolve<IHttpClientFactory>();

                var comp = new ObjectRepository( factory );

                return comp;

            })
            .As<IObjectRepository>()
            .InstancePerDependency();


        return builder;


    }



}