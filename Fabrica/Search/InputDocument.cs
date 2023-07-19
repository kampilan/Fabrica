
namespace Fabrica.Search;

public class InputDocument
{

    public string Entity { get; set; } = "";

    public long Id { get; set; } = 0;

    public Dictionary<string,string> Keywords { get; set; } = new ();

}

