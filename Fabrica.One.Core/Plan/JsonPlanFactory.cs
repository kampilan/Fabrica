using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fabrica.Exceptions;
using Fabrica.Watch;
using Fabrica.Watch.Sink;
using Json.More;
using Json.Schema;
using Json.Schema.Generation;
using Json.Schema.Generation.Intents;

namespace Fabrica.One.Plan
{

    public class JsonPlanFactory: IPlanFactory
    {

        private class JsonObjectRefinder : ISchemaRefiner
        {
            public bool ShouldRun(SchemaGeneratorContext context)
            {
                return context.Type == typeof(JsonObject);
            }

            public void Run(SchemaGeneratorContext context)
            {
                var intent = context.Intents.FirstOrDefault();
                if( intent is TypeIntent ti )
                    ti.Type = SchemaValueType.Object;
            }
        }


        static JsonPlanFactory()
        {

            var config = new SchemaGeneratorConfiguration();
            config.Refiners.Add(new JsonObjectRefinder());

            var builder = new JsonSchemaBuilder();
            builder.FromType<PlanImpl>(config);

            PlanSchema = builder.Build();
            SchemaJson = PlanSchema.ToJsonDocument(new JsonSerializerOptions{WriteIndented = true}).RootElement.GetRawText();

        }

        private static JsonSchema PlanSchema { get; }
        private static string SchemaJson { get; }

        public JsonPlanFactory(string repositoryRoot, string installationRoot )
        {

            RepositoryRoot   = repositoryRoot;
            InstallationRoot = installationRoot;

        }


        private string RepositoryRoot { get; }
        private string InstallationRoot { get; }

        public IPlan Create( Stream source )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to read json from source stream");
                var missionPlanJson="";
                using (source)
                using (var reader = new StreamReader(source))
                {
                    missionPlanJson = reader.ReadToEnd();
                }

                logger.LogJson("Mission Plan", missionPlanJson);




                // *****************************************************************
                JsonElement root;
                try
                {

                    logger.Debug("Attempting to parse JSON");
                    var document = JsonDocument.Parse(missionPlanJson);

                    logger.Debug("Attempting to get RootElement");
                    root = document.RootElement;

                }
                catch( JsonException cause )
                {

                    var je = logger.CreateEvent(Level.Error, "Malformed Mission Plan JSON", PayloadType.Text, missionPlanJson);
                    logger.LogEvent(je);

                    var exp = new PredicateException( "Bad JSON encountered during parse.", cause );
                    exp
                        .WithErrorCode("MalformedMissionJson")
                        .WithExplaination(cause.Message);

                    throw exp;

                }
                catch( Exception cause )
                {
                    logger.Error( cause, "Failed to parse Mission Plan JSON");
                    throw;
                }



                // *****************************************************************
                logger.Debug("Attempting to validate JSON againset Schema");
                var results = PlanSchema.Validate(root, new ValidationOptions{OutputFormat = OutputFormat.Detailed});

                logger.Inspect(nameof(results.IsValid), results.IsValid);

                if( !results.IsValid )
                {

                    var se = logger.CreateEvent(Level.Error, "Mission Plan JSON Schema", PayloadType.Json, SchemaJson);
                    logger.LogEvent(se);

                    var je = logger.CreateEvent(Level.Error, "Invalid Mission Plan JSON", PayloadType.Json, missionPlanJson);
                    logger.LogEvent(je);

                    var details = results.NestedResults.Select(e => new EventDetail
                        {
                            Category    = EventDetail.EventCategory.Violation,
                            Group       = "Mission Plan JSON",
                            RuleName    = "JSON Schema",
                            Source      = e.SchemaLocation.ToString(),
                            Explanation = $"{e.InstanceLocation} - {e.Message}"
                    })
                        .ToList();

                    var exp = new PredicateException("Invalid Mission Plan JSON encountered");

                    if (details.Count == 0)
                        exp.WithErrorCode("InvalidMissionJson").WithExplaination($"{results.InstanceLocation} - {results.Message}");
                    else
                        exp.WithErrorCode("InvalidMissionJson").WithDetails(details);

                    throw exp;

                }



                // *****************************************************************
                logger.Debug("Attempting to deserialize Root element to Plan");
                var plan = root.Deserialize<PlanImpl>();

                if (plan is null)
                    throw new Exception("Null Plan produced by Root JsonElement");



                // *****************************************************************
                logger.Debug("Attempting to set Repository and Installation roots");
                plan.RepositoryRoot = RepositoryRoot;
                plan.InstallationRoot = InstallationRoot;



                // *****************************************************************
                logger.Debug("Attempting to generate mission token signing key");
                var rng = RandomNumberGenerator.Create();
                var key = new byte[64];
                rng.GetNonZeroBytes(key);

                var tokenSigningKey = Convert.ToBase64String(key);



                // *****************************************************************
                logger.Debug("Attempting to calculate and populate deployment unit locations");
                var rv = Path.Combine(plan.RepositoryRoot, plan.RepositoryVersion);
                foreach (var unit in plan.Deployments)
                {

                    unit.RepositoryLocation   = $"{rv}{Path.DirectorySeparatorChar}{unit.Name}-{unit.Build}.zip";
                    unit.InstallationLocation = $"{InstallationRoot}{Path.DirectorySeparatorChar}{unit.Alias}{Path.DirectorySeparatorChar}{unit.Id}";

                    unit.EnvironmentConfigLocation = $"{unit.InstallationLocation}{Path.DirectorySeparatorChar}environment.json";
                    unit.MissionConfigLocation     = $"{unit.InstallationLocation}{Path.DirectorySeparatorChar}mission.json";

                    unit.ExecutionCommand   = "dotnet";
                    unit.ExecutionArguments = $"{unit.InstallationLocation}{Path.DirectorySeparatorChar}{unit.Assembly}.dll";

                    unit.MissionConfiguration["MissionName"]     = plan.Name;
                    unit.MissionConfiguration["TokenSigningKey"] = tokenSigningKey;
                    unit.MissionConfiguration["ApplianceId"]     = unit.Id;
                    unit.MissionConfiguration["ApplianceName"]   = unit.Name;
                    unit.MissionConfiguration["ApplianceBuild"]  = unit.Build;
                    unit.MissionConfiguration["Environment"]     = unit.Environment;

                }



                // *****************************************************************
                logger.LogObject(nameof(plan), plan);
                return plan;


            }
            finally
            {
                logger.LeaveMethod();
            }

        }

    }

}
