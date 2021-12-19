using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Fabrica.Models.Support;
using Fabrica.Utilities.Container;

namespace Fabrica.Models
{

    
    public static class AutofacExtensions
    {


        public static ContainerBuilder UseModelMeta( this ContainerBuilder builder )
        {

            builder.Register(c =>
                {

                    var sources = c.Resolve<IEnumerable<ModelMetaSource>>();

                    var comp = new ModelMetaService( sources );

                    return comp;

                })
                .As<IModelMetaService>()
                .As<IRequiresStart>()
                .SingleInstance()
                .AutoActivate();

            return builder;

        }


        public static ContainerBuilder AddModelMetaSource(this ContainerBuilder builder, params Assembly[] assemblies)
        {

            builder.Register(c =>
                {

                    var comp = new ModelMetaSource();

                    comp.AddTypes(assemblies);

                    return comp;

                })
                .AsSelf()
                .SingleInstance()
                .AutoActivate();

            return builder;

        }


    }

}
