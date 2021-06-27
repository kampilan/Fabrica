using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Aws;
using Fabrica.Identity.Contexts;
using Fabrica.Identity.Services;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fabrica.Identity.Appliance
{


    public class TheModule: Module, IAwsCredentialModule
    {


        public string Profile { get; set; } = "";
        public string RegionName { get; set; } = "";
        public string AccessKey { get; set; } = "";
        public string SecretKey { get; set; } = "";
        public bool RunningOnEC2 { get; set; } = true;


        public string OriginDbConnectionStr { get; set; } = "";



        protected override void Load(ContainerBuilder builder)
        {

            builder.AddCorrelation();

            builder.UseAws(this);


            var services = new ServiceCollection();

            LoadServices( services );

            builder.Populate( services );


        }


        private void LoadServices( IServiceCollection services )
        {



        }



    }



}
