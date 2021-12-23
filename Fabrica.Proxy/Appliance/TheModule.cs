using System;
using Autofac;
using Fabrica.Api.Support.Identity.Proxy;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Utilities.Container;
using Microsoft.ReverseProxy.Service;

namespace Fabrica.Proxy.Appliance
{


    public class TheModule: Module
    {


        public string TokenSigningKey { get; set; } = "";
        public int TokenTimeToLiveSecs { get; set; } = 30;

        protected override void Load(ContainerBuilder builder)
        {


            builder.AddCorrelation();


            builder.RegisterType<AuthHeaderProxyConfigFilter>()
                .As<IProxyConfigFilter>()
                .InstancePerDependency();

            builder.Register(_ =>
                {

                    byte[] key = null;
                    if (!string.IsNullOrWhiteSpace(TokenSigningKey))
                        key = Convert.FromBase64String(TokenSigningKey);

                    var comp = new ProxyTokenJwtEncoder
                    {
                        TokenSigningKey = key,
                        TokenTimeToLive = TimeSpan.FromSeconds(TokenTimeToLiveSecs)
                    };

                    return comp;

                })
                .As<IProxyTokenEncoder>()
                .SingleInstance();


            builder.Register(_ =>
                {
                    var comp = new ClaimTokenPayloadBuilder();
                    return comp;
                })
                .As<IProxyTokenPayloadBuilder>()
                .SingleInstance()
                .AutoActivate();


        }


    }


}
