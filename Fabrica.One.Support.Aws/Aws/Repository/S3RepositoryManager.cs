using System.Net;
using System.Text.Json;
using Amazon;
using Amazon.AppConfig;
using Amazon.AppConfig.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using Fabrica.Exceptions;
using Fabrica.One.Models;
using Fabrica.One.Repository;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Humanizer;

namespace Fabrica.One.Support.Aws.Repository;

public class S3RepositoryManager: IRepositoryManager, IRequiresStart
{

    public bool RunningOnEc2 { get; set; } = true;


    public string MissionPrefix { get; set; } = "missions/";
    public Func<string, bool> MissionFilter { get; set; } = k=>k.EndsWith("-mission-plan.json");

    public string BuildPrefix { get; set; } = "appliances/";
    public Func<string, bool> BuildFilter { get; set; } = k=>k.EndsWith("-manifest.json");


    public Task Refresh()
    {

        using var logger = this.EnterMethod();

        _currentRepository = null;

        Repositories.Clear();
        Missions.Clear();
        Builds.Clear();

        return Task.CompletedTask;

    }


    private RepositoryModel? _currentRepository;


    private string CurrentProfileName
    {

        get
        {
            if (_currentRepository != null && _currentRepository.Properties.TryGetValue("AwsProfileName", out var profile))
                return profile;

            return "";

        }

    }

    private string CurrentRegionName
    {

        get
        {
            if (_currentRepository != null && _currentRepository.Properties.TryGetValue("Region", out var region))
                return region;

            return "";

        }

    }

    private string CurrentBucketName
    {

        get
        {
            if (_currentRepository != null && _currentRepository.Properties.TryGetValue("Bucket", out var bucket))
                return bucket;

            return "";

        }

    }


    public async Task Start()
    {
            
        using var logger = this.EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to load repositories");
        await LoadRepositories();

    }


    public RepositoryModel CurrentRepository => _currentRepository;


    public async Task SetCurrentRepository( RepositoryModel current )
    {

        using var logger = this.EnterMethod();

        _currentRepository = current;

        if( _currentRepository is null )
        {
            Missions.Clear();
            Builds.Clear();
        }
        else
        {
            await LoadMissions();
            await LoadBuilds();
        }

    }


    public async Task<IEnumerable<RepositoryModel>> GetRepositories(Func<RepositoryModel, bool>? predicate = null)
    {

        using var logger = this.EnterMethod();

        logger.Inspect(nameof(Repositories.Count), Repositories.Count);


        if( Repositories.Count == 0 )
            await LoadRepositories();

        IEnumerable<RepositoryModel> results = Repositories;
        if( predicate is not null )
            results = Repositories.Where(predicate);


        // *****************************************************************
        return results;


    }

    public async Task<IEnumerable<MissionModel>> GetMissions(Func<MissionModel, bool>? predicate = null)
    {

        using var logger = this.EnterMethod();


        if (_currentRepository != null && Missions.Count == 0)
            await LoadMissions();


        IEnumerable<MissionModel> results = Missions;
        if( predicate is not null )
            results = Missions.Where(predicate);

        return results;

    }

    public async Task<IEnumerable<ApplianceModel>> GetAppliances( Func<ApplianceModel, bool>? predicate = null )
    {

        using var logger = this.EnterMethod();

        if( _currentRepository != null && Builds.Count == 0 )
            await LoadBuilds();



        // *****************************************************************
        logger.Debug("Attempting to group Builds into Appliances");
        var groupings = Builds.GroupBy(b => b.Name);
        var appliances = groupings.Select(g => new ApplianceModel {Name = g.Key, LatestBuildDate = g.Max(b => b.BuildDate), TotalBuilds = g.Count()}).ToList();

        logger.Inspect(nameof(appliances.Count), appliances.Count);



        // *****************************************************************
        logger.Debug("Attempting to apply predicate");
        IEnumerable<ApplianceModel> results = appliances;
        if( predicate is not null )
            results = appliances.Where(predicate);



        // *****************************************************************
        return results;


    }

    public async Task<IEnumerable<BuildModel>> GetBuilds(Func<BuildModel, bool>? predicate = null)
    {


        using var logger = this.EnterMethod();

        if (_currentRepository != null && Builds.Count == 0)
            await LoadBuilds();


        IEnumerable<BuildModel> results = Builds;
        if( predicate is not null )
            results = Builds.Where(predicate);


        return results;


    }



    public async Task<IEnumerable<DeploymentExplorerModel>> GetMissionBuildUsage()
    {

        using var logger = this.EnterMethod();


        if (_currentRepository != null && Missions.Count == 0)
            await LoadMissions();


        if (_currentRepository != null && Builds.Count == 0)
            await LoadBuilds();


        var explorers = new List<DeploymentExplorerModel>();

        foreach( var d in Missions.SelectMany(m => m.Deployments) )
        {

            var build = Builds.FirstOrDefault(b => b.Name == d.Name && b.BuildNum == d.Build);
            if (build is null)
                continue;

            var exp = new DeploymentExplorerModel
            {
                Mission         = d.Parent.Fqmn,
                BuildName       = build.Name,
                BuildNum        = build.BuildNum,
                BuildDate       = build.BuildDate,
                IsAbsoluteBuild = build.IsAbsoluteBuild
            };

            explorers.Add(exp);

        }

        return explorers;

    }


    public async Task<MissionModel> CreateMission( string name, string environment, string? customName="")
    {

        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        if (string.IsNullOrWhiteSpace(environment)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(environment));

        using var logger = this.EnterMethod();


        logger.Inspect(nameof(name), name);
        logger.Inspect(nameof(environment), environment);
        logger.Inspect(nameof(customName), customName);



        if( string.IsNullOrWhiteSpace(customName) )
        {


            logger.Debug("Attempting to validate candidate mission name");
            var nameOk = name.All(char.IsLetterOrDigit);
            if (!nameOk)
                throw new ArgumentException("Must be [a-z][A-Z][0-9}", name);



            logger.Debug("Attempting to validate candidate mission environment");
            var envOk = environment.All(char.IsLetterOrDigit);
            if (!envOk)
                throw new ArgumentException("Must be [a-z][A-Z][0-9}", environment);


            var exists = Missions.Exists(m => m.Name == name && m.Environment == environment);
            if (exists)
                throw new Exception($"A mission with the name ({name}) and environment ({environment}) already exists.");

            customName = $"{name.ToLowerInvariant()}-{environment.ToLowerInvariant()}";

        }



        // *****************************************************************
        logger.Debug("Attempting to populate new mission with reasonable defaults");
        var mission = new MissionModel
        {
            Name                    = name,
            Environment             = environment,
            DeployAppliances        = true,
            StartAppliances         = true,
            StartInParallel         = true,
            AllAppliancesMustDeploy = true,
            WaitForDeploySeconds    = 10,
            WaitForStartSeconds     = 10,
            WaitForStopSeconds      = 10,
            RepositoryLocation      = $"missions/{customName}-mission-plan.json",
            AppConfigApplication    = name,
            AppConfigEnvironment    = environment,
            AppConfigConfigProfile  = $"Mission-{environment}",
            AppConfigStrategy       = "Standard"
        };

        logger.LogObject(nameof(mission), mission);



        // *****************************************************************
        logger.Debug("Attempting to save new mission to repository");
        await Save(mission);


        // *****************************************************************
        return mission;


    }

    public async Task<MissionModel> CopyMission( string name, string environment, MissionModel source, string customName="" )
    {

        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        if (string.IsNullOrWhiteSpace(environment)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(environment));

        using var logger = this.EnterMethod();


        logger.Inspect(nameof(name), name);
        logger.Inspect(nameof(environment), environment);
        logger.Inspect(nameof(customName), customName);



        if (string.IsNullOrWhiteSpace(customName))
        {


            logger.Debug("Attempting to validate candidate mission name");
            var nameOk = name.All(char.IsLetterOrDigit);
            if (!nameOk)
                throw new ArgumentException("Must be [a-z][A-Z][0-9}", name);



            logger.Debug("Attempting to validate candidate mission environment");
            var envOk = environment.All(char.IsLetterOrDigit);
            if (!envOk)
                throw new ArgumentException("Must be [a-z][A-Z][0-9}", environment);


            var exists = Missions.Exists(m => m.Name == name && m.Environment == environment);
            if (exists)
                throw new Exception($"A mission with the name ({name}) and environment ({environment}) already exists.");

            customName = $"{name.ToLowerInvariant()}-{environment.ToLowerInvariant()}";

        }



        // *****************************************************************
        logger.Debug("Attempting to populate new mission with reasonable defaults");
        var mission = new MissionModel
        {
            Name                    = name,
            Environment             = environment,
            DeployAppliances        = source.DeployAppliances,
            StartAppliances         = source.StartAppliances,
            StartInParallel         = source.StartInParallel,
            AllAppliancesMustDeploy = source.AllAppliancesMustDeploy,
            WaitForDeploySeconds    = source.WaitForDeploySeconds,
            WaitForStartSeconds     = source.WaitForStartSeconds,
            WaitForStopSeconds      = source.WaitForStopSeconds,
            RepositoryLocation      = $"missions/{customName}-mission-plan.json",
            AppConfigApplication    = name,
            AppConfigEnvironment    = environment,
            AppConfigConfigProfile  = $"Mission-{environment}",
            AppConfigStrategy       = "Standard"
        };



        // *****************************************************************
        logger.Debug("Attempting to copy each appliance from source to new mission");
        foreach( var app in source.Deployments )
        {

            var model = mission.AddDeployment();

            model.Name         = app.Name;
            model.Build        = app.Build;
            model.Checksum     = app.Checksum;
            model.Alias        = app.Alias;
            model.Assembly     = app.Assembly;
            model.ShowWindow   = app.ShowWindow;
            model.Deploy       = app.Deploy;
            model.WaitForStart = app.WaitForStart;

            model.ConfigurationAsJson = app.ConfigurationAsJson;

        }

        mission.Post();


        logger.LogObject(nameof(mission), mission);



        // *****************************************************************
        logger.Debug("Attempting to save new mission to repository");
        await Save(mission);


        // *****************************************************************
        return mission;


    }

    public async Task Save( MissionModel mission )
    {

        using var logger = this.EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to serialize mission to JSON");
        var json = JsonSerializer.Serialize(mission, new JsonSerializerOptions { WriteIndented = true });



        // *****************************************************************
        logger.Debug("Attempting to build S3 Client");
        using var client = BuildS3Client();



        // *****************************************************************
        logger.Debug("Attempting to build and send PutObject request");
        var req = new PutObjectRequest
        {
            BucketName  = CurrentBucketName,
            Key         = mission.RepositoryLocation,
            ContentBody = json
        };

        var res = await client.PutObjectAsync(req);
        logger.LogObject(nameof(res), res);

        if (res.HttpStatusCode != HttpStatusCode.OK)
            throw new Exception("The result from PutObject indicates failure");

        Missions.Add(mission);

    }

    public async Task Delete(MissionModel mission)
    {

        using var logger = this.EnterMethod();

        using var client = BuildS3Client();


        // *****************************************************************
        logger.Debug("Attempting to build and send DeleteObject request");
        var req = new DeleteObjectRequest
        {
            BucketName = CurrentBucketName,
            Key = mission.RepositoryLocation
        };


        var res = await client.DeleteObjectAsync(req);
        logger.LogObject(nameof(res), res);


        if (res.HttpStatusCode != HttpStatusCode.NoContent)
            throw new Exception("The result from DeleteObject indicates failure");


        // *****************************************************************
        logger.Debug("Attempting to remove deleted mission for Missions");
        Missions.Remove(mission);


    }

    public async Task Deploy(MissionModel mission, string version = "")
    {

        using var logger = this.EnterMethod();

        try
        {


            using var s3Client = BuildS3Client();

            var verReq = new GetObjectMetadataRequest
            {
                BucketName = CurrentBucketName,
                Key = mission.RepositoryLocation
            };

            // *****************************************************************
            logger.Debug("Attempting to get Mission Plan version id");
            logger.LogObject(nameof(verReq), verReq);

            var verRes = await s3Client.GetObjectMetadataAsync(verReq);
            var ver = verRes.VersionId;
            logger.Inspect(nameof(ver), ver);



            using var appConfigClient = BuildAppConfigClient();


            // *****************************************************************
            logger.Debug("Attempting to build StartDeployment request");
            var req = new StartDeploymentRequest
            {
                Description = $"Mission: ({mission.Name}) deployment for Environment: ({mission.Environment})",
                ConfigurationVersion = ver
            };



            // *****************************************************************
            logger.DebugFormat("Attempting to find Application using name = ({0})", mission.AppConfigApplication);

            var appRes = await appConfigClient.ListApplicationsAsync(new ListApplicationsRequest());
            var app = appRes.Items.SingleOrDefault(a => a.Name == mission.AppConfigApplication);
            if (app is null)
                throw new NotFoundException($"Could not find Application with name = ({mission.AppConfigApplication})");

            logger.LogObject(nameof(app), app);

            req.ApplicationId = app.Id;



            // *****************************************************************
            logger.DebugFormat("Attempting to find Config AwsProfileName using name = ({0})", mission.AppConfigConfigProfile);

            var cfgRes = await appConfigClient.ListConfigurationProfilesAsync(new ListConfigurationProfilesRequest { ApplicationId = app.Id });
            var cfg = cfgRes.Items.SingleOrDefault(a => a.Name == mission.AppConfigConfigProfile);
            if (cfg is null)
                throw new NotFoundException($"Could not find Config AwsProfileName with name = ({mission.AppConfigConfigProfile})");

            logger.LogObject(nameof(cfg), cfg);

            req.ConfigurationProfileId = cfg.Id;



            // *****************************************************************
            logger.DebugFormat("Attempting to find Environment using name = ({0})", mission.AppConfigEnvironment);

            var envRes = await appConfigClient.ListEnvironmentsAsync(new ListEnvironmentsRequest { ApplicationId = app.Id });
            var env = envRes.Items.SingleOrDefault(a => a.Name == mission.AppConfigEnvironment);

            if (env is null)
                throw new NotFoundException($"Could not find Environment with name = ({mission.AppConfigConfigProfile})");

            logger.LogObject(nameof(env), env);

            req.EnvironmentId = env.Id;



            // *****************************************************************
            logger.DebugFormat("Attempting to find Strategy using name = ({0})", mission.AppConfigStrategy);

            var strRes = await appConfigClient.ListDeploymentStrategiesAsync(new ListDeploymentStrategiesRequest());
            var str = strRes.Items.SingleOrDefault(a => a.Name == mission.AppConfigStrategy);

            if (str is null)
                throw new NotFoundException($"Could not find Strategy with name = ({mission.AppConfigEnvironment})");

            logger.LogObject(nameof(str), str);

            req.DeploymentStrategyId = str.Id;



            // *****************************************************************
            logger.Debug("Attempting to send StartDeployment request");
            logger.LogObject(nameof(req), req);

            var res = await appConfigClient.StartDeploymentAsync(req);
            logger.LogObject(nameof(res), res);

            if (res.HttpStatusCode != HttpStatusCode.Created && res.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception("The result from StartDeployment indicates failure");


        }
        catch (Exception cause)
        {
            var ctx = new
            {
                mission.Name,
                mission.Environment,
                mission.AppConfigApplication,
                mission.AppConfigConfigProfile,
                mission.AppConfigEnvironment,
                mission.AppConfigStrategy
            };
            logger.ErrorWithContext(cause, ctx, "Deploy Mission failed");
            throw;
        }


    }




    private AWSCredentials BuildCredentials()
    {

        using var logger = this.EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to check if running on EC2");
        AWSCredentials credentials;
        if (RunningOnEc2)
        {
            logger.Debug("Attempting to build instance profile credentials");
            credentials = !string.IsNullOrWhiteSpace(CurrentProfileName) ? new InstanceProfileAWSCredentials(CurrentProfileName) : new InstanceProfileAWSCredentials();
        }
        else
        {

            var sharedFile = new SharedCredentialsFile();
            if (!(sharedFile.TryGetProfile(CurrentProfileName, out var profile) && AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out credentials)))
                throw new Exception($"Local profile {profile} could not be loaded");

        }

        return credentials;

    }

    private AmazonS3Client BuildS3Client()
    {

        using var logger = this.EnterMethod();


        // *****************************************************************
        if ( !string.IsNullOrWhiteSpace(CurrentRegionName) )
        {

            var credentials = BuildCredentials();

            logger.Debug("Attempting to create Credentials and Region Endpoint");
            var endpoint = RegionEndpoint.GetBySystemName(CurrentRegionName);

            logger.Debug("Attempting to create Amazon S3 client");
            var client = new AmazonS3Client(credentials, endpoint);

            return client;

        }
        else
        {

            logger.Debug("Attempting to create Amazon S3 client");
            var client = new AmazonS3Client();

            return client;

        }

    }

    private AmazonAppConfigClient BuildAppConfigClient()
    {

        using var logger = this.EnterMethod();


        // *****************************************************************
        if (!string.IsNullOrWhiteSpace(CurrentRegionName))
        {

            var credentials = BuildCredentials();

            logger.Debug("Attempting to create Credentials and Region Endpoint");
            var endpoint = RegionEndpoint.GetBySystemName(CurrentRegionName);

            logger.Debug("Attempting to create Amazon AppConfig client");
            var client = new AmazonAppConfigClient(credentials, endpoint);

            return client;

        }
        else
        {

            logger.Debug("Attempting to create Amazon AppConfig client");
            var client = new AmazonAppConfigClient();

            return client;

        }

    }


    private async Task<List<string>> FetchBuckets(ICredentialProfileSource file, CredentialProfile cp)
    {

        using var logger = this.EnterMethod();


        var buckets = new List<string>();


        // *****************************************************************
        logger.Debug("Attempting to fetch credentials");
        if (!AWSCredentialsFactory.TryGetAWSCredentials(cp, file, out var credentials))
            return buckets;


        // *****************************************************************
        logger.Debug("Attempting to build S3 client");
        using var client = new AmazonS3Client(credentials, cp.Region);



        // *****************************************************************
        logger.Debug("Attempting to list buckets");
        var res = await client.ListBucketsAsync(new ListBucketsRequest());
        if (res.HttpStatusCode != HttpStatusCode.OK)
            throw new Exception($"Could not fetch buckets for AwsProfileName {cp.Name} Status: {res.HttpStatusCode}");


        // *****************************************************************
        logger.Debug("Attempting to load bucket names that end with '-appliance-repository'");
        buckets.AddRange(res.Buckets.Where(b => b.BucketName.EndsWith("-appliance-repository")).Select(b => b.BucketName));

        logger.Inspect(nameof(buckets.Count), buckets.Count);



        // *****************************************************************
        return buckets;

    }

    private async Task<List<string>> FetchBuckets()
    {

        using var logger = this.EnterMethod();


        var buckets = new List<string>();



        // *****************************************************************
        logger.Debug("Attempting to build S3 client");
        using var client = new AmazonS3Client();



        // *****************************************************************
        logger.Debug("Attempting to list buckets");
        var res = await client.ListBucketsAsync(new ListBucketsRequest());
        if (res.HttpStatusCode != HttpStatusCode.OK)
            throw new Exception($"Could not fetch buckets for AwsProfileName Instance Status: {res.HttpStatusCode}");


        // *****************************************************************
        logger.Debug("Attempting to load bucket names that end with '-appliance-repository'");
        buckets.AddRange(res.Buckets.Where(b => b.BucketName.EndsWith("-appliance-repository")).Select(b => b.BucketName));

        logger.Inspect(nameof(buckets.Count), buckets.Count);



        // *****************************************************************
        return buckets;

    }



    private List<RepositoryModel> Repositories { get; } = new ();
    private async Task LoadRepositories()
    {

        using var logger = this.EnterMethod();


        if ( RunningOnEc2 )
        {

            logger.Debug("Attempting to fetch buckets");
            var repos = await FetchBuckets();

            logger.Inspect(nameof(repos.Count), repos.Count);

            RepositoryModel Build(string repo)
            {
                var model = new RepositoryModel
                {
                    Description = $" {repo}: [Instance]",
                    Properties =
                    {
                        ["AwsProfileName"] = "Instance",
                        ["Region"]  = "",
                        ["Bucket"]  = repo
                    }
                };

                return model;

            }


            logger.Debug("Attempting to build repository models");
            foreach (var model in repos.Select(Build))
                Repositories.Add(model);

        }
        else
        {

            // *****************************************************************
            logger.Debug("Attempting to load credential profiles");

            var file = new SharedCredentialsFile();
            var profiles = file.ListProfiles();

            logger.Inspect(nameof(profiles.Count), profiles.Count);



            // *****************************************************************
            logger.Debug("Attempting to process each profile");
            foreach (var p in profiles.Where(p => p.Region != null))
            {

                logger.Inspect(nameof(p.Name), p.Name);
                logger.Inspect(nameof(p.Region), p.Region);



                logger.Debug("Attempting to fetch buckets");
                var repos = await FetchBuckets(file, p);

                logger.Inspect(nameof(repos.Count), repos.Count);

                RepositoryModel Build(string repo)
                {
                    var model = new RepositoryModel
                    {
                        Description = $" {repo}: [{p.Name.Pascalize()} {p.Region.DisplayName}]",
                        Properties =
                        {
                            ["AwsProfileName"] = p.Name,
                            ["Region"]  = p.Region.SystemName,
                            ["Bucket"]  = repo
                        }
                    };

                    return model;

                }


                logger.Debug("Attempting to build repository models");
                foreach (var model in repos.Select(Build))
                    Repositories.Add(model);

            }



        }


        logger.Inspect(nameof(Repositories.Count), Repositories.Count);


    }

    private List<MissionModel> Missions { get; } = new ();
    private async Task LoadMissions()
    {

        using var logger = this.EnterMethod();

        using var client = BuildS3Client();

        Missions.Clear();



        // *****************************************************************
        logger.Debug("Attempting to create paginator");
        var missions = client.Paginators.ListObjectsV2(new ListObjectsV2Request { BucketName = CurrentBucketName, Prefix = MissionPrefix });



        // *****************************************************************
        logger.Debug("Attempting to process S3Objects");
        await foreach (var o in missions.S3Objects)
        {

            if (!MissionFilter(o.Key))
                continue;


            var res = await client.GetObjectAsync(new GetObjectRequest { BucketName = o.BucketName, Key = o.Key });

            if (res.HttpStatusCode != HttpStatusCode.OK)
                continue;


            await using var stream = res.ResponseStream;

            var model = await JsonSerializer.DeserializeAsync<MissionModel>(stream);
            if (model is null)
                continue;

            model.RepositoryLocation = o.Key;

            Missions.Add(model);

        }

        // *****************************************************************
        logger.DebugFormat("Loaded {0} Mission(s)", Missions.Count);


    }

    private List<BuildModel> Builds { get; } = new ();
    private async Task LoadBuilds()
    {

        using var logger = this.EnterMethod();

        using var client = BuildS3Client();


        Builds.Clear();



        // *****************************************************************
        logger.Debug("Attempting to create paginator");
        var builds = client.Paginators.ListObjectsV2(new ListObjectsV2Request { BucketName = CurrentBucketName, Prefix = BuildPrefix });



        // *****************************************************************
        logger.Debug("Attempting to process S3Objects");
        await foreach (var o in builds.S3Objects)
        {

            if (!BuildFilter(o.Key))
                continue;

            var res = await client.GetObjectAsync(new GetObjectRequest { BucketName = o.BucketName, Key = o.Key });

            if (res.HttpStatusCode != HttpStatusCode.OK)
                continue;

            await using var stream = res.ResponseStream;

            var model = await JsonSerializer.DeserializeAsync<BuildModel>(stream);
            if (model is null)
                continue;


            Builds.Add(model);


        }

        // *****************************************************************
        logger.DebugFormat("Loaded {0} Build(s)", Builds.Count);


    }


}