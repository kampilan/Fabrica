using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using JetBrains.Annotations;

namespace Fabrica.Rql.Serialization
{


    public static class LambdaSerializerExtensions
    {


        public static Func<TEntity, bool> ToLambda<TEntity>( [NotNull] this IRqlFilter<TEntity> filter, bool insensitive = false) where TEntity : class
        {

            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var expression = filter.ToExpression(insensitive);
            var lambda = expression.Compile();

            return lambda;

        }

        public static Func<TEntity, bool> ToLambda<TEntity>( [NotNull]this string rql, bool insensitive = false) where TEntity : class
        {

            if (string.IsNullOrWhiteSpace(rql))
                throw new ArgumentException("RQL cannot be null or whitespace.", nameof(rql));

            var tree   = RqlLanguageParser.ToFilter(rql);
            var filter = new RqlFilterBuilder<TEntity>(tree);
            var lambda = filter.ToLambda(insensitive);

            return lambda;

        }

        public static Expression<Func<TEntity, bool>> ToExpression<TEntity>( [NotNull]this string rql, bool insensitive = false) where TEntity : class
        {

            if (string.IsNullOrWhiteSpace(rql))
                throw new ArgumentException("RQL cannot be null or whitespace.", nameof(rql));

            var tree   = RqlLanguageParser.ToFilter(rql);
            var filter = new RqlFilterBuilder<TEntity>(tree);

            var exp = filter.ToExpression(insensitive);

            return exp;

        }

        public static Expression<Func<TEntity, bool>> ToExpression<TEntity>( [NotNull] this IRqlFilter<TEntity> filter, bool insensitive=false ) where TEntity : class
        {

            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var entity = Expression.Parameter(typeof(TEntity), "e");

            Expression running = null;

            foreach( var predicate in filter.Criteria )
            {

                switch (predicate.Operator)
                {

                    case RqlOperator.Equals:
                        running = BuildEqual( running, entity, predicate );
                        break;
                    case RqlOperator.NotEquals:
                        running = BuildNotEqual( running, entity, predicate );
                        break;
                    case RqlOperator.LesserThan:
                        running = BuildLesserThan( running, entity, predicate );
                        break;
                    case RqlOperator.GreaterThan:
                        running = BuildGreaterThan( running, entity, predicate );
                        break;
                    case RqlOperator.LesserThanOrEqual:
                        running = BuildLesserThanOrEqual( running, entity, predicate );
                        break;
                    case RqlOperator.GreaterThanOrEqual:
                        running = BuildGreaterThanOrEqual( running, entity, predicate );
                        break;
                    case RqlOperator.StartsWith when predicate.DataType == typeof(string) && insensitive:
                        running = BuildStartsWithCi(running, entity, predicate);
                        break;
                    case RqlOperator.StartsWith when predicate.DataType == typeof(string) && !insensitive:
                        running = BuildStartsWith(running, entity, predicate);
                        break;
                    case RqlOperator.Contains when predicate.DataType == typeof(string) && insensitive:
                        running = BuildContainsCi(running, entity, predicate);
                        break;
                    case RqlOperator.Contains when predicate.DataType == typeof(string) && !insensitive:
                        running = BuildContains(running, entity, predicate);
                        break;
                    case RqlOperator.Between:
                        running = BuildBetween( running, entity, predicate );
                        break;
                    case RqlOperator.In:
                        running = BuildIn(running, entity, predicate);
                        break;
                    case RqlOperator.NotIn:
                        running = BuildNotIn(running, entity, predicate);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();

                }

            }


            if( running == null )
            {
                Expression<Func<TEntity, bool>> none = _=> true;
                return none;
            }

            return Expression.Lambda<Func<TEntity,bool>>( running, entity );

        }

        private static (Expression left, Expression right) BuildOperands(Expression entity, string name, Type dataType, object value)
        {

            var left = Expression.Property(entity, name);
            ConstantExpression right;

            var prop  = (PropertyInfo)left.Member;
            if (prop.PropertyType != dataType)
            {
                var conv = Convert.ChangeType(value, prop.PropertyType);
                right = Expression.Constant(conv, prop.PropertyType);
            }
            else
                right = Expression.Constant(value, dataType);

            return (left, right);

        }

        private static (Expression left, IEnumerable<Expression> right) BuildOperandsInsensitive(Expression entity, string name, string value )
        {

            var left  = Expression.Property(entity, name);
            var right = new List<Expression> {Expression.Constant(value, typeof(string)), Expression.Constant(StringComparison.InvariantCultureIgnoreCase, typeof(StringComparison)) };

            return (left, right);

        }

        private static Expression BuildEqual(Expression running, Expression entity, IRqlPredicate predicate )
        {

            var (left, right) = BuildOperands(entity, predicate.Target.Name, predicate.DataType, predicate.Values[0]);

            var exp = Expression.Equal(left, right);

            if (running == null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildNotEqual( Expression running, Expression entity, IRqlPredicate predicate )
        {

            var (left, right) = BuildOperands(entity, predicate.Target.Name, predicate.DataType, predicate.Values[0]);

            var exp = Expression.NotEqual(left, right);

            if (running == null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildLesserThan( Expression running, Expression entity, IRqlPredicate predicate )
        {

            var (left, right) = BuildOperands(entity, predicate.Target.Name, predicate.DataType, predicate.Values[0]);

            var exp = Expression.LessThan(left, right);

            if (running == null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildLesserThanOrEqual( Expression running, Expression entity, IRqlPredicate predicate )
        {

            var (left, right) = BuildOperands(entity, predicate.Target.Name, predicate.DataType, predicate.Values[0]);

            var exp = Expression.LessThanOrEqual(left, right);

            if (running == null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildGreaterThan( Expression running, Expression entity, IRqlPredicate predicate )
        {

            var (left, right) = BuildOperands(entity, predicate.Target.Name, predicate.DataType, predicate.Values[0]);

            var exp = Expression.GreaterThan(left, right);

            if (running == null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildGreaterThanOrEqual( Expression running, Expression entity, IRqlPredicate predicate )
        {

            var (left, right) = BuildOperands(entity, predicate.Target.Name, predicate.DataType, predicate.Values[0]);

            var exp = Expression.GreaterThanOrEqual(left, right);

            if (running == null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildBetween(Expression running, Expression entity, IRqlPredicate predicate)
        {

            var (leftFrom, rightFrom) = BuildOperands(entity, predicate.Target.Name, predicate.DataType, predicate.Values[0]);

            var from = Expression.GreaterThanOrEqual(leftFrom, rightFrom);

            var (leftTo, rightTo) = BuildOperands(entity, predicate.Target.Name, predicate.DataType, predicate.Values[1]);

            var to = Expression.LessThanOrEqual(leftTo, rightTo);

            var exp = Expression.AndAlso(from,to);

            if (running == null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildStartsWith( Expression running, Expression entity, IRqlPredicate predicate )
        {

            var (left, right) = BuildOperands( entity, predicate.Target.Name, typeof(string), predicate.Values[0].ToString() );

            var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            if (method == null)
                throw new Exception("String does not have a StartsWith method");

            var exp = Expression.Call(left, method, right);

            if (running == null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildStartsWithCi(Expression running, Expression entity, IRqlPredicate predicate)
        {

            var (left, right) = BuildOperandsInsensitive(entity, predicate.Target.Name, predicate.Values[0].ToString());

            var method = typeof(string).GetMethod("StartsWith", new[] { typeof(string), typeof(StringComparison) });
            if (method == null)
                throw new Exception("String does not have a StartsWith method");

            var exp = Expression.Call(left, method, right);

            if (running == null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildContains(Expression running, Expression entity, IRqlPredicate predicate)
        {

            var (left, right) = BuildOperands(entity, predicate.Target.Name, typeof(string),predicate.Values[0].ToString());

            var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            if( method == null )
                throw new Exception("String does not have a Contains method");

            var exp = Expression.Call( left, method, right );

            if (running == null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildContainsCi(Expression running, Expression entity, IRqlPredicate predicate)
        {

            var (left, right) = BuildOperandsInsensitive(entity, predicate.Target.Name, predicate.Values[0].ToString());

            var method = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
            if (method == null)
                throw new Exception("String does not have a Contains method");

            var exp = Expression.Call(left, method, right);

            if (running == null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildIn( Expression running, Expression entity, IRqlPredicate predicate )
        {

            var method = typeof(List<object>).GetMethod("Contains", new[] { typeof(object) });
            if (method == null)
                throw new Exception("List does not have a Contains method.");

            var left  = Expression.Constant(predicate.Values, typeof(List<object>));
            var cand  = Expression.Property(entity, predicate.Target.Name);
            var right = Expression.Convert(cand, typeof(object));


            var exp = Expression.Call(left, method, right);

            if (running == null)
                return exp;

            return Expression.AndAlso(running, exp);

        }

        private static Expression BuildNotIn(Expression running, Expression entity, IRqlPredicate predicate)
        {

            var method = typeof(List<object>).GetMethod("Contains", new[] { typeof(object) });
            if (method == null)
                throw new Exception("List does not have a Contains method");

            var left  = Expression.Constant(predicate.Values, typeof(List<object>));
            var cand  = Expression.Property(entity, predicate.Target.Name);
            var right = Expression.Convert(cand, typeof(object));

            var found = Expression.Call(left, method, right);
            var exp = Expression.Not(found);

            if (running == null)
                return exp;

            return Expression.AndAlso(running, exp);

        }


    }








}
