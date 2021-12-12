using Autofac;
using AutoMapper;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Aws;
using Fabrica.Fake.Services;
using Fabrica.Rules;
using Fabrica.Utilities.Container;

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


        protected override void Load(ContainerBuilder builder)
        {

            builder.AddCorrelation();

            builder.UseAws(this);

            builder.UseRules();

            builder.RegisterAutoMapper();

            if(!string.IsNullOrWhiteSpace(TokenSigningKey) )
                builder.AddProxyTokenEncoder(TokenSigningKey);

            builder.Register(c =>
                {

                    var corr = c.Resolve<ICorrelation>();
                    var mapper = c.Resolve<IMapper>();

                    var comp = new FakeDataComponent( corr, mapper )
                    {
                        PersonCount = PersonCount,
                        CompanyCount = CompanyCount
                    };

                    return comp;

                })
                .AsSelf()
                .As<IStartable>()
                .SingleInstance();

        }


    }

}
