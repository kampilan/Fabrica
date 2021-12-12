using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fabrica.Models.Support;
using Fabrica.Watch;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Fabrica.Models.Patch.Builder
{


    public class PatchSet
    {


        public static PatchSet FromJsonOne( string json )
        {

            var patch = JsonConvert.DeserializeObject<ModelPatch>(json, Settings);

            var set = new PatchSet();
            set.Patches.Add(patch);

            return set;

        }

        public static PatchSet FromJsonOne( Stream strm )
        {

            using (var reader = new StreamReader(strm))
            {
                var set = FromJsonOne(reader);
                return set;
            }

        }

        public static PatchSet FromJsonOne( TextReader reader )
        {

            using (var jr = new JsonTextReader(reader))
            {

                var serializer = new JsonSerializer()
                {
                    Formatting                 = Formatting.Indented,
                    DateFormatHandling         = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling       = DateTimeZoneHandling.RoundtripKind,
                    DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate
                };

                var patch = serializer.Deserialize<ModelPatch>(jr);

                var set = new PatchSet();
                set.Patches.Add(patch);


                return set;

            }

        }



        public static PatchSet FromJsonMany( string json )
        {

            var set = new PatchSet
            {
                Patches = JsonConvert.DeserializeObject<List<ModelPatch>>(json, Settings)
            };

            return set;

        }

        public static PatchSet FromJsonMany( Stream strm )
        {

            using( var reader = new StreamReader(strm) )
            {
                var set = FromJsonMany(reader);
                return set;
            }

        }    

        public static PatchSet FromJsonMany( TextReader reader )
        {

            using( var jr = new JsonTextReader(reader) )
            {

                var serializer = new JsonSerializer
                {
                    Formatting                 = Formatting.Indented,
                    DateFormatHandling         = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling       = DateTimeZoneHandling.RoundtripKind,
                    DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate
                };

                var patches = serializer.Deserialize<List<ModelPatch>>( jr );

                var set = new PatchSet
                {
                    Patches = patches
                };

                return set;

            }

        }



        public static PatchSet Create( IMutableModel model)
        {

            var set = new PatchSet();
            set.Add( model );

            return set;

        }

        public static PatchSet Create( IEnumerable<IMutableModel> models )
        {

            var set = new PatchSet();
            set.Add(models);

            return set;

        }

        public static PatchSet Create( params IMutableModel[] models )
        {

            var set = new PatchSet();
            set.Add(models);

            return set;

        }


        public PatchSet()
        {
            Logger = WatchFactoryLocator.Factory.GetLogger(GetType());
        }


        private ILogger Logger { get; }


        public void Add( IMutableModel model )
        {
            HandleObject( "", "", "",  model );

            After();

        }

        public void Add( params IMutableModel[] models )
        {

            foreach( var m in models )
                HandleObject( "", "", "", m );

            After();

        }

        public void Add( IEnumerable<IMutableModel> models )
        {

            foreach( var m in models )
                HandleObject( "", "", "", m );

            After();

        }

        public void Add( PatchSet set )
        {
            if( !set.Equals(this) )
                Patches.AddRange(set.Patches);
        }

        public void Add( IEnumerable<ModelPatch> patches )
        {
            Patches.AddRange(patches);
        }

        public void Add( params ModelPatch[] patches )
        {
            Patches.AddRange(patches);
        }


        private void After()
        {

            bool Rules(ModelPatch p)
            {

                if (p.Verb == PatchVerb.Unmodified)
                    return false;

                if (p.Verb == PatchVerb.Update && p.Properties.Count == 0 )
                    return false;

                if (p.Verb == PatchVerb.Create && p.Properties.Count == 0)
                    return false;

                return true;

            }

            Patches = Patches.Where(Rules).ToList();

        }


        private List<ModelPatch> Patches { get; set; } = new List<ModelPatch>();

        public int Count => Patches.Count;

        public bool HasModifications => Patches.Count > 0;

        public IEnumerable<ModelPatch> GetPatches()
        {

            bool Rules(ModelPatch p)
            {

                if (p.Verb == PatchVerb.Unmodified)
                    return false;

                if (p.Verb == PatchVerb.Update && p.Properties.Count == 0)
                    return false;

                if (p.Verb == PatchVerb.Create && p.Properties.Count == 0)
                    return false;

                return true;

            }

            var patches = Patches.Where(Rules);
            
            return patches;

        }


        private static readonly JsonSerializerSettings Settings  = new JsonSerializerSettings
        {
            Formatting                 = Formatting.Indented,
            DateFormatHandling         = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling       = DateTimeZoneHandling.RoundtripKind,
            DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate
        };

        public string ToJson()
        {
            var json = JsonConvert.SerializeObject( Patches, Settings );
            return json;
        }


        protected virtual void HandleObject([NotNull] string parentType, [NotNull] string parentUid, [NotNull] string parentProperty, [NotNull] IMutableModel source)
        {

            if (parentType == null) throw new ArgumentNullException(nameof(parentType));
            if (parentUid == null) throw new ArgumentNullException(nameof(parentUid));
            if (parentProperty == null) throw new ArgumentNullException(nameof(parentProperty));
            if (source == null) throw new ArgumentNullException(nameof(source));

            try
            {

                Logger.EnterMethod();


                Logger.Inspect(nameof(parentType), parentType);
                Logger.Inspect(nameof(parentUid), parentUid);
                Logger.Inspect(nameof(parentProperty), parentProperty);

                Logger.Inspect("Model Type", source.GetType().FullName);
                Logger.Inspect("Model Target", source.ToAlias());
                Logger.Inspect("Model IsModified", source.IsModified());
                Logger.Inspect("Model IsAdded", source.IsAdded());
                Logger.Inspect("Model IsRemoved", source.IsRemoved());


                var state = PatchVerb.Unmodified;
                if (source.IsAdded())
                    state = PatchVerb.Create;
                else if (source.IsRemoved())
                    state = PatchVerb.Delete;
                else if (source.IsModified())
                    state = PatchVerb.Update;

                var patch = new ModelPatch
                {
                    Model = source.ToAlias(),
                    Uid   = source.Uid,
                    Verb  = state,
                };

                if( !string.IsNullOrWhiteSpace(parentType) )
                {
                    var ap = new PropertyPath
                    {
                        Model    = parentType,
                        Uid      = parentUid,
                        Property = parentProperty
                    };

                    patch.Membership = ap;
                }



                // *****************************************************************
                Logger.Debug("Attempting to add Patch to PatchSet");
                Patches.Add(patch);



                // *****************************************************************
                if (source.IsRemoved())
                {
                    Logger.Debug("Model being removed. No need to look for modification");
                    return;
                }



                // *****************************************************************
                Logger.Debug("Attempting to look for modified properties");
                foreach (var prop in source.GetDelta().Values)
                {

                    if( prop.IsReference && prop.Current == null && prop.Original is IReferenceModel )
                        patch.Properties[prop.Name] = "";
                    else if ( prop.IsReference && prop.Current is IReferenceModel cur )
                        patch.Properties[prop.Name] = cur.Uid;
                    else if( prop.IsCollection && prop.Current is IAggregateCollection collection )
                        HandlCollection(prop.Parent, prop.ParentUid, prop.Name, collection);
                    else
                        patch.Properties[prop.Name] = prop.Current;

                }


                Logger.LogObject(nameof(patch), patch);


            }
            finally
            {
                Logger.LeaveMethod();
            }
        }
 
        protected virtual void HandlCollection([NotNull] string parentType, [NotNull] string parentUid, [NotNull] string parentProperty, [NotNull] IAggregateCollection set)
        {


            if (set == null) throw new ArgumentNullException(nameof(set));
            if (string.IsNullOrWhiteSpace(parentType))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(parentType));
            if (string.IsNullOrWhiteSpace(parentUid))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(parentUid));
            if (string.IsNullOrWhiteSpace(parentProperty))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(parentProperty));

            try
            {

                Logger.EnterMethod();

                foreach (var agg in set.DeltaMembers.Cast<IAggregateModel>())
                {
                    if( agg.IsModified() && !agg.IsAdded() && !agg.IsRemoved() )
                        HandleObject( "", "", "", agg );
                    else
                        HandleObject(parentType, parentUid, parentProperty, agg);
                }

            }
            finally
            {
                Logger.LeaveMethod();
            }
        }



    }

}
