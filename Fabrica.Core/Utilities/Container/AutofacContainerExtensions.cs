using Autofac;

namespace Fabrica.Utilities.Container;

public static class AutofacContainerExtensions
{


    public static async Task<IContainer> BuildAndStart(this ContainerBuilder builder )
    {

        var container = builder.Build();
        var requires = container.Resolve<IEnumerable<IRequiresStart>>();
        foreach (var rs in requires)
            await rs.Start();

        return container;

    }


    public static async Task StartComponents(this IContainer container)
    {

        var requires = container.Resolve<IEnumerable<IRequiresStart>>();
        foreach (var rs in requires)
            await rs.Start();

    }


}