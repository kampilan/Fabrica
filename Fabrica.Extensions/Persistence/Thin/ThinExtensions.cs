using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Fabrica.Watch;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Fabrica.Persistence.Thin
{

    public static class ThinExtensions
    {


        public static async Task<string> ToJson([NotNull] this DbDataReader reader, ISet<string> exclusions=null)
        {

            using (var writer = new StringWriter())
            {
                await ToJson( reader, writer, exclusions );

                return writer.ToString();
            }

        }


        public static async Task ToJson([NotNull] this DbDataReader reader, Stream output, ISet<string> exclusions = null)
        {

            var writer = new StreamWriter(output);
            await ToJson( reader, writer, exclusions );

            await writer.FlushAsync();

        }


        public static async Task ToJson( [NotNull] this DbDataReader reader, TextWriter writer, ISet<string> exclusions = null)
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
                var jw = new JsonTextWriter(writer)
                {
                    DateFormatHandling   = DateFormatHandling.IsoDateFormat,
                    Formatting           = Formatting.None,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                };


                await jw.WriteStartArrayAsync();

                while( await reader.ReadAsync() )
                {

                    await jw.WriteStartObjectAsync();
                  
                    foreach (var col in names)
                    {
                        await jw.WritePropertyNameAsync(col);
                        await jw.WriteValueAsync(reader[col]);
                    }

                    await jw.WriteEndObjectAsync();

                }

                await jw.WriteEndArrayAsync();



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
