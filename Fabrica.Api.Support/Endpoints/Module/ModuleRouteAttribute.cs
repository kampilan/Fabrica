namespace Fabrica.Api.Support.Endpoints.Module;

[AttributeUsage(AttributeTargets.Class)]
public class ModuleRouteAttribute : Attribute
{

    public string Prefix { get; set; } = "";
    public string Resource { get; set; } = "";
    public string Member { get; set; } = "";

    public string Path => $"{Prefix}/{Resource}";

}