using System.Reflection;
using Autofac;
using AutoMapper;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Aws;
using Fabrica.Fake.Persistence;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.Patch;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Module = Autofac.Module;

namespace Fabrica.Fake.Appliance
{

    public class TheModule: Module, IAwsCredentialModule
    {


        public string Profile { get; set; } = "";
        public string RegionName { get; set; } = "";
        public string AccessKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
        public bool RunningOnEC2 { get; set; } = true;


        public string TokenSigningKey { get; set; }

        public int PersonCount { get; set; } = 1000;
        public int CompanyCount { get; set; } = 1000;


        private static InMemoryDatabaseRoot _root = new();

        protected override void Load(ContainerBuilder builder)
        {

            builder.AddCorrelation();

            builder.UseAws(this);

            builder.UseRules();

            builder.RegisterAutoMapper();

            builder.UseModelMeta()
                .AddModelMetaSource(Assembly.GetExecutingAssembly());

            builder.UseMediator(Assembly.GetExecutingAssembly());


            if(!string.IsNullOrWhiteSpace(TokenSigningKey) )
                builder.AddProxyTokenEncoder(TokenSigningKey);


            builder.RegisterType<InMemoryUnitOfWork>()
                .As<IUnitOfWork>()
                .InstancePerLifetimeScope();


            builder.Register(c =>
                {

                    var corr    = c.Resolve<ICorrelation>();
                    var rules   = c.Resolve<IRuleSet>();
                    var factory = c.ResolveOptional<ILoggerFactory>();


                    var ob = new DbContextOptionsBuilder();
                    ob.UseInMemoryDatabase("Faker", _root);

                    var ctx = new FakeOriginDbContext(corr, rules, ob.Options, factory);

                    return ctx;

                })
                .AsSelf()
                .InstancePerLifetimeScope();


            builder.Register(c =>
                {

                    var corr    = c.Resolve<ICorrelation>();
                    var factory = c.ResolveOptional<ILoggerFactory>();

                    var ob = new DbContextOptionsBuilder();
                    ob.UseInMemoryDatabase("Faker", _root);

                    var ctx = new FakeReplicaDbContext(corr, ob.Options, factory);

                    return ctx;

                })
                .AsSelf()
                .InstancePerLifetimeScope();


            builder.Register(c =>
                {
                    var corr = c.Resolve<ICorrelation>();
                    var comp = new MediatorRequestFactory(corr);
                    return comp;
                })
                .As<IMediatorRequestFactory>()
                .SingleInstance();

            builder.Register(c =>
                {

                    var corr = c.Resolve<ICorrelation>();
                    var meta = c.Resolve<IModelMetaService>();
                    var mediator = c.Resolve<IMessageMediator>();
                    var factory = c.Resolve<IMediatorRequestFactory>();

                    var comp = new PatchResolver(corr, meta, mediator, factory);
                    return comp;



                })
                .AsSelf()
                .As<IPatchResolver>()
                .InstancePerLifetimeScope();




        }


    }

}
