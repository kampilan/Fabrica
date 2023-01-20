using Fabrica.Api.Support.ActionResult;
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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;

namespace Fabrica.Api.Support.Controllers;

public abstract class BaseMediatorController : BaseController
{


    protected BaseMediatorController(ICorrelation correlation, IModelMetaService meta, IMessageMediator mediator ) : base(correlation)
    {

        Meta     = meta;
        Mediator = mediator;

    }


    protected IModelMetaService Meta { get; }
    protected IMessageMediator Mediator { get; }


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

            var errors = ModelState.Keys.SelectMany(x => ModelState[x].Errors);

            foreach (var e in errors)
                info.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Violation, RuleName = "ModelState.Validator", Explanation = e.ErrorMessage, Group = "Model" });

            error = BuildErrorResult(info);

            return false;

        }


        if( criteria is null )
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

    protected virtual bool TryValidate(BaseDelta? delta, out IActionResult error)
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(delta), delta);

        error = null!;


        if (!ModelState.IsValid)
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


        if (delta is null)
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



        if (delta.IsOverposted())
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "DisallowedProperties",
                Explanation = $"The following properties were not found or are not mutable: ({string.Join(',', delta.GetOverpostNames())})"
            };

            error = BuildErrorResult(info);

            return false;

        }


        return true;


    }

    protected virtual bool TryValidate<TEntity>(IDictionary<string,object>? delta, OperationType op, out IActionResult error) where TEntity: class, IModel
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(delta), delta);

        error = null!;


        if (!ModelState.IsValid)
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "ModelInvalid",
                Explanation = $"Errors occurred while parsing delta for {Request.Method} at {Request.Path}"
            };

            var errors = ModelState.Keys.SelectMany(x => ModelState[x]?.Errors);

            foreach (var e in errors)
                info.Details.Add(new EventDetail { Category = EventDetail.EventCategory.Violation, RuleName = "ModelState.Validator", Explanation = e.ErrorMessage, Group = "Model" });

            error = BuildErrorResult(info);

            return false;

        }


        if (delta is null)
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "ModelInvalid",
                Explanation = $"Errors occurred while parsing delta for {Request.Method} at {Request.Path}"
            };

            error = BuildErrorResult(info);

            return false;

        }


        var mm = Meta.GetMetaFromType(typeof(TEntity));

        ISet<string> ob;
        switch (op)
        {
            case OperationType.Create:
                ob = mm.CheckForCreate(delta.Keys);
                break;
            case OperationType.Update:
                ob = mm.CheckForUpdate(delta.Keys);
                break;
            case OperationType.None:
                return true;
            default:
                throw new ArgumentOutOfRangeException(nameof(op), op, null);
        }


        if( ob.Count > 0 )
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "DisallowedProperties",
                Explanation = $"The following properties were not found or are not mutable: ({string.Join(',', ob)})"
            };

            error = BuildErrorResult(info);

            return false;

        }


        return true;


    }



    protected virtual async Task<IActionResult> Send<TValue>( IRequest<Response<TValue>> request )
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

    protected virtual async Task<IActionResult> Send( IRequest<Response<MemoryStream>> request)
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

    protected virtual async Task<IActionResult> Send( IRequest<Response> request)
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

    protected virtual List<IRqlFilter<TExplorer>> ProduceFilters<TExplorer>(IEnumerable<string> rqls) where TExplorer : class, IModel
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

    protected virtual List<IRqlFilter<TExplorer>> ProduceFilters<TExplorer>( ICriteria criteria ) where TExplorer : class, IModel
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

        var jo = await JObject.LoadAsync(jreader);
        var delta = jo.ToObject<Dictionary<string, object>>();

        return delta;

    }

    protected virtual Dictionary<string, object> FromDelta(object delta)
    {

        using var logger = EnterMethod();


        var dict = new Dictionary<string, object>();

        foreach (var pi in delta.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead))
        {
            var value = pi.GetValue(delta, null);
            if (value is not null)
                dict[pi.Name] = value;
        }

        return dict;

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

    protected virtual async Task<IActionResult> HandleQuery<TExplorer,TCriteria>( TCriteria criteria ) where TExplorer : class, IExplorableModel where TCriteria : BaseCriteria
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