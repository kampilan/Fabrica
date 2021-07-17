using Autofac;
using Fabrica.Utilities.Signature;

namespace Fabrica.Press.Generation.Formatters
{


    public static class AutofacExtensions
    {


        public static ContainerBuilder AddStandardMergeFormatters(this ContainerBuilder builder)
        {

            builder.Register( c => new DateFormatter {Tag = "D", Format = "MM/dd/yyyy"} )
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(c => new DateFormatter { Tag = "DL", Format = "MMM dd yyyy" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(c => new DateFormatter { Tag = "DT", Format = "MM/dd/yyyy hh:mm TT" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(c => new DateFormatter { Tag = "DM", Format = "MM/dd/yyyy HH:mm" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(c => new DateFormatter { Tag = "DTL", Format = "MMM dd yyyy hh:mm TT" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(c => new DateFormatter { Tag = "DML", Format = "MMM dd yyyy HH:mm" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(c => new DateFormatter { Tag = "TS", Format = "yyyy-MM-dd HH:mm:ss" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(c => new CurrencyFormatter { Tag = "CUR" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();


            return builder;

        }


        public static ContainerBuilder AddPaddingFormatters( this ContainerBuilder builder, int minLen=1, int maxLen=100 )
        {


            for( var i=minLen; i<=maxLen; i++ )
            {

                var len = i;
                builder.Register(c => new PadFormatter($"PL{len}", PadFormatter.AlignmentType.Left, len) )
                    .As<IMergeFieldFormatter>()
                    .SingleInstance();

                builder.Register(c => new PadFormatter($"PR{len}", PadFormatter.AlignmentType.Right, len) )
                    .As<IMergeFieldFormatter>()
                    .SingleInstance();

                builder.Register(c => new PadFormatter( $"PC{len}", PadFormatter.AlignmentType.Center, len) )
                    .As<IMergeFieldFormatter>()
                    .SingleInstance();

            }


            return builder;


        }


    }


}
