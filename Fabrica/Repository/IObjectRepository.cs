using System.Text;

namespace Fabrica.Repository;

public interface IObjectRepository
{

    Task<string> CreateKey(string extension = "", DateTime date = default);

    Task<bool> Get( Action<GetOptions> builder, CancellationToken token = default );
    Task<string> Put( Action<PutOptions> builder, CancellationToken token = default );

}


public class GetOptions
{

    public string Key { get; set; } = "";
    public string Url { get; set; } = "";

    public Stream Content { get; set; }

    public bool Rewind { get; set; } = true;
    public bool Close { get; set; } = false;

}


public class PutOptions
{

    public Stream Content { get; set; }

    public string Extension { get; set; } = "";

    public string ContentType { get; set; } = "";

    public string Key { get; set; } = "";

    public bool Transient { get; set; } = false;

    public string Url { get; set; } = "";

    public bool Rewind { get; set; } = true;


    public void FromString( string source )
    {

        Content = new MemoryStream();

        using var writer = new StreamWriter(Content, Encoding.Default, 1024, true);

        writer.Write(source);
        writer.Flush();

    }


}
