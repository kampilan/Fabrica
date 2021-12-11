using System.Collections.Generic;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;

namespace Fabrica.Persistence.Patch;

public static class PatchResolverExtensions
{


    public static IEnumerable<PatchRequest> Resolve(this IPatchResolver resolver, params IMutableModel[] models)
    {

        var set = PatchSet.Create(models);
        var requests = resolver.Resolve(set);

        return requests;

    }

    public static IEnumerable<PatchRequest> Resolve(this IPatchResolver resolver, IEnumerable<IMutableModel> models )
    {

        var set = PatchSet.Create(models);
        var requests = resolver.Resolve(set);

        return requests;

    }


}