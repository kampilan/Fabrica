using System;
using System.Linq;
using AutoMapper;
using Fabrica.Models.Support;
using Fabrica.Persistence.Ef.Mediator.Handlers;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Work.Persistence.Contexts;
using Fabrica.Work.Persistence.Entities;

namespace Fabrica.Work.Mediator.Handlers;

public class QueryWorkTopicHandler : BaseQueryHandler<QueryEntityRequest<WorkTopic>, WorkTopic, ExplorerDbContext>
{


    public QueryWorkTopicHandler(ICorrelation correlation, IRuleSet rules, ExplorerDbContext context) : base(correlation, rules, context)
    {
    }

    protected override Func<ExplorerDbContext,IQueryable<WorkTopic>> Many => c => c.WorkTopics;


}

public class RetrieveWorkTopicHandler : BaseRetrieveHandler<RetrieveEntityRequest<WorkTopic>, WorkTopic, WorkDbContext>
{

    public RetrieveWorkTopicHandler(ICorrelation correlation, WorkDbContext context) : base(correlation, context)
    {
    }

    protected override Func<WorkDbContext, IQueryable<WorkTopic>> One => c => c.WorkTopics;

}

public class CreateWorkTopicHandler : BaseCreateHandler<CreateEntityRequest<WorkTopic>, WorkTopic, WorkDbContext>
{

    public CreateWorkTopicHandler(ICorrelation correlation, IModelMetaService meta, IUnitOfWork uow, WorkDbContext context,
        IMapper mapper) : base(correlation, meta, uow, context, mapper)
    {
    }

    protected override Func<WorkDbContext, IQueryable<WorkTopic>> One => c => c.WorkTopics;

}


public class UpdateWorkTopicHandler : BaseUpdateHandler<UpdateEntityRequest<WorkTopic>, WorkTopic, WorkDbContext>
{

    public UpdateWorkTopicHandler(ICorrelation correlation, IModelMetaService meta, IUnitOfWork uow, WorkDbContext context, IMapper mapper) : base(correlation, meta, uow, context, mapper)
    {
    }

    protected override Func<WorkDbContext, IQueryable<WorkTopic>> One => c => c.WorkTopics;

}

public class DeleteWorkTopicHandler : BaseDeleteHandler<DeleteEntityRequest<WorkTopic>, WorkTopic, WorkDbContext>
{

    public DeleteWorkTopicHandler(ICorrelation correlation, IUnitOfWork uow, WorkDbContext context) : base(correlation, uow,
        context)
    {
    }

    protected override Func<WorkDbContext, IQueryable<WorkTopic>> One => c => c.WorkTopics;

}

