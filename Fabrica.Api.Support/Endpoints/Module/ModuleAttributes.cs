namespace Fabrica.Api.Support.Endpoints.Module;

[AttributeUsage(AttributeTargets.Class)]
public class ModulePolicyAttribute: Attribute
{

    public ModulePolicyAttribute(params string[] policyNames )
    {
        _policyNames = new List<string>(policyNames);
    }

    private readonly List<string> _policyNames;

    public IEnumerable<string> PolicyNames => _policyNames;

}


[AttributeUsage(AttributeTargets.Class)]
public class ModulePublicPolicyAttribute : ModulePolicyAttribute
{

    public ModulePublicPolicyAttribute(): base(ModuleConstants.PublicPolicyName)
    {
    }

}

[AttributeUsage(AttributeTargets.Class)]
public class ModuleAdminPolicyAttribute : ModulePolicyAttribute
{

    public ModuleAdminPolicyAttribute() : base(ModuleConstants.AdminPolicyName)
    {
    }

}



[AttributeUsage(AttributeTargets.Class)]
public class ModuleRouteAttribute : Attribute
{

    public string Prefix { get; set; } = "";
    public string Resource { get; set; } = "";
    public string Member { get; set; } = "";

    public string Path => $"{Prefix}/{Resource}";

}