
using Autofac;
using Fabrica.One.Repository;

namespace Fabrica.One.Configuration
{

    public static class AutofacExtensions
    {

        public static ContainerBuilder UseFabricaOne(this ContainerBuilder builder, string oneRoot)
        {

            var module = new OneMissionModule
            {
                OneRoot               = oneRoot,
                UseExternalPlanSource = false
            };

            builder.RegisterModule(module);

            return builder;

        }


        public static ContainerBuilder AddFileStatusRepository(this ContainerBuilder builder, string oneRoot)
        {

            builder.Register(c =>
                {

                    var comp = new FileStatusRepository
                    {
                        OneRoot = oneRoot
                    };

                    return comp;

                })
                .As<IStatusRepository>()
                .SingleInstance();


            return builder;

        }


    }

}
