using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Autofac;
using Fabrica.Models.Serialization;
using Fabrica.Utilities.Text;
using Fabrica.Utilities.Types;
using JetBrains.Annotations;
// ReSharper disable CollectionNeverUpdated.Local

namespace Fabrica.Models.Support
{


    public interface IModelMetaService
    {

        ModelMeta GetMetaFromAlias( [NotNull] string alias );
        ModelMeta GetMetaFromResource( [NotNull] string resource );
        ModelMeta GetMetaFromType( [NotNull] Type type );

        IEnumerable<ModelMeta> Where( Func<ModelMeta, bool> predicate );

    }


    public class ModelMeta
    {

        public ModelMeta( string alias, string resourece, Type target, Type reference, Type explorer, Type rto, IEnumerable<string> exclusions, IEnumerable<string> creatables, IEnumerable<string> updatables )
        {

            Alias     = alias;
            Resource  = resourece;
            Target    = target;
            Reference = reference;
            Explorer  = explorer;
            Rto       = rto;

            Exclusions  = new HashSet<string>( exclusions );
            Creatable = new HashSet<string>( creatables );
            Updatable    = new HashSet<string>( updatables );

        }


        public string Alias { get; }
        public string Resource { get; }

        public Type Target { get; }

        public Type Reference { get; }

        public Type Explorer { get; }

        public Type Rto { get; }


        public ISet<string> Exclusions { get; }
        public ISet<string> Creatable { get; }
        public ISet<string> Updatable { get; }


        public ISet<string> CheckForCreate( IEnumerable<string> candidates )
        {

            var combo = new HashSet<string>(candidates);

            combo.ExceptWith( Creatable );
            combo.ExceptWith( Updatable );

            return combo;

        }

        public ISet<string> CheckForUpdate( IEnumerable<string> candidates )
        {

            var combo = new HashSet<string>(candidates);

            combo.ExceptWith(Updatable);

            return combo;

        }



    }


    public class ModelMetaSource: TypeSource
    {

        private static Func<Type, bool> Predicate { get; } = t =>
        {

            if( typeof(IModel).IsAssignableFrom(t) && t.GetCustomAttribute<ModelAttribute>() is { } attr )
                return !attr.Ignore;

            if (typeof(IModel).IsAssignableFrom(t))
                return true;

            return false;

        };

        protected override Func<Type, bool> GetPredicate()
        {
            return Predicate;
        }

    }


    public class ModelMetaService: IModelMetaService, IStartable
    {

        public ModelMetaService(IEnumerable<ModelMetaSource> sources)
        {
            Sources = sources;
        }

        private IEnumerable<ModelMetaSource> Sources { get; }

        private static ISet<string> Empty { get; } = new HashSet<string>();


        private IReadOnlyDictionary<string,ModelMeta> AliasMap { get; set; }
        private IReadOnlyDictionary<string,ModelMeta> ResourceMap { get; set; }
        private IReadOnlyDictionary<Type,ModelMeta> TypeMap { get; set; }
        public void Start()
        {


            if (AliasMap != null)
                return;

            
            var amap = new Dictionary<string,ModelMeta>();
            var rmap = new Dictionary<string, ModelMeta>();
            var tmap = new Dictionary<Type,ModelMeta>();

            foreach( var target in Sources.SelectMany(s=>s.GetTypes()) )
            {

                var attr = target.GetCustomAttribute<ModelAttribute>();

                var aliasKey = (attr == null || string.IsNullOrWhiteSpace(attr.Alias) ? target.Name : attr.Alias).ToLowerInvariant();
                if (amap.TryGetValue(aliasKey, out var adup))
                    throw new InvalidOperationException($" Attempting to add an Alias ({aliasKey}) for Type ({target.FullName}) when Type ({adup.Target.FullName}) is already using it.");

                var rto = attr?.Rto;

                var exclusions = target.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p =>
                    {
                        var ia = p.GetCustomAttribute<RtoAttribute>();
                        return ia != null && ia.Mutability == RtoMutability.Never;
                    })
                    .Select(p => p.Name);

                var creatables = target.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p =>
                    {
                        var ia = p.GetCustomAttribute<RtoAttribute>();
                        return ia != null && ia.Mutability == RtoMutability.CreateOnly;
                    })
                    .Select(p => p.Name);

                var updatables = target.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p =>
                    {
                        var ia = p.GetCustomAttribute<RtoAttribute>();
                        return ia == null || ia.Mutability == RtoMutability.Always;
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

                    var reference = attr?.Reference??target;
                    var explorer  = attr?.Explorer??reference;

                    var meta = new ModelMeta(aliasKey, resourceKey, target, reference, explorer, rto != target ? rto : null, exclusions, creatables, updatables );

                    amap[aliasKey]    = meta;
                    rmap[resourceKey] = meta;
                    tmap[target]      = meta;

                }

                void ForReferences()
                {

                    var meta = new ModelMeta(aliasKey, "", target, null, null, rto, exclusions, creatables, updatables);

                    amap[aliasKey] = meta;
                    tmap[target]   = meta;

                }

                void ForAggregates()
                {

                    var resourceKey = (attr == null || string.IsNullOrWhiteSpace(attr.Resource) ? target.Name.Pluralize() : attr.Resource).ToLowerInvariant();
                    if (rmap.TryGetValue(resourceKey, out var rdup))
                        throw new InvalidOperationException($" Attempting to add a Resource ({resourceKey}) for Type ({target.FullName}) when Type ({rdup.Target.FullName}) is already using it.");

                    var explorer = attr?.Explorer??target;

                    var meta = new ModelMeta( aliasKey, resourceKey, target, null, explorer, rto, exclusions, creatables, updatables );

                    amap[aliasKey]    = meta;
                    rmap[resourceKey] = meta;
                    tmap[target]      = meta;

                }


            }

            AliasMap    = new ReadOnlyDictionary<string, ModelMeta>(amap);
            ResourceMap = new ReadOnlyDictionary<string, ModelMeta>(rmap);
            TypeMap     = new ReadOnlyDictionary<Type, ModelMeta>(tmap);

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

            var reference = attr?.Reference??type;
            var explorer  = attr?.Explorer??reference;
            var rto       = attr?.Rto;

            var fly = new ModelMeta(aliasKey, resourceKey, type, reference, explorer, rto, Empty, Empty, Empty );

            return fly;

        }



    }



}
