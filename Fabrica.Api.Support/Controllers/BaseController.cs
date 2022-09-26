/*
The MIT License (MIT)

Copyright (c) 2021 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using Fabrica.Api.Support.ActionResult;
using Fabrica.Api.Support.Models;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Support.Controllers;

public abstract class BaseController: Controller
{


    protected BaseController( ICorrelation correlation  )
    {

        Correlation = correlation;

    }


    protected ICorrelation Correlation { get; }


    protected ILogger GetLogger()
    {

        var logger = Correlation.GetLogger(this);

        return logger;

    }

    protected ILogger EnterMethod( [CallerMemberName] string name="" )
    {

        var logger = Correlation.EnterMethod(GetType(), name);

        return logger;

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




}