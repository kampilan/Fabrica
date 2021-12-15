using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Fabrica.Models.Support;

public interface IModelMetaService
{

    ModelMeta GetMetaFromAlias( [NotNull] string alias );
    ModelMeta GetMetaFromResource( [NotNull] string resource );
    ModelMeta GetMetaFromType( [NotNull] Type type );

    IEnumerable<ModelMeta> Where( Func<ModelMeta, bool> predicate );

}