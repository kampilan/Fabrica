using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Fake.Persistence;
using Microsoft.Extensions.Hosting;

namespace Fabrica.Fake.Appliance;

public class InitService: IHostedService
{




    public InitService(ILifetimeScope scope)
    {
        RootScope = scope;
    }

    private ILifetimeScope RootScope { get; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        using( var scope = RootScope.BeginLifetimeScope() )
        {

            var ctx = scope.Resolve<FakeReplicaDbContext>();

            await ctx.Database.EnsureCreatedAsync(cancellationToken);

        }

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

}