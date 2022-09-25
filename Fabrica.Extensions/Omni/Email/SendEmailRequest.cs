using System.Collections.Generic;
using Fabrica.Mediator;
using MediatR;

namespace Fabrica.Omni.Email;

public class SendEmailRequest: IRequest<Response<SendResponse>>
{

    public string FromAddress { get; set; } = "";
    public string ReplyToAddress { get; set; } = "";

    public List<string> ToAddresses { get; set; } = new();
    public List<string> CcAddresses { get; set; } = new();
    public List<string> BccAddresses { get; set; } = new();

    public string Subject { get; set; } = "";

    public string TextBody { get; set; } = "";
    public string HtmlBody { get; set; } = "";

    public string TemplateName { get; set; } = "";
    public Dictionary<string, object> Model { get; set; } = new();

    public List<SendAttachment> Attachments { get; set; } = new();


}


public class SendAttachment
{

    public string Location { get; set; } = "";
    public string Name { get; set; } = "";
    public string ContentType { get; set; } = "";

}    

public class SendResponse
{

    public string MessageUid { get; set; } = "";
    public string ErrorCode { get; set; } = "";
    public string ErrorMessage { get; set; } = "";


}