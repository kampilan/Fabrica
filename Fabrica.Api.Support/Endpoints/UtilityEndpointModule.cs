
// ReSharper disable UnusedMember.Global

using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.One;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Support.Endpoints;

public abstract class UtilityEndpointModule : BaseEndpointModule<UtilityEndpointModule>
{

    protected UtilityEndpointModule()
    {

    }

    protected UtilityEndpointModule(string route) : base(route)
    {

    }


    protected class MissionHandler : BaseEndpointHandler
    {


        [FromServices]
        public IMissionContext Mission { get; set; } = null!;

        [FromServices]
        public IEndpointResultBuilder Builder { get; set; } = null!;

        public override Task<IResult> Handle()
        {

            var result = Builder.Create(Mission);

            return Task.FromResult(result);


        }

    }


    protected class PingHandler : BaseEndpointHandler
    {


        [FromServices]
        public IEndpointResultBuilder Builder { get; set; } = null!;


        public override Task<IResult> Handle()
        {

            using var logger = EnterMethod();


            var utc = DateTime.UtcNow;

            DateTime Convert(int diff)
            {

                var dt = utc.AddHours(diff);

                return dt;

            }

            var dict = new Dictionary<string, DateTime>
            {
                ["Beverly Hills"] = Convert(-7),
                ["Monte Carlo"] = Convert(2),
                ["London"] = Convert(1),
                ["Paris"] = Convert(2),
                ["Rome"] = Convert(2),
                ["Gstaad"] = Convert(2)
            };


            var response = new Response<Dictionary<string, DateTime>>(dict)
                .IsOk()
                .WithKind(ErrorKind.None)
                .WithDetail(new EventDetail
                {
                    Category = EventDetail.EventCategory.Info,
                    Explanation = "Time according to the Rouchefoucauld",
                    Group = "Tests",
                    Source = "Louis Winthorpe III"
                });


            var result = Builder.Create(response);

            return Task.FromResult(result);

        }

    }



}