using Autofac;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Utilities.Container;
using Fabrica.Watch.Api.Components;

namespace Fabrica.Watch.Api.Appliance
{


    public class TheModule: Module
    {

        public string TokenSigningKey { get; set; } = "";


        protected override void Load(ContainerBuilder builder)
        {

            builder.AddCorrelation();


            builder.AddProxyTokenEncoder(TokenSigningKey);

            builder.Register(c =>
                {

                    var corr = c.Resolve<ICorrelation>();
                    var options = c.Resolve<WatchOptions>();

                    var comp = new WatchFactoryCache(corr, options);

                    return comp;

                })
                .AsSelf()
                .SingleInstance()
                .AutoActivate();

        }

    }


}
