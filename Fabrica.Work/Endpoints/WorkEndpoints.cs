using System.Collections.Generic;
using Fabrica.Api.Support.Endpoints;
using Fabrica.Api.Support.Models;
using Fabrica.Models;
using Fabrica.Work.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Work.Endpoints;

[ApiExplorerSettings(GroupName = "WorkTopics")]
[SwaggerResponse(200, "Success", typeof(List<WorkTopic>))]
[SwaggerResponse(400, "Bad Request", typeof(ErrorResponseModel))]
[Route("/api/worktopics")]
public class WorkTopicsQueryEndpoint : BaseQueryFromRqlEndpoint<Persistence.Entities.WorkTopic>
{
    
    public WorkTopicsQueryEndpoint(IEndpointComponent component) : base(component)
    {
    }

}

[ApiExplorerSettings(GroupName = "WorkTopics")]
[SwaggerResponse(200, "Success", typeof(WorkTopic))]
[SwaggerResponse(404, "Not Found", typeof(ErrorResponseModel))]
[Route("/api/worktopics")]
public class WorkTopicRetrieveEndpoint : BaseRetrieveByUidEndpoint<WorkTopic>
{

    public WorkTopicRetrieveEndpoint(IEndpointComponent component) : base(component)
    {
    }

}

[ApiExplorerSettings(GroupName = "WorkTopics")]
[SwaggerResponse(200, "Success", typeof(WorkTopic))]
[SwaggerResponse(422, "Not Found", typeof(ErrorResponseModel))]
[Route("/api/worktopics")]
public class WorkTopicCreateEndpoint : BaseCreateFromDeltaEndpoint<WorkTopic, WorkTopicDelta>
{
    public WorkTopicCreateEndpoint(IEndpointComponent component) : base(component)
    {
    }
}


[ApiExplorerSettings(GroupName = "WorkTopics")]
[SwaggerResponse(200, "Success", typeof(WorkTopic))]
[SwaggerResponse(404, "Not Found", typeof(ErrorResponseModel))]
[SwaggerResponse(422, "Not Found", typeof(ErrorResponseModel))]
[Route("/api/worktopics")]
public class WorkTopicUpdateEndpoint : BaseUpdateFromDeltaEndpoint<WorkTopic, WorkTopicDelta>
{
    public WorkTopicUpdateEndpoint(IEndpointComponent component) : base(component)
    {
    }
}

[ApiExplorerSettings(GroupName = "WorkTopics")]
[SwaggerResponse(200, "Success")]
[SwaggerResponse(404, "Not Found", typeof(ErrorResponseModel))]
[Route("/api/worktopics")]
public class WorkTopicDeleteEndpoint : BaseDeleteByUidEndpoint<WorkTopic>
{
    public WorkTopicDeleteEndpoint(IEndpointComponent component) : base(component)
    {
    }
}

[ApiExplorerSettings(IgnoreApi = true)]
[SwaggerResponse(200, "Success", typeof(WorkTopic))]
[SwaggerResponse(422, "Validation Errors", typeof(ErrorResponseModel))]
[Route("/api/worktopics")]
public class WorkTopicPatchEndpoint : BaseApplyPatchEndpoint<WorkTopic>
{

    public WorkTopicPatchEndpoint(IEndpointComponent component) : base(component)
    {
    }

}


[ApiExplorerSettings(GroupName = "WorkTopics")]
[SwaggerResponse(200, "Success", typeof(List<AuditJournalModel>))]
[Route("/api/worktopics")]
public class WorkTopicJournalEndpoint : BaseJournalByUidEndpoint<WorkTopic>
{
    public WorkTopicJournalEndpoint(IEndpointComponent component) : base(component)
    {
    }

}
