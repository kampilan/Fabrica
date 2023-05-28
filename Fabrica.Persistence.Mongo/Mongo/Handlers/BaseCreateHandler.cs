﻿using AutoMapper;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;
using MediatR;
using MongoDB.Driver;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Persistence.Mongo.Handlers;

public class BaseCreateHandler<TRequest,TResponse>: BaseHandler<TRequest, TResponse> where TRequest : class, ICreateEntityRequest, IRequest<Response<TResponse>> where TResponse : class, IModel, new ()
{


    public BaseCreateHandler( ICorrelation correlation, IMongoDbContext context, IMapper mapper ) : base(correlation)
    {

        Collection = context.GetCollection<TResponse>();
        Mapper = mapper;

    }


    private IMongoCollection<TResponse> Collection { get; }
    private IMapper Mapper { get; }


    protected override async Task<TResponse> Perform(CancellationToken cancellationToken = default)
    {


        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to Create Entity and map Delta");
        var entity = new TResponse();
        Mapper.Map( Request.Delta, entity );

        logger.LogObject(nameof(entity), entity);



        // *****************************************************************
        logger.Debug("Attempting to Insert Entity");
        await Collection.InsertOneAsync( entity, cancellationToken: cancellationToken );



        // *****************************************************************
        return entity;



    }


}