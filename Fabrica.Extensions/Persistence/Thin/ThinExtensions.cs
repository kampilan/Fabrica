using System.Data.Common;
using System.Text.Json;
using Fabrica.Watch;

namespace Fabrica.Persistence.Thin
{

    public static class ThinExtensions
    {


        public static async Task<string> ToJson( this DbDataReader reader, ISet<string>? exclusions=null)
        {

            await using var stream = new MemoryStream();
            
            await ToJson( reader, stream, exclusions );

            stream.Seek(0, SeekOrigin.Begin);
            using var sr = new StreamReader(stream);

            return await sr.ReadToEndAsync();

        }


        public static async Task ToJson(  this DbDataReader reader, Stream output, ISet<string>? exclusions = null)
        {

            var type = typeof(ThinExtensions);

            var logger = WatchFactoryLocator.Factory.GetLogger(type);

            try
            {

                logger.EnterScope( $"{type.FullName}.{nameof(ToJson)}" );


                if( exclusions == null )
                    exclusions = new HashSet<string>();



                // *****************************************************************
                logger.Debug("Attempting to build property names list");
                var names = new List<string>();
                for( var i = 0; i < reader.FieldCount; i++ )
                {
                    var name = reader.GetName(i);
                    if( !exclusions.Contains(name) )
                        names.Add( name );
                }

                logger.LogObject( nameof(names), names );



                // *****************************************************************
                logger.Debug("Attempting to serialize each row into json");
                var jw = new Utf8JsonWriter(output);


                jw.WriteStartArray();

                while( await reader.ReadAsync() )
                {

                    jw.WriteStartObject();
                  
                    foreach (var col in names)
                    {
                        jw.WritePropertyName(col);

                        switch (reader[col])
                        {
                            case string v:
                                jw.WriteStringValue(v);
                                break;
                            case int v:
                                jw.WriteNumberValue(v);
                                break;
                            case long v:
                                jw.WriteNumberValue(v);
                                break;
                            case short v:
                                jw.WriteNumberValue(v);
                                break;
                            case float v:
                                jw.WriteNumberValue(v);
                                break;
                            case double v:
                                jw.WriteNumberValue(v);
                                break;
                            case decimal v:
                                jw.WriteNumberValue(v);
                                break;
                            case bool v:
                                jw.WriteBooleanValue(v);
                                break;
                            case DateTime v:
                                jw.WriteStringValue(v);
                                break;
                        }

                    }

                    jw.WriteEndObject();

                }

                jw.WriteEndArray();



                // *****************************************************************
                logger.Debug("Attempting to flush JSON writer");
                await jw.FlushAsync();


            }
            finally
            {
                logger.LeaveScope( $"{type.FullName}.{nameof(ToJson)}" );
            }


        }


    }

}
