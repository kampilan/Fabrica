using System.Threading.Tasks;
using Autofac;
using Fabrica.Configuration.Yaml;
using Fabrica.Identity;
using Fabrica.Utilities.Drawing;
using Fabrica.Watch;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Fabrica.Tests.Identity
{

    [TestFixture]
    public class IdentityTests
    {


        [OneTimeSetUp]
        public void Setup()
        {

            var maker = new WatchFactoryBuilder();
//            maker.UseRealtime();
            maker.UseLocalSwitchSource()
                .WhenNotMatched(Level.Debug, Color.Aqua);

            maker.Build();


            var cb = new ConfigurationBuilder();
            cb.AddUserSecrets<IdentityTests>();
            cb.AddYamlFile("identity-tests.yml");
            TheConfiguration = cb.Build();


            var builder = new ContainerBuilder();

            var module = TheConfiguration.Get<IdentityModule>();
            builder.RegisterModule( module );

            TheContainer = builder.Build();

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

            Assert.IsNotEmpty(token);
            Assert.IsFalse( comp.HasExpired );

        }


        [Test]
        public async Task Test0100_0200_AddUser()
        {
            
            await using var scope = TheContainer.BeginLifetimeScope();

            var comp = scope.Resolve<IIdentityProvider>();


            var request = new SyncUserRequest
            {
                NewEmail     = "moring.gabby@gmail.com",
                NewFirstName = "Gabby",
                NewLastName  = "Moring"
            };


            var result = await comp.SyncUser( request );

            Assert.IsNotNull(result);
            Assert.IsTrue( result.Created);
            Assert.IsNotEmpty(result.IdentityUid);
            Assert.IsNotEmpty(result.Password);


            var request2 = new SyncUserRequest
            {
                IdentityUid  = result.IdentityUid,
                CurrentEmail = "moring.gabby@gmail.com",
                NewEmail     = "her@gabbymoring.com",
                NewFirstName = "Gabriela",
                NewLastName  = "Moring"
            };



            var result2 = await comp.SyncUser( request2 );

            Assert.IsNotNull(result2);
            Assert.IsFalse(result2.Created);
            Assert.IsNotEmpty(result2.IdentityUid);
            Assert.IsEmpty(result2.Password);


        }


    }

}
