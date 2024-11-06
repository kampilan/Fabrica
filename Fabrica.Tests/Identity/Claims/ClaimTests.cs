using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Api.Support.Identity.Gateway;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Identity;
using Fabrica.Utilities.Container;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Fabrica.Tests.Identity.Claims;

[TestFixture]
public class ClaimTests
{

    protected class TheModule : Module
    {

        protected override void Load(ContainerBuilder builder)
        {

        }

    }


    [OneTimeSetUp]
    public async Task Setup()
    {

        var builder = new ContainerBuilder();

        var module = new TheModule();

        builder.RegisterModule(module);

        RootScope = await builder.BuildAndStart();

    }

    [OneTimeTearDown]
    public void Teardown()
    {

    }

    private ILifetimeScope RootScope { get; set; }


    private ClaimSetModel CreateClaimSet()
    {

        var set = new ClaimSetModel
        {

            AuthenticationType = "cookie",
            AuthenticationFlow = "AuthCode",
            Tenant = "Moring",
            Subject = "1234567890",
            AltSubject = "0987654321",
            UserName = "jmoring",
            GivenName = "James",
            FamilyName = "Moring",
            Name = "James Moring",
            Email = "me@jamesmoring.com",
            Picture = "https/gmail.com"

        };

        set.Roles.Add("User");
        set.Roles.Add("Admin");
        set.Roles.Add("SysAdmin");

        return set;

    }

    private IClaimSet CreateClaimSetAsIntf()
    {

        var set = new ClaimSetModel
        {

  //          AuthenticationType = "cookie",
//            AuthenticationFlow = "AuthCode",
//            Tenant = "Moring",
            Subject = "1234567890",
//            AltSubject = "0987654321",
            UserName = "jmoring",
            GivenName = "James",
            FamilyName = "Moring",
            Name = "James Moring",
//            Email = "me@jamesmoring.com",
//            Picture = "https/gmail.com"

        };

        set.Roles.Add("User");
        set.Roles.Add("Admin");
        set.Roles.Add("SysAdmin");

        return set;

    }



    [Test]
    public void Test8100_0100_CreateClaimIdentity()
    {

        var set = CreateClaimSet();

        var ci = new ClaimsIdentity();
        ci.Populate(set);
        
        var set2 = ci.ToPayload();

        ClassicAssert.IsNotNull(set2);
        ClassicAssert.AreEqual( set.AuthenticationType, set2.AuthenticationType);
        ClassicAssert.AreEqual(set.AuthenticationFlow, set2.AuthenticationFlow);
        ClassicAssert.AreEqual(set.Tenant, set2.Tenant);
        ClassicAssert.AreEqual(set.Subject, set2.Subject);
        ClassicAssert.AreEqual(set.AltSubject, set2.AltSubject);
        ClassicAssert.AreEqual(set.UserName, set2.UserName);
        ClassicAssert.AreEqual(set.GivenName, set2.GivenName);
        ClassicAssert.AreEqual(set.FamilyName, set2.FamilyName);
        ClassicAssert.AreEqual(set.Name, set2.Name);
        ClassicAssert.AreEqual(set.Email, set2.Email);
        ClassicAssert.AreEqual(set.Picture, set2.Picture);


    }


    [Test]
    public void Test8100_0200_CreateClaimIdentityJson()
    {

        var set = CreateClaimSetAsIntf();

        var json = JsonSerializer.Serialize(set, set.GetType());

        ClassicAssert.IsNotNull(json);
        ClassicAssert.IsNotEmpty(json);

        var ci = new ClaimsIdentity();
        ci.Populate(json);


        ClassicAssert.IsNotNull(ci);
        ClassicAssert.AreEqual(set.AuthenticationType, ci.GetAuthType());
        ClassicAssert.AreEqual(set.AuthenticationFlow, ci.GetAuthFlow());
        ClassicAssert.AreEqual(set.Tenant, ci.GetTenant());
        ClassicAssert.AreEqual(set.Subject, ci.GetSubject());
        ClassicAssert.AreEqual(set.AltSubject, ci.GetAltSubject());
        ClassicAssert.AreEqual(set.UserName, ci.GetUserName());
        ClassicAssert.AreEqual(set.GivenName, ci.GetGivenName());
        ClassicAssert.AreEqual(set.FamilyName, ci.GetFamilyName());
        ClassicAssert.AreEqual(set.Name, ci.GetName());
        ClassicAssert.AreEqual(set.Email, ci.GetEmail());
        ClassicAssert.AreEqual(set.Picture, ci.GetPictureUrl());


    }


    [Test]
    public void Test8100_0300_CreateAndParseToken()
    {

        var key = RandomNumberGenerator.GetBytes(64);

        var strKey = Convert.ToBase64String(key);

        var set = CreateClaimSet();
        set.AuthenticationType = "Jwt";

        var encoder = new GatewayTokenJwtEncoder
        {
            TokenSigningKey = key,
            TokenTimeToLive = TimeSpan.FromSeconds(3600)
            
        };

        var token = encoder.Encode(set);

        ClassicAssert.IsNotNull(token);
        ClassicAssert.IsNotEmpty(token);


        var set2 = encoder.Decode("Jwt", token);


        ClassicAssert.IsNotNull(set2);
        ClassicAssert.AreEqual(set.AuthenticationType, set2.AuthenticationType);
        ClassicAssert.AreEqual(set.AuthenticationFlow, set2.AuthenticationFlow);
        ClassicAssert.AreEqual(set.Tenant, set2.Tenant);
        ClassicAssert.AreEqual(set.Subject, set2.Subject);
        ClassicAssert.AreEqual(set.AltSubject, set2.AltSubject);
        ClassicAssert.AreEqual(set.UserName, set2.UserName);
        ClassicAssert.AreEqual(set.GivenName, set2.GivenName);
        ClassicAssert.AreEqual(set.FamilyName, set2.FamilyName);
        ClassicAssert.AreEqual(set.Name, set2.Name);
        ClassicAssert.AreEqual(set.Email, set2.Email);
        ClassicAssert.AreEqual(set.Picture, set2.Picture);



    }


    [Test]
    public void Test8100_0400_SimulateGateway()
    {

        var set = CreateClaimSet();
        set.AuthenticationType = "Jwt";

        var ci = new ClaimsIdentity();
        ci.Populate(set);

        ClassicAssert.IsNotNull(ci);


        var builder = new ClaimGatewayTokenPayloadBuilder();
        var set1 = builder.Build(ci.Claims);

        ClassicAssert.IsNotNull(set1);



        var key = RandomNumberGenerator.GetBytes(64);
        var strKey = Convert.ToBase64String(key);

        var encoder = new GatewayTokenJwtEncoder
        {
            TokenSigningKey = key,
            TokenTimeToLive = TimeSpan.FromSeconds(3600)

        };

        var token = encoder.Encode(set1);

        ClassicAssert.IsNotNull(token);
        ClassicAssert.IsNotEmpty(token);


        var decoder = new GatewayTokenJwtEncoder
        {
            TokenSigningKey = key,
            TokenTimeToLive = TimeSpan.FromSeconds(3600)

        };

        var set2 = decoder.Decode("Jwt", token);

        ClassicAssert.IsNotNull(set2);
        ClassicAssert.AreEqual(set.AuthenticationType, set2.AuthenticationType);
        ClassicAssert.AreEqual(set.AuthenticationFlow, set2.AuthenticationFlow);
        ClassicAssert.AreEqual(set.Tenant, set2.Tenant);
        ClassicAssert.AreEqual(set.Subject, set2.Subject);
        ClassicAssert.AreEqual(set.AltSubject, set2.AltSubject);
        ClassicAssert.AreEqual(set.UserName, set2.UserName);
        ClassicAssert.AreEqual(set.GivenName, set2.GivenName);
        ClassicAssert.AreEqual(set.FamilyName, set2.FamilyName);
        ClassicAssert.AreEqual(set.Name, set2.Name);
        ClassicAssert.AreEqual(set.Email, set2.Email);
        ClassicAssert.AreEqual(set.Picture, set2.Picture);


    }




    [Test]
    public void Test8100_0500_SimulateGatewayWithMapping()
    {

        var claims = new List<Claim>
        {
            new ("sub", "be8d1276-5c28-476b-bcbc-ea62962da7a6"),
            new ("sid", "be8d1276-5c28-476b-bcbc-ea62962daXXX"),
            new ("preferred_username", "james.moring"),
            new ("name", "James Moring"),
            new ("given_name", "James"),
            new ("family_name", "Moring"),
            new ("email", "james.moring@pondhawktech.com")
        };


        var mappings = new Dictionary<string, string>
        {
            { "sub", FabricaClaims.SubjectClaim },
            { "sid", FabricaClaims.AltSubjectClaim },
            { "preferred_username", FabricaClaims.UserNameClaim },
            { "name", FabricaClaims.NameClaim },
            { "given_name", FabricaClaims.GivenNameClaim },
            { "family_name", FabricaClaims.FamilyNameClaim },
            { "email", FabricaClaims.EmailClaim }
        };

        var builder = new ClaimGatewayTokenPayloadBuilder(mappings);
        var set = builder.Build(claims);

        ClassicAssert.IsNotNull(set);



        var key = RandomNumberGenerator.GetBytes(64);
        var strKey = Convert.ToBase64String(key);

        var encoder = new GatewayTokenJwtEncoder
        {
            TokenSigningKey = key,
            TokenTimeToLive = TimeSpan.FromSeconds(3600)
        };

        var token = encoder.Encode(set);

        ClassicAssert.IsNotNull(token);
        ClassicAssert.IsNotEmpty(token);


        var decoder = new GatewayTokenJwtEncoder
        {
            TokenSigningKey = key,
            TokenTimeToLive = TimeSpan.FromSeconds(3600)

        };

        var set2 = decoder.Decode("Jwt", token);

        ClassicAssert.IsNotNull(set2);
        ClassicAssert.AreEqual(set.Subject, set2.Subject);
        ClassicAssert.AreEqual(set.AltSubject, set2.AltSubject);
        ClassicAssert.AreEqual(set.UserName, set2.UserName);
        ClassicAssert.AreEqual(set.GivenName, set2.GivenName);
        ClassicAssert.AreEqual(set.FamilyName, set2.FamilyName);
        ClassicAssert.AreEqual(set.Name, set2.Name);
        ClassicAssert.AreEqual(set.Email, set2.Email);


    }





}