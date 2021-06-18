using System;
using Fabrica.Models.Support;
using NUnit.Framework;

namespace Fabrica.Tests.Models
{


    [TestFixture]
    public class DeltaPropertySetTests
    {


        [Test]
        public void Test0001_0100_PopulateAllNull()
        {

            var delta = new TestDelta();

            var dd = new DeltaPropertySet(delta);

            Assert.IsTrue(dd.Count == 0);

        }


        [Test]
        public void Test0001_0200_PopulateStrings()
        {

            var delta = new TestDelta
            {
                FirstName = "James",
                LastName  = "Moring"
            };

            var dd = new DeltaPropertySet(delta);

            Assert.IsTrue(dd.Count == 2);

        }

        [Test]
        public void Test0001_0300_PopulateNullables()
        {

            var delta = new TestDelta
            {
                Active = true,
                BirthDate = new DateTime(1962,1,21,7,0,0,0,0),
                Count = 23,
                Salary = 45678.90m
            };

            var dd = new DeltaPropertySet(delta);

            Assert.IsTrue(dd.Count == 4);

        }


        [Test]
        public void Test0001_0400_PopulateNullables()
        {

            var delta = new TestDelta
            {
                Active    = true,
                BirthDate = new DateTime(1962, 1, 21, 7, 0, 0, 0, 0),
                Count     = 23,
                Salary    = 45678.90m
            };

            var dd = delta.GetPropertySet();

            Assert.IsTrue(dd.Count == 4);

        }




    }


    public class TestDelta : BaseDelta
    {

        public bool? Active { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? BirthDate { get; set; }

        public int? Count { get; set; }

        public decimal? Salary { get; set; }


    }    


}
