using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fabrica.Identity;
using Fabrica.Repository;
using Fabrica.Watch;
using Fabrica.Work.Models;
using Fabrica.Work.Persistence.Contexts;
using Newtonsoft.Json;

namespace Fabrica.Work.Processor;

internal class IngestionWorkProcessor: AbstractWorkProcessor
{


    public IngestionWorkProcessor(IHttpClientFactory factory, WorkDbContext context, IAccessTokenSource tokenSource, IRepositoryProvider provider ) : base(factory, context, tokenSource)
    {

        Provider = provider;

    }

    private IRepositoryProvider Provider { get; }



    protected override async Task<(bool proceed,string payload)> SerializePayload( WorkRequest request )
    {

        using var logger = this.EnterMethod();

        var ie = new IngestionEvent();

        try
        {

            var s3 = request.Payload.ToObject<S3CreateEvent>();


            if( s3 is null )
                throw new InvalidOperationException( "WorkRequest Payload produced Null S3CreateEvent" );


            if (s3.Key.EndsWith("/"))
                return (false, "");


            var url = await Provider.CreateGetUrl(s3.Key, TimeSpan.FromSeconds(120));

            ie.Endpoint = url;
            ie.Size     = s3.Size;

        }
        catch (Exception cause)
        {
            var ctx = new {request.Uid, request.Topic};
            logger.ErrorWithContext( cause, ctx, "CreateGetUrl failed");
            throw;
        }


        var json = JsonConvert.SerializeObject(ie);

        return (true,json);


    }

    protected override async Task Accepted( WorkRequest request )
    {

        using var logger = this.EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to parse payload");
        var s3 = request.Payload.ToObject<S3CreateEvent>();

        if( s3 is null )
            throw new InvalidOperationException("WorkRequest Payload produced Null S3CreateEvent");

        logger.LogObject(nameof(s3), s3);


        // *****************************************************************
        logger.Debug("Attempting to build accepted path");
        var destKey = _buildPath("accepted", s3.Key);

        logger.Inspect(nameof(destKey), destKey);


        // *****************************************************************
        logger.Debug("Attempting to Move object from Source to Destination");
        await Provider.Move(s3.Key, destKey );


    }

    protected override async Task Rejected(WorkRequest request)
    {

        using var logger = this.EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to parse payload");
        var s3 = request.Payload.ToObject<S3CreateEvent>();

        if (s3 is null)
            throw new InvalidOperationException("WorkRequest Payload produced Null S3CreateEvent");

        logger.LogObject(nameof(s3), s3);



        // *****************************************************************
        logger.Debug("Attempting to build rejected path");
        var destKey = _buildPath( "rejected", s3.Key );



        // *****************************************************************
        logger.Debug("Attempting to Move object from Source to Destination");
        await Provider.Move(s3.Key, destKey);


    }


    private string _buildPath( string prefix, string key )
    {

        string path;
        var segs = key.Split("/");
        if (segs.Length == 1)
            path = $"{prefix}/{segs[0]}";
        else
        {

            var root = segs[0];

            var subs = segs.Skip(2);
            var sub = string.Join("/", subs);

            path = $"{root}/{prefix}/{sub}";
        }

        return path;

    }



}