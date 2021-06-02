using System;
using Amazon.S3;
using Amazon.S3.Model;
using Fabrica.Utilities.Repository;

namespace Fabrica.Aws.Repository
{


    public class S3RepositoryUrlProvider : IRepositoryUrlProvider
    {


        public S3RepositoryUrlProvider( IAmazonS3 client, string permanent, string transient, string resource )
        {

            Client = client;

            PermanentBucket = permanent;
            TransientBucket = transient;
            ResourceBucket = resource;
        }


        private IAmazonS3 Client { get; }

        private string PermanentBucket { get; }
        private string TransientBucket { get; }
        private string ResourceBucket { get; }

        private string MapRepositoryType(RepositoryType type)
        {

            switch (type)
            {
                case RepositoryType.Transient:
                    return TransientBucket;
                case RepositoryType.Permanent:
                    return PermanentBucket;
                case RepositoryType.Resource:
                    return ResourceBucket;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }


        public string CreateGetUrl(string repositoryName, string key, string contentType = "", TimeSpan ttl = default)
        {

            if (ttl == default)
                ttl = TimeSpan.FromSeconds(60);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = repositoryName,
                Key = key,
                Protocol = Protocol.HTTPS,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow + ttl
            };

            if (!string.IsNullOrWhiteSpace(contentType))
                request.ContentType = contentType;


            var url = Client.GetPreSignedURL(request);

            return url;

        }

        public string CreateGetUrl(RepositoryType type, string key, string contentType = "", TimeSpan ttl = default)
        {

            var repo = MapRepositoryType(type);
            var url = CreateGetUrl(repo, key, contentType, ttl);

            return url;

        }

        public string CreatePutUrl(string repositoryName, string key, string contentType = "", TimeSpan ttl = default)
        {

            if (ttl == default)
                ttl = TimeSpan.FromSeconds(60);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = repositoryName,
                Key = key,
                Protocol = Protocol.HTTPS,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow + ttl
            };

            if (!string.IsNullOrWhiteSpace(contentType))
                request.ContentType = contentType;


            var url = Client.GetPreSignedURL(request);

            return url;

        }

        public string CreatePutUrl(RepositoryType type, string key, string contentType = "", TimeSpan ttl = default)
        {

            var repo = MapRepositoryType(type);
            var url = CreatePutUrl(repo, key, contentType, ttl);

            return url;

        }


    }


}
