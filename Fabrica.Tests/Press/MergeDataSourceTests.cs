using System.Collections.Generic;
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

    }

    public class Person
    {

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";

        public int Age { get; set; } = 0;

    }    



}
