﻿using System.Collections.Generic;
using System.Linq;
using Fabrica.Press.Generation;
using Fabrica.Press.Generation.DataSources;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Fabrica.Tests.Press
{


    [TestFixture]
    public class MergeDataSourceTests
    {


        [Test]
        public void Test0900_0100_CreateMergeModel()
        {

            var builder = new MergeModelBuilder();
            builder.Single( new { Name="James", Age=58 } );
            builder.Single( new { Name = "Wilma", Age = 45 } );

            var sources = builder.Sources;

            Assert.AreEqual(2, sources.Count );

            var json = builder.ToJson(true);

            Assert.IsNotNull(json);
            Assert.IsNotEmpty(json);

        }


        [Test]
        public void Test0900_0200_ManyCreateMergeModel()
        {

            var builder = new MergeModelBuilder();
            builder.Many( new List<object> { new { Name = "James", Age = 58 }, new { Name = "Wilma", Age = 45 } }, "People" );

            var sources = builder.Sources;

            Assert.AreEqual(1, sources.Count);

            var json = builder.ToJson(true);

            Assert.IsNotNull(json);
            Assert.IsNotEmpty(json);


            var ds = JsonConvert.DeserializeObject<List<JsonDataSource>>(json);

            Assert.IsNotNull( ds );

            var ms = ds.FirstOrDefault();
            while (ms.MoveNext())
            {

                ms.TryGetValue("Name", out var name);
                ms.TryGetValue("Age", out var age);

                Assert.IsNotNull( name );
                Assert.IsNotNull( age );

            }

        }

        [Test]
        public void Test0900_0300_ManyCreateDict()
        {

            var d1 = new Dictionary<string,object>();
            d1["FullName"] = "James Moring";
            d1["Age"] = 58;

            var d2 = new Dictionary<string, object>();
            d2["FullName"] = "Wilma Laluna";
            d2["Age"]      = 45;

            var ds = new DictionaryDataSource( "People", d1, d2 ); 

            while (ds.MoveNext())
            {

                ds.TryGetValue("FullName", out var name);
                ds.TryGetValue("Age", out var age);

                Assert.IsNotNull(name);
                Assert.IsNotNull(age);

            }

        }


        [Test]
        public void Test0900_0400_ManyCreateExpando()
        {

            var o1 = new Person { FirstName = "James", LastName="Moring", Age = 58};
            var o2 = new Person { FirstName = "Wilma", LastName = "Laluna", Age = 45 };

            var ds = new ModelDataSource<Person>( o1, o2 );
            ds.AddDerivedProperty("FullName", o => $"{o.FirstName} {o.LastName}");

            var region = ds.Region;

            while (ds.MoveNext())
            {

                ds.TryGetValue("FullName", out var name);
                ds.TryGetValue("Age", out var age);

                Assert.IsNotNull(name);
                Assert.IsNotNull(age);

            }

        }





    }

    public class Person
    {

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";

        public int Age { get; set; } = 0;

    }    



}
