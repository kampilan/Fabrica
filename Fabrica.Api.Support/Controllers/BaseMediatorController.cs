using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fabrica.Api.Support.ActionResult;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Rql;
using Fabrica.Utilities.Container;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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



    protected virtual bool TryValidate([CanBeNull] BaseCriteria criteria, out IActionResult error)
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(criteria), criteria);

        error = null;


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

    protected virtual bool TryValidate([CanBeNull] BaseDelta delta, out IActionResult error)
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(delta), delta);

        error = null;


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

    protected virtual bool TryValidate<TEntity>([CanBeNull] IDictionary<string,object> delta, OperationType op, out IActionResult error) where TEntity: class, IModel
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(delta), delta);

        error = null;


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