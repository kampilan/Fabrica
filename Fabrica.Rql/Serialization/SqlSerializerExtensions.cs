/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


// ReSharper disable UnusedMember.Global


using System.Globalization;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;

namespace Fabrica.Rql.Serialization;

public static class SqlSerializerExtensions
{


    static SqlSerializerExtensions()
    {

        // ***************************************************************************
        var operatorMap = new Dictionary<RqlOperator, KindSpec>();

        object DefaultKindFormatter(object o) => o;

        operatorMap[RqlOperator.Equals] = new KindSpec { Operation = "{0} = {1}", Style = ValueStyle.Single, Formatter = DefaultKindFormatter };
        operatorMap[RqlOperator.NotEquals] = new KindSpec { Operation = "{0} <> {1}", Style = ValueStyle.Single, Formatter = DefaultKindFormatter };
        operatorMap[RqlOperator.Contains] = new KindSpec { Operation = "{0} like {1}", Style = ValueStyle.Single, Formatter = _containsFormatter };
        operatorMap[RqlOperator.StartsWith] = new KindSpec { Operation = "{0} like {1}", Style = ValueStyle.Single, Formatter = _startsWithFormatter };
        operatorMap[RqlOperator.LesserThan] = new KindSpec { Operation = "{0} < {1}", Style = ValueStyle.Single, Formatter = DefaultKindFormatter };
        operatorMap[RqlOperator.GreaterThan] = new KindSpec { Operation = "{0} > {1}", Style = ValueStyle.Single, Formatter = DefaultKindFormatter };
        operatorMap[RqlOperator.LesserThanOrEqual] = new KindSpec { Operation = "{0} <= {1}", Style = ValueStyle.Single, Formatter = DefaultKindFormatter };
        operatorMap[RqlOperator.GreaterThanOrEqual] = new KindSpec { Operation = "{0} >= {1}", Style = ValueStyle.Single, Formatter = DefaultKindFormatter };
        operatorMap[RqlOperator.Between] = new KindSpec { Operation = "{0} between {1} and {2}", Style = ValueStyle.Pair, Formatter = DefaultKindFormatter };
        operatorMap[RqlOperator.In] = new KindSpec { Operation = "{0} in ({1})", Style = ValueStyle.Enumeration, Formatter = DefaultKindFormatter };
        operatorMap[RqlOperator.NotIn] = new KindSpec { Operation = "{0} not in ({1})", Style = ValueStyle.Enumeration, Formatter = DefaultKindFormatter };

        OperatorMap = operatorMap;



        // ***************************************************************************
        var typeMap = new Dictionary<Type, TypeSpec>();

        string DefaultFormatter(object o) => o.ToString()??"";

        typeMap[typeof(string)]    = new TypeSpec { NeedsQuotes = true, Formatter = DefaultFormatter };
        typeMap[typeof(bool)]      = new TypeSpec { NeedsQuotes = false, Formatter = DefaultFormatter };
        typeMap[typeof(DateTime)]  = new TypeSpec { NeedsQuotes = true, Formatter = _dateTimeFormatter };
        typeMap[typeof(decimal)]   = new TypeSpec { NeedsQuotes = false, Formatter = DefaultFormatter };
        typeMap[typeof(short)]     = new TypeSpec { NeedsQuotes = false, Formatter = DefaultFormatter };
        typeMap[typeof(int)]       = new TypeSpec { NeedsQuotes = false, Formatter = DefaultFormatter };
        typeMap[typeof(long)]      = new TypeSpec { NeedsQuotes = false, Formatter = DefaultFormatter };

        TypeMap = typeMap;


    }

    private static string _dateTimeFormatter( object source)
    {

        if (source == null) throw new ArgumentNullException(nameof(source));

        if (source is not DateTime time )
            throw new InvalidOperationException($"Object of type: {source.GetType().FullName} can not be cast to a DateTime");

        var dtStr = time.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");

        return dtStr;

    }

    private static object _containsFormatter( object value )
    {

        if (value == null) throw new ArgumentNullException(nameof(value));

        return $"%{value}%";

    }

    private static object _startsWithFormatter( object value )
    {

        if (value == null) throw new ArgumentNullException(nameof(value));

        return $"{value}%";

    }


    private static IReadOnlyDictionary<RqlOperator, KindSpec> OperatorMap { get; }
    private static IReadOnlyDictionary<Type, TypeSpec> TypeMap { get; }


    private enum ValueStyle
    {
        Single,
        Pair,
        Enumeration
    };

    private struct KindSpec
    {
        public string Operation;
        public ValueStyle Style;
        public Func<object, object> Formatter;
    }


    private struct TypeSpec
    {
        public bool NeedsQuotes;
        public Func<object, string> Formatter;
    }



    public static (string sql, object[]? parameters) ToSqlQuery<TEntity>( this RqlFilterBuilder<TEntity> builder, IEnumerable<string>? projection=null ) where TEntity : class
    {

        var tableName = typeof(TEntity).Name;

        var result = ToSqlQuery(builder, tableName, projection);

        return result;

    }

    public static (string sql, object[]? parameters) ToSqlQuery( this IRqlFilter builder, string tableName, IEnumerable<string>? projection=null, bool indexed=true )
    {
            
        if (tableName == null) throw new ArgumentNullException(nameof(tableName));


        projection ??= builder.HasProjection ? builder.Projection : new[] {"*"};


        var pair = builder.ToSqlWhere(indexed);

        if( string.IsNullOrWhiteSpace(pair.sql) && builder.RowLimit > 0 )
        {
            var query = $"select {string.Join(",",projection)} from {tableName} limit {builder.RowLimit}";
            return (query, new object[]{});
        }

        if( string.IsNullOrWhiteSpace(pair.sql) )
        {
            var query = $"select {string.Join(",", projection)} from {tableName}";
            return (query, new object[] { });
        }
            
        if( builder.RowLimit > 0)
        {
            var query = $"select {string.Join(",", projection)} from {tableName} where {pair.sql} limit {builder.RowLimit}";
            return (query, pair.parameters);
        }
        else
        {
            var query = $"select {string.Join(",", projection)} from {tableName} where {pair.sql}";
            return (query, pair.parameters);
        }




    }


    public static (string sql, object[]? parameters) ToSqlWhere( this IRqlFilter builder, bool indexed=true )
    {

        string Build( int index )
        {
            return indexed ? $"{{{index}}}" : "?";
        }

        var parameters = new List<object>();
        var parts = new List<string>();

        foreach (var op in builder.Criteria)
        {

            if (!OperatorMap.TryGetValue(op.Operator, out var kindSpec))
                throw new Exception($"{op.Operator} is not a supported operation");


            if (!TypeMap.TryGetValue(op.DataType, out var typeSpec))
                throw new Exception($"{op.DataType.Name} is not a supported data type");


            if (kindSpec.Style == ValueStyle.Single)
            {

                var value = kindSpec.Formatter(op.Values[0]);

                var actValue = value;
                if (value is IConvertible convertible)
                    actValue = convertible.ToType(op.DataType, CultureInfo.CurrentCulture);

                parameters.Add(actValue);

                parts.Add(string.Format(kindSpec.Operation, op.Target, Build(parameters.Count-1) ));

            }
            else if (kindSpec.Style == ValueStyle.Pair)
            {

                var value1 = kindSpec.Formatter(op.Values[0]);
                var actValue1 = value1;
                if (value1 is IConvertible convertible1)
                    actValue1 = convertible1.ToType(op.DataType, CultureInfo.CurrentCulture);

                parameters.Add(actValue1);

                var value2 = kindSpec.Formatter(op.Values[1]);
                var actValue2 = value2;
                if (value2 is IConvertible convertible2)
                    actValue2 = convertible2.ToType(op.DataType, CultureInfo.CurrentCulture);

                parameters.Add(actValue2);

                parts.Add( string.Format(kindSpec.Operation, op.Target, Build(parameters.Count-2), Build(parameters.Count-1)));

            }
            else
            {

                var values = new List<string>();

                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (typeSpec.NeedsQuotes)
                    values.AddRange(op.Values.Select(v => $"'{typeSpec.Formatter(kindSpec.Formatter(v))}'"));
                else
                    values.AddRange(op.Values.Select(v => typeSpec.Formatter(kindSpec.Formatter(v))));

                parts.Add(string.Format(kindSpec.Operation, op.Target, string.Join(",", values)));


            }


        }


        if (parts.Count == 0)
            return ("",null);


        var join = string.Join(" and ", parts);

        return (join,parameters.ToArray());

    }


}