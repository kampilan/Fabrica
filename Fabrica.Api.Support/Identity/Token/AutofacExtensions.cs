using System;
using Autofac;
using JetBrains.Annotations;

namespace Fabrica.Api.Support.Identity.Token
{

    
    public static  class AutofacExtensions
    {


        public static ContainerBuilder AddProxyTokenEncoder( [NotNull] this ContainerBuilder builder,  [NotNull] string tokenSigningKey )
        {

            builder.Register(c =>
                {

                    byte[] key = null;
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


}
