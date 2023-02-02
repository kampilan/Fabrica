using Fabrica.Exceptions;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Api.Support.Endpoints.Prev;

public abstract class BaseQueryFromCriteriaEndpoint<TExplorer, TCriteria> : BaseEndpoint where TExplorer : class, IExplorableModel where TCriteria : BaseCriteria
{


    protected BaseQueryFromCriteriaEndpoint(IEndpointComponent component) : base(component)
    {
    }


    protected virtual bool TryValidate(BaseCriteria? criteria, out IActionResult error)
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(criteria), criteria);

        error = null!;


        if (!ModelState.IsValid)
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "CriteriaInvalid",
                Explanation = $"Errors occurred while parsing criteria for {Request.Method} at {Request.Path}"
            };

            var errors = ModelState.Keys.SelectMany(x => ModelState[x]?.Errors);

            foreach (var e in errors)
                info.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Violation, RuleName = "ModelState.Validator", Explanation = e.ErrorMessage, Group = "Model" });

            error = BuildErrorResult(info);

            return false;

        }


        if (criteria is null)
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "CriteriaInvalid",
                Explanation = $"Errors occurred while parsing criteria for {Request.Method} at {Request.Path}"
            };

            error = BuildErrorResult(info);

            return false;

        }



        if (criteria.IsOverposted())
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "DisallowedProperties",
                Explanation = $"The following properties were not found or are not mutable: ({string.Join(',', criteria.GetOverpostNames())})"
            };

            error = BuildErrorResult(info);

            return false;

        }


        return true;


    }



    [SwaggerOperation(Summary = "Using Criteria", Description = "Query using Criteria model")]
    [HttpGet]
    public async Task<IActionResult> Handle([FromBody] TCriteria criteria, [FromQuery, SwaggerParameter(Description = "Limit", Required = false)] int limit = 0)
    {

        using var logger = EnterMethod();



        if (!TryValidate(criteria, out var error))
            return error;



        // *****************************************************************
        logger.LogObject(nameof(criteria), criteria);
        logger.Inspect(nameof(limit), limit);



        // *****************************************************************
        logger.Debug("Attempting to produce filters from Criteria");
        var filters = new List<IRqlFilter<TExplorer>>();
        if (criteria.Rql.Length > 0)
        {
            filters.AddRange(criteria.Rql.Select(s =>
            {
                var tree = RqlLanguageParser.ToCriteria(s);
                return new RqlFilterBuilder<TExplorer>(tree) { RowLimit = limit };
            }));
        }
        else
        {

            // *****************************************************************
            logger.Debug("Attempting to introspect criteria RQL");
            var filter = RqlFilterBuilder<TExplorer>.Create().Introspect(criteria);
            filter.RowLimit = limit;

            // *****************************************************************
            filters.Add(filter);

        }



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = new QueryEntityRequest<TExplorer>
        {
            Filters = filters,
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