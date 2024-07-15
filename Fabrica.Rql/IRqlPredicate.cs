using System;
using System.Collections.Generic;
using Fabrica.Rql.Parser;

namespace Fabrica.Rql
{
 
    
    public interface IRqlPredicate
    {

        RqlOperator Operator { get; }

        Target Target { get; }

        Type DataType { get; }

        IReadOnlyList<object> Values { get; }

    }



}
