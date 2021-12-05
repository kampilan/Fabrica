using Autofac;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Aws;
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
        
        protected override void Load(ContainerBuilder builder)
        {

            builder.AddCorrelation();

            builder.UseAws(this);

            if(!string.IsNullOrWhiteSpace(TokenSigningKey) )
                builder.AddProxyTokenEncoder(TokenSigningKey);

        }


    }

}
