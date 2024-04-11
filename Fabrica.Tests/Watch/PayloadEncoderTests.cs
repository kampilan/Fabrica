using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using Dumpify;
using Fabrica.Test.Models.Patch;
using Fabrica.Utilities.Text;
using Fabrica.Watch.Sink;
using Fabrica.Watch.Utilities;
using NUnit.Framework;

namespace Fabrica.Tests.Watch;


[TestFixture]
public class PayloadEncoderTests
{


    [Test]
    public void Test_08200_0100_ShouldRoundTripJson()
    {

        var model = new Person
        {
            Uid = Base62Converter.NewGuid(),
            FirstName = "Gabby",
            MiddleName = "Marie",
            LastName = "Moring",
            Email = "moring.gabby@gmail.com",
            Gender = Person.GenderKind.Female,
            BirthDate = new DateTime(2009, 4, 13, 13, 30, 0),
            Salary = 25123.00M,
            PhoneNumber = "352-555-1212"

        };

        var json1 = JsonSerializer.Serialize(model);

        var base64 = WatchPayloadEncoder.Encode(json1);

        var json2 = WatchPayloadEncoder.DecodeToString(base64);

        Assert.AreEqual(json1,json2);


    }


    [Test]
    public void Test_08200_0200_ShouldBeInteroperable()
    {

        var model = new Person
        {
            Uid = Base62Converter.NewGuid(),
            FirstName = "Gabby",
            MiddleName = "Marie",
            LastName = "Moring",
            Email = "moring.gabby@gmail.com",
            Gender = Person.GenderKind.Female,
            BirthDate = new DateTime(2009, 4, 13, 13, 30, 0),
            Salary = 25123.00M,
            PhoneNumber = "352-555-1212"

        };

        var json1 = JsonSerializer.Serialize(model);

        var base64 = WatchPayloadEncoder.Encode(json1);

        var raw = Convert.FromBase64String(base64);
        var json2 = Encoding.ASCII.GetString(raw);


        Assert.AreEqual(json1, json2);


    }


    [Test]
    public void Test_08200_0300_ShouldBeInteroperable()
    {

        var model = new Person
        {
            Uid = Base62Converter.NewGuid(),
            FirstName = "Gabby",
            MiddleName = "Marie",
            LastName = "Moring",
            Email = "moring.gabby@gmail.com",
            Gender = Person.GenderKind.Female,
            BirthDate = new DateTime(2009, 4, 13, 13, 30, 0),
            Salary = 25123.00M,
            PhoneNumber = "352-555-1212"

        };

        var json1 = JsonSerializer.Serialize(model);

        var buf = Encoding.ASCII.GetBytes(json1);
        var base64 = Convert.ToBase64String(buf);

        var json2 = WatchPayloadEncoder.DecodeToString(base64);

        Assert.AreEqual(json1, json2);


    }


    [Test]
    public void Test_08200_0400_ShouldHandleBadObject()
    {

        var bad = new BadObject();

        var json = JsonSerializer.Serialize(bad,JsonWatchObjectSerializer.WatchOptions);

        Assert.IsNotEmpty(json);


    }




    [Test]
    public void Test_08200_0500_ShouldBeFast()
    {

        var model = new Person
        {
            Uid = Base62Converter.NewGuid(),
            FirstName = "Gabby",
            MiddleName = "Marie",
            LastName = "Moring",
            Email = "moring.gabby@gmail.com",
            Gender = Person.GenderKind.Female,
            BirthDate = new DateTime(2009, 4, 13, 13, 30, 0),
            Salary = 25123.00M,
            PhoneNumber = "352-555-1212"

        };


        var sw = new Stopwatch();
        sw.Start();
        for (var i = 0; i < 10000; i++)
        {

            var json1 = JsonSerializer.Serialize(model);
            var base64 = WatchPayloadEncoder.Encode(json1);
            var json2 = WatchPayloadEncoder.DecodeToString(base64);

        }
        sw.Stop();

        var ticks = sw.ElapsedTicks;
        var avg = sw.ElapsedTicks / 10000;

        Console.Out.WriteLine( $"Total: {ticks} tick(s)" );
        Console.Out.WriteLine( $"Avg: {avg} tick(s)" );

    }


    [Test]
    public void Test_08200_0500_ShouldDumpToStreamWriter()
    {

        var company = new Company
        {
            Uid = Base62Converter.NewGuid(),
            Name = "Pond Hawk Technologies Inc.",
            Address1 = "10161 Vancouver Rd",
            City = "Spring Hill",
            State = "FL",
            Zip = "34608",
            EmployeeCount = 1,
            MainPhone = "972-468-0990"
        };


        var parent = new Person
        {
            Uid = Base62Converter.NewGuid(),
            Parent = company,
            FirstName = "James",
            MiddleName = "Peter",
            LastName = "Moring",
            Email = "me@jamesmoring.com",
            Gender = Person.GenderKind.Male,
            BirthDate = new DateTime(1962, 1, 21, 7, 0, 0),
            Salary = 123456.00M,
            PhoneNumber = "214-228-9941"
        };

        company.Employees.Add(parent);


        var payload = company.DumpText();

        Assert.IsNotNull(payload);
        Assert.IsNotEmpty(payload);


        var sw = new Stopwatch();


//        var ops = new JsonSerializerOptions(JsonSerializerOptions.Default);
//        ops.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        sw.Reset();
        sw.Start();
        for (var i = 0; i < 10000; i++)
        {
            var json1 = JsonSerializer.Serialize(company, JsonWatchObjectSerializer.WatchOptions );
            var base64 = WatchPayloadEncoder.Encode(json1);
            var p2 = WatchPayloadEncoder.DecodeToString(base64);

        }
        sw.Stop();


        Console.Out.WriteLine($"JSON Total: {sw.ElapsedTicks} tick(s)");
        Console.Out.WriteLine($"JSON   Avg: {sw.ElapsedTicks/10000} tick(s)");


        sw.Reset();
        sw.Start();
        for (var i = 0; i < 10000; i++)
        {
            var p = company.DumpText();
            var base64 = WatchPayloadEncoder.Encode(p);
            var p2 = WatchPayloadEncoder.DecodeToString(base64);

        }
        sw.Stop();

        Console.Out.WriteLine($"Dump Total: {sw.ElapsedTicks} tick(s)");
        Console.Out.WriteLine($"Dump   Avg: {sw.ElapsedTicks / 10000} tick(s)");


    }







}

public class BadObject
{

    public string Test { get; set; } = null;
    public Stream Bad { get; set; } = new MemoryStream();

}