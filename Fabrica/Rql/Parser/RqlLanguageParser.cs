using System;
using System.Collections.Generic;
using System.Globalization;
using Sprache;

namespace Fabrica.Rql.Parser
{


    public class RqlLanguageParser
    {

        static RqlLanguageParser()
        {


            OperatorMap = new Dictionary<string, RqlOperator>
            {
                ["eq"] = RqlOperator.Equals,
                ["ne"] = RqlOperator.NotEquals,
                ["lt"] = RqlOperator.LesserThan,
                ["gt"] = RqlOperator.GreaterThan,
                ["le"] = RqlOperator.LesserThanOrEqual,
                ["ge"] = RqlOperator.GreaterThanOrEqual,
                ["sw"] = RqlOperator.StartsWith,
                ["cn"] = RqlOperator.Contains,
                ["bt"] = RqlOperator.Between,
                ["in"] = RqlOperator.In,
                ["ni"] = RqlOperator.NotIn
            };



        }


        private static readonly IDictionary<string, RqlOperator> OperatorMap;



        private static readonly Parser<IEnumerable<char>> Whitespace = Parse.Char(' ').Many();

        private static readonly Parser<char> ProjectionOpener    = Parse.Char('(');
        private static readonly Parser<char> ProjectionCloser    = Parse.Char(')');
        private static readonly Parser<char> ProjectionSeparator = Parse.Char(',');

        private static readonly Parser<char> ProjectedPropertyName = Parse.AnyChar.Except(ProjectionSeparator).Except(ProjectionCloser).Except(Parse.String(Environment.NewLine));

        private static readonly Parser<string> ProjectedProperty = ProjectedPropertyName.XMany().Text();

        private static readonly Parser<char> RestrictionOpener  = Parse.Char('(');
        private static readonly Parser<char> RestrictionCloser  = Parse.Char(')');
        private static readonly Parser<char> PredicateSeparator = Parse.Char(',');

        private static readonly Parser<char> PredicateOpener = Parse.Char('(');
        private static readonly Parser<char> PredicateCloser = Parse.Char(')');

        private static readonly Parser<char>   PredicateTypeTerm = Parse.AnyChar.Except(PredicateOpener);
        private static readonly Parser<string> PredicateType     = PredicateTypeTerm.XAtLeastOnce().Text();

        private static readonly Parser<char> PredicateValueSeparator = Parse.Char(',');

        private static readonly Parser<char>   PredicateValueTerm   = Parse.AnyChar.Except(PredicateValueSeparator).Except(PredicateCloser);
        private static readonly Parser<string> PredicateTargetName  = PredicateValueTerm.XAtLeastOnce().Text();
        private static readonly Parser<string> PredicateTargetValue = PredicateValueTerm.XAtLeastOnce().Text();


        private static readonly Parser<IRqlPredicate> Predicate =

            from ws     in Whitespace
            from type   in PredicateType
            from opener in PredicateOpener
            from target in PredicateTargetName
            from values in PredicateSeparator.Then(_ => PredicateTargetValue).Many()
            from closer in PredicateCloser

            select BuildPredicate(type, target, values);



        private static readonly Parser<IEnumerable<string>> Projection =

            from open    in ProjectionOpener
            from leading in ProjectedProperty.Optional()
            from rest    in ProjectionSeparator.Then(_ => ProjectedProperty).Many()
            from close   in ProjectionCloser

            select MergeProjectionProperties(leading.GetOrDefault(), rest);



        private static readonly Parser<IEnumerable<IRqlPredicate>> Restriction =

            from open    in RestrictionOpener
            from leading in Predicate.Optional()
            from rest    in PredicateSeparator.Then(_ => Predicate).Many()
            from close   in RestrictionCloser

            select MergePredicates(leading.GetOrDefault(), rest);



        private static readonly Parser<IEnumerable<IRqlPredicate>> RestrictionOnly =

            from open    in RestrictionOpener
            from leading in Predicate.Optional()
            from rest    in PredicateSeparator.Then(_ => Predicate).Many()
            from close   in RestrictionCloser

            select MergePredicates(leading.GetOrDefault(), rest);



        private static readonly Parser<(IEnumerable<string> proj, IEnumerable<IRqlPredicate> ops)> Expression =

            from projection in Projection
            from ws         in Whitespace
            from operations in Restriction.End()

            select (projection, operations);


        private static IEnumerable<string> MergeProjectionProperties(string head, IEnumerable<string> rest)
        {

            if (!string.IsNullOrWhiteSpace(head) && head[0] != '*')
                yield return head;

            foreach (var item in rest)
                yield return item;

        }

        private static IEnumerable<IRqlPredicate> MergePredicates(IRqlPredicate head, IEnumerable<IRqlPredicate> rest)
        {

            if (head != null)
                yield return head;

            foreach (var item in rest)
                yield return item;

        }

        private static IRqlPredicate BuildPredicate( string op, string name, IEnumerable<string> values )
        {


            if (string.IsNullOrWhiteSpace(op))
                return null;

            var dataType = typeof(string);

            var raw   = new List<string>(values);
            var typed = new List<object>();

            foreach( var v in raw )
            {
                
                var indicator = v[0];
                if( indicator == '@' && DateTime.TryParse(v.Substring(1), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var date) )
                {
                    dataType = typeof(DateTime);
                    typed.Add(date);    
                }
                else if (indicator == '#' && decimal.TryParse(v.Substring(1), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var decm) )
                {
                    dataType = typeof(decimal);
                    typed.Add(decm);
                }
                else if (indicator == '\''  )
                {
                    dataType = typeof(string);
                    var len = v.Length - 2;
                    var s = v.Substring(1, len);
                    typed.Add(s);
                }
                else if( int.TryParse(v, out var iv) )
                {
                    dataType = typeof(int);
                    typed.Add(iv);
                }
                else if( long.TryParse(v, out var lv) )
                {
                    dataType = typeof(long);
                    typed.Add(lv);
                }
                else if( decimal.TryParse(v, out var dv) )
                {
                    dataType = typeof(decimal);
                    typed.Add(dv);
                }
                else if( bool.TryParse(v, out var bv) )
                {
                    dataType = typeof(bool);
                    typed.Add(bv);
                }
                else
                {
                    dataType = typeof(string);
                    typed.Add(v);
                }


            }


            if( !OperatorMap.TryGetValue(op, out var opr) )
                throw new Exception($"Invalid RQL operator: ({op})");

            var predicate = new RqlPredicate( opr, name, dataType, typed );

            return predicate;


        }



        public static RqlTree ToFilter( string input )
        {

            try
            {

                var (proj, ops) = Expression.Parse(input);

                var expr = new RqlTree();
                expr.Projection.AddRange(proj);
                expr.Criteria.AddRange(ops);

                return expr;

            }
            catch (ParseException cause)
            {
                throw new RqlException( $"Could not parse supplied RQL '{input}'. {cause.Message}", cause );
            }

        }


        public static RqlTree ToCriteria( string input )
        {

            try
            {

                var ops = RestrictionOnly.Parse(input);

                var expr = new RqlTree();
                expr.Criteria.AddRange(ops);

                return expr;

            }
            catch (ParseException cause)
            {
                throw new RqlException( $"Could not parse supplied RQL '{input}'. {cause.Message}", cause );
            }

        }


    }

}
