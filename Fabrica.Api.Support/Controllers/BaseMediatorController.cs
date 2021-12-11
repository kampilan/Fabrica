using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Fabrica.Api.Support.ActionResult;
using Fabrica.Api.Support.Models;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using Fabrica.Utilities.Container;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabrica.Api.Support.Controllers;

public abstract class BaseMediatorController : BaseController
{


    protected BaseMediatorController(ICorrelation correlation, IMessageMediator mediator ) : base(correlation)
    {

        Mediator = mediator;

    }


    protected IMessageMediator Mediator { get; }

    protected virtual HttpStatusCode MapErrorToStatus(ErrorKind kind)
    {

        using var logger = EnterMethod();

        logger.Inspect(nameof(kind), kind);


        var statusCode = HttpStatusCode.InternalServerError;

        switch (kind)
        {

            case ErrorKind.None:
                statusCode = HttpStatusCode.OK;
                break;

            case ErrorKind.NotFound:
                statusCode = HttpStatusCode.NotFound;
                break;

            case ErrorKind.NotImplemented:
                statusCode = HttpStatusCode.NotImplemented;
                break;

            case ErrorKind.Predicate:
                statusCode = HttpStatusCode.BadRequest;
                break;

            case ErrorKind.Conflict:
                statusCode = HttpStatusCode.Conflict;
                break;

            case ErrorKind.Functional:
                statusCode = HttpStatusCode.InternalServerError;
                break;

            case ErrorKind.Concurrency:
                statusCode = HttpStatusCode.Gone;
                break;

            case ErrorKind.BadRequest:
                statusCode = HttpStatusCode.BadRequest;
                break;

            case ErrorKind.AuthenticationRequired:
                statusCode = HttpStatusCode.Unauthorized;
                break;

            case ErrorKind.NotAuthorized:
                statusCode = HttpStatusCode.Forbidden;
                break;

            case ErrorKind.System:
            case ErrorKind.Unknown:
                statusCode = HttpStatusCode.InternalServerError;
                break;

        }

        logger.Inspect(nameof(statusCode), statusCode);


        return statusCode;

    }

    protected virtual IActionResult BuildResult<TValue>(Response<TValue> response)
    {

        using var logger = EnterMethod();


        logger.LogObject(nameof(response), response);



        // *****************************************************************
        logger.Debug("Attempting to check for success");
        logger.Inspect(nameof(response.Ok), response.Ok);
        if (response.Ok && typeof(TValue).IsValueType)
            return Ok();

        if (response.Ok)
            return Ok(response.Value);


        return BuildErrorResult(response);


    }

    protected virtual IActionResult BuildResult(Response<MemoryStream> response)
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to check for success");
        logger.Inspect(nameof(response.Ok), response.Ok);
        if (response.Ok)
            return new JsonStreamResult(response.Value);


        return BuildErrorResult(response);


    }

    protected virtual IActionResult BuildResult(Response response)
    {

        using var logger = EnterMethod();



        logger.LogObject(nameof(response), response);



        // *****************************************************************
        logger.Debug("Attempting to check for success");
        logger.Inspect(nameof(response.Ok), response.Ok);
        if (response.Ok)
            return Ok();


        return BuildErrorResult(response);



    }

    protected IActionResult BuildErrorResult(IExceptionInfo error)
    {


        using var logger = EnterMethod();


        logger.LogObject(nameof(error), error);



        // *****************************************************************
        logger.Debug("Attempting to build ErrorResponseModel");
        var model = new ErrorResponseModel
        {
            ErrorCode = error.ErrorCode,
            Explanation = error.Explanation,
            Details = new List<EventDetail>(error.Details),
            CorrelationId = Correlation.Uid
        };



        // *****************************************************************
        logger.Debug("Attempting to map error Kind to HttpStatusCode");
        var status = MapErrorToStatus(error.Kind);

        logger.Inspect(nameof(status), status);



        // *****************************************************************
        logger.Debug("Attempting to build ObjectResult");
        var result = new ObjectResult(model)
        {
            StatusCode = (int)status
        };



        // *****************************************************************
        return result;


    }



    protected virtual List<IRqlFilter<TExplorer>> ProduceFilters<TExplorer>() where TExplorer : class, IModel
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to digout RQL query parameters");

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
        return filters;


    }

    protected virtual List<IRqlFilter<TExplorer>> ProduceFilters<TExplorer>( IEnumerable<string> rqls ) where TExplorer : class, IModel
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to produce filters from supplied RQL");
        var filters = new List<IRqlFilter<TExplorer>>();
        filters.AddRange(rqls.Select(s =>
        {
            var tree = RqlLanguageParser.ToCriteria(s);
            return new RqlFilterBuilder<TExplorer>(tree);
        }));


        // *****************************************************************
        return filters;


    }


    protected virtual List<IRqlFilter<TExplorer>> ProduceFilters<TExplorer>([NotNull] ICriteria criteria) where TExplorer : class, IModel
    {

        if (criteria == null) throw new ArgumentNullException(nameof(criteria));


        using var logger = EnterMethod();


        var filters = new List<IRqlFilter<TExplorer>>();
        if (criteria.Rql?.Length > 0)
        {
            filters.AddRange(criteria.Rql.Select(s =>
            {
                var tree = RqlLanguageParser.ToCriteria(s);
                return new RqlFilterBuilder<TExplorer>(tree);
            }));
        }
        else
        {

            // *****************************************************************
            logger.Debug("Attempting to introspect criteria RQL");
            var filter = RqlFilterBuilder<TExplorer>.Create().Introspect(criteria);


            // *****************************************************************
            filters.Add(filter);

        }

        return filters;

    }


    protected virtual List<IRqlFilter<TExplorer>> ProduceFilters<TExplorer, TCriteria>() where TExplorer : class, IModel where TCriteria : class, ICriteria, new()
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to digout query parameters");

        var rqls = new List<string>();
        var parameters = new Dictionary<string, string>();
        foreach (var key in Request.Query.Keys)
        {

            logger.Inspect(nameof(key), key);
            logger.LogObject("values", Request.Query[key]);

            if (key == "rql")
                rqls.AddRange(Request.Query[key].ToArray());
            else
                parameters[key] = Request.Query[key].First();

        }



        var filters = new List<IRqlFilter<TExplorer>>();
        if (rqls.Count > 0)
        {
            filters.AddRange(rqls.Select(s =>
            {
                var tree = RqlLanguageParser.ToCriteria(s);
                return new RqlFilterBuilder<TExplorer>(tree);
            }));
        }
        else
        {


            // *****************************************************************
            logger.Debug("Attempting to map parameters to criteria model");
            var jo = JObject.FromObject(parameters);
            var criteria = jo.ToObject<TCriteria>();

            criteria ??= new TCriteria();

            logger.LogObject(nameof(criteria), criteria);



            // *****************************************************************
            logger.Debug("Attempting to introspect criteria RQL");
            var filter = RqlFilterBuilder<TExplorer>.Create().Introspect(criteria);


            // *****************************************************************
            filters.Add(filter);


        }


        return filters;


    }


    protected virtual async Task<Dictionary<string, object>> FromBody()
    {

        using var logger = EnterMethod();


        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        using var jreader = new JsonTextReader(reader);
        
        var jo    = await JObject.LoadAsync( jreader );
        var delta = jo.ToObject<Dictionary<string, object>>();

        return delta;

    }



    protected virtual bool TryValidate( [CanBeNull] IApiModel model, out IActionResult error )
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(model), model);

        error = null;


        if( !ModelState.IsValid )
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "ModelInvalid",
                Explanation = $"Errors occurred while parsing model for {Request.Method} at {Request.Path}"
            };

            var errors = ModelState.Keys.SelectMany(x => ModelState[x]?.Errors);

            foreach (var e in errors)
                info.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Violation, RuleName = "ModelState.Validator", Explanation = e.ErrorMessage, Group = "Model" });

            error = BuildErrorResult(info);

            return false;

        }


        if( model is null )
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "ModelInvalid",
                Explanation = $"Errors occurred while parsing model for {Request.Method} at {Request.Path}"
            };

            error = BuildErrorResult(info);

            return false;

        }



        if (  model.IsOverposted() )
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "DisallowedProperties",
                Explanation = $"The following properties were not found or are not mutable: ({string.Join(',', model.GetOverpostNames())})"
            };

            error = BuildErrorResult(info);

            return false;

        }


        return true;


    }



    protected virtual async Task<IActionResult> Send<TValue>([NotNull] IRequest<Response<TValue>> request)
    {

        if (request == null) throw new ArgumentNullException(nameof(request));

        using var logger = EnterMethod();



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


    protected virtual async Task<IActionResult> Send([NotNull] IRequest<Response<MemoryStream>> request)
    {

        if (request == null) throw new ArgumentNullException(nameof(request));

        using var logger = EnterMethod();



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


    protected virtual async Task<IActionResult> Send([NotNull] IRequest<Response> request)
    {

        if (request == null) throw new ArgumentNullException(nameof(request));

        using var logger = EnterMethod();



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



    protected virtual async Task<IActionResult> HandleQuery<TExplorer,TCriteria>() where TExplorer : class, IExplorableModel where TCriteria : class, ICriteria, new()
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to produce filters");

        var filters = ProduceFilters<TExplorer,TCriteria>();



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = new QueryEntityRequest<TExplorer>
        {
            Filters = filters
        };



        // *****************************************************************
        logger.Debug("Attempting to send request");
        var result = await Send(request);



        // *****************************************************************
        return result;


    }


    protected virtual async Task<IActionResult> HandleQuery<TExplorer,TCriteria>( TCriteria criteria ) where TExplorer : class, IExplorableModel where TCriteria : class, ICriteria, IApiModel
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to validate criteria model");
        if (!TryValidate(criteria, out var error))
            return error;



        // *****************************************************************
        logger.Debug("Attempting to produce filters");

        var filters = ProduceFilters<TExplorer>(criteria);



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = new QueryEntityRequest<TExplorer>
        {
            Filters = filters
        };



        // *****************************************************************
        logger.Debug("Attempting to send request");
        var result = await Send(request);



        // *****************************************************************
        return result;


    }


    protected virtual async Task<IActionResult> HandleQuery<TExplorer>( IEnumerable<string> rqls ) where TExplorer : class, IExplorableModel
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to produce filters");

        var filters = ProduceFilters<TExplorer>( rqls );



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = new QueryEntityRequest<TExplorer>
        {
            Filters = filters
        };



        // *****************************************************************
        logger.Debug("Attempting to send request");
        var result = await Send(request);



        // *****************************************************************
        return result;


    }


    protected virtual async Task<IActionResult> HandleQuery<TExplorer>() where TExplorer : class, IExplorableModel
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to produce filters");

        var filters = ProduceFilters<TExplorer>();



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = new QueryEntityRequest<TExplorer>
        {
            Filters = filters
        };



        // *****************************************************************
        logger.Debug("Attempting to send request");
        var result = await Send(request);



        // *****************************************************************
        return result;


    }



    protected virtual async Task<IActionResult> HandleRetrieve<TEntity>( string uid ) where TEntity : class, IModel
    {

        using var logger = EnterMethod();


        var request = new RetrieveEntityRequest<TEntity>
        {
            Uid = uid
        };

        // *****************************************************************
        logger.Debug("Attempting to send request");
        var result = await Send(request);



        // *****************************************************************
        return result;


    }



    protected virtual async Task<IActionResult> HandleCreate<TEntity>() where TEntity : class, IMutableModel
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to read delta from body");
        var delta = await FromBody();



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = new CreateEntityRequest<TEntity>
        {
            Delta = delta
        };



        // *****************************************************************
        logger.Debug("Attempting to send request");
        var result = await Send(request);



        // *****************************************************************
        return result;



    }


    protected virtual async Task<IActionResult> HandleCreate<TEntity,TDelta>( TDelta delta ) where TEntity: class, IMutableModel where TDelta: BaseDelta
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to validate delta model");
        if (!TryValidate(delta, out var error))
            return error;



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = new CreateEntityRequest<TEntity>();

        request.FromObject(delta);



        // *****************************************************************
        logger.Debug("Attempting to send request");
        var result = await Send(request);



        // *****************************************************************
        return result;



    }



    protected virtual async Task<IActionResult> HandleUpdate<TEntity>() where TEntity : class, IMutableModel
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to read delta from body");
        var delta = await FromBody();



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = new UpdateEntityRequest<TEntity>
        {
            Delta = delta
        };



        // *****************************************************************
        logger.Debug("Attempting to send request");
        var result = await Send(request);



        // *****************************************************************
        return result;



    }


    protected virtual async Task<IActionResult> HandleUpdate<TEntity, TDelta>(TDelta delta) where TEntity : class, IMutableModel where TDelta : BaseDelta
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to validate delta model");
        if (!TryValidate(delta, out var error))
            return error;



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = new UpdateEntityRequest<TEntity>();

        request.FromObject(delta);



        // *****************************************************************
        logger.Debug("Attempting to send request");
        var result = await Send(request);



        // *****************************************************************
        return result;



    }



    protected virtual async Task<IActionResult> HandleDelete<TEntity>(string uid) where TEntity : class, IModel
    {

        using var logger = EnterMethod();


        var request = new DeleteEntityRequest<TEntity>
        {
            Uid = uid
        };

        // *****************************************************************
        logger.Debug("Attempting to send request");
        var result = await Send(request);



        // *****************************************************************
        return result;


    }



    protected virtual async Task<IActionResult> HandleJournal<TEntity>( string uid ) where TEntity : class, IMutableModel
    {


        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to dispatch request");
        var request = new AuditJournalStreamRequest
        {
            Entity    = typeof(TEntity).FullName, 
            EntityUid = uid
        };

        var response = await Mediator.Send(request);



        // *****************************************************************
        logger.Debug("Attempting to build result");
        var result = response.Ok ? new JsonStreamResult(response.Value) : BuildErrorResult(response);



        // *****************************************************************
        return result;


    }




}