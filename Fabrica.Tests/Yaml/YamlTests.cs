using YamlDotNet.RepresentationModel;

namespace Fabrica.Tests.Yaml;

public class YamlTests
{


    public void Test0120_0010_ParseTest()
    {

    }


}

public class Model
{

    public string Name { get; set; } = "";
    public int Age { get; set; } = 0;

    public YamlMappingNode Configuration { get; set; } = new ();


}