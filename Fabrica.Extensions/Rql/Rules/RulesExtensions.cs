using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Fabrica.Models;
using Fabrica.Models.Support;
using Fabrica.Rql.Parser;
using Fabrica.Rules;

namespace Fabrica.Rql.Rules
{


    public static class RulesExtensions
    {


        public static Rule<IRqlFilter<TEntity>> IfNotBy<TEntity,TValue>( this Rule<IRqlFilter<TEntity>> rule, Expression<Func<TEntity,TValue>> prop, params RqlOperator[] allowed ) where TEntity: class, IModel
        {

            if (prop == null) throw new ArgumentNullException(nameof(prop));

            if (prop.Body == null)
                throw new ArgumentNullException(nameof(prop));

            if (!(prop.Body is MemberExpression))
                throw new ArgumentException("Targets of a rule must be a field or a property");

            var propExpr = (MemberExpression)prop.Body;
            var name     = propExpr.Member.Name;


            var set = new HashSet<RqlOperator>(allowed);

            rule.If(f => f.AtLeastOne(p => p.Target.Name == name && !set.Contains(p.Operator)));

            return rule;

        }


        public static Rule<IRqlFilter> IfNotBy( this Rule<IRqlFilter> rule, string name, params RqlOperator[] allowed )
        {

            var set = new HashSet<RqlOperator>(allowed);

            rule.If( f => f.AtLeastOne(p => p.Target.Name == name && !set.Contains(p.Operator)) );

            return rule;

        }

        public static Rule<IRqlFilter> IfTypeNotBy(this Rule<IRqlFilter> rule, Type type, params RqlOperator[] allowed)
        {

            var set = new HashSet<RqlOperator>(allowed);

            rule.If(f => f.AtLeastOne(p => p.DataType == type && !set.Contains(p.Operator)));

            return rule;

        }


        public static Rule<IRqlFilter> IfOutOfRange( this Rule<IRqlFilter> rule, TimeSpan range )
        {

            bool Check( IRqlPredicate predicate )
            {

                if( predicate.DataType == typeof(DateTime) && predicate.Operator == RqlOperator.Between )
                {
                    var from = (DateTime)predicate.Values[0];
                    var to   = (DateTime)predicate.Values[1];

                    var span = to - from;

                    return span > range;
                }

                return false;

            }

            rule.If(f => f.AtLeastOne(Check));

            return rule;

        }


        public static Rule<IRqlFilter<TEntity>> IfOutOfRange<TEntity,TValue>(this Rule<IRqlFilter<TEntity>> rule, Expression<Func<TEntity,TValue>> prop, TimeSpan range) where TEntity : class, IModel
        {

            if (prop == null) throw new ArgumentNullException(nameof(prop));

            if (prop.Body == null)
                throw new ArgumentNullException(nameof(prop));

            if (!(prop.Body is MemberExpression))
                throw new ArgumentException("Targets of a rule must be a field or a property");
            
            var propExpr = (MemberExpression)prop.Body;
            var name     = propExpr.Member.Name;


            bool Check( IRqlPredicate predicate )
            {

                if( predicate.DataType == typeof(DateTime) && predicate.Operator == RqlOperator.Between && predicate.Target.Name == name )
                {
                    var from = (DateTime)predicate.Values[0];
                    var to   = (DateTime)predicate.Values[1];

                    var span = to - from;

                    return span > range;
                }

                return false;

            }

            rule.If(f => f.AtLeastOne(Check));

            return rule;

        }

        public static Rule<IRqlFilter<TEntity>> IfOutOfRange<TEntity, TValue>(this Rule<IRqlFilter<TEntity>> rule, Expression<Func<TEntity, TValue>> prop, int range) where TEntity : class, IModel
        {

            if (prop == null) throw new ArgumentNullException(nameof(prop));

            if (prop.Body == null)
                throw new ArgumentNullException(nameof(prop));

            if (!(prop.Body is MemberExpression))
                throw new ArgumentException("Targets of a rule must be a field or a property");

            var propExpr = (MemberExpression)prop.Body;
            var name     = propExpr.Member.Name;


            bool Check(IRqlPredicate predicate)
            {

                if( predicate.DataType == typeof(int) && predicate.Operator == RqlOperator.Between && predicate.Target.Name == name )
                {

                    var from = (int)predicate.Values[0];
                    var to   = (int)predicate.Values[1];

                    var span = to - from;

                    return span > range;
                }

                return false;

            }

            rule.If(f => f.AtLeastOne(Check));

            return rule;

        }


        public static Rule<IRqlFilter<TEntity>> IfOutOfRange<TEntity, TValue>(this Rule<IRqlFilter<TEntity>> rule, Expression<Func<TEntity, TValue>> prop, long range) where TEntity : class, IModel
        {

            if (prop == null) throw new ArgumentNullException(nameof(prop));

            if (prop.Body == null)
                throw new ArgumentNullException(nameof(prop));

            if (!(prop.Body is MemberExpression))
                throw new ArgumentException("Targets of a rule must be a field or a property");

            var propExpr = (MemberExpression)prop.Body;
            var name     = propExpr.Member.Name;


            bool Check(IRqlPredicate predicate)
            {

                if (predicate.DataType == typeof(long) && predicate.Operator == RqlOperator.Between && predicate.Target.Name == name)
                {

                    var from = (long)predicate.Values[0];
                    var to   = (long)predicate.Values[1];

                    var span = to - from;

                    return span > range;
                }

                return false;

            }

            rule.If(f => f.AtLeastOne(Check));

            return rule;

        }


        public static Rule<IRqlFilter<TEntity>> IfOutOfRange<TEntity, TValue>(this Rule<IRqlFilter<TEntity>> rule, Expression<Func<TEntity, TValue>> prop, decimal range) where TEntity : class, IModel
        {

            if (prop == null) throw new ArgumentNullException(nameof(prop));

            if (prop.Body == null)
                throw new ArgumentNullException(nameof(prop));

            if (!(prop.Body is MemberExpression))
                throw new ArgumentException("Targets of a rule must be a field or a property");

            var propExpr = (MemberExpression)prop.Body;
            var name     = propExpr.Member.Name;


            bool Check(IRqlPredicate predicate)
            {

                if (predicate.DataType == typeof(decimal) && predicate.Operator == RqlOperator.Between && predicate.Target.Name == name)
                {

                    var from = (decimal)predicate.Values[0];
                    var to   = (decimal)predicate.Values[1];

                    var span = to - from;

                    return span > range;
                }

                return false;

            }

            rule.If(f => f.AtLeastOne(Check));

            return rule;

        }


        public static Rule<IRqlFilter> IfOutOfRange(this Rule<IRqlFilter> rule, int range )
        {

            bool Check(IRqlPredicate predicate)
            {

                if( predicate.DataType == typeof(int) && predicate.Operator == RqlOperator.Between )
                {
                    var from = (int)predicate.Values[0];
                    var to   = (int)predicate.Values[1];

                    var diff = to-from;

                    return  Math.Abs(diff) > range;
                }

                return false;

            }

            rule.If(f => f.AtLeastOne(Check));

            return rule;

        }


        public static Rule<IRqlFilter> IfOutOfRange(this Rule<IRqlFilter> rule, long range)
        {

            bool Check(IRqlPredicate predicate)
            {

                if (predicate.DataType == typeof(long) && predicate.Operator == RqlOperator.Between)
                {
                    var from = (long)predicate.Values[0];
                    var to   = (long)predicate.Values[1];

                    var diff = to - from;

                    return Math.Abs(diff) > range;
                }

                return false;

            }

            rule.If(f => f.AtLeastOne(Check));

            return rule;

        }


        public static Rule<IRqlFilter> IfOutOfRange(this Rule<IRqlFilter> rule, decimal range)
        {

            bool Check(IRqlPredicate predicate)
            {

                if (predicate.DataType == typeof(decimal) && predicate.Operator == RqlOperator.Between)
                {
                    var from = (decimal)predicate.Values[0];
                    var to   = (decimal)predicate.Values[1];

                    var diff = to - from;

                    return Math.Abs(diff) > range;
                }

                return false;

            }

            rule.If(f => f.AtLeastOne(Check));

            return rule;

        }


        public static Rule<IRqlFilter> IfNotMinLength( this Rule<IRqlFilter> rule, int minLength, params RqlOperator[] restricted )
        {

            var set = new HashSet<RqlOperator>(restricted);

            bool Check(IRqlPredicate predicate)
            {

                if( predicate.DataType == typeof(string) && set.Contains(predicate.Operator) )
                {


                    var to   = "";
                    var from = (string)predicate.Values[0];
                    if( predicate.Values.Count > 1 )
                        to = (string)predicate.Values[1];

                    if (!string.IsNullOrWhiteSpace(from) && from.Length < minLength)
                        return true;

                    if( !string.IsNullOrWhiteSpace(to) && to.Length < minLength )
                        return true;

                }

                return false;

            }

            rule.If(f => f.AtLeastOne(Check));

            return rule;

        }


    }


}
