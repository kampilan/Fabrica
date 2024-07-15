namespace Fabrica.Rql.Parser;

public class RqlTree
{

    public bool HasProjection => Projection.Count > 0;
    public List<string> Projection { get; } = new List<string>();

    public bool HasCriteria => Criteria.Count > 0;
    public List<IRqlPredicate> Criteria { get;  } = new List<IRqlPredicate>();

}