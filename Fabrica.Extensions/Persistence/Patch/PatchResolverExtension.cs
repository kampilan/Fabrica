using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;
using JetBrains.Annotations;

namespace Fabrica.Persistence.Patch
{

    
    public static class PatchResolverExtension
    {


        public static async Task Resolve(this IPatchResolverComponent resolver, [NotNull]IMutableModel model )
        {

            if (model == null) throw new ArgumentNullException(nameof(model));

            var set = PatchSet.Create(model);
            await resolver.Apply(set);

        }


        public static async Task Resolve(this IPatchResolverComponent resolver, [NotNull] IEnumerable<IMutableModel> models )
        {

            if (models == null) throw new ArgumentNullException(nameof(models));

            var set = PatchSet.Create(models);
            await resolver.Apply(set);

        }


        public static async Task Resolve(this IPatchResolverComponent resolver, [NotNull] params IMutableModel[] models )
        {

            if (models == null) throw new ArgumentNullException(nameof(models));

            var set = PatchSet.Create(models);
            await resolver.Apply(set);

        }


        public static async Task Resolve(this IPatchResolverComponent resolver, [NotNull] IEnumerable<ModelPatch> patches )
        {

            if (patches == null) throw new ArgumentNullException(nameof(patches));

            var set = new PatchSet();
            set.Add(patches);

            await resolver.Apply(set);

        }


        public static async Task Resolve(this IPatchResolverComponent resolver, [NotNull] params ModelPatch[] patches )
        {

            if (patches == null) throw new ArgumentNullException(nameof(patches));

            var set = new PatchSet();
            set.Add( patches );

            await resolver.Apply(set);

        }






    }


}
