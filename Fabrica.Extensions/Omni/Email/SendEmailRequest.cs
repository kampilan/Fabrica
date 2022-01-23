using System.Collections.Generic;
using Fabrica.Mediator;
using MediatR;

namespace Fabrica.Omni.Email;

public class SendEmailRequest: IRequest<Response<bool>>
{

    public string FromAddress { get; set; } = "";

    public List<string> ToAddresss { get; set; } = new();
    public List<string> CcAddresss { get; set; } = new();
    public List<string> BccAddresss { get; set; } = new();

    public string Subject { get; set; } = "";

    public string TextBody { get; set; } = "";
    public string HtmlBody { get; set; } = "";

    public string TemplateName { get; set; } = "";
    public Dictionary<string, object> Model { get; set; } = new();



}