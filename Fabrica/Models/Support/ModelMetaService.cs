using System.Collections.ObjectModel;
using System.Reflection;
using Fabrica.Models.Serialization;
using Fabrica.Utilities.Container;
using Humanizer;

// ReSharper disable CollectionNeverUpdated.Local

namespace Fabrica.Models.Support;

public class ModelMetaService: IModelMetaService, IRequiresStart
{

    public ModelMetaService(IEnumerable<ModelMetaSource> sources)
    {
        Sources = sources;
    }

    private IEnumerable<ModelMetaSource> Sources { get; }

    private static ISet<string> Empty { get; } = new HashSet<string>();


    private IReadOnlyDictionary<string, ModelMeta> AliasMap { get; set; } = null!;
    private IReadOnlyDictionary<string,ModelMeta> ResourceMap { get; set; } = null!;
    private IReadOnlyDictionary<Type,ModelMeta> TypeMap { get; set; } = null!;
    public Task Start()
    {


        if (AliasMap != null)
            return Task.CompletedTask;

            
        var amap = new Dictionary<string,ModelMeta>();
        var rmap = new Dictionary<string, ModelMeta>();
        var tmap = new Dictionary<Type,ModelMeta>();

        foreach( var target in Sources.SelectMany(s=>s.GetTypes()) )
        {

            var attr = target.GetCustomAttribute<ModelAttribute>();

            var aliasKey = (attr == null || string.IsNullOrWhiteSpace(attr.Alias) ? target.Name : attr.Alias).ToLowerInvariant();
            if (amap.TryGetValue(aliasKey, out var adup))
                throw new InvalidOperationException($" Attempting to add an Alias ({aliasKey}) for Type ({target.FullName}) when Type ({adup.Target.FullName}) is already using it.");


            var projection = target.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                {
                    var ia = p.GetCustomAttribute<ModelMetaAttribute>();
                    return ia == null || ia.Scope is PropertyScope.Immutable or PropertyScope.Mutable;
                })
                .Select(p => p.Name);

            var exclusions = target.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                {
                    var ia = p.GetCustomAttribute<ModelMetaAttribute>();
                    return ia != null && ia.Scope == PropertyScope.Exclude;
                })
                .Select(p => p.Name);

            var creatables = target.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                {
                    var ia = p.GetCustomAttribute<ModelMetaAttribute>();
                    return ia != null && ia.Scope == PropertyScope.Immutable;
                })
                .Select(p => p.Name);

            var updatables = target.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                {
                    var ia = p.GetCustomAttribute<ModelMetaAttribute>();
                    return ia == null || ia.Scope == PropertyScope.Mutable;
                })
                .Select(p => p.Name);




            if ( typeof(IRootModel).IsAssignableFrom(target))
                ForRoots();
            else if(typeof(IReferenceModel).IsAssignableFrom(target))
                ForReferences();
            else if (typeof(IAggregateModel).IsAssignableFrom(target))
                ForAggregates();


            void ForRoots()
            {

                var resourceKey = (attr == null || string.IsNullOrWhiteSpace(attr.Resource) ? target.Name.Pluralize() : attr.Resource).ToLowerInvariant();
                if (rmap.TryGetValue(resourceKey, out var rdup))
                    throw new InvalidOperationException($" Attempting to add a Resource ({resourceKey}) for Type ({target.FullName}) when Type ({rdup.Target.FullName}) is already using it.");


                var meta = new ModelMeta(aliasKey, resourceKey, target, projection, exclusions, creatables, updatables );

                amap[aliasKey]    = meta;
                rmap[resourceKey] = meta;
                tmap[target]      = meta;

            }

            void ForReferences()
            {

                var resourceKey = (attr == null || string.IsNullOrWhiteSpace(attr.Resource) ? target.Name.Pluralize() : attr.Resource).ToLowerInvariant();
                var meta = new ModelMeta(aliasKey, resourceKey, target, projection, exclusions, creatables, updatables);

                amap[aliasKey] = meta;
                tmap[target]   = meta;

            }

            void ForAggregates()
            {

                var resourceKey = (attr == null || string.IsNullOrWhiteSpace(attr.Resource) ? target.Name.Pluralize() : attr.Resource).ToLowerInvariant();
                if (rmap.TryGetValue(resourceKey, out var rdup))
                    throw new InvalidOperationException($" Attempting to add a Resource ({resourceKey}) for Type ({target.FullName}) when Type ({rdup.Target.FullName}) is already using it.");

                var meta = new ModelMeta( aliasKey, resourceKey, target, projection, exclusions, creatables, updatables );

                amap[aliasKey]    = meta;
                rmap[resourceKey] = meta;
                tmap[target]      = meta;

            }


        }

        AliasMap    = new ReadOnlyDictionary<string, ModelMeta>(amap);
        ResourceMap = new ReadOnlyDictionary<string, ModelMeta>(rmap);
        TypeMap     = new ReadOnlyDictionary<Type, ModelMeta>(tmap);


        return Task.CompletedTask;

    }



    public IEnumerable<ModelMeta> Where( Func<ModelMeta, bool> predicate )
    {
        return TypeMap.Values.Where(predicate);
    }

    public ModelMeta GetMetaFromAlias( string alias )
    {

        if (alias == null) throw new ArgumentNullException(nameof(alias));

        var key = alias.ToLowerInvariant();

        if( AliasMap.TryGetValue(key, out var meta) )
            return meta;

        return null;

    }

    public ModelMeta GetMetaFromResource( string resource )
    {

        if (resource == null) throw new ArgumentNullException(nameof(resource));

        var key = resource.ToLowerInvariant();

        if (ResourceMap.TryGetValue(key, out var meta))
            return meta;

        return null;

    }

    public ModelMeta GetMetaFromType( Type type )
    {

        if (type == null) throw new ArgumentNullException(nameof(type));

        if( TypeMap.TryGetValue(type, out var meta) )
            return meta;

        var attr = (ModelAttribute)type.GetCustomAttribute(typeof(ModelAttribute));

        var aliasKey    = (attr == null || string.IsNullOrWhiteSpace(attr.Alias) ? type.Name : attr.Alias).ToLowerInvariant();
        var resourceKey = (attr == null || string.IsNullOrWhiteSpace(attr.Resource) ? type.Name.Pluralize() : attr.Resource).ToLowerInvariant();

        var fly = new ModelMeta(aliasKey, resourceKey, type, Empty, Empty, Empty, Empty );

        return fly;

    }



}