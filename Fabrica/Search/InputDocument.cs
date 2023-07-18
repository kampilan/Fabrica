namespace Fabrica.Search;

public class InputDocument
{

    public InputKey Key { get; set; }

    public Dictionary<string,string> Keywords { get; set; } = new ();

}