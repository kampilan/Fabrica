using System.Reflection;
using Autofac;
using Fabrica.Persistence.Connection;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Rql;
using Fabrica.Rql.Parser;
using Fabrica.Utilities.Container;

// ReSharper disable UnusedMember.Global
namespace Fabrica.Persistence
{

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


/*
            // ************************************************
            builder.Register(c =>
                {

                    var meta       = c.Resolve<IModelMetaService>();
                    var repository = c.Resolve<IMasterRepository>();
                    var mapper     = c.Resolve<IMappingComponent>();

                    var comp = new PatchResolverComponent( meta, repository, mapper);

                    return comp;

                })
                .AsSelf()
                .As<IPatchResolverComponent>()
                .InstancePerLifetimeScope();
*/

            // ************************************************
            return builder;

        }




    }


}

