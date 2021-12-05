using Autofac;
using Fabrica.One.Persistence.Work;
using Fabrica.Utilities.Container;
using MongoDB.Driver;

namespace Fabrica.One.Persistence;

public static class AutofacExtensions
{


    public static ContainerBuilder UseOnePersitence( this ContainerBuilder builder, string serverUri, string databaseName = "" )
    {

        builder.Register(c =>
            {

                var comp = new MongoClient(serverUri);

                return comp;

            })
            .AsSelf()
            .InstancePerLifetimeScope();


        builder.Register(c =>
            {

                if( string.IsNullOrWhiteSpace( databaseName ) )
                    databaseName = "fabrica_one";

                var corr     = c.Resolve<ICorrelation>();
                var client   = c.Resolve<MongoClient>();
                var database = client.GetDatabase(databaseName);

                var comp = new WorkRepository( corr, database) ;

                return comp;

            })
            .AsSelf()
            .InstancePerDependency();


        return builder;


    }    


}