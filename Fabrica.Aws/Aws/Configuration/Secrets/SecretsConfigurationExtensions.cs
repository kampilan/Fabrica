using System;
using Microsoft.Extensions.Configuration;

namespace Fabrica.Aws.Configuration.Secrets;

public static class SecretsConfigurationExtensions
{


    public static IConfigurationBuilder AddAwsSecrets(this IConfigurationBuilder builder, Action<SecretsConfigurationSource> config)
    {

        builder.Add(config);

        return builder;

    }



}