using Fabrica.Watch;

namespace Fabrica.Utilities.Storage;

public class FileSystemStorageProvider: ILocalStorageProvider
{


    public DirectoryInfo BaseDirectory { get; set; } = new DirectoryInfo( "c:/" );

    private FileInfo BuildFile( string root, string key )
    {
        var path = $"{BaseDirectory.FullName}{Path.DirectorySeparatorChar}{root}{Path.DirectorySeparatorChar}{key}";
        var info = new FileInfo(path);
        return info;
    }

    public bool Exists(string root, string key)
    {

        var logger = this.GetLogger();

        try
        {

            logger.EnterMethod();

            var info = BuildFile(root, key);

            return info.Exists;

        }
        finally
        {
            logger.LeaveMethod();
        }

    }

    public Task<bool> ExistsAsync(string root, string key)
    {

        var logger = this.GetLogger();

        try
        {

            logger.EnterMethod();


            var info = BuildFile(root, key);

            return Task.FromResult(info.Exists);

        }
        finally
        {
            logger.LeaveMethod();
        }

    }

    public void Get(string root, string key, Stream content, bool rewind = true)
    {

        var logger = this.GetLogger();

        try
        {

            logger.EnterMethod();

            var info = BuildFile(root, key);

            using( var fs = new FileStream(info.ToString(), FileMode.Open, FileAccess.Read) )
            {
                fs.CopyTo(content);
                if( rewind )
                    content.Seek(0, SeekOrigin.Begin);
            }


        }
        finally
        {
            logger.LeaveMethod();
        }


    }

    public async Task GetAsync(string root, string key, Stream content, bool rewind = true)
    {

        var logger = this.GetLogger();

        try
        {

            logger.EnterMethod();

            var info = BuildFile(root, key);

            using (var fs = new FileStream(info.ToString(), FileMode.Open, FileAccess.Read))
            {
                await fs.CopyToAsync(content);
                if (rewind)
                    content.Seek(0, SeekOrigin.Begin);
            }


        }
        finally
        {
            logger.LeaveMethod();
        }

    }

    public void Put(string root, string key, Stream content, string contentType = "", bool rewind = true, bool autoClose = false)
    {

        var logger = this.GetLogger();

        try
        {

            logger.EnterMethod();

            var info = BuildFile(root, key);

            using( var fs = new FileStream(info.ToString(), FileMode.Create, FileAccess.Write) )
            {

                if (rewind)
                    content.Seek(0, SeekOrigin.Begin);

                content.CopyTo(fs);

            }

            if( autoClose )
                content.Close();

        }
        finally
        {
            logger.LeaveMethod();
        }


    }

    public async Task PutAsync(string root, string key, Stream content, string contentType = "", bool rewind = true, bool autoClose = false)
    {

        var logger = this.GetLogger();

        try
        {

            logger.EnterMethod();

            var info = BuildFile(root, key);

            using (var fs = new FileStream(info.ToString(), FileMode.Create, FileAccess.Write))
            {

                if (rewind)
                    content.Seek(0, SeekOrigin.Begin);

                await content.CopyToAsync(fs);

            }

            if( autoClose )
                content.Close();

        }
        finally
        {
            logger.LeaveMethod();
        }


    }

    public void Delete(string root, string key)
    {

        var logger = this.GetLogger();

        try
        {

            logger.EnterMethod();

            var info = BuildFile(root, key);

            info.Delete();

        }
        finally
        {
            logger.LeaveMethod();
        }


    }

    public Task DeleteAsync(string root, string key)
    {

        var logger = this.GetLogger();

        try
        {

            logger.EnterMethod();

            var info = BuildFile(root, key);

            info.Delete();

            return Task.CompletedTask;

        }
        finally
        {
            logger.LeaveMethod();
        }


    }

    public string GetReference(string root, string key, TimeSpan timeToLive)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetReferenceAsync(string root, string key, TimeSpan timeToLive)
    {
        throw new NotImplementedException();
    }


}