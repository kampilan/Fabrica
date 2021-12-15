using System;
using System.Collections.Generic;

namespace Fabrica.Models.Support;


public class ModelMeta
{

    public ModelMeta( string alias, string resource, Type target, IEnumerable<string> projection, IEnumerable<string> exclusions, IEnumerable<string> immutables, IEnumerable<string> mutables )
    {

        Alias     = alias;
        Resource  = resource;
        Target    = target;

        Projection = new HashSet<string>( projection );
        Exclusions = new HashSet<string>( exclusions );
        Immutables = new HashSet<string>( immutables );
        Mutables   = new HashSet<string>( mutables );

    }


    public string Alias { get; }
    public string Resource { get; }

    public Type Target { get; }


    public ISet<string> Projection { get; }
    public ISet<string> Exclusions { get; }
    public ISet<string> Immutables { get; }
    public ISet<string> Mutables { get; }




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