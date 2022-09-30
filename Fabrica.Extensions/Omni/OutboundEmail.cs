using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Fabrica.Omni
{


    public static class OutboundEmailExtensions
    {


        public static OutboundEmail WithSource(this OutboundEmail model, string source )
        {

            model.Source = source;

            return model;

        }

        public static OutboundEmail WithTo(this OutboundEmail model, string address, string name = "")
        {

            model.ToAddresses.Add( new EmailAddress {Address = address, Name = name});

            return model;

        }

        public static OutboundEmail WithCc(this OutboundEmail model, string address, string name = "")
        {

            model.CcAddresses.Add(new EmailAddress { Address = address, Name = name });

            return model;

        }

        public static OutboundEmail WithBcc( this OutboundEmail model, string address, string name = "" )
        {

            model.BccAddresses.Add(new EmailAddress { Address = address, Name = name });

            return model;

        }


        public static OutboundEmail WithFrom(this OutboundEmail model, string address, string name = "")
        {

            model.FromAddress = $"{name} <{address}>";
            
            return model;

        }


        public static OutboundEmail WithReplyTo(this OutboundEmail model, string address, string name = "")
        {

            model.ReplyToAddress = $"{name} <{address}>";

            return model;

        }

        public static OutboundEmail WithSubject( this OutboundEmail model, string subject )
        {

            model.Subject = subject;

            return model;

        }


        public static OutboundEmail WithTemplate<T>( this OutboundEmail email, string templateName, T model ) where T: class
        {

            var jo = JObject.FromObject(model);

            email.TemplateName = templateName;
            email.Model = jo;

            return email;

        }

        public static OutboundEmail WithTemplate(this OutboundEmail email, string templateName, IDictionary<string,object> model )
        {

            var jo = JObject.FromObject(model);

            email.TemplateName = templateName;
            email.Model = jo;

            return email;

        }


        public static OutboundEmail WithBody(this OutboundEmail model, string textBody, string htmlBody )
        {

            model.HtmlBody = htmlBody;
            model.TextBody = textBody;

            return model;

        }


        public static OutboundEmail WithAttachment(this OutboundEmail model, string fileName, string location )
        {
            
            model.Attachments.Add( new OutboundEmailAttachment{FileName = fileName, ContentReference = location} );
            
            return model;

        }


    }




    public class EmailAddress
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
    }

    public class OutboundEmail
    {

        public string Source { get; set; } = "";


        public List<EmailAddress> ToAddresses { get; set; } = new ();
        public List<EmailAddress> CcAddresses { get; set; } = new ();
        public List<EmailAddress> BccAddresses { get; set; } = new ();


        public string FromAddress { get; set; } = "";
        public string ReplyToAddress { get; set; } = "";


        public string Subject { get; set; } = "";


        public string HtmlBody { get; set; } = "";
        public string TextBody { get; set; } = "";


        public string TemplateName { get; set; } = "";

        public JObject Model { get; set; } = new JObject();

        public List<OutboundEmailAttachment> Attachments { get; set; } = new List<OutboundEmailAttachment>();


    }
}
