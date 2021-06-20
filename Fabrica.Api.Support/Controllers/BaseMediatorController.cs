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
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using Fabrica.Utilities.Container;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Fabrica.Api.Support.Controllers
{


    public abstract class BaseMediatorController : BaseController
    {


        protected BaseMediatorController( IMessageMediator mediator, ICorrelation correlation ) : base( correlation )
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
                    statusCode = HttpStatusCode.UnprocessableEntity;
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

        protected virtual IActionResult BuildResult<TValue>( Response<TValue> response )
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

        protected virtual IActionResult BuildResult( Response<MemoryStream> response )
        {

            using var logger = EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to check for success");
            logger.Inspect(nameof(response.Ok), response.Ok);
            if( response.Ok )
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
                ErrorCode     = error.ErrorCode,
                Explanation   = error.Explanation,
                Details       = new List<EventDetail>(error.Details),
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

                if( key == "rql" )
                    rqls.AddRange(Request.Query[key].ToArray());

            }



            // *****************************************************************
            logger.Debug("Attempting to produce filters from supplied RQL");
            var filters = new List<IRqlFilter<TExplorer>>();
            if (rqls.Count > 0)
                filters.AddRange(rqls.Select( r=> RqlFilterBuilder<TExplorer>.FromRql(r).AutoProject()));



            // *****************************************************************
            return filters;


        }

        protected virtual List<IRqlFilter<TExplorer>> ProduceFilters<TExplorer,TCriteria>() where TExplorer : class, IModel where TCriteria : class, ICriteria, new()
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
                filters.AddRange(rqls.Select(RqlFilterBuilder<TExplorer>.FromRql));
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
                var filter = RqlFilterBuilder<TExplorer>.Create().Introspect(criteria).AutoProject();


                // *****************************************************************
                filters.Add(filter);


            }


            return filters;


        }



        protected virtual bool TryValidate( [NotNull] BaseDelta delta, out IActionResult error )
        {

            if (delta == null) throw new ArgumentNullException(nameof(delta));


            using var logger = EnterMethod();

            logger.LogObject(nameof(delta), delta);


            error = null;

            if( delta.IsOverposted() )
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



        protected virtual async Task<IActionResult> Send<TValue>( [NotNull] IRequest<Response<TValue>> request )
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


        protected virtual async Task<IActionResult> Send( [NotNull] IRequest<Response<MemoryStream>> request )
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


        protected virtual async Task<IActionResult> Send( [NotNull] IRequest<Response> request )
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



    }


}
