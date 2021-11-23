using Autofac;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Utilities.Container;

namespace Fabrica.Fake.Appliance
{

    public class TheModule: Module
    {

        public string TokenSigningKey { get; set; }
        
        protected override void Load(ContainerBuilder builder)
        {

            builder.AddCorrelation();

            if(!string.IsNullOrWhiteSpace(TokenSigningKey) )
                builder.AddProxyTokenEncoder(TokenSigningKey);

        }

    }

}
