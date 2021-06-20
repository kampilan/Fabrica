﻿using Fabrica.Models.Support;

namespace Fabrica.Mediator.Requests
{

    public abstract class BaseMemberCreateRequest<TDelta>: IMemberCreateRequest where TDelta: BaseDelta, new()
    {

        public string ParentUid { get; set; } = "";

        public TDelta Delta { get; set; } = new ();

        BaseDelta IMutableRequest.Delta => Delta;

    }

}