using Autofac;
using Fabrica.Persistence.Connection;
using Fabrica.Persistence.Ef.Mediator.Handlers;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Rql;
using Fabrica.Rql.Parser;
using Fabrica.Utilities.Container;

// ReSharper disable UnusedMember.Global
namespace Fabrica.Persistence;

public static class AutofacExtensions
{


    public static ContainerBuilder UsePersistence(this ContainerBuilder builder )
    {


        // ************************************************
        builder.Register(c =>
            {

                var correlation = c.Resolve<ICorrelation>();
                var resolver    = c.Resolve<IConnectionResolver>();

                var comp = new UnitOfWork.UnitOfWork(correlation, resolver);
                return comp;

            })
            .As<IUnitOfWork>()
            .AsSelf()
            .InstancePerLifetimeScope();



        // ************************************************
        builder.Register(c =>
            {

                var correlation = c.Resolve<ICorrelation>();

                var comp = new RqlParserComponentImpl(correlation);
                return comp;

            })
            .As<IRqlParserComponent>();


        // ************************************************
        return builder;

    }

    public static ContainerBuilder AddAuditJournalHandler(this ContainerBuilder builder)
    {

        builder.RegisterType<AuditJournalQueryHandler>()
            .AsImplementedInterfaces()
            .InstancePerDependency();
        
        
        return builder;
    }

}