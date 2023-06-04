
// ReSharper disable UnusedMember.Global

using Autofac;
using Fabrica.Identity;

namespace Fabrica.Api.Support.Identity.Token;

public static  class AutofacExtensions
{


    public static ContainerBuilder AddGatewayTokenEncoder( this ContainerBuilder builder,  string tokenSigningKey )
    {

        builder.Register(c =>
            {

                byte[] key = null!;
                if (!string.IsNullOrWhiteSpace(tokenSigningKey))
                    key = Convert.FromBase64String(tokenSigningKey);

                var comp = new GatewayTokenJwtEncoder
                {
                    TokenSigningKey = key
                };

                return comp;

            })
            .As<IGatewayTokenEncoder>()
            .SingleInstance();


        return builder;

    }

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