/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fabrica.Watch.Sink;
using MongoDB.Bson;
using MongoDB.Driver;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedMember.Local

namespace Fabrica.Watch.Mongo.Sink
{

    public class MongoEventSink: IEventSink
    {

        // ReSharper disable once ClassNeverInstantiated.Local
        private class DomainModel
        {


            public ObjectId Id { get; set; }

            public string Uid { get; set; }

            public string Name { get; set; } = "";
            public string Description { get; set; } = "";


            public string ServerUri { get; set; } = "";
            public string Database { get; set; } = "";
            public string Collection { get; set; } = "";

        }


        public string ServerUri { get; set; } = "";
        public MongoEventSink WithServerUri( [JetBrains.Annotations.NotNull] string uri )
        {

            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));

            ServerUri = uri;
            return this;
        }

        public string DomainName { get; set; } = "";
        public MongoEventSink WithDomainName( [JetBrains.Annotations.NotNull] string domainName )
        {

            if (string.IsNullOrWhiteSpace(domainName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(domainName));

            DomainName = domainName;
            return this;
        }


        public TimeSpan DebugTimeToLive { get; set; } = TimeSpan.FromHours(4);
        public TimeSpan NonDebugTimeToLive { get; set; } = TimeSpan.FromDays(7);


        public void Start()
        {


            // ***************************************************
            var wc   = new MongoClient( ServerUri );
            var wdb  = wc.GetDatabase("fabrica_watch");
            var wcol = wdb.GetCollection<DomainModel>("domains");



            // ***************************************************
            var cursor = wcol.Find(d => d.Name == DomainName);
            var domain = cursor.FirstOrDefault();

            if( domain == null )
                throw new Exception( $"Could not find Domain using name: {DomainName}" );



            // ***************************************************
            var client    = new MongoClient( domain.ServerUri );
            var database  = client.GetDatabase( domain.Database );

            Collection = database.GetCollection<BsonDocument>(domain.Collection);

            var models = BuildIndexModels();
            if( models.Count > 0 )
                Collection.Indexes.CreateMany(models);

        }


        protected virtual List<CreateIndexModel<BsonDocument>> BuildIndexModels()
        {

            var list    = new List<CreateIndexModel<BsonDocument>>();
            var builder = new IndexKeysDefinitionBuilder<BsonDocument>();

            list.Add(new CreateIndexModel<BsonDocument>(builder.Ascending(nameof(MongoLogEntity.TimeToLive)), new CreateIndexOptions
            {
                Name        = "Cleanup",
                ExpireAfter = TimeSpan.Zero
            }));

            list.Add(new CreateIndexModel<BsonDocument>(builder.Ascending(nameof(MongoLogEntity.Occurred)), new CreateIndexOptions
            {
                Name = "ByOccurred"
            }));

            list.Add(new CreateIndexModel<BsonDocument>(builder.Ascending(nameof(MongoLogEntity.CorrelationId)), new CreateIndexOptions
            {
                Name = "ByCorrelation"
            }));

            return list;

        }


        private IMongoCollection<BsonDocument> Collection { get; set; }
        private MemoryStream Stream { get; } = new();


        private ConsoleEventSink DebugSink { get; } = new();

        public async Task Accept( [JetBrains.Annotations.NotNull] ILogEvent logEvent )
        {

            try
            {

                var document = _buildDocument(logEvent);

                await Collection.InsertOneAsync( document );

            }
            catch (Exception cause)
            {
                var le = new LogEvent
                {
                    Category = GetType().FullName,
                    Level    = Level.Debug,
                    Title    = cause.Message,
                    Payload  = cause.StackTrace
                };

                await DebugSink.Accept( le );

            }


        }

        public async Task Accept( [JetBrains.Annotations.NotNull] IEnumerable<ILogEvent> batch )
        {

            try
            {

                var list = batch.Select(_buildDocument).ToList();

                await Collection.InsertManyAsync(list);

            }
            catch (Exception cause)
            {

                var le = new LogEvent
                {
                    Category = GetType().FullName,
                    Level    = Level.Debug,
                    Title    = cause.Message,
                    Payload  = cause.StackTrace
                };

                await DebugSink.Accept( le );

            }

        }

        public void Stop()
        {
        }


        private BsonDocument _buildDocument( [JetBrains.Annotations.NotNull] ILogEvent logEvent )
        {


            var timeToLive = NonDebugTimeToLive;
            if (logEvent.Level == Level.Debug || logEvent.Level == Level.Trace)
                timeToLive = DebugTimeToLive;


            var entity = new MongoLogEntity
            {
                Category      = logEvent.Category,
                CorrelationId = logEvent.CorrelationId,

                Title         = logEvent.Title,

                Tenant        = logEvent.Tenant,
                Subject       = logEvent.Subject,
                Tag           = logEvent.Tag,

                Level         = (int)logEvent.Level,
                Color         = logEvent.Color,
                Nesting       = logEvent.Nesting,

                Type          = (int)logEvent.Type,
                Payload       = logEvent.Payload,
                
                Occurred      = logEvent.Occurred,
                TimeToLive    = (logEvent.Occurred + timeToLive),
            
            };


            var document = entity.ToBsonDocument();


            return document;

        }


    }

}
