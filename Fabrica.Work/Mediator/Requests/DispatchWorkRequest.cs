using System;
using Fabrica.Mediator;
using MediatR;
using Newtonsoft.Json.Linq;

namespace Fabrica.Work.Mediator.Requests;

public class DispatchWorkRequest: IRequest<Response<JToken>>
{

    public string TopicName { get; set; } = "";

    public JObject Payload { get; set; } = null!;

    public TimeSpan DeliveryDelay { get; set; }
    public TimeSpan TimeToLive { get; set; }

}



