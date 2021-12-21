using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Extensions.Caching;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Secrets;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Fabrica.Aws.Secrets;


public class AwsSecretComponent: CorrelatedObject, ISecretComponent
{


    public AwsSecretComponent(ICorrelation correlation,IAmazonSecretsManager manager): base(correlation)
    {

        Cache = new SecretsManagerCache(manager);

    }

    private SecretsManagerCache Cache { get; }


    public string SecretId { get; set; } = "";
   
    
    public async Task<string> GetSecret([NotNull] string key)
    {

        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

        using var logger = EnterMethod();

        logger.Inspect(nameof(key), key);



        // *****************************************************************
        logger.Debug("Attempting to get AWS Secret json from Cache");
        var json = await Cache.GetSecretString( SecretId );
        if( string.IsNullOrWhiteSpace(json))
            throw new InvalidOperationException("AWS Secrets Manager produced a blank or null json string");

        logger.Inspect(nameof(json.Length), json.Length);



        // *****************************************************************
        logger.Debug("Attempting to parse json into dictionary");
        var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>( json );
        if (dict is null)
            throw new InvalidOperationException("AWS Secrets Manager produced an unparsable json string");



        // *****************************************************************
        logger.Debug("Attempting to get key from dictionary");
        if (dict.TryGetValue(key, out var secret))
            return secret;


        // *****************************************************************
        throw new InvalidOperationException($" Key: ({key}) not found in AWS Secrets");


    }



    public async Task<T> GetSecrets<T>() where T: class
    {


        using var logger = EnterMethod();

        logger.Inspect("Type Name", typeof(T).FullName);



        // *****************************************************************
        logger.Debug("Attempting to get AWS Secret json from Cache");
        var json = await Cache.GetSecretString(SecretId);
        if( string.IsNullOrWhiteSpace(json) )
            throw new InvalidOperationException("AWS Secrets Manager produced a blank or null json string");

        logger.Inspect(nameof(json.Length), json.Length);



        // *****************************************************************
        logger.Debug("Attempting to parse json into dictionary");
        var obj = JsonConvert.DeserializeObject<T>(json);
        if( obj is null )
            throw new InvalidOperationException("AWS Secrets Manager produced an unparsable json string");

        return obj;

    }


}
