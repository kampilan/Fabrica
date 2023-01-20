﻿using Fabrica.Exceptions;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Support.Endpoints;

public abstract class BaseCreateFromDictEndpoint<TEntity>: BaseEndpoint where TEntity: class, IModel
{

    protected BaseCreateFromDictEndpoint(IEndpointComponent component) : base(component)
    {
    }

    protected virtual bool TryValidate( IDictionary<string, object>? delta, out IActionResult error )
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

        var ob = mm.CheckForCreate(delta.Keys);

        if (ob.Count > 0)
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


    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] Dictionary<string,object> delta)
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to validate delta model");
        if( !TryValidate(delta, out var error) )
            return error;



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = new CreateEntityRequest<TEntity>
        {
            Delta = delta
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