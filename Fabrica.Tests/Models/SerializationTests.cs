using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Autofac;
using Bogus;
using System.Threading.Tasks;
using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Text;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using NUnit.Framework;
using System.Linq;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Fabrica.Test.Models.Patch;
using Person = Fabrica.Test.Models.Patch.Person;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Persistence.Patch;
using Fabrica.Rules;
using Fabrica.Test.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Module = Autofac.Module;
using Microsoft.Extensions.Options;
using Sprache;

namespace Fabrica.Tests.Models;

public class SerializationTests
{


    [OneTimeSetUp]
    public async Task Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource().WhenNotMatched(Level.Debug, Color.BurlyWood);

        maker.Build();


        var builder = new ContainerBuilder();

        builder.RegisterModule<TheSerializationModule>();

        TheContainer = await builder.BuildAndStart();


    }

    [OneTimeTearDown]
    public void Teardown()
    {

        TheContainer.Dispose();
        WatchFactoryLocator.Factory.Stop();

    }

    private Autofac.IContainer TheContainer { get; set; }

    private Company _buildCompany(int employees, bool asNew = false)
    {

        var compRules = new Faker<Company>();

        compRules
            .RuleFor(p => p.Uid, _ => Base62Converter.NewGuid())
            .RuleFor(c => c.Name, f => f.Company.CompanyName())
            .RuleFor(c => c.Address1, f => f.Address.StreetAddress())
            .RuleFor(c => c.Address2, f => f.Address.SecondaryAddress())
            .RuleFor(c => c.City, f => f.Address.City())
            .RuleFor(c => c.State, f => f.Address.State());

        var company = compRules.Generate();

        var personRules = new Faker<Test.Models.Patch.Person>();

        personRules
            .RuleFor(p => p.Uid, _ => Base62Converter.NewGuid())
            .RuleFor(p => p.FirstName, f => f.Name.FirstName())
            .RuleFor(p => p.LastName, f => f.Name.LastName());

        var emps = personRules.Generate(employees);

        company.Employees = emps;

        if (!asNew)
            company.Post();

        return company;

    }


    [Test]
    public void Test0002_0100_Serializes_Company()
    {

        var company = _buildCompany(2);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new ModelJsonTypeInfoResolver(),
            WriteIndented = true
        };


        var json = JsonSerializer.Serialize(company, options);

        Assert.IsNotNull(json);
        Assert.IsNotEmpty(json);
        Assert.AreNotEqual("{}", json );

    }


    [Test]
    public void Test0002_0200_Serializes_Company_Then_Deserializes()
    {


        var company = _buildCompany(2);

        var json = "{}";
        for (var i = 0; i < 100; i++)
        {

            using (var scope = TheContainer.BeginLifetimeScope())
            {

                var options = scope.Resolve<JsonSerializerOptions>();
                json = JsonSerializer.Serialize(company, options);

                Assert.IsNotNull(json);
                Assert.IsNotEmpty(json);
                Assert.AreNotEqual("{}", json);
            }

        }


        using (var scope = TheContainer.BeginLifetimeScope())
        {

            var options = scope.Resolve<JsonSerializerOptions>();
            var rt = JsonSerializer.Deserialize<Company>(json, options);

            Assert.IsNotNull(rt);
            Assert.AreNotEqual(company.Uid, rt.Uid);
            Assert.IsEmpty(rt.GetDelta());
            Assert.AreEqual(company.Name, rt.Name);

            rt.Name = "Jones";

            Assert.IsNotEmpty(rt.GetDelta());

        }


    }



}


public class TheSerializationModule : Module
{

    protected override void Load(ContainerBuilder builder)
    {

        builder.AddCorrelation();

        builder.UseRules();

        builder.RegisterAutoMapper(typeof(IAssemblyFinder).Assembly);

        builder.UseModelMeta()
            .AddModelMetaSource(typeof(IAssemblyFinder).Assembly);

        builder.UseMediator(typeof(IAssemblyFinder).Assembly);

        builder.UsePatchResolver();


        var ops = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            TypeInfoResolver = new ModelJsonTypeInfoResolver(),
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            PropertyNameCaseInsensitive = true
        };

        var services = new ServiceCollection();
        services.Configure<JsonOptions>(o =>
        {
            o.SerializerOptions.PropertyNamingPolicy = ops.PropertyNamingPolicy;
            o.SerializerOptions.DefaultIgnoreCondition = ops.DefaultIgnoreCondition;
            o.SerializerOptions.DictionaryKeyPolicy = ops.DictionaryKeyPolicy;
            o.SerializerOptions.ReferenceHandler = ops.ReferenceHandler;
            o.SerializerOptions.PropertyNameCaseInsensitive = ops.PropertyNameCaseInsensitive;
            o.SerializerOptions.UnmappedMemberHandling = ops.UnmappedMemberHandling;
            o.SerializerOptions.WriteIndented = ops.WriteIndented;
            o.SerializerOptions.TypeInfoResolver = ops.TypeInfoResolver;
        });

        builder.RegisterInstance(ops)
            .AsSelf()
            .SingleInstance();

    }


}





public class ModelJsonTypeInfoResolver: DefaultJsonTypeInfoResolver
{

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {


        var typeInfo = base.GetTypeInfo(type, options);

        if( typeInfo.Kind != JsonTypeInfoKind.Object )
            return typeInfo;

        var mAttr = type.GetCustomAttribute<ModelAttribute>();
        if( mAttr is null )
            return typeInfo;

        foreach( var prop in typeInfo.Properties )
        {

            ModelMetaAttribute attr = null;
            if (prop.AttributeProvider is not null && prop.AttributeProvider.IsDefined(typeof(ModelMetaAttribute), false))
                attr = prop.AttributeProvider.GetCustomAttributes(typeof(ModelMetaAttribute), false).Cast<ModelMetaAttribute>().First();

            if( attr is null || attr.Scope == PropertyScope.Exclude )
            {
                prop.Get = null;
                prop.Set = null;
            }
            else if (attr.Scope == PropertyScope.Immutable)
            {
                prop.Set = null;
            }
            else if (prop.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
            {

                prop.ShouldSerialize = Should;

                static bool Should(object o, [CanBeNull] object value)
                {
                    var result = true;
                    if (value is IEnumerable enumerable)
                    {
                        var e = enumerable.GetEnumerator();
                        using var unknown = e as IDisposable;
                        result = e.MoveNext();
                    }
                    return result;
                }

            }
            else
            {
                var field = $"_{char.ToLowerInvariant(prop.Name[0])}{prop.Name.Substring(1)}";
                var fi = type.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
                prop.Set = fi is not null?fi.SetValue:null;
            }


        }


        return typeInfo;

    }

}

public class MetaJsonTypeInfoResolver(IEnumerable<JsonTypeInfo> infos) : DefaultJsonTypeInfoResolver
{


    private IDictionary<Type,JsonTypeInfo> _types = new Dictionary<Type, JsonTypeInfo>( infos.ToDictionary(p=>p.Type ));


    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        return _types[type];
    }


}

