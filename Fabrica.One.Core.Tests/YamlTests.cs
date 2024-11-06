using System.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Fabrica.One.Core.Tests;


[TestFixture]
public class YamlTests
{

    [Test]
    public void Test0200_0100_LoadYaml()
    {

        string yaml;
        using (var strm = new StreamReader(new FileStream("e:/testing/fake-kampilan-auth0.yml", FileMode.Open, FileAccess.Read)))
            yaml = strm.ReadToEnd();

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();

        var p = deserializer.Deserialize<Model>(yaml);

        ClassicAssert.IsNotNull(p);
        ClassicAssert.IsNotNull(p.Proxy);

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var py = serializer.Serialize(p.Proxy);


        ClassicAssert.IsNotEmpty(py);

        var po = deserializer.Deserialize<object>(py);

        ClassicAssert.IsNotNull(po);


    }


}

public class Model
{

    public bool RunningOnEc2 { get; set; }
    public string ProfileName { get; set; } = "";
    public string RegionName { get; set; } = "";

    public object Proxy { get; set; } = new ();


}