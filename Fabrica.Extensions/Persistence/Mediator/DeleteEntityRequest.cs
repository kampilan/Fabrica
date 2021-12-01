﻿using Fabrica.Mediator;
using Fabrica.Models.Support;
using MediatR;


// ReSharper disable UnusedTypeParameter

namespace Fabrica.Persistence.Mediator;


public class DeleteEntityRequest<TEntity>: IDeleteEntityRequest, IRequest<Response> where TEntity: class, IModel
{
    
    public string Uid { get; set; } = "";


}