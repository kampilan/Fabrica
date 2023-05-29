using Autofac;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;

namespace Fabrica.Persistence.Patch;

public static class AutofacExtensions
{

    public static ContainerBuilder UsePatchResolver(this ContainerBuilder builder)
    {


        builder.Register(c =>
            {

                var meta = c.Resolve<IModelMetaService>();
                var factory = c.Resolve<IMediatorRequestFactory>();

                var comp = new PatchResolver(meta, factory);
                return comp;


            })
            .AsSelf()
            .As<IPatchResolver>()
            .SingleInstance();


        builder.Register(c =>
            {
                var comp = new MediatorRequestFactory();
                return comp;

            })
            .As<IMediatorRequestFactory>()
            .SingleInstance();

        return builder;


    }

}