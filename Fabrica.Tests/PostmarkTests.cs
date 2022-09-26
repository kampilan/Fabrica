using Autofac;
using Fabrica.Aws;
using Fabrica.Mediator;
using Fabrica.Omni.Postmark.Handlers;
using Fabrica.Repository;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Repository;
using Fabrica.Watch;
using NUnit.Framework;
using System.Drawing;
using System.Threading.Tasks;
using Fabrica.Omni.Email;
using Fabrica.Watch.Realtime;

namespace Fabrica.Tests;


[TestFixture]
public class PostmarkTests
{

    [OneTimeSetUp]
    public async Task Setup()
    {

        var maker = new WatchFactoryBuilder();
        maker.UseRealtime();
        maker.UseLocalSwitchSource().WhenNotMatched(Level.Debug, Color.BurlyWood);

        maker.Build();


        var builder = new ContainerBuilder();

        builder.RegisterModule(new PostmarkModule());

        TheContainer = await builder.BuildAndStart();

    }

    [OneTimeTearDown]
    public void Teardown()
    {

        TheContainer?.Dispose();
        TheContainer = null;

        WatchFactoryLocator.Factory.Stop();

    }

    private IContainer TheContainer { get; set; }



    [Test]
    public async Task Test_1200_0100_SendEmail()
    {

        using var scope = TheContainer.BeginLifetimeScope();

        var request = new SendEmailRequest();

        request.ToAddresses.Add("me@jamesmoring.com");
        request.CcAddresses.Add("james.moring@kampilangroup.com");
        request.BccAddresses.Add("jmoring@gmail.com");

        request.FromAddress    = "james.moring@pondhawktech.com";
        request.ReplyToAddress = "james.moring@pondhawktech.com";
        
        request.TemplateName = "pond-hawk-standard";
        request.Model.Add("Subject", "More testing");
        request.Model.Add("Salutation", "Jimboy");
        request.Model.Add( "Body", "This is a message from Fabrica");

        var mediator = scope.Resolve<IMessageMediator>();

        var response = await mediator.Send(request);

        Assert.IsNotNull(response);
        Assert.IsTrue( response.Ok );
        Assert.IsNotNull(response.Value);
        Assert.IsNotEmpty(response.Value.MessageUid);


    }


    [Test]
    public async Task Test_1200_0200_SendEmailWithAttachment()
    {

        using var logger = this.EnterMethod();


        using var scope = TheContainer.BeginLifetimeScope();

        var request = new SendEmailRequest();

        request.ToAddresses.Add("me@jamesmoring.com");
        request.CcAddresses.Add("james.moring@kampilangroup.com");
        request.BccAddresses.Add("jmoring@gmail.com");

        request.FromAddress = "james.moring@pondhawktech.com";
        request.ReplyToAddress = "james.moring@pondhawktech.com";

        request.Attachments.Add(new SendAttachment { Location = "transient/1WkqVob0KwQ6SueN3UWUoM/document.pdf", ContentType = "application/pdf", Name = "stuff.pdf" });
        request.Attachments.Add(new SendAttachment { Location = "2022/09/23/1xTepB2vFkVzmN7nKRdMEx/document.pdf", ContentType = "application/pdf", Name = "stuff2.pdf" });

        request.TemplateName = "pond-hawk-standard";
        request.Model.Add("Subject", "More testing. Now with attachment");
        request.Model.Add("Salutation", "Jimboy");
        request.Model.Add("Body", $"This is a message from Fabrica. It has {request.Attachments.Count} attachment(s)");

        var mediator = scope.Resolve<IMessageMediator>();

        var response = await mediator.Send(request);

        logger.LogObject(nameof(response), response);            

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Ok);
        Assert.IsNotNull(response.Value);
        Assert.IsNotEmpty(response.Value.MessageUid);


    }


    [Test]
    public async Task Test_1200_0200_SendEmailWithAttachmentForFax()
    {

        using var logger = this.EnterMethod();


        using var scope = TheContainer.BeginLifetimeScope();

        var request = new SendEmailRequest();

        request.ToAddresses.Add("8662461521@fax.smtpengine.io");

        request.FromAddress = "james.moring@pondhawktech.com";
        request.ReplyToAddress = "james.moring@pondhawktech.com";

        request.Attachments.Add(new SendAttachment { Location = "transient/1WkqVob0KwQ6SueN3UWUoM/document.pdf", ContentType = "application/pdf", Name = "stuff.pdf" });

        request.TemplateName = "outbound-fax";
        request.Model.Add("Attention", "Jim Moring");

        var mediator = scope.Resolve<IMessageMediator>();

        var response = await mediator.Send(request);

        logger.LogObject(nameof(response), response);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.Ok);
        Assert.IsNotNull(response.Value);
        Assert.IsNotEmpty(response.Value.MessageUid);


    }




}

public class PostmarkModule: Module, IPostmarkConfiguration
{

    public string PostmarkAppApiKey => "";

    protected override void Load(ContainerBuilder builder)
    {

        builder.AddCorrelation();

        builder.UseRepositoryRemoteClient("https://fabrica.ngrok.io/repository");

        builder.UseRules();
        builder.UseMediator(typeof(SendEmailHandler).Assembly);

        builder.RegisterInstance(this)
            .As<IPostmarkConfiguration>()
            .SingleInstance();

    }


}