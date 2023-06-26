using Amazon.S3;
using Amazon.S3.Model;
using Fabrica.Repository;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Repository;
using Fabrica.Watch;

namespace Fabrica.Aws.Repository;

public class S3RepositoryProvider : CorrelatedObject, IRepositoryProvider
{


    public S3RepositoryProvider( ICorrelation correlation, IAmazonS3 client, string repository ): base(correlation)
    {

        Client = client;

        RepositoryBucket = repository;
    }


    private IAmazonS3 Client { get; }

    private string RepositoryBucket { get; }


    public async Task<RepositoryObjectMeta> GetMetaData( string key )
    {

        var logger = GetLogger();

        try
        {

            logger.EnterMethod();


            logger.Inspect(nameof(key), key);


            // *****************************************************************
            logger.Debug("Attempting to build request");
            var request = new GetObjectMetadataRequest
            {
                BucketName = RepositoryBucket,
                Key = key
            };


            GetObjectMetadataResponse response;
            try
            {

                // *****************************************************************
                logger.Debug("Attempting to send request");
                response = await Client.GetObjectMetadataAsync(request);

            }
            catch( Exception cause )
            {
                logger.Debug( cause, "GetObjectMetadata failed. Not found?");
                return new RepositoryObjectMeta(false, "", 0, DateTime.MinValue);
            }


            // *****************************************************************
            logger.Debug("Attempting to process response");
            var result = new RepositoryObjectMeta(true, response.Headers.ContentType, response.Headers.ContentLength, response.LastModified);

            logger.LogObject(nameof(result), result);



            // *****************************************************************
            return result;


        }
        catch (Exception cause)
        {
            var ctx = new { Key = key};
            logger.ErrorWithContext( cause, ctx,"GetMetaData failed");
            throw;
        }
        finally
        {
            logger.LeaveMethod();
        }

    }


    public Task<string> CreateGetUrl( string key, TimeSpan ttl = default )
    {

        if (ttl == default)
            ttl = TimeSpan.FromSeconds(60);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = RepositoryBucket,
            Key        = key,
            Protocol   = Protocol.HTTPS,
            Verb       = HttpVerb.GET,
            Expires    = DateTime.UtcNow + ttl
        };

        var url = Client.GetPreSignedURL(request);

        return Task.FromResult( url );

    }

    public Task<string> CreatePutUrl( string key, string contentType = "", TimeSpan ttl = default )
    {

        if (ttl == default)
            ttl = TimeSpan.FromSeconds(60);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = RepositoryBucket,
            Key        = key,
            Protocol   = Protocol.HTTPS,
            Verb       = HttpVerb.PUT,
            Expires    = DateTime.UtcNow + ttl
        };

        if (!string.IsNullOrWhiteSpace(contentType))
            request.ContentType = contentType;


        var url = Client.GetPreSignedURL(request);

        return Task.FromResult(url);

    }

    public async Task Move(string sourceKey, string destinationKey)
    {

        var logger = GetLogger();

        try
        {

            logger.EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to build CopyObjectRequest");
            var copyReq = new CopyObjectRequest
            {
                SourceBucket = RepositoryBucket,
                SourceKey = sourceKey,
                DestinationBucket = RepositoryBucket,
                DestinationKey = destinationKey
            };

            // *****************************************************************
            logger.Debug("Attempting to call CopyObject");
            var copyRes = await Client.CopyObjectAsync(copyReq);

            logger.LogObject(nameof(copyRes), copyRes);

            var delReq = new DeleteObjectRequest
            {
                BucketName = RepositoryBucket,
                Key        = sourceKey
            };


            var delRes = await Client.DeleteObjectAsync(delReq);

            logger.LogObject(nameof(delRes), delRes);


        }
        catch (Exception cause)
        {
            var ctx = new {RepositoryBucket, SourceKey = sourceKey, DestinationKey = destinationKey};
            logger.ErrorWithContext( cause, ctx, "Move failed");
            throw;
        }
        finally
        {
            logger.LeaveMethod();
        }


    }

    public async Task Delete( string key )
    {

        var logger = GetLogger();

        try
        {

            logger.EnterMethod();


            var delReq = new DeleteObjectRequest
            {
                BucketName = RepositoryBucket,
                Key        = key
            };


            var delRes = await Client.DeleteObjectAsync(delReq);

            logger.LogObject(nameof(delRes), delRes);


        }
        catch (Exception cause)
        {
            var ctx = new { RepositoryBucket, Key = key };
            logger.ErrorWithContext(cause, ctx, "Delete failed");
            throw;
        }
        finally
        {
            logger.LeaveMethod();
        }


    }

}