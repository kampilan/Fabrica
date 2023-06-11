using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Autofac;
using Fabrica.Watch;

namespace Fabrica.Search;

public class ClusterSearchIndexManager<TIndex>: SearchIndexManager<TIndex> where TIndex: class
{

    public ClusterSearchIndexManager(ILifetimeScope scope, IAmazonS3 client) : base(scope)
    {
        Client = client;
    }

    private IAmazonS3 Client { get; }

    public required string BucketName { get; init; }
    public required string IndexRoot { get; init; }
    private string IndexKey => $"{IndexRoot}/{typeof(TIndex).Name}.bin";

    private string _lastChecksum = string.Empty;


    protected override async Task Load()
    {

        using var logger = this.EnterMethod();

        logger.Inspect(nameof(BucketName), BucketName);
        logger.Inspect(nameof(IndexKey), IndexKey);
        logger.Inspect(nameof(_lastChecksum), _lastChecksum);

        if (string.IsNullOrWhiteSpace(BucketName) && string.IsNullOrWhiteSpace(IndexKey))
            return;


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to fetch Index stream meta from S3");
            var metaReq = new GetObjectMetadataRequest
            {
                BucketName = BucketName,
                Key = IndexKey
            };

            var metaRes = await Client.GetObjectMetadataAsync(metaReq);

            logger.Inspect(nameof(metaRes.HttpStatusCode), metaRes.HttpStatusCode);

            if (metaRes.HttpStatusCode != HttpStatusCode.OK)
                return;



            // *****************************************************************
            logger.Debug("Attempting to compare checksums to see if index has changed");
            var checksum = string.IsNullOrWhiteSpace(metaRes.ChecksumSHA1) ? "" : metaRes.ChecksumSHA1;

            var result = new { Current = checksum, Last = _lastChecksum, Matched = (checksum == _lastChecksum) };

            logger.LogObject(nameof(result), result);


            if( result.Matched )
                return;



        }
        catch (AmazonS3Exception)
        {
            return;
        }
        catch (Exception cause)
        {
            logger.Error(cause, "GetObjectMeta failed");
            return;
        }



        try
        {

            // *****************************************************************
            logger.Debug("Attempting to fetch Index stream from S3");

            var getReq = new GetObjectRequest
            {
                BucketName = BucketName,
                Key        = IndexKey
            };

            var getRes = await Client.GetObjectAsync(getReq);

            if (getRes.HttpStatusCode != HttpStatusCode.OK)
                return;


            using var ms = new MemoryStream();
            await getRes.ResponseStream.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);

            // *****************************************************************
            logger.Debug("Attempting to update source");
            UpdateSource(ms.ToArray());

            _lastChecksum = getRes.ChecksumSHA1;


        }
        catch (Exception cause)
        {
            logger.Error(cause, "GetObject failed");
        }


    }


    protected override async Task Save( Memory<byte> source )
    {

        using var logger = this.EnterMethod();


        if (string.IsNullOrWhiteSpace(BucketName) || string.IsNullOrWhiteSpace(IndexKey))
            return;

        try
        {

            using var stream = new MemoryStream(source.ToArray(), false);

            // *****************************************************************
            logger.Debug("Attempting to build S3 put request and copy source stream to request input stream");

            var objReq = new PutObjectRequest
            {
                BucketName = BucketName,
                Key = IndexKey,
                InputStream = stream
            };


            // *****************************************************************
            logger.Debug("Attempting to call S3 Put Object");
            var objRes = await Client.PutObjectAsync(objReq);

            logger.Inspect(nameof(objRes.HttpStatusCode), objRes.HttpStatusCode);


        }
        catch (Exception cause)
        {
            logger.Error(cause, "Put S3 object failed");
        }


    }


}