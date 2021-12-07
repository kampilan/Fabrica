using Autofac;
using AutoMapper;
using Fabrica.Rules;
using Fabrica.Utilities.Container;

namespace Fabrica.Persistence.Etl;

public static class AutofacExtensions
{


    public static ContainerBuilder UseEtl( this ContainerBuilder builder )
    {


        builder.Register(c =>
            {

                var corr   = c.Resolve<ICorrelation>();
                var mapper = c.Resolve<IMapper>();
                var rules  = c.Resolve<IRuleSet>();

                var comp = new EtlComponent(corr, mapper, rules);

                return comp;

            })
            .AsSelf()
            .InstancePerDependency();


        return builder;

    }

}