using Autofac;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Aws;
using Fabrica.Fake.Services;
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

            if(!string.IsNullOrWhiteSpace(TokenSigningKey) )
                builder.AddProxyTokenEncoder(TokenSigningKey);

            builder.Register(c =>
                {

                    var comp = new FakeDataComponent();

                    return comp;

                })
                .AsSelf()
                .As<IStartable>()
                .SingleInstance();

        }


    }

}
