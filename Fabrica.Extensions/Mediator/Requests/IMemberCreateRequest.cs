﻿namespace Fabrica.Mediator.Requests
{


    public interface IMemberCreateRequest: IDeltaRequest
    {

        string ParentUid { get; set; }


    }


}
