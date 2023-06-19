using Fabrica.Utilities.Container;
using Fabrica.Watch;

namespace Fabrica.One.Plan;

public class FilePlanSource : AbstractPlanSource, IRequiresStart, IDisposable, IPlanSource
{


    public string FileDir { get; set; } = "";
    public string FileName { get; set; } = "";


    private FileSystemWatcher? Watcher { get; set; }
    private string FilePath => $"{FileDir}{Path.DirectorySeparatorChar}{FileName}";
    private bool IsUpdated { get; set; }

    public Task Start()
    {

        using var logger = this.EnterMethod();



        Watcher = new FileSystemWatcher(FileDir, FileName)
        {
            NotifyFilter = NotifyFilters.LastWrite
        };

        Watcher.Changed += OnChanged;

        Watcher.EnableRaisingEvents = true;


        IsUpdated = true;


        return Task.CompletedTask;


    }

    private void OnChanged(object sender, FileSystemEventArgs args)
    {

        using var logger = this.EnterMethod();



        logger.LogObject(nameof(args), args);

        if (args.ChangeType == WatcherChangeTypes.Changed)
            IsUpdated = true;

    }


    public void Dispose()
    {
        Watcher?.Dispose();
        IsUpdated = false;
    }


    private bool _isEmpty()
    {

        using var logger = this.EnterMethod();


        logger.Inspect(nameof(FilePath), FilePath);


        // *****************************************************************
        logger.Debug("Attempting to build FileInfo");
        var fi = new FileInfo(FilePath);

        logger.Inspect(nameof(fi.Exists), fi.Exists);


        // *****************************************************************
        return !fi.Exists;

    }

    public async Task<Stream> GetSource()
    {

        using var logger = this.EnterMethod();




        logger.Inspect(nameof(FilePath), FilePath);
        logger.Inspect(nameof(_isEmpty), _isEmpty());


        if (_isEmpty())
        {
            IsUpdated = false;
            return new MemoryStream();
        }



        // *****************************************************************
        logger.Debug("Attempting to load plan file into stream");
        var stream = new MemoryStream();
        using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
        {
            await fs.CopyToAsync(stream);
        }

        stream.Seek(0, SeekOrigin.Begin);


        IsUpdated = false;


        // *****************************************************************
        return stream;


    }


    public void Touch(DateTime moment = default)
    {

        if (moment == default)
            moment = DateTime.Now;

        File.SetLastWriteTime(FilePath, moment);

    }


    public Task Reload()
    {
        Touch();
        return Task.CompletedTask;
    }


    protected override Task<bool> CheckForUpdate()
    {

        return Task.FromResult(IsUpdated);

    }


}