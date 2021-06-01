using System;
using System.Collections.Generic;
using Autofac;
using Fabrica.Api.Support.Identity.Key;
using Fabrica.Api.Support.Identity.Proxy;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Utilities.Container;
using Microsoft.ReverseProxy.Service;

namespace Fabrica.Proxy.Appliance
{


    public class TheModule: Module
    {


        public string TokenSigningKey { get; set; } = "";


        public string ApiKey { get; set; } = "";
        public string ApiKeyTenant { get; set; } = "";
        public string ApiKeySubject { get; set; } = "";
        public string ApiKeyName { get; set; } = "";
        public string ApiKeyEmail { get; set; } = "";
        public string ApiKeyRoles { get; set; } = "";


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
                        TokenSigningKey = key
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


            if( !string.IsNullOrWhiteSpace(ApiKey) )
            {

                builder.Register(_ =>
                    {

                        var comp = new ApiKeyService
                        {
                            ApiKey  = ApiKey,
                            Tenant  = ApiKeyTenant,
                            Subject = ApiKeySubject,
                            Name    = ApiKeyName,
                            Email   = ApiKeyEmail,
                            Roles   = new List<string>(ApiKeyRoles.Split(',', StringSplitOptions.RemoveEmptyEntries))
                        };

                        return comp;

                    })
                    .AsSelf()
                    .SingleInstance();

            }



        }


    }


}
