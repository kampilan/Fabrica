using Autofac;
using Fabrica.Utilities.Container;

namespace Fabrica.Monitor.Appliance
{
    
    public class TheModule: Module
    {

        protected override void Load(ContainerBuilder builder)
        {

            builder.AddCorrelation();

        }

    }

}
