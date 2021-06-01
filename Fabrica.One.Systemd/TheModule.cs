using Autofac;
using Fabrica.Aws;
using Fabrica.One.Configuration;

namespace Fabrica.One
{


    public class TheModule: OneModule, IAwsCredentialModule
    {


        public string Profile { get; set; }
        public string RegionName { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }


        protected override void Load(ContainerBuilder builder)
        {

            builder.UseAws(this);
            
            base.Load(builder);

        }


    }


}
