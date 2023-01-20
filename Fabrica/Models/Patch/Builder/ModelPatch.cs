using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Fabrica.Models.Support;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fabrica.Models.Patch.Builder;

public class ModelPatch
{

    [DefaultValue(PatchVerb.Update)]
    [JsonConverter(typeof(StringEnumConverter))]
    public PatchVerb Verb { get; set; } = PatchVerb.Update;
    public string Model { get; set; } = "";
    public string Uid { get; set; } = "";


    [JsonIgnore]
    public bool IsMember => Membership != null;

    [DefaultValue(null)]
    public PropertyPath? Membership { get; set; }
    public bool ShouldSerializeMembership() => Membership != null;


    public IDictionary<string,object> Properties { get; set; } = new Dictionary<string,object>();
    public bool ShouldSerializeProperties() => Properties.Count > 0;



}


public class ModelPatch<TModel> : ModelPatch where TModel: class
{

    public ModelPatch( string uid, PatchVerb verb=PatchVerb.Update )
    {

        var attr = typeof(TModel).GetCustomAttribute<ModelAttribute>();
        if (attr != null)
            Model = attr.Alias;

        if( string.IsNullOrWhiteSpace(Model))
            Model = typeof(TModel).Name;

        Uid  = uid;
        Verb = verb;

    }


    public ModelPatch<TModel> Set<TProp>( Expression<Func<TModel,TProp>> prop, TProp value )
    {

        if( prop.Body is MemberExpression {NodeType: ExpressionType.MemberAccess} me ) 
        {
            Properties.Add( me.Member.Name, value! );
        }

        return this;

    }

}