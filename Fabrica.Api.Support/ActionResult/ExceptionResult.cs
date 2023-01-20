
// ReSharper disable UnusedMember.Global

using Fabrica.Exceptions;

namespace Fabrica.Api.Support.ActionResult;

public class ExceptionResult: Microsoft.AspNetCore.Mvc.ActionResult, IExceptionInfo
{


    public ErrorKind Kind { get; set; } = ErrorKind.System;
    public string ErrorCode { get; set; } = "";
    public string Explanation { get; set; } = "";
    public List<EventDetail> Details { get; } = new();



    public void ForNotFound(string explanation)
    {
        Kind = ErrorKind.NotFound;
        ErrorCode = "NotFound";
        Explanation = explanation;
        Details.Clear();

    }


    public void ForBadRequest(string explanation)
    {

        Kind = ErrorKind.BadRequest;
        ErrorCode = "BadRequest";
        Explanation = explanation;
        Details.Clear();

    }


}