using System;
using System.Collections.Generic;

namespace Fabrica.Models.Support;


public class ModelMeta(string alias, string resource, Type target, IEnumerable<string> projection, IEnumerable<string> exclusions, IEnumerable<string> immutables, IEnumerable<string> mutables)
{

    public string Alias { get; } = alias;
    public string Resource { get; } = resource;

    public Type Target { get; } = target;


    public ISet<string> Projection { get; } = new HashSet<string>( projection );
    public ISet<string> Exclusions { get; } = new HashSet<string>( exclusions );
    public ISet<string> Immutables { get; } = new HashSet<string>( immutables );
    public ISet<string> Mutables { get; } = new HashSet<string>( mutables );


    public ISet<string> CheckForCreate( IEnumerable<string> candidates )
    {

        var combo = new HashSet<string>(candidates);

        combo.ExceptWith( Immutables );
        combo.ExceptWith( Mutables );

        return combo;

    }

    public ISet<string> CheckForUpdate( IEnumerable<string> candidates )
    {

        var combo = new HashSet<string>(candidates);

        combo.ExceptWith(Mutables);

        return combo;

    }



}