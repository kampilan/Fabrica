using System.IO;
using Fabrica.Watch;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// ReSharper disable UnusedMember.Global
namespace Fabrica.One.Plan
{


    public class YamlPlanFactory: IPlanFactory
    {


        public string InstallationRoot { get; set; } = $"{Path.DirectorySeparatorChar}appliances";


        protected virtual void Validate( IPlan plan )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to validate Plan using built-in validation");
                plan.Validate();


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public IPlan Create( Stream source )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Instance metadata not available. Loading from PlanSource");
                string yaml;
                using( source )
                using( var reader = new StreamReader(source) )
                    yaml = reader.ReadToEnd();

                logger.LogYaml( nameof(yaml), yaml );



                // *****************************************************************
                logger.Debug("Attempting to parse yaml into Plan");
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(PascalCaseNamingConvention.Instance)
                    .Build();

                var plan = deserializer.Deserialize<PlanImpl>(yaml);


                plan.InstallationRoot = InstallationRoot;


                // *****************************************************************
                logger.Debug("Attempting to validate the Plan");
                Validate(plan);



                // *****************************************************************
                return plan;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }


}
