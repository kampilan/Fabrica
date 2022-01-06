
using Autofac;

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


    }

}
