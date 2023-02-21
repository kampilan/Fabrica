
// ReSharper disable UnusedMember.Global

using Autofac;
using Fabrica.Identity;

namespace Fabrica.Api.Support.Identity.Token;

public static  class AutofacExtensions
{


    public static ContainerBuilder AddProxyTokenEncoder( this ContainerBuilder builder,  string tokenSigningKey )
    {

        builder.Register(c =>
            {

                byte[] key = null!;
                if (!string.IsNullOrWhiteSpace(tokenSigningKey))
                    key = Convert.FromBase64String(tokenSigningKey);

                var comp = new ProxyTokenJwtEncoder
                {
                    TokenSigningKey = key
                };

                return comp;

            })
            .As<IProxyTokenEncoder>()
            .SingleInstance();


        return builder;

    }

    public static ContainerBuilder AddProxyAccessTokenSource(this ContainerBuilder builder, IClaimSet claims )
    {

        builder.Register(c =>
            {

                var encoder = c.Resolve<IProxyTokenEncoder>();
                var comp = new ProxyAccessTokenSource(encoder, claims);
                
                return comp;

            })
            .AsSelf()
            .As<IAccessTokenSource>()
            .SingleInstance();


        return builder;

    }





}