using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Fabrica.Exceptions;
using Fabrica.Utilities.Types;
using Fabrica.Watch;
using Fabrica.Watch.Sink;
using JetBrains.Annotations;
using Json.More;
using Json.Schema;
using Json.Schema.Generation;
using Json.Schema.Generation.Intents;

namespace Fabrica.One.Plan
{

    public class JsonPlanFactory: IPlanFactory
    {

        private class JsonObjectRefiner : ISchemaRefiner
        {
            public bool ShouldRun(SchemaGenerationContextBase context)
            {
                return context.Type == typeof(JsonObject);
            }

            public void Run(SchemaGenerationContextBase context)
            {
                var intent = context.Intents.FirstOrDefault();
                if( intent is TypeIntent ti )
                    ti.Type = SchemaValueType.Object;
            }
        }


        static JsonPlanFactory()
        {

            var config = new SchemaGeneratorConfiguration();
            config.Refiners.Add(new JsonObjectRefiner());

            var builder = new JsonSchemaBuilder();
            builder.FromType<PlanImpl>(config);

            PlanSchema = builder.Build();
            SchemaJson = PlanSchema.ToJsonDocument(new JsonSerializerOptions{WriteIndented = true}).RootElement.GetRawText();

        }

        private static JsonSchema PlanSchema { get; }
        private static string SchemaJson { get; }

        public JsonPlanFactory(string repositoryRoot, string installationRoot )
        {

            RepositoryRoot    = repositoryRoot;
            InstallationRoot  = installationRoot;

        }

        private string RepositoryRoot { get; }
        private string InstallationRoot { get; }



        public async Task<IPlan> Create( IPlanSource source, bool produceEmptyPlan=false )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                // *****************************************************************
                logger.Debug("Attempting to check for empty plan");
                logger.Inspect(nameof(produceEmptyPlan), produceEmptyPlan);



                // *****************************************************************
                logger.Debug("Attempting to read json from source stream");
                string missionPlanJson;
                using (var strm = await source.GetSource())
                using (var reader = new StreamReader(strm))
                {
                    missionPlanJson = await reader.ReadToEndAsync();
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

                    if( produceEmptyPlan )
                    {
                        logger.Error( cause, "Returning empty Plan per produceEmptyPlan=true" );
                        return GetEmptyPlan();
                    }


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
                logger.Debug("Attempting to validate JSON against Schema");
                var results = PlanSchema.Validate(root, new ValidationOptions{OutputFormat = OutputFormat.Detailed});

                logger.Inspect(nameof(results.IsValid), results.IsValid);

                if( !results.IsValid )
                {

                    var se = logger.CreateEvent(Level.Error, "Mission Plan JSON Schema", PayloadType.Json, SchemaJson);
                    logger.LogEvent(se);

                    var je = logger.CreateEvent(Level.Error, "Invalid Mission Plan JSON", PayloadType.Json, missionPlanJson);
                    logger.LogEvent(je);


                    var exp = new PredicateException("Invalid Mission Plan JSON encountered");

                    var details = results.NestedResults.Select(e => new EventDetail
                        {
                            Category = EventDetail.EventCategory.Violation,
                            Group = "Mission Plan JSON",
                            RuleName = "JSON Schema",
                            Source = e.SchemaLocation.ToString(),
                            Explanation = $"{e.InstanceLocation} - {e.Message}"
                        })
                        .ToList();


                    if (details.Count == 0)
                        exp.WithErrorCode("InvalidMissionJson").WithExplaination($"{results.InstanceLocation} - {results.Message}");
                    else
                        exp.WithErrorCode("InvalidMissionJson").WithDetails(details);

                    if( produceEmptyPlan )
                    {
                        logger.Error( exp, "Returning empty Plan per produceEmptyPlan=true" );
                        return GetEmptyPlan();
                    }


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
                logger.Debug("Attempting to check for RepositoryVersion");
                var rv = RepositoryRoot;
                if (!string.IsNullOrWhiteSpace(plan.RepositoryVersion))
                {
                    logger.DebugFormat("Using RepositoryVersion: ({0})", plan.RepositoryVersion);
                    rv = Path.Combine(plan.RepositoryRoot, plan.RepositoryVersion);
                }



                // *****************************************************************
                logger.Debug("Attempting to calculate and populate deployment unit locations");
                foreach (var unit in plan.Deployments)
                {


                    logger.Debug("Attempting to check for blank Alias");
                    if (string.IsNullOrWhiteSpace(unit.Alias))
                    {
                        logger.Debug("Encountered blank Alias defaulting to Name");
                        unit.Alias = unit.Name;
                    }


                    unit.RepositoryLocation   = $"{rv}{Path.DirectorySeparatorChar}{unit.Name}-{unit.Build}.zip";
                    unit.InstallationLocation = $"{InstallationRoot}{Path.DirectorySeparatorChar}{unit.Alias}{Path.DirectorySeparatorChar}{unit.Uid}";

                    unit.UnitConfigLocation    = $"{unit.InstallationLocation}{Path.DirectorySeparatorChar}environment.json";
                    unit.MissionConfigLocation = $"{unit.InstallationLocation}{Path.DirectorySeparatorChar}mission.json";

                    unit.ExecutionCommand   = "dotnet";
                    unit.ExecutionArguments = $"{unit.InstallationLocation}{Path.DirectorySeparatorChar}{unit.Assembly}.dll";

                    unit.MissionConfiguration["MissionName"]     = plan.Name;
                    unit.MissionConfiguration["Environment"]     = plan.Environment;
                    unit.MissionConfiguration["ApplianceName"]   = unit.Name;
                    unit.MissionConfiguration["ApplianceBuild"]  = unit.Build;
                    unit.MissionConfiguration["ApplianceId"]     = unit.Uid;
                    unit.MissionConfiguration["ApplianceRoot"]   = unit.InstallationLocation;
                    unit.MissionConfiguration["TokenSigningKey"] = tokenSigningKey;

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

        public Task CreateRepositoryVersion( IPlan plan )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                // *****************************************************************
                logger.Debug("Attempting to create new RepositoryVersion id");
                var repoVersion = $"v-{DateTime.UtcNow.ToTimestampString()}";
                plan.SetRepositoryVersion( repoVersion );

                logger.Inspect(nameof(repoVersion), repoVersion);



                // *****************************************************************
                logger.Debug("Attempting to recalulate RepositoryLocation for each DeploymentUnit");
                foreach( var unit in plan.Deployments )
                    unit.RepositoryLocation = $"{plan.RepositoryRoot}{Path.DirectorySeparatorChar}{repoVersion}{Path.DirectorySeparatorChar}{unit.Name}-{unit.Build}.zip";



                // *****************************************************************
                return Task.CompletedTask;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public async Task Save([NotNull] IPlan plan, [NotNull] IPlanWriter writer )
        {

            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                logger.Inspect("Plan Type", plan.GetType().FullName );
                logger.Inspect("Writer Type", plan.GetType().FullName);



                // *****************************************************************
                logger.Debug("Attempting to check for valid IPlan");
                if( plan is PlanImpl impl )
                {

                    
                    logger.Debug("Attempting to serialize plan to JSON");
                    var json = JsonSerializer.Serialize(impl, new JsonSerializerOptions {WriteIndented = true});
                    logger.LogJson("Mission Plan JSON", json);


                    logger.Debug("Attempting to writer JSON to writer");
                    await writer.Write(json);


                }
                else
                    throw new Exception( $"Invalid Plan type. Expected ({typeof(PlanImpl).FullName}) but found ({plan.GetType().FullName}) instead." );


            }
            finally
            {
                logger.LeaveMethod();
            }

        }


        protected IPlan GetEmptyPlan()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                // *****************************************************************
                logger.Debug("Attempting to build empty plan");
                var empty = new PlanImpl
                {

                    Name = "Empty",

                    RepositoryRoot    = RepositoryRoot,
                    RepositoryVersion = "",

                    InstallationRoot  = InstallationRoot,

                    DeployAppliances        = false,
                    StartAppliances         = false,
                    AllAppliancesMustDeploy = false,

                    WaitForDeploySeconds = 10,
                    WaitForStartSeconds = 10,
                    WaitForStopSeconds = 10

                };

                logger.LogObject(nameof(empty), empty);


                // *****************************************************************
                return empty;


            }
            finally
            {
                logger.LeaveMethod();
            }

        }



    }

}
