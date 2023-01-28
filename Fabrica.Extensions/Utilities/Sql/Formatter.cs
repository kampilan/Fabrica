namespace Fabrica.Utilities.Sql;

public static class Formatter
{

        
    public static (string sql, object[] parameters) FromParams( string indexed, params object[] inputs )
    {

        var list    = inputs.ToList();
        var markers = list.Select(o => "?").Cast<object>().ToArray();

        var sql = string.Format(indexed, markers);

        return (sql, inputs);

    }


    public static (string sql, object[] parameters) FromList( string indexed, IEnumerable<object> inputs )
    {

        var list    = inputs.ToList();
        var markers = list.Select(o => "?").Cast<object>().ToArray();

        var sql = string.Format(indexed, markers);

        return (sql, list.ToArray());

    }


}