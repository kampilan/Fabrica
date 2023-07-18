using Amazon.S3.Model;
using Amazon.S3;
using System.Net;
using System.Security.Cryptography;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Search;

public abstract class AbstractClusterSearchProvider<TDocument, TIndex>: AbstractSearchProvider<TDocument, TIndex> where TDocument : class where TIndex : class
{


    protected AbstractClusterSearchProvider(ICorrelation correlation, IAmazonS3 client) : base(correlation)
    {
        Client = client;
    }

    private IAmazonS3 Client { get; }

    public required string BucketName { get; init; }
    public required string IndexRoot { get; init; }
    private string IndexKey => $"{IndexRoot}/{typeof(TIndex).Name}.bin";

    private string _lastChecksum = string.Empty;


    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(10);
    private DateTime _lastCheck = DateTime.MinValue;

    protected sealed override async Task<bool> Load()
    {


        using var logger = EnterMethod();

        logger.Inspect(nameof(BucketName), BucketName);
        logger.Inspect(nameof(IndexKey), IndexKey);
        logger.Inspect(nameof(_lastChecksum), _lastChecksum);


        if( string.IsNullOrWhiteSpace(BucketName) && string.IsNullOrWhiteSpace(IndexKey) )
            return false;


        if( DateTime.Now < _lastCheck + CheckInterval )
            return false;

        _lastCheck = DateTime.Now;


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to fetch Index stream meta from S3");
            var metaReq = new GetObjectMetadataRequest
            {
                BucketName = BucketName,
                Key        = IndexKey,
                ChecksumMode = ChecksumMode.ENABLED
            };

            var metaRes = await Client.GetObjectMetadataAsync(metaReq);

            logger.Inspect(nameof(metaRes.HttpStatusCode), metaRes.HttpStatusCode);

            if (metaRes.HttpStatusCode != HttpStatusCode.OK)
                return false;



            // *****************************************************************
            logger.Debug("Attempting to compare checksums to see if index has changed");
            var checksum = string.IsNullOrWhiteSpace(metaRes.ChecksumSHA1) ? "" : metaRes.ChecksumSHA1;

            var result = new { Current = checksum, Last = _lastChecksum, Matched = (checksum == _lastChecksum) };

            logger.LogObject(nameof(result), result);


            if( result.Matched )
                return false;



        }
        catch (AmazonS3Exception)
        {
            return false;
        }
        catch (Exception cause)
        {
            logger.Error(cause, "GetObjectMeta failed");
            return false;
        }



        try
        {

            // *****************************************************************
            logger.Debug("Attempting to fetch Index stream from S3");

            var getReq = new GetObjectRequest
            {
                BucketName   = BucketName,
                Key          = IndexKey,
                ChecksumMode = ChecksumMode.ENABLED
            };

            var getRes = await Client.GetObjectAsync(getReq);

            if( getRes.HttpStatusCode != HttpStatusCode.OK )
                return false;


            using var stream = new MemoryStream();
            await getRes.ResponseStream.CopyToAsync(stream);
            stream.Seek( 0, SeekOrigin.Begin );



            // *****************************************************************
            logger.Debug("Attempting to update index");
            await UpdateIndex(stream);

            _lastChecksum = getRes.ChecksumSHA1;

            return true;

        }
        catch (Exception cause)
        {
            logger.Error(cause, "GetObject failed");
            return false;
        }


    }


    protected override async Task Save( MemoryStream stream )
    {

        using var logger = EnterMethod();


        if (string.IsNullOrWhiteSpace(BucketName) || string.IsNullOrWhiteSpace(IndexKey))
            return;

        try
        {

            // *****************************************************************
            logger.Debug("Attempting to calc SHA1");
            var sha1 = SHA1.HashData(stream.ToArray());
            var sha1Str = Convert.ToBase64String(sha1);



            // *****************************************************************
            logger.Debug("Attempting to build S3 put request and copy source stream to request input stream");

            var objReq = new PutObjectRequest
            {
                BucketName   = BucketName,
                Key          = IndexKey,
                ChecksumSHA1 = sha1Str,
                InputStream  = stream
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




public abstract class AbstractClusterSearchProvider<TIndex> : AbstractSearchProvider<TIndex> where TIndex : class
{


    protected AbstractClusterSearchProvider(ICorrelation correlation, IAmazonS3 client) : base(correlation)
    {
        Client = client;
    }

    private IAmazonS3 Client { get; }

    public required string BucketName { get; init; }
    public required string IndexRoot { get; init; }
    private string IndexKey => $"{IndexRoot}/{typeof(TIndex).Name}.bin";

    private string _lastChecksum = string.Empty;


    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(10);
    private DateTime _lastCheck = DateTime.MinValue;

    protected sealed override async Task<bool> Load()
    {


        using var logger = EnterMethod();

        logger.Inspect(nameof(BucketName), BucketName);
        logger.Inspect(nameof(IndexKey), IndexKey);
        logger.Inspect(nameof(_lastChecksum), _lastChecksum);


        if (string.IsNullOrWhiteSpace(BucketName) && string.IsNullOrWhiteSpace(IndexKey))
            return false;


        if (DateTime.Now < _lastCheck + CheckInterval)
            return false;

        _lastCheck = DateTime.Now;


        try
        {


            // *****************************************************************
            logger.Debug("Attempting to fetch Index stream meta from S3");
            var metaReq = new GetObjectMetadataRequest
            {
                BucketName = BucketName,
                Key = IndexKey,
                ChecksumMode = ChecksumMode.ENABLED
            };

            var metaRes = await Client.GetObjectMetadataAsync(metaReq);

            logger.Inspect(nameof(metaRes.HttpStatusCode), metaRes.HttpStatusCode);

            if (metaRes.HttpStatusCode != HttpStatusCode.OK)
                return false;



            // *****************************************************************
            logger.Debug("Attempting to compare checksums to see if index has changed");
            var checksum = string.IsNullOrWhiteSpace(metaRes.ChecksumSHA1) ? "" : metaRes.ChecksumSHA1;

            var result = new { Current = checksum, Last = _lastChecksum, Matched = (checksum == _lastChecksum) };

            logger.LogObject(nameof(result), result);


            if (result.Matched)
                return false;



        }
        catch (AmazonS3Exception)
        {
            return false;
        }
        catch (Exception cause)
        {
            logger.Error(cause, "GetObjectMeta failed");
            return false;
        }



        try
        {

            // *****************************************************************
            logger.Debug("Attempting to fetch Index stream from S3");

            var getReq = new GetObjectRequest
            {
                BucketName = BucketName,
                Key = IndexKey,
                ChecksumMode = ChecksumMode.ENABLED
            };

            var getRes = await Client.GetObjectAsync(getReq);

            if (getRes.HttpStatusCode != HttpStatusCode.OK)
                return false;


            using var stream = new MemoryStream();
            await getRes.ResponseStream.CopyToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);



            // *****************************************************************
            logger.Debug("Attempting to update index");
            await UpdateIndex(stream);

            _lastChecksum = getRes.ChecksumSHA1;

            return true;

        }
        catch (Exception cause)
        {
            logger.Error(cause, "GetObject failed");
            return false;
        }


    }


    protected override async Task Save(MemoryStream stream)
    {

        using var logger = EnterMethod();


        if (string.IsNullOrWhiteSpace(BucketName) || string.IsNullOrWhiteSpace(IndexKey))
            return;

        try
        {

            // *****************************************************************
            logger.Debug("Attempting to calc SHA1");
            var sha1 = SHA1.HashData(stream.ToArray());
            var sha1Str = Convert.ToBase64String(sha1);



            // *****************************************************************
            logger.Debug("Attempting to build S3 put request and copy source stream to request input stream");

            var objReq = new PutObjectRequest
            {
                BucketName = BucketName,
                Key = IndexKey,
                ChecksumSHA1 = sha1Str,
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