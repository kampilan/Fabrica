using System;
using Fabrica.Rql.Parser;

namespace Fabrica.Rql.Builder
{


    public enum OperandKind { Single, From, To, List, ListOfInt, ListOfLong }


    [AttributeUsage(AttributeTargets.Property)]
    public class CriterionAttribute: Attribute
    {

        public string Name { get; set; } = "";

        public RqlOperator Operation { get; set; } = RqlOperator.NotSet;

        public OperandKind Operand { get; set; } = OperandKind.Single;

    }


}
