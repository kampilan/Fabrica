using System.Linq.Expressions;
using Fabrica.Utilities.Types;

namespace Fabrica.Models.Support;

public interface IDuplicateCheckBuilder
{

}


public class DuplicateCheckBuilder<TModel>: IDuplicateCheckBuilder where TModel: class, IModel
{

        
    public DuplicateCheckBuilder( Func<TModel,TModel,string> template, Func<TModel,Expression<Func<TModel,bool>>> predicate )
    {

        Template  = template;
        Predicate = predicate;

    }        
        

    private Func<TModel,TModel,string> Template { get; }
        
    private Func<TModel,Expression<Func<TModel,bool>>> Predicate { get; }


    public (Expression<Func<TModel,bool>> checker, Func<TModel, TModel, string> template) Build( TModel source )
    {

        var expr    = PredicateBuilder.True<TModel>().And( Predicate(source) ).And( t=>t.Uid != source.Uid );
                
        // *****************************************************************                
        return (expr, Template);

    }
        
        
}