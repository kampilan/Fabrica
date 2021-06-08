using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Api.Support.Models;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Serialization;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabrica.Api.Support.Controllers
{

    
    public abstract class BaseMediatorController: BaseController
    {


        protected enum Operation { Create, Update }


        protected BaseMediatorController( ILifetimeScope scope ) : base( scope )
        {

            Meta        = Scope.Resolve<IModelMetaService>();
            TheMediator = Scope.Resolve<IMessageMediator>();

        }


        protected IModelMetaService Meta { get; }
        protected IMessageMediator TheMediator { get; }


        protected virtual HttpStatusCode MapErrorToStatus(ErrorKind kind)
        {

            var statusCode = HttpStatusCode.InternalServerError;

            switch (kind)
            {

                case ErrorKind.None:
                    statusCode = HttpStatusCode.OK;
                    break;

                case ErrorKind.NotFound:
                    statusCode = HttpStatusCode.NoContent;
                    break;

                case ErrorKind.NotImplemented:
                    statusCode = HttpStatusCode.NotImplemented;
                    break;

                case ErrorKind.Predicate:
                    statusCode = (HttpStatusCode)422;
                    break;

                case ErrorKind.Conflict:
                    statusCode = HttpStatusCode.Conflict;
                    break;

                case ErrorKind.Functional:
                    statusCode = (HttpStatusCode)420;
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

            return statusCode;

        }

        protected virtual IActionResult BuildResult<TValue>( Response<TValue> response )
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();


                logger.LogObject(nameof(response), response);



                // *****************************************************************
                logger.Debug("Attempting to check for success");
                logger.Inspect(nameof(response.Ok), response.Ok);
                if( response.Ok && typeof(TValue).IsValueType )
                    return Ok();
                else if( response.Ok )
                    return Ok( response.Value );


                return BuildErrorResult(response);


            }
            finally
            {
                logger.LeaveMethod();
            }


        }

        protected virtual IActionResult BuildResult( Response response )
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();


                logger.LogObject(nameof(response), response);



                // *****************************************************************
                logger.Debug("Attempting to check for success");
                logger.Inspect(nameof(response.Ok), response.Ok);
                if (response.Ok )
                    return Ok();


                return BuildErrorResult(response);


            }
            finally
            {
                logger.LeaveMethod();
            }


        }

        protected IActionResult BuildErrorResult(IExceptionInfo error)
        {


            var logger = GetLogger();

            try
            {

                logger.EnterMethod();


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
            finally
            {
                logger.LeaveMethod();
            }

        }



        protected virtual List<string> ProduceRql<TExplorer>() where TExplorer : class, IModel
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();


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




                var filters = new List<string>();
                if (rqls.Count > 0)
                    filters.AddRange(rqls);
                else
                {


                    // *****************************************************************
                    logger.Debug("Attempting to resolve criteria given target");
                    var type = Scope.ResolveOptionalNamed<Type>($"Rql.Criteria.Target:{typeof(TExplorer).FullName}");
                    if (type == null)
                        return filters;



                    // *****************************************************************
                    logger.Debug("Attempting to map parameters to criteria model");
                    var jo = JObject.FromObject(parameters);
                    var criteria = jo.ToObject(type);

                    logger.LogObject(nameof(criteria), criteria);



                    // *****************************************************************
                    logger.Debug("Attempting to introspect criteria RQL");
                    var filter = RqlFilterBuilder<TExplorer>.Create().Introspect(criteria);
                    var rql = filter.ToRqlCriteria();

                    logger.Inspect(nameof(rql), rql);



                    // *****************************************************************
                    filters.Add(rql);


                }


                return filters;

            }
            finally
            {
                logger.LeaveMethod();
            }


        }

        protected virtual List<IRqlFilter<TExplorer>> ProduceFilter<TExplorer>() where TExplorer : class, IModel
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();


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
                    logger.Debug("Attempting to resolve criteria given target");
                    var type = Scope.ResolveOptionalNamed<Type>($"Rql.Criteria.Target:{typeof(TExplorer).FullName}");
                    if (type == null)
                        return filters;



                    // *****************************************************************
                    logger.Debug("Attempting to map parameters to criteria model");
                    var jo = JObject.FromObject(parameters);
                    var criteria = jo.ToObject(type);

                    logger.LogObject(nameof(criteria), criteria);



                    // *****************************************************************
                    logger.Debug("Attempting to introspect criteria RQL");
                    var filter = RqlFilterBuilder<TExplorer>.Create().Introspect(criteria);



                    // *****************************************************************
                    filters.Add(filter);


                }


                return filters;

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        protected virtual List<string> ProduceRql<TExplorer,TCriteria>() where TExplorer : class, IModel where TCriteria: class
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();


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




                var filters = new List<string>();
                if (rqls.Count > 0)
                    filters.AddRange(rqls);
                else
                {


                    // *****************************************************************
                    logger.Debug("Attempting to map parameters to criteria model");
                    var jo = JObject.FromObject(parameters);
                    var criteria = jo.ToObject<TCriteria>();

                    logger.LogObject(nameof(criteria), criteria);



                    // *****************************************************************
                    logger.Debug("Attempting to introspect criteria RQL");
                    var filter = RqlFilterBuilder<TExplorer>.Create().Introspect(criteria);
                    var rql = filter.ToRqlCriteria();

                    logger.Inspect(nameof(rql), rql);



                    // *****************************************************************
                    filters.Add(rql);


                }


                return filters;

            }
            finally
            {
                logger.LeaveMethod();
            }


        }

        protected virtual List<IRqlFilter<TExplorer>> ProduceFilter<TExplorer, TCriteria>() where TExplorer : class, IModel where TCriteria : class
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();


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

                    logger.LogObject(nameof(criteria), criteria);



                    // *****************************************************************
                    logger.Debug("Attempting to introspect criteria RQL");
                    var filter = RqlFilterBuilder<TExplorer>.Create().Introspect(criteria);


                    // *****************************************************************
                    filters.Add(filter);


                }


                return filters;

            }
            finally
            {
                logger.LeaveMethod();
            }


        }



        protected virtual async Task<Dictionary<string, object>> FromBody()
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();


                // *****************************************************************
                logger.Debug("Attempting to parse request body");
                var jo = await JObject.LoadAsync(new JsonTextReader(new StreamReader(Request.Body)));
                var properties = jo.ToObject<Dictionary<string, object>>();



                // *****************************************************************
                return properties;

            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        protected virtual bool TryValidate<TModel>( Operation oper, Dictionary<string, object> properties, out IActionResult error) where TModel : class, IModel
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();


                // *****************************************************************
                logger.Debug("Attempting to get meta from given model");
                var meta = Meta.GetMetaFromType(typeof(TModel));



                // *****************************************************************
                logger.Debug("Attempting to check properties for over-posting");

                var overposted = oper == Operation.Create ? meta.CheckForCreate(properties.Keys) : meta.CheckForUpdate(properties.Keys);

                if (overposted.Count > 0)
                {
                    var erm = new ErrorResponseModel
                    {
                        CorrelationId = Correlation.Uid,
                        ErrorCode = "DisallowedProperties",
                        Explanation = $"The following properties were not found or are not mutable: ({string.Join(',', overposted)})"
                    };

                    error = new BadRequestObjectResult(erm);
                    return false;

                }


                // *****************************************************************
                error = null;

                return true;

            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        protected virtual async Task<IActionResult> Dispatch<TValue>([NotNull] IRequest<Response<TValue>> request )
        {

            if (request == null) throw new ArgumentNullException(nameof(request));

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.LogObject(nameof(request), request);



                // *****************************************************************
                logger.Debug("Attempting to send request via Mediator");
                var response = await TheMediator.Send(request);

                logger.Inspect(nameof(response.Ok), response.Ok);



                // *****************************************************************
                logger.Debug("Attempting to build result");
                var result = BuildResult(response);



                // *****************************************************************
                return result;


            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        protected virtual async Task<IActionResult> Dispatch([NotNull] IRequest<Response> request)
        {

            if (request == null) throw new ArgumentNullException(nameof(request));

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.LogObject(nameof(request), request);



                // *****************************************************************
                logger.Debug("Attempting to send request via Mediator");
                var response = await TheMediator.Send(request);

                logger.Inspect(nameof(response.Ok), response.Ok);



                // *****************************************************************
                logger.Debug("Attempting to build result");
                var result = BuildResult(response);



                // *****************************************************************
                return result;


            }
            finally
            {
                logger.LeaveMethod();
            }

        }




    }


}
