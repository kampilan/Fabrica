using Autofac;

namespace Fabrica.Utilities.Storage;

public static  class AutofacExtensions
{


    public static ContainerBuilder AddFileSystemStorage( this ContainerBuilder builder, string directory )
    {

        builder.Register( _ =>
            {

                var provider = new FileSystemStorageProvider
                {
                    BaseDirectory = new DirectoryInfo(directory)
                };

                return provider;

            })
            .As<IStorageProvider>()
            .As<ILocalStorageProvider>()
            .SingleInstance();


        return builder;

    }


}