using Fabrica.Mediator;
using Fabrica.Omni.Email;
using Fabrica.Repository;
using Fabrica.Utilities.Container;
using PostmarkDotNet;
using PostmarkDotNet.Model;

namespace Fabrica.Omni.Postmark.Handlers;

public class SendEmailHandler: AbstractRequestHandler<SendEmailRequest,SendResponse>
{

    public SendEmailHandler(ICorrelation correlation, IPostmarkConfiguration configuration, IObjectRepository repository ) : base(correlation)
    {

        ApiKey     = configuration.PostmarkAppApiKey;
        Repository = repository;

    }


    private string ApiKey { get; }  
    private IObjectRepository Repository { get; }

    protected override async Task<SendResponse> Perform(CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();

        var to  = string.Join(',', Request.ToAddresses);
        var cc  = string.Join(',', Request.CcAddresses);
        var bcc = string.Join(',', Request.BccAddresses);



        var client = new PostmarkClient(ApiKey);

        PostmarkResponse pmr;

        if ( !string.IsNullOrWhiteSpace(Request.TemplateName) )
        {
            var msg = BuildTemplatedMessage(to, cc, bcc);
            await BuildAttachments(a => msg.Attachments.Add(a), cancellationToken);
            pmr = await client.SendEmailWithTemplateAsync(msg);
        }
        else
        {
            var msg = BuildMessage(to, cc, bcc);
            await BuildAttachments(a => msg.Attachments.Add(a), cancellationToken);
            pmr = await client.SendMessageAsync(msg);
        }

        var response = new SendResponse
        {
            MessageUid   = pmr.MessageID.ToString(),
            ErrorCode    = pmr.ErrorCode.ToString(),
            ErrorMessage = pmr.Message??""
        };



        return response;

    }

    private PostmarkMessage BuildMessage( string to, string cc, string bcc )
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to build Message");
        var message = new PostmarkMessage
        {
            To         = to,
            Cc         = cc,
            Bcc        = bcc,
            From       = Request.FromAddress,
            ReplyTo    = Request.ReplyToAddress,
            TrackOpens = true,
            HtmlBody   = Request.HtmlBody,
            TextBody   = Request.TextBody
        };

        logger.LogObject(nameof(message), message);



        // *****************************************************************
        return message;

    }

    private TemplatedPostmarkMessage BuildTemplatedMessage( string to, string cc, string bcc )
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to build Templated Message");
        var message = new TemplatedPostmarkMessage
        {

            To            = to,
            Cc            = cc,
            Bcc           = bcc,
            From          = Request.FromAddress,
            ReplyTo       = Request.ReplyToAddress,
            TrackOpens    = true,
            TemplateAlias = Request.TemplateName,
            TemplateModel = Request.Model

        };

        logger.LogObject(nameof(message), message);



        // *****************************************************************
        return message;

    }


    private async Task BuildAttachments( Action<PostmarkMessageAttachment> adder, CancellationToken token )
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to add attachments");
        foreach (var sa in Request.Attachments)
        {

            logger.LogObject(nameof(sa), sa);

            using var ms = new MemoryStream();

            logger.Debug("Attempting to fetch attachment from Repository");
            var found = await Repository.Get( ob=> { ob.Key = sa.Location; ob.Content = ms; ob.Rewind = true; }, token );
            if (!found)
            {
                logger.Debug("Attachment not found");
                continue;
            }

            var pma = new PostmarkMessageAttachment(ms.ToArray(), sa.Name, sa.ContentType);
            adder(pma);

        }


    }


}

public interface IPostmarkConfiguration
{

    public string PostmarkAppApiKey { get; }

}