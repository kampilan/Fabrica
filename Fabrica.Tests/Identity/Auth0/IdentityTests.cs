using System.Drawing;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Configuration.Yaml;
using Fabrica.Identity;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Realtime;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Fabrica.Tests.Identity.Auth0
{

    [TestFixture]
    public class IdentityTests
    {


        [OneTimeSetUp]
        public async Task Setup()
        {

            var maker = new WatchFactoryBuilder();
            maker.UseRealtime();
            maker.UseLocalSwitchSource()
                .WhenNotMatched(Level.Debug, Color.Aqua);

            maker.Build();


            var cb = new ConfigurationBuilder();
            cb.AddUserSecrets<IdentityTests>();
            cb.AddYamlFile("identity-tests.yml");
            TheConfiguration = cb.Build();


            var builder = new ContainerBuilder();

            var module = TheConfiguration.Get<IdentityModule>();
            builder.RegisterModule(module);

            TheContainer = await builder.BuildAndStart();

        }


        [OneTimeTearDown]
        public void Teardown()
        {

            TheContainer?.Dispose();
            TheContainer = null;

            WatchFactoryLocator.Factory.Stop();

        }


        private IConfiguration TheConfiguration { get; set; }
        private IContainer TheContainer { get; set; }


        [Test]
        public async Task Test0001_0100_GetAccessToken()
        {


            await using var scope = TheContainer.BeginLifetimeScope();

            var comp = scope.Resolve<IAccessTokenSource>();

            var token = await comp.GetToken();

            ClassicAssert.IsNotEmpty(token);
            ClassicAssert.IsFalse(comp.HasExpired);

        }


        [Test]
        public async Task Test0100_0200_AddUser()
        {

            await using var scope = TheContainer.BeginLifetimeScope();

            var comp = scope.Resolve<IIdentityProvider>();


            var request = new SyncUserRequest
            {
                NewEmail = "moring.gabby@gmail.com",
                NewFirstName = "Gabby",
                NewLastName = "Moring"
            };


            var result = await comp.SyncUser(request);

            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsTrue(result.Created);
            ClassicAssert.IsNotEmpty(result.IdentityUid);
            ClassicAssert.IsNotEmpty(result.Password);


            var request2 = new SyncUserRequest
            {
                IdentityUid = result.IdentityUid,
                CurrentEmail = "moring.gabby@gmail.com",
                NewEmail = "her@gabbymoring.com",
                NewFirstName = "Gabriela",
                NewLastName = "Moring"
            };



            var result2 = await comp.SyncUser(request2);

            ClassicAssert.IsNotNull(result2);
            ClassicAssert.IsFalse(result2.Created);
            ClassicAssert.IsNotEmpty(result2.IdentityUid);
            ClassicAssert.IsEmpty(result2.Password);


        }


    }

}
