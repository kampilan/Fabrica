using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Api.Support.Endpoints;

public abstract class BaseQueryFromRqlEndpoint<TExplorer>: BaseEndpoint where TExplorer: class, IExplorableModel
{


    protected BaseQueryFromRqlEndpoint( IEndpointComponent component ) : base( component )
    {
    }


    [HttpGet]
    [SwaggerOperation(Summary = "Query", Description = "Query using RQL")]
    public virtual async Task<IActionResult> Handle([FromQuery, SwaggerParameter(Description = "RQL", Required = true)] string rql )
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to dig out RQL query parameters");

        var rqls = new List<string>();
        foreach (var key in Request.Query.Keys)
        {

            logger.Inspect(nameof(key), key);
            logger.LogObject("values", Request.Query[key]);

            if (key == "rql")
                rqls.AddRange(Request.Query[key].ToArray());

        }


        // *****************************************************************
        logger.Debug("Attempting to produce filters from supplied RQL");
        var filters = new List<IRqlFilter<TExplorer>>();
        if (rqls.Count > 0)
            filters.AddRange(rqls.Select(s =>
            {
                var tree = RqlLanguageParser.ToCriteria(s);
                return new RqlFilterBuilder<TExplorer>(tree);
            }));




        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = new QueryEntityRequest<TExplorer>
        {
            Filters = filters
        };



        // *****************************************************************
        logger.Debug("Attempting to send request via Mediator");
        var response = await Mediator.Send(request);

        logger.Inspect(nameof(response.Ok), response.Ok);



        // *****************************************************************
        logger.Debug("Attempting to build result");
        var result = BuildResult(response);



        // *****************************************************************
        return result;

    }



}