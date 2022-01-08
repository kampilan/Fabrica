using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using Fabrica.One.Models;
using Fabrica.One.Repository;
using Fabrica.Watch;

namespace Fabrica.One.Orchestrator.Aws.Repository
{


    public class S3Repository: IRepository
    {


        public string ProfileName { get; set; } = "";
        public string RegionName { get; set; } = "";
        public bool RunningOnEc2 { get; set; } = true;

        public string BucketName { get; set; } = "";

        public string MissionPrefix { get; set; } = "missions/";
        public Func<string, bool> MissionFilter { get; set; } = k=>k.EndsWith("-mission-plan.json");

        public string BuildPrefix { get; set; } = "appliances/";
        public Func<string, bool> BuildFilter { get; set; } = k=>k.EndsWith("-manifest.json");

        private AWSCredentials BuildCredentials()
        {

            using var logger = this.EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to check if running on EC2");
            AWSCredentials credentials;
            if (RunningOnEc2)
            {
                logger.Debug("Attempting to build instance profile credentials");
                credentials = new InstanceProfileAWSCredentials(ProfileName);
            }
            else
            {

                var sharedFile = new SharedCredentialsFile();
                if (!(sharedFile.TryGetProfile(ProfileName, out var profile) && AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out credentials)))
                    throw new Exception($"Local profile {profile} could not be loaded");

            }

            return credentials;

        }

        private AmazonS3Client BuildClient()
        {

            using var logger = this.EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to create Credentials and Region Endpoint");
            var credentials = BuildCredentials();
            var endpoint = RegionEndpoint.GetBySystemName(RegionName);



            // *****************************************************************
            logger.Debug("Attempting to create Amazaon S3 client");
            var client = new AmazonS3Client(credentials, endpoint);



            // *****************************************************************
            return client;

        }

        public async Task Reload()
        {

            using var logger = this.EnterMethod();
            
            await LoadMissions();
            await LoadBuilds();

        }

        public async Task LoadMissions()
        {

            using var logger = this.EnterMethod();

            using var client = BuildClient();

            Missions.Clear();



            // *****************************************************************
            logger.Debug("Attempting to create paginator");
            var missions = client.Paginators.ListObjectsV2(new ListObjectsV2Request { BucketName = BucketName, Prefix = MissionPrefix });



            // *****************************************************************
            logger.Debug("Attempting to process S3Objects");
            await foreach( var o in missions.S3Objects )
            {

                if( !MissionFilter(o.Key) )
                    continue;

                var res = await client.GetObjectAsync(new GetObjectRequest { BucketName = o.BucketName, Key = o.Key });

                if (res.HttpStatusCode != HttpStatusCode.OK)
                    continue;

                await using var strm = res.ResponseStream;

                var model = await JsonSerializer.DeserializeAsync<MissionModel>(strm);
                if (model is null)
                    continue;

                model.RepositoryLocation = o.Key;
                
                Missions.Add(model);

            }

            // *****************************************************************
            logger.DebugFormat( "Loaded {0} Mission(s)", Missions.Count );


        }


        public async Task LoadBuilds()
        {

            using var logger = this.EnterMethod();

            using var client = BuildClient();


            Builds.Clear();



            // *****************************************************************
            logger.Debug("Attempting to create paginator");
            var missions = client.Paginators.ListObjectsV2(new ListObjectsV2Request { BucketName = BucketName, Prefix = BuildPrefix });



            // *****************************************************************
            logger.Debug("Attempting to process S3Objects");
            await foreach (var o in missions.S3Objects)
            {

                if (!BuildFilter(o.Key))
                    continue;

                var res = await client.GetObjectAsync(new GetObjectRequest { BucketName = o.BucketName, Key = o.Key });

                if (res.HttpStatusCode != HttpStatusCode.OK)
                    continue;

                await using var strm = res.ResponseStream;

                var model = await JsonSerializer.DeserializeAsync<BuildModel>(strm);
                if (model is null)
                    continue;


                Builds.Add(model);


            }

            // *****************************************************************
            logger.DebugFormat("Loaded {0} Build(s)", Builds.Count);


        }


        private List<MissionModel> Missions { get; } = new List<MissionModel>();
        public async Task<IEnumerable<MissionModel>> GetMissions( Func<MissionModel,bool> predicate=null )
        {

            using var logger = this.EnterMethod();

            if (Missions.Count == 0)
                await LoadMissions();
           
           
            IEnumerable<MissionModel> results = Missions;
            if(!(predicate is null))
                results = Missions.Where(predicate);

            return results;

        }


        private List<BuildModel> Builds { get; } = new List<BuildModel>();
        public async Task<IEnumerable<BuildModel>> GetBuilds(Func<BuildModel, bool> predicate = null)
        {

            using var logger = this.EnterMethod();

            if (Builds.Count == 0)
                await LoadBuilds();


            IEnumerable<BuildModel> results = Builds;
            if( !(predicate is null) )
                results = Builds.Where(predicate);


            return results;

        }


        public Task<MissionModel> CreateMission( string name )
        {

            using var logger = this.EnterMethod();

            var exists = Missions.Exists(m => m.Name == name);
            if (exists)
                throw new Exception($"A mission with the name ({name}) already exists.");

            var mission = new MissionModel
            {
                Name = name,
                RepositoryLocation = $"missions/{name}-mission-plan.json"
            };
            
            return Task.FromResult(mission);

        }


        public async Task Save( MissionModel mission )
        {

            using var logger = this.EnterMethod();

            using var client = BuildClient();


            // *****************************************************************
            logger.Debug("Attempting to serialize mission to JSON");
            var json = JsonSerializer.Serialize(mission, new JsonSerializerOptions {WriteIndented = true});



            // *****************************************************************
            logger.Debug("Attempting to build and send PutObject request");
            var req = new PutObjectRequest
            {
                BucketName  = BucketName,
                Key         = mission.RepositoryLocation,
                ContentBody = json
            };


            var res = await client.PutObjectAsync(req);
            logger.LogObject(nameof(res), res);

            if (res.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception("The result from PutObject indicates failure");

            Missions.Add(mission);

        }

        public async Task Delete( MissionModel mission )
        {

            using var logger = this.EnterMethod();

            using var client = BuildClient();


            // *****************************************************************
            logger.Debug("Attempting to build and send DeleteObject request");
            var req = new DeleteObjectRequest
            {
                BucketName = BucketName,
                Key = mission.RepositoryLocation
            };


            var res = await client.DeleteObjectAsync(req);
            logger.LogObject(nameof(res), res);


            if (res.HttpStatusCode != HttpStatusCode.NoContent)
                throw new Exception("The result from DeleteObject indicates failure");

        }


    }


}
