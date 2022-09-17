using System.IO.Compression;
using System.Management.Automation;
using System.Security.Cryptography;
using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using GemBox.Document;

namespace Fabrica.One.Builder;

[Cmdlet(VerbsCommon.New, "Appliance")]
public class BuildApplianceCmdLet: Cmdlet
{



    [Parameter(Position = 0, Mandatory = true, HelpMessage = "The name of the appliance being built.")]
    [Alias("N")]
    public string Name { get; set; } = "";

    [Parameter(Position = 1, Mandatory = true, HelpMessage = "The build identifier of the appliance. Used in building a unique name.")]
    [Alias("B")]
    public string Build { get; set; } = "";

    [Parameter(Position = 2, Mandatory = true, HelpMessage = "The path to the contents of the appliance.")]
    [Alias("S")]
    public string Source { get; set; } = "";

    [Parameter(Position = 3, Mandatory = true, HelpMessage = "The AWS region where the appliance will be saved.")]
    [Alias("R")]
    public string Region { get; set; } = "";

    [Parameter(Position = 4, Mandatory = true, HelpMessage = "The AWS bucket that is acting as the appliance repository.")]
    [Alias("T")]
    public string Bucket { get; set; } = "";


    [Parameter(Position = 5, HelpMessage = "AWS Profile. When set to blank an Instance Profile is used.")]
    public string Profile { get; set; } = "fabrica-one-build";

    [Parameter(Position = 6, HelpMessage = "Template used to create appliance name and location in repository.")]
    public string OutputTemplate { get; set; } = "appliances/{0}/{0}-{1}.zip";

    [Parameter(Position = 7, HelpMessage = "Template used to create appliance manifest name in repository.")]
    public string ManifestTemplate { get; set; } = "appliances/{0}/{0}-{1}-manifest.json";

    [Parameter(Position = 8, HelpMessage = "Output directory in appliance package. Most appliance are packaged into the root.")]
    public string TargetDir { get; set; } = "";

    [Parameter(Position = 9, HelpMessage = "In addition to specified build also generate a 'latest' version")]
    public bool GenerateLatest { get; set; } = true;

    [Parameter(Position = 10, HelpMessage = "Entry Assembly Name")]
    public string Assembly { get; set; } = "Appliance";

    [Parameter(Position = 11, HelpMessage = "Gembox License")]
    public string License { get; set; } = "";

    [Parameter(Position = 12, HelpMessage = "Path to documentation template (docx)")]
    public string DocumentSource { get; set; } = "";

    [Parameter(Position = 13, HelpMessage = "Template used to build appliance configuration documentation output (pdf) name.")]
    public string PdfTemplate { get; set; } = "appliances/{0}/{0}-{1}-documentation.pdf";



    protected override void ProcessRecord()
    {

        base.ProcessRecord();

        var task = Task.Run(async () => await ProcessRecordAsync());
        task.Wait();

    }


    private async Task ProcessRecordAsync()
    {


        // ********************************************************************************************
        var package = "";
        try
        {
            package = _package(Source);
        }
        catch (Exception cause)
        {
            var rec = new ErrorRecord(cause, "Appliance Packaging", ErrorCategory.WriteError, null);
            ThrowTerminatingError(rec);
        }



        // ********************************************************************************************
        try
        {

            await _store(package);

        }
        catch (Exception cause)
        {
            var rec = new ErrorRecord(cause, "Appliance Store", ErrorCategory.ConnectionError, null);
            ThrowTerminatingError(rec);
        }
        finally
        {
            if (File.Exists(package))
                File.Delete(package);
        }


    }


    private string _package( string source )
    {


        var path = Path.GetTempPath();
        var temp = $"{path}{Path.DirectorySeparatorChar}{Path.GetRandomFileName()}";

        var currentFile = "";
        try
        {
            ZipFile.CreateFromDirectory(source, temp, CompressionLevel.Fastest, false);
        }
        catch (Exception cause)
        {

            if( File.Exists(temp) )
                File.Delete(temp);

            throw new Exception( $"Failed to package file: {currentFile} Cause: {cause.Message}", cause  );

        }


        return temp;


    }


    private async Task _store( string package )
    {

        var key = "Not Set";

        try
        {


            var fullBuildNum = Build.PadLeft(5, '0');

            var endpoint = RegionEndpoint.GetBySystemName(Region);


            AWSCredentials credentials;
            if( string.IsNullOrWhiteSpace(Profile) )
                credentials = new InstanceProfileAWSCredentials();
            else
            {

                var sharedFile = new SharedCredentialsFile();
                if (!(sharedFile.TryGetProfile( Profile, out var profile) && AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out credentials)))
                    throw new Exception($"Local profile {profile} could not be loaded");

            }


            using var client = new AmazonS3Client(credentials, endpoint);

            key          = string.Format( OutputTemplate, Name.ToLowerInvariant(), fullBuildNum );
            var manifest = string.Format( ManifestTemplate, Name.ToLowerInvariant(), fullBuildNum );

            await _uploadToS3( client, key, manifest, package, fullBuildNum, "" );


            if( GenerateLatest )
            {
                key = string.Format(OutputTemplate, Name.ToLowerInvariant(), "latest");
                manifest = string.Format(ManifestTemplate, Name.ToLowerInvariant(), "latest");

                await _uploadToS3(client, key, manifest, package, "latest", "");
            }


        }
        catch (Exception cause)
        {
            throw new Exception($"Failed to store appliance in S3: Key: {key} Cause: {cause.Message}", cause);
        }


    }


    private async Task _uploadToS3( IAmazonS3 client, string key, string manifest, string package, string build, string documentation )
    {

            
        await using( var content = new FileStream(package, FileMode.Open, FileAccess.Read) )
        {

            var request = new PutObjectRequest
            {
                BucketName                 = Bucket,
                Key                        = key,
                InputStream                = content,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
            };

            await client.PutObjectAsync(request);

        }


        await using( var content = new FileStream(package, FileMode.Open, FileAccess.Read) )
        {

            var sha = SHA256.Create();
            sha.Initialize();

            var bytes = await sha.ComputeHashAsync(content);
            var hashHex = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();

            var bm = new BuildModel
            {
                Name      = Name,
                BuildNum  = build,
                BuildDate = DateTime.UtcNow,
                BuildSize = content.Length,
                Checksum  = hashHex,
                Assembly  = Assembly
            };

            var json = JsonSerializer.Serialize( bm, new JsonSerializerOptions {WriteIndented = true} );


            await using (var manifestStrm = new MemoryStream())
            await using (var writer = new StreamWriter(manifestStrm))
            {

                await writer.WriteAsync(json);
                await writer.FlushAsync();

                manifestStrm.Seek(0, SeekOrigin.Begin);

                var hashReq = new PutObjectRequest
                {
                    BucketName                 = Bucket,
                    Key                        = manifest,
                    InputStream                = manifestStrm,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                };

                await client.PutObjectAsync(hashReq);

            }




            await using (var docoStrm = new MemoryStream())
            await using (var writer = new StreamWriter(docoStrm))
            {

                ComponentInfo.SetLicense(License);

                var doc = DocumentModel.Load(DocumentSource);

                doc.MailMerge.Execute(bm);

                doc.Save(docoStrm,SaveOptions.PdfDefault);

                docoStrm.Seek(0, SeekOrigin.Begin);

                var hashReq = new PutObjectRequest
                {
                    BucketName = Bucket,
                    Key = documentation,
                    InputStream = docoStrm,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                };

                await client.PutObjectAsync(hashReq);

            }








        }


    }

}

public class BuildModel
{

    public string Name { get; set; } = "";

    public string BuildNum { get; set; } = "";

    public DateTime BuildDate { get; set; } = DateTime.MinValue;

    public long BuildSize { get; set; }

    public string Checksum { get; set; } = "";

    public string Assembly { get; set; } = "";


}