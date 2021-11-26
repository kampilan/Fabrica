namespace Fabrica.Api.Support.One
{

    
    public interface IApplianceOptions
    {

        string Environment { get;  }

        public bool AllowAnyIp { get; }
        int ListeningPort { get; }

        string MissionName { get;  }
        bool RunningAsMission { get; }

        bool RequiresAuthentication { get; }

    }

}