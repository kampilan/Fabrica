using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Api.Support.One;
using Fabrica.Fake.Persistence;

namespace Fabrica.Fake.Appliance;

public class FakeInitService: InitService
{

    public FakeInitService( ILifetimeScope scope ): base( scope )
    {
    }


    public override async Task StartAsync(CancellationToken cancellationToken)
    {

        await base.StartAsync(cancellationToken);

        using( var scope = RootScope.BeginLifetimeScope() )
        {

            var ctx = scope.Resolve<FakeReplicaDbContext>();

            await ctx.Database.EnsureCreatedAsync(cancellationToken);

        }

    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {

        await base.StopAsync(cancellationToken);
        
    }

}