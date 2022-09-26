using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabrica.Utilities.Text;
using Fabrica.Watch;
using Fabrica.Work.Models;
using Newtonsoft.Json;

namespace Fabrica.Work.Processor.Parsers;

public class S3EventMessageBodyParser: IMessageBodyParser
{

    public S3EventMessageBodyParser(WorkTopicTransformer transformer)
    {

        Transformer = transformer;
    }

    private WorkTopicTransformer Transformer { get; }

    public Task<(bool ok, WorkRequest? request)> Parse( string body )
    {

        if (string.IsNullOrWhiteSpace(body)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(body));

        using var logger = this.EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to Deserialize S3 Event JSON");
        S3Event s3Event;
        try
        {
            s3Event = JsonConvert.DeserializeObject<S3Event>(body)!;
            if (s3Event is null)
                throw new Exception("Could not parse S3 Event JSON message");

            logger.LogObject(nameof(s3Event), s3Event);

        }
        catch( Exception cause )
        {
            var ctx = new {Body = body};
            logger.ErrorWithContext( cause, ctx, "Could not parse JSON body");
            return Task.FromResult((false,(WorkRequest?)null));
        }


        // *****************************************************************
        logger.Debug("Attempting to get single s3Event record");    
        var record = s3Event.Records?.FirstOrDefault();

        if( record is null )
            return Task.FromResult((false, (WorkRequest?)null));


        logger.LogObject(nameof(S3EventMessageBodyParser), this);



        // *****************************************************************
        logger.Debug("Attempting to build WorkRequest payload");
        var payload = new S3CreateEvent
        {
            Region      = record.AwsRegion, 
            Bucket      = record.S3.Bucket.Name, 
            Key         = record.S3.Object.Key, 
            Size        = record.S3.Object.Size, 
            Operation   = record.EventName, 
            Timestamp   = record.EventTime
        };

        logger.LogObject(nameof(payload), payload);



        // *****************************************************************
        logger.Debug("Attempting to dig out Topic from S3 Event Key");

        var topic = Transformer.Transform(payload.Key);

        logger.Inspect(nameof(topic), topic);

        

        // *****************************************************************
        logger.Debug("Attempting to build WorkRequest");
        var request = new WorkRequest
        {
            Topic = topic
        };

        request.ToPayload(payload);



        // *****************************************************************
        return Task.FromResult((true,(WorkRequest?)request));


    }

}

[JsonObject(MemberSerialization.OptIn)]
public class S3Event
{

    [JsonProperty("Records")] 
    public List<S3EventRecord> Records { get; set; } = null!;

}



[JsonObject(MemberSerialization.OptIn)]
public class S3EventRecord
{

    [JsonProperty("eventVersion")]
    public string EventVersion { get; set; } = "";

    [JsonProperty("eventSource")]
    public string EventSource { get; set; } = "";

    [JsonProperty("awsRegion")]
    public string AwsRegion { get; set; } = "";

    [JsonProperty("eventTime")] 
    public string EventTime { get; set; } = "";

    [JsonProperty("eventName")]
    public string EventName { get; set; } = "";

    [JsonProperty("s3")] 
    public S3EventS3 S3 { get; set; } = null!;

}

[JsonObject(MemberSerialization.OptIn)]
public class S3EventS3
{

    [JsonProperty("bucket")] 
    public S3EventBucket Bucket { get; set; } = null!;

    [JsonProperty("object")] 
    public S3EventObject Object { get; set; } = null!;

}

[JsonObject(MemberSerialization.OptIn)]
public class S3EventBucket
{

    [JsonProperty("name")] 
    public string Name { get; set; } = "";

    [JsonProperty("arn")] 
    public string Arn { get; set; } = "";

}


[JsonObject(MemberSerialization.OptIn)]
public class S3EventObject
{

    [JsonProperty("key")]
    public string Key { get; set; } = "";
    [JsonProperty("size")]
    public long Size { get; set; }
    [JsonProperty("eTag")]
    public string ETag { get; set; } = "";
    [JsonProperty("versionId")]
    public string VersionId { get; set; } = "";
    [JsonProperty("sequencer")]
    public string Sequencer { get; set; } = "";

}

