
// ReSharper disable UnusedMember.Global

using Autofac;
using Fabrica.Identity;

namespace Fabrica.Api.Support.Identity.Token;

public static  class AutofacExtensions
{

    public static ContainerBuilder AddGatewayAccessTokenSource(this ContainerBuilder builder, IClaimSet claims )
    {

        builder.Register(c =>
            {

                var encoder = c.Resolve<IGatewayTokenEncoder>();
                var comp = new GatewayAccessTokenSource(encoder, claims);
                
                return comp;

            })
            .AsSelf()
            .As<IAccessTokenSource>()
            .SingleInstance();


        return builder;

    }





}