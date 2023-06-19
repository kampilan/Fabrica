using Autofac;
using Fabrica.One.Plan;
using Fabrica.Watch;

namespace Fabrica.One;

public class MissionObserver
{


    public MissionObserver(ILifetimeScope rootScope)
    {
        RootScope = rootScope;
    }


    public string MissionStatusDir { get; set; } = "";

    public async Task UpdateMissionStatus()
    {


        if (CurrentMission == null || string.IsNullOrWhiteSpace(MissionStatusDir))
            return;


        var file = $"{MissionStatusDir}{Path.DirectorySeparatorChar}mission-status.json";
        var json = CurrentMission.GetStatusAsJson();


        await using (var fo = new FileStream(file, FileMode.Create, FileAccess.Write))
        await using (var writer = new StreamWriter(fo))
        {
            await writer.WriteAsync(json);
            await writer.FlushAsync();
        }



    }

    private ILifetimeScope RootScope { get; }
    private ILifetimeScope? CurrentScope { get; set; }

    private Mission? CurrentMission { get; set; }

    public void Start()
    {

        using var logger = this.EnterMethod();


        if (CurrentMission != null)
            return;


        try
        {

            CurrentScope = RootScope.BeginLifetimeScope();

            CurrentMission = CurrentScope.Resolve<Mission>();

        }
        catch (Exception cause)
        {
            logger.Error(cause, "Mission acquisition failed.");
            throw;
        }



        var result = CurrentMission.Run();

        if (!result.Successful)
        {
            var ev = logger.CreateEvent(Level.Error, "MissionObserver Start failed.", result);
            logger.LogEvent(ev);
            throw new Exception("Mission start failed.");
        }

    }

    private DateTime LastStatusUpdate { get; set; } = DateTime.Now.AddSeconds(-20);
    public async Task Check()
    {


        if (CurrentScope is null)
            return;


        var source = CurrentScope.Resolve<IPlanSource>();

        var updated = await source.HasUpdatedPlan();


        if (updated)
        {

            using (var logger = this.GetLogger())
            {
                logger.Debug("Attempting to Stop and Start after update detected");

                Stop();
                Start();

                logger.Debug("Update completed");

            }

        }

        if (LastStatusUpdate + TimeSpan.FromSeconds(10) < DateTime.Now)
        {
            await UpdateMissionStatus();
            LastStatusUpdate = DateTime.Now;
        }


    }


    public async Task Reload()
    {

        using var logger = this.EnterMethod();




        var source = CurrentScope?.Resolve<IPlanSource>();

        if (source is not null)
        {
            logger.Debug("Attempting to request Reload");
            await source.Reload();
        }

    }


    public void Stop()
    {

        using var logger = this.EnterMethod();


        CurrentMission?.Terminate();
        CurrentMission = null;

        CurrentScope?.Dispose();
        CurrentScope = null;

    }




}