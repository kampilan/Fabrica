using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json.Serialization;

namespace Fabrica.Http
{

    
    public class HttpRequestBuilder
    {


        private HttpRequestBuilder()
        {
        }


        [NotNull]
        public static HttpRequestBuilder Get( string httpClientName="Api" )
        {
            var builder = new HttpRequestBuilder
            {
                HttpClientName = httpClientName,
                Method = HttpMethod.Get
            };

            return builder;
        }

        [NotNull]
        public static HttpRequestBuilder Post(string httpClientName = "Api")
        {
            var builder = new HttpRequestBuilder
            {
                HttpClientName = httpClientName,
                Method         = HttpMethod.Post
            };

            return builder;
        }

        [NotNull]
        public static HttpRequestBuilder Put(string httpClientName = "Api")
        {
            var builder = new HttpRequestBuilder
            {
                HttpClientName = httpClientName,
                Method = HttpMethod.Put
            };

            return builder;
        }

        [NotNull]
        public static HttpRequestBuilder Patch(string httpClientName = "Api")
        {
            var builder = new HttpRequestBuilder
            {
                HttpClientName = httpClientName,
                Method = new HttpMethod("PATCH")
            };

            return builder;
        }


        [NotNull]
        public static HttpRequestBuilder Delete(string httpClientName = "Api")
        {
            var builder = new HttpRequestBuilder
            {
                HttpClientName = httpClientName,
                Method = HttpMethod.Delete
            };

            return builder;
        }


        public static implicit operator string( [NotNull] HttpRequestBuilder builder )
        {

            var path = builder.ToString();

            return path;

        }


        [NotNull]
        public static implicit operator HttpRequest( [NotNull] HttpRequestBuilder builder )
        {

            var request = new HttpRequest
            {
                DebugMode      = builder.DebugMode,
                HttpClientName = builder.HttpClientName,
                Method         = builder.Method,
                Path           = builder.ToString(),
            };


            if (builder.DebugMode)
                request.CustomHeaders["Fabrica-Watch-Debug"] = "1";


            if( string.IsNullOrWhiteSpace(builder.Json) && builder.Body != null )
                request.ToBody(builder.Body, builder.Resolver);
            else if( !string.IsNullOrWhiteSpace(builder.Json) )
                request.ToBody( builder.Json );
            else if( builder.BodyStream != null )
            {

                var content = new MemoryStream();
                builder.BodyStream.CopyTo( content );
                content.Seek(0, SeekOrigin.Begin);

                request.BodyContent = new StreamContent( content );

            }


            return request;

        }


        private HttpMethod Method { get; set; } = HttpMethod.Get;

        private bool DebugMode { get; set; }


        private string HttpClientName { get; set; } = "";

        private bool AtRoot { get; set; }
        private string Path { get; set; } = "";
        private string Uid { get; set; } = "";
        private string SubResource { get; set; } = "";
        private string SubUid { get; set; } = "";
        private List<string> Rql { get; } = new ();

        private List<KeyValuePair<string,object>> QueryParameters { get; set; } = new ();


        private string Json { get; set; }

        private object Body { get; set; }
        private IContractResolver Resolver { get; set; }

        private Stream BodyStream { get; set; }


        public bool AddHost { get; set; } = true;


        [NotNull]
        public HttpRequestBuilder InDebugMode()
        {
            DebugMode = true;
            return this;
        }


        [NotNull]
        public HttpRequestBuilder ForPath( params string[] segments )
        {

            Path = string.Join("/", segments );

            Uid         = "";
            SubResource = "";
            SubUid      = "";

            return this;

        }




        [NotNull]
        public HttpRequestBuilder ForResource( string resource, bool atRoot=false )
        {

            Path   = resource;
            AtRoot = atRoot;

            return this;

        }


        [NotNull]
        public HttpRequestBuilder ForResource( ModelMeta meta, bool atRoot = false )
        {

            Path   = meta.Resource;
            AtRoot = atRoot;

            return this;

        }



        [NotNull]
        public HttpRequestBuilder WithIdentifier( string uid )
        {
            Uid = uid;
            return this;

        }

        [NotNull]
        public HttpRequestBuilder WithSubResource( string sub )
        {
            SubResource = sub;
            return this;

        }


        [NotNull]
        public HttpRequestBuilder WithSubIdentifier(string uid)
        {
            SubUid = uid;
            return this;

        }



        [NotNull]
        public HttpRequestBuilder WithRql( params string[] filters )
        {
            Rql.AddRange( filters );
            return this;
        }

        [NotNull]
        public HttpRequestBuilder WithRql( [NotNull] IEnumerable<string> filters )
        {
            Rql.AddRange(filters);
            return this;
        }


        [NotNull]
        public HttpRequestBuilder WithRql(params IRqlFilter[] filters)
        {
            Rql.AddRange( filters.Select(f => f.ToRqlCriteria()) );
            return this;
        }


        [NotNull]
        public HttpRequestBuilder WithRql( [NotNull] IEnumerable<IRqlFilter> filters )
        {
            Rql.AddRange( filters.Select(f=>f.ToRqlCriteria()) );
            return this;
        }


        [NotNull]
        public HttpRequestBuilder WithRql(params ICriteria[] filters)
        {
            Rql.AddRange( filters.Select(c => RqlFilterBuilder.Create().Introspect(c).ToRqlCriteria()) );
            return this;
        }


        [NotNull]
        public HttpRequestBuilder WithRql( [NotNull] IEnumerable<ICriteria> filters )
        {
            Rql.AddRange( filters.Select( c=> RqlFilterBuilder.Create().Introspect(c).ToRqlCriteria()) );
            return this;
        }


        [NotNull]
        public HttpRequestBuilder AddParameter( string name, object value )
        {
            QueryParameters.Add( new KeyValuePair<string,object>(name, value) );
            return this;
        }

        [NotNull]
        public HttpRequestBuilder WithParameters( IEnumerable<KeyValuePair<string,object>> parameters )
        {
            QueryParameters = new List<KeyValuePair<string,object>>( parameters );
            return this;
        }

        [NotNull]
        public HttpRequestBuilder WithJson( string json )
        {
            Json = json;
            return this;
        }


        [NotNull]
        public HttpRequestBuilder WithPatch(PatchSet set)
        {
            Json = set.ToJson();
            return this;
        }


        [NotNull]
        public HttpRequestBuilder WithBody( object body, [CanBeNull] IContractResolver resolver=null )
        {
            Body = body;
            Resolver = resolver;
            return this;
        }

        [NotNull]
        public HttpRequestBuilder WithStream( Stream body )
        {
            BodyStream  = body;
            return this;
        }




        [NotNull]
        public HttpRequest ToRequest()
        {
            return this;
        }

        public override string ToString()
        {

            var builder = new StringBuilder();


            if ( !string.IsNullOrWhiteSpace(Path) )
            {
                if( AtRoot && !Path.StartsWith("/") )
                    builder.Append("/");
                builder.Append(Path);
            }

            if( !string.IsNullOrWhiteSpace(Uid) )
            {
                builder.Append("/");
                builder.Append(Uid);
            }

            if( !string.IsNullOrWhiteSpace(SubResource) )
            {
                if (!SubResource.StartsWith("/"))
                    builder.Append("/");
                builder.Append(SubResource);
            }

            if( !string.IsNullOrWhiteSpace(SubUid) && !string.IsNullOrWhiteSpace(SubResource) )
            {
                builder.Append("/");
                builder.Append(SubUid);
            }

            if( Rql.Count > 0 && QueryParameters.Count == 0 )
            {
                var join = string.Join( "&", Rql.Select(r => $"rql={r}") );
                builder.Append("?");
                builder.Append( join );
            }

            if( QueryParameters.Count > 0 )
            {

                string MakeValue( object value )
                {
                    switch (value)
                    {
                        case string:
                            return $"'{value}'";
                        case short:
                        case int:
                        case long:
                        case double:
                        case decimal:
                        case bool:
                            return $"{value}";
                        case DateTime dt:
                            return $"{dt:o}";
                        default:
                            return $"'{value}'";
                    }
                }

                builder.Append("?");

                var list  = QueryParameters.Select(p => $"{p.Key}={MakeValue(p.Value)}");
                var query = string.Join("&", list);

                builder.Append(query);

            }


            var path = builder.ToString();


            return path;


        }


    }


}
