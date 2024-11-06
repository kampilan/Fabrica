using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Fabrica.Exceptions;
using Fabrica.One.Plan;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Fabrica.One.Core.Tests;

[TestFixture]
public class JsonPlanFactoryTests: BaseOneTest
{


    [Test]
    public async Task Test0110_No_Appliance_Plan_Should_Not_Be_Valid()
    {

        var source  = await NoAppliancePlanSource();
        var factory = GetFactory();

        ClassicAssert.ThrowsAsync<PredicateException>( async () => await factory.Create(source) );

    }


    [Test]
    public async Task Test0111_Empty_Plan_Should_Not_Be_Valid()
    {

        var source  = await EmptyJsonPlanSource();
        var factory = GetFactory();

        ClassicAssert.ThrowsAsync<PredicateException>(async () => await factory.Create(source));

    }

    [Test]
    public async Task Test0112_Bad_Plan_Should_Not_Be_Valid()
    {

        var source  = await BadJsonPlanSource();
        var factory = GetFactory();

        ClassicAssert.ThrowsAsync<PredicateException>(async () => await factory.Create(source));

    }


    [Test]
    public async Task Test0120_Load_Valid_Plan_One_Deployments()
    {

        var source = await OneAppliancePlanSourceWithNoChecksum();
        var factory = GetFactory();

        var plan = await factory.Create(source);

        ClassicAssert.IsNotNull(plan);
        ClassicAssert.IsTrue(plan.Deployments.Count == 1);
        var d1 = plan.Deployments[0];

        ClassicAssert.IsNotNull(d1.Configuration);

        var cd = d1.Configuration.Deserialize<Dictionary<string, object>>();
        ClassicAssert.IsNotNull(cd);
        ClassicAssert.IsNotEmpty(cd);

        var json = JsonSerializer.Serialize(plan);



    }



    [Test]
    public async Task Test0130_Load_Valid_Plan_Multi_Deployments()
    {

        var source = await MultiAppliancePlanSource();
        var factory = GetFactory();

        var plan = await factory.Create(source);

        ClassicAssert.IsNotNull(plan);
        ClassicAssert.IsTrue(plan.Deployments.Count == 2);
        var d1 = plan.Deployments[1];

        ClassicAssert.IsNotNull(d1.Configuration);

        var cd = d1.Configuration.Deserialize<Dictionary<string, object>>();
        ClassicAssert.IsNotNull(cd);
        ClassicAssert.IsNotEmpty(cd);


        var json2 = d1.Configuration.ToString();
        ClassicAssert.IsNotEmpty(json2);


    }


    [Test]
    public async Task Test0140_Write_Plan_To_Disk()
    {

        var source  = await OneAppliancePlanSourceWithGoodChecksum();
        var factory = GetFactory();

        var plan = await factory.Create(source);

        ClassicAssert.IsNotNull(plan);


        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize( (PlanImpl)plan, options );

        
        await using var fs = new FileStream("e:/fabrica-one/mission-plan.json", FileMode.Create, FileAccess.Write);
        await using var writer = new StreamWriter(fs);

        await writer.WriteAsync(json);
        await writer.FlushAsync();

    }

}

public class Tester
{

    public JsonObject Config { get; set; } = new();


}    