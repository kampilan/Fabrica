﻿using System;
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
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Text;
using Fabrica.Watch;

namespace Fabrica.One.Orchestrator.Aws.Repository
{


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


        private RepositoryModel _currentRepository;


        private string CurrentProfileName
        {

            get
            {
                if (_currentRepository != null && _currentRepository.Properties.TryGetValue("Profile", out var profile))
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


            // *****************************************************************
            logger.Debug("Attempting to load missions");
            await LoadMissions();


            // *****************************************************************
            logger.Debug("Attempting to load builds");
            await LoadBuilds();


        }


        public RepositoryModel CurrentRepository => _currentRepository;


        public async Task SetCurrentRepository( RepositoryModel current )
        {

            using var logger = this.EnterMethod();

            _currentRepository = current;

            await LoadMissions();
            await LoadBuilds();

        }


        public async Task<IEnumerable<RepositoryModel>> GetRepositories(Func<RepositoryModel, bool> predicate = null)
        {

            using var logger = this.EnterMethod();

            logger.Inspect(nameof(Repositories.Count), Repositories.Count);


            if( Repositories.Count == 0 )
                await LoadRepositories();

            IEnumerable<RepositoryModel> results = Repositories;
            if (!(predicate is null))
                results = Repositories.Where(predicate);


            // *****************************************************************
            return results;


        }

        public async Task<IEnumerable<MissionModel>> GetMissions(Func<MissionModel, bool> predicate = null)
        {

            using var logger = this.EnterMethod();


            if ( _currentRepository != null && Missions.Count == 0 )
                await LoadMissions();


            IEnumerable<MissionModel> results = Missions;
            if (!(predicate is null))
                results = Missions.Where(predicate);

            return results;

        }

        public async Task<IEnumerable<BuildModel>> GetBuilds(Func<BuildModel, bool> predicate = null)
        {

            using var logger = this.EnterMethod();

            if ( _currentRepository != null &&  Builds.Count == 0 )
                await LoadBuilds();


            IEnumerable<BuildModel> results = Builds;
            if (!(predicate is null))
                results = Builds.Where(predicate);


            return results;

        }
        


        public Task<MissionModel> CreateMission(string name)
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

        public async Task Save(MissionModel mission)
        {

            using var logger = this.EnterMethod();

            using var client = BuildClient();


            // *****************************************************************
            logger.Debug("Attempting to serialize mission to JSON");
            var json = JsonSerializer.Serialize(mission, new JsonSerializerOptions { WriteIndented = true });



            // *****************************************************************
            logger.Debug("Attempting to build and send PutObject request");
            var req = new PutObjectRequest
            {
                BucketName = CurrentBucketName,
                Key = mission.RepositoryLocation,
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

            using var client = BuildClient();


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
                credentials = new InstanceProfileAWSCredentials(CurrentProfileName);
            }
            else
            {

                var sharedFile = new SharedCredentialsFile();
                if (!(sharedFile.TryGetProfile(CurrentProfileName, out var profile) && AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out credentials)))
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
            var endpoint = RegionEndpoint.GetBySystemName(CurrentRegionName);



            // *****************************************************************
            logger.Debug("Attempting to create Amazaon S3 client");
            var client = new AmazonS3Client(credentials, endpoint);



            // *****************************************************************
            return client;

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
                throw new Exception($"Could not fetch buckets for Profile {cp.Name} Status: {res.HttpStatusCode}");


            // *****************************************************************
            logger.Debug("Attempting to load bucket names that end with '-appliance-repository'");
            buckets.AddRange(res.Buckets.Where(b => b.BucketName.EndsWith("-appliance-repository")).Select(b => b.BucketName));

            logger.Inspect(nameof(buckets.Count), buckets.Count);



            // *****************************************************************
            return buckets;

        }


        private List<RepositoryModel> Repositories { get; } = new List<RepositoryModel>();
        private async Task LoadRepositories()
        {

            using var logger = this.EnterMethod();


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
                        Description = $"{p.Name.Pascalize()} [{p.Region.DisplayName}]: {repo}",
                        Properties =
                        {
                            ["Profile"] = p.Name,
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


            logger.Inspect(nameof(Repositories.Count), Repositories.Count);


        }

        private List<MissionModel> Missions { get; } = new List<MissionModel>();
        private async Task LoadMissions()
        {

            using var logger = this.EnterMethod();

            using var client = BuildClient();

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

                await using var strm = res.ResponseStream;

                var model = await JsonSerializer.DeserializeAsync<MissionModel>(strm);
                if (model is null)
                    continue;

                model.RepositoryLocation = o.Key;

                Missions.Add(model);

            }

            // *****************************************************************
            logger.DebugFormat("Loaded {0} Mission(s)", Missions.Count);


        }

        private List<BuildModel> Builds { get; } = new List<BuildModel>();
        private async Task LoadBuilds()
        {

            using var logger = this.EnterMethod();

            using var client = BuildClient();


            Builds.Clear();



            // *****************************************************************
            logger.Debug("Attempting to create paginator");
            var missions = client.Paginators.ListObjectsV2(new ListObjectsV2Request { BucketName = CurrentBucketName, Prefix = BuildPrefix });



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


    }


}
