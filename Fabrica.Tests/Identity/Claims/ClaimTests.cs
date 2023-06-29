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

        Assert.IsNotNull(set2);
        Assert.AreEqual( set.AuthenticationType, set2.AuthenticationType);
        Assert.AreEqual(set.AuthenticationFlow, set2.AuthenticationFlow);
        Assert.AreEqual(set.Tenant, set2.Tenant);
        Assert.AreEqual(set.Subject, set2.Subject);
        Assert.AreEqual(set.AltSubject, set2.AltSubject);
        Assert.AreEqual(set.UserName, set2.UserName);
        Assert.AreEqual(set.GivenName, set2.GivenName);
        Assert.AreEqual(set.FamilyName, set2.FamilyName);
        Assert.AreEqual(set.Name, set2.Name);
        Assert.AreEqual(set.Email, set2.Email);
        Assert.AreEqual(set.Picture, set2.Picture);


    }


    [Test]
    public void Test8100_0200_CreateClaimIdentityJson()
    {

        var set = CreateClaimSetAsIntf();

        var json = JsonSerializer.Serialize(set, set.GetType());

        Assert.IsNotNull(json);
        Assert.IsNotEmpty(json);

        var ci = new ClaimsIdentity();
        ci.Populate(json);


        Assert.IsNotNull(ci);
        Assert.AreEqual(set.AuthenticationType, ci.GetAuthType());
        Assert.AreEqual(set.AuthenticationFlow, ci.GetAuthFlow());
        Assert.AreEqual(set.Tenant, ci.GetTenant());
        Assert.AreEqual(set.Subject, ci.GetSubject());
        Assert.AreEqual(set.AltSubject, ci.GetAltSubject());
        Assert.AreEqual(set.UserName, ci.GetUserName());
        Assert.AreEqual(set.GivenName, ci.GetGivenName());
        Assert.AreEqual(set.FamilyName, ci.GetFamilyName());
        Assert.AreEqual(set.Name, ci.GetName());
        Assert.AreEqual(set.Email, ci.GetEmail());
        Assert.AreEqual(set.Picture, ci.GetPictureUrl());


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

        Assert.IsNotNull(token);
        Assert.IsNotEmpty(token);


        var set2 = encoder.Decode("Jwt", token);


        Assert.IsNotNull(set2);
        Assert.AreEqual(set.AuthenticationType, set2.AuthenticationType);
        Assert.AreEqual(set.AuthenticationFlow, set2.AuthenticationFlow);
        Assert.AreEqual(set.Tenant, set2.Tenant);
        Assert.AreEqual(set.Subject, set2.Subject);
        Assert.AreEqual(set.AltSubject, set2.AltSubject);
        Assert.AreEqual(set.UserName, set2.UserName);
        Assert.AreEqual(set.GivenName, set2.GivenName);
        Assert.AreEqual(set.FamilyName, set2.FamilyName);
        Assert.AreEqual(set.Name, set2.Name);
        Assert.AreEqual(set.Email, set2.Email);
        Assert.AreEqual(set.Picture, set2.Picture);



    }


    [Test]
    public void Test8100_0400_SimulateGateway()
    {

        var set = CreateClaimSet();
        set.AuthenticationType = "Jwt";

        var ci = new ClaimsIdentity();
        ci.Populate(set);

        Assert.IsNotNull(ci);


        var builder = new ClaimGatewayTokenPayloadBuilder();
        var set1 = builder.Build(ci.Claims);

        Assert.IsNotNull(set1);



        var key = RandomNumberGenerator.GetBytes(64);
        var strKey = Convert.ToBase64String(key);

        var encoder = new GatewayTokenJwtEncoder
        {
            TokenSigningKey = key,
            TokenTimeToLive = TimeSpan.FromSeconds(3600)

        };

        var token = encoder.Encode(set1);

        Assert.IsNotNull(token);
        Assert.IsNotEmpty(token);


        var decoder = new GatewayTokenJwtEncoder
        {
            TokenSigningKey = key,
            TokenTimeToLive = TimeSpan.FromSeconds(3600)

        };

        var set2 = decoder.Decode("Jwt", token);

        Assert.IsNotNull(set2);
        Assert.AreEqual(set.AuthenticationType, set2.AuthenticationType);
        Assert.AreEqual(set.AuthenticationFlow, set2.AuthenticationFlow);
        Assert.AreEqual(set.Tenant, set2.Tenant);
        Assert.AreEqual(set.Subject, set2.Subject);
        Assert.AreEqual(set.AltSubject, set2.AltSubject);
        Assert.AreEqual(set.UserName, set2.UserName);
        Assert.AreEqual(set.GivenName, set2.GivenName);
        Assert.AreEqual(set.FamilyName, set2.FamilyName);
        Assert.AreEqual(set.Name, set2.Name);
        Assert.AreEqual(set.Email, set2.Email);
        Assert.AreEqual(set.Picture, set2.Picture);


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

        Assert.IsNotNull(set);



        var key = RandomNumberGenerator.GetBytes(64);
        var strKey = Convert.ToBase64String(key);

        var encoder = new GatewayTokenJwtEncoder
        {
            TokenSigningKey = key,
            TokenTimeToLive = TimeSpan.FromSeconds(3600)
        };

        var token = encoder.Encode(set);

        Assert.IsNotNull(token);
        Assert.IsNotEmpty(token);


        var decoder = new GatewayTokenJwtEncoder
        {
            TokenSigningKey = key,
            TokenTimeToLive = TimeSpan.FromSeconds(3600)

        };

        var set2 = decoder.Decode("Jwt", token);

        Assert.IsNotNull(set2);
        Assert.AreEqual(set.Subject, set2.Subject);
        Assert.AreEqual(set.AltSubject, set2.AltSubject);
        Assert.AreEqual(set.UserName, set2.UserName);
        Assert.AreEqual(set.GivenName, set2.GivenName);
        Assert.AreEqual(set.FamilyName, set2.FamilyName);
        Assert.AreEqual(set.Name, set2.Name);
        Assert.AreEqual(set.Email, set2.Email);


    }





}