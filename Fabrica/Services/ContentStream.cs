using System.Net.Http.Headers;
using System.Net.Mime;

namespace Fabrica.Services;

public class ContentStream: MemoryStream
{

    public MediaTypeHeaderValue ContentType { get; set; } = new (MediaTypeNames.Application.Json);

}