namespace Fabrica.Api.Support.One
{


    public class ApplianceOptions : IApplianceOptions
    {

        public string Environment { get; set; } = "Development";

        public bool AllowAnyIp { get; set; } = false;
        public int ListeningPort { get; set; } = 8080;

        public string MissionName { get; set; } = "";
        public bool RunningAsMission => !string.IsNullOrWhiteSpace(MissionName);

        public bool RequiresAuthentication { get; set; } = true;

    }


}
