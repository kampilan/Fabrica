using System;
using System.Threading.Tasks;
using System.Xml.Schema;
using Autofac;
using Fabrica.Identity;
using NUnit.Framework;

namespace Fabrica.Tests.Identity
{

    [TestFixture]
    public class IdentityTests
    {


        [OneTimeSetUp]
        public void Setup()
        {

            var builder = new ContainerBuilder();

            builder.RegisterModule<IdentityModule>();

            TheContainer = builder.Build();

        }


        [OneTimeTearDown]
        public void Teardown()
        {

            TheContainer?.Dispose();
            TheContainer = null;

        }


        private IContainer TheContainer { get; set; }


        [Test]
        public async Task Test0001_0100_GetAccessToken()
        {

            using var scope = TheContainer.BeginLifetimeScope();

            var comp = scope.Resolve<IAccessTokenSource>();

            var token = await comp.GetToken();

            Assert.IsNotEmpty(token);
            Assert.IsFalse( comp.HasExpired );

        }


    }

}
