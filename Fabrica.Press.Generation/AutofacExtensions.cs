using System.Collections.Generic;
using Autofac;
using Fabrica.Press.Generation;
using Fabrica.Press.Generation.Formatters;
using Fabrica.Press.Generation.Mediator;
using Fabrica.Press.Utilities;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Storage;

namespace Fabrica.Press
{


    public static class AutofacExtensions
    {


        public static ContainerBuilder UsePressGenerator( this ContainerBuilder builder, string root, string basePath)
        {

            builder.Register(c =>
                {

                    var corr    = c.Resolve<ICorrelation>();
                    var storage = c.Resolve<IStorageProvider>();

                    var comp = new TemplateSourceProvider( corr, storage, root, basePath );

                    return comp;

                })
                .As<ITemplateSourceProvider>()
                .SingleInstance();


            builder.RegisterType<GenerateFromKeysHandler>()
                .AsImplementedInterfaces()
                .InstancePerDependency();

            builder.RegisterType<GenerateFromTemplatesHandler>()
                .AsImplementedInterfaces()
                .InstancePerDependency();


            return builder;

        }




        public static ContainerBuilder AddStandardMergeFormatters(this ContainerBuilder builder)
        {

            builder.Register(_ => new DateFormatter { Tag = "D", Format = "MM/dd/yyyy" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(_ => new DateFormatter { Tag = "DL", Format = "MMM dd yyyy" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(_ => new DateFormatter { Tag = "DT", Format = "MM/dd/yyyy hh:mm TT" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(_ => new DateFormatter { Tag = "DM", Format = "MM/dd/yyyy HH:mm" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(_ => new DateFormatter { Tag = "DTL", Format = "MMM dd yyyy hh:mm TT" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(_ => new DateFormatter { Tag = "DML", Format = "MMM dd yyyy HH:mm" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(_ => new DateFormatter { Tag = "TS", Format = "yyyy-MM-dd HH:mm:ss" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register(_ => new CurrencyFormatter { Tag = "CUR" })
                .As<IMergeFieldFormatter>()
                .SingleInstance();


            return builder;

        }



        public static ContainerBuilder AddPaddingFormatters(this ContainerBuilder builder, int minLen = 1, int maxLen = 100)
        {


            for (var i = minLen; i <= maxLen; i++)
            {

                var len = i;
                builder.Register(_ => new PadFormatter($"PL{len}", PadFormatter.AlignmentType.Left, len))
                    .As<IMergeFieldFormatter>()
                    .SingleInstance();

                builder.Register(_ => new PadFormatter($"PR{len}", PadFormatter.AlignmentType.Right, len))
                    .As<IMergeFieldFormatter>()
                    .SingleInstance();

                builder.Register(_ => new PadFormatter($"PC{len}", PadFormatter.AlignmentType.Center, len))
                    .As<IMergeFieldFormatter>()
                    .SingleInstance();

            }


            return builder;


        }



        public static ContainerBuilder AddDocusignAnchorFormatters(this ContainerBuilder builder)
        {


            builder.Register( _ => new DocusignAnchorFormatter("ACHSIG", "[*.{0}.1]") )
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register( _ => new DocusignAnchorFormatter("ACHINT", "[*.{0}.2]") )
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            builder.Register( _ => new DocusignAnchorFormatter("ACHDAT", "[*.{0}.3]") )
                .As<IMergeFieldFormatter>()
                .SingleInstance();


            return builder;

        }



        public static ContainerBuilder AddSignatureFormatters(this ContainerBuilder builder)
        {


            // Full Signature formatter
            builder.Register( _ => new SignatureFormatter(SignatureGenerator.SizeType.Sig))
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            // 90% Signature formatter
            builder.Register( _ => new SignatureFormatter(SignatureGenerator.SizeType.Sig90))
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            // 80% Signature formatter
            builder.Register( _ => new SignatureFormatter(SignatureGenerator.SizeType.Sig80))
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            // 70% Signature formatter
            builder.Register( _ => new SignatureFormatter(SignatureGenerator.SizeType.Sig70))
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            // 60% Signature formatter
            builder.Register( _ => new SignatureFormatter(SignatureGenerator.SizeType.Sig60))
                .As<IMergeFieldFormatter>()
                .SingleInstance();

            // 50% Signature formatter
            builder.Register( _ => new SignatureFormatter(SignatureGenerator.SizeType.Sig50))
                .As<IMergeFieldFormatter>()
                .SingleInstance();


            return builder;

        }


    }


}
