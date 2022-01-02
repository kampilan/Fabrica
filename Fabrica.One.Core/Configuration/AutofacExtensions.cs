
using Autofac;
using Fabrica.One.Plan;

namespace Fabrica.One.Configuration
{

    public static class AutofacExtensions
    {

        public static ContainerBuilder UseFabricaOne(this ContainerBuilder builder, string repsitoryRoot, string installationRoot, bool useExternalPlan = true)
        {

            var module = new OneModule
            {
                RepositoryRoot     = repsitoryRoot,
                InstallationRoot   = installationRoot,
                ExternalPlanSource = useExternalPlan
            };

            builder.RegisterModule(module);

            return builder;

        }

        public static ContainerBuilder AddMemoryPlanSource(this ContainerBuilder builder)
        {


            builder.Register(c =>
                {

                    var comp = new MemoryPlanSource();
                    return comp;

                })
                .AsSelf()
                .As<IPlanSource>()
                .SingleInstance();


            return builder;

        }


    }

}
