using System.Data;

namespace Fabrica.Utilities.Sql;

public static class DbCommandExtensions
{


    public static void FromSqlRawList( this IDbCommand cmd, string indexed, IEnumerable<object> inputs )
    {

        var (sql, parameters) = Formatter.FromList( indexed, inputs );

        cmd.CommandType = CommandType.Text;
        cmd.CommandText = sql;

        foreach( var val in parameters )
        {
            var param = cmd.CreateParameter();
            param.Value = val;
            cmd.Parameters.Add(param);
        }


    }


    public static void FromSqlRaw( this IDbCommand cmd, string indexed, params object[] inputs )
    {

        var (sql, parameters) = Formatter.FromParams( indexed, inputs );

        cmd.CommandType = CommandType.Text;
        cmd.CommandText = sql;

        foreach (var val in parameters )
        {
            var param = cmd.CreateParameter();
            param.Value = val;
            cmd.Parameters.Add(param);
        }

    }


}