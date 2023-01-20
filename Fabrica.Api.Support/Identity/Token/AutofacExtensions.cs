
// ReSharper disable UnusedMember.Global

using Autofac;

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


}