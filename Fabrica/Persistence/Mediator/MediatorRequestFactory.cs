using System;
using System.Collections.Generic;
using Fabrica.Utilities.Container;
using JetBrains.Annotations;

namespace Fabrica.Persistence.Mediator;

public class MediatorRequestFactory : CorrelatedObject, IMediatorRequestFactory
{


    public MediatorRequestFactory(ICorrelation correlation) : base(correlation)
    {
    }


    private object _makeRequest(Type request, params Type[] args)
    {

        var gen = request.MakeGenericType(args);
        var obj = Activator.CreateInstance(gen);

        return obj;

    }


    public virtual ICreateEntityRequest GetCreateRequest(Type entity, [NotNull] string uid, [NotNull] IDictionary<string, object> delta)
    {

        if (delta == null) throw new ArgumentNullException(nameof(delta));
        if (string.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(uid));

        using var logger = EnterMethod();

        var obj = _makeRequest(typeof(CreateEntityRequest<>), entity);
        if (obj is ICreateEntityRequest request)
        {

            request.Uid = uid;
            request.Delta = new Dictionary<string, object>(delta);

            return request;

        }

        throw new InvalidOperationException($"Request type: ({obj.GetType().Name}) does not implment ICreateEntityRequest");

    }

    public virtual ICreateMemberEntityRequest GetCreateMemberRequest(Type parent, [NotNull] string parentUid, Type member, [NotNull] string uid, [NotNull] IDictionary<string, object> delta)
    {

        if (delta == null) throw new ArgumentNullException(nameof(delta));
        if (string.IsNullOrWhiteSpace(parentUid)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(parentUid));
        if (string.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(uid));

        using var logger = EnterMethod();


        var obj = _makeRequest(typeof(CreateMemberEntityRequest<,>), parent, member);
        if (obj is ICreateMemberEntityRequest request)
        {

            request.ParentUid = parentUid;
            request.Uid = uid;
            request.Delta = new Dictionary<string, object>(delta);

            return request;

        }

        throw new InvalidOperationException($"Request type: ({obj.GetType().Name}) does not implment ICreateMemberEntityRequest");

    }


    public virtual IUpdateEntityRequest GetUpdateRequest(Type entity, [NotNull] string uid, [NotNull] IDictionary<string, object> delta)
    {

        if (delta == null) throw new ArgumentNullException(nameof(delta));
        if (string.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(uid));

        using var logger = EnterMethod();

        var obj = _makeRequest(typeof(UpdateEntityRequest<>), entity);
        if (obj is IUpdateEntityRequest request)
        {

            request.Uid = uid;
            request.Delta = new Dictionary<string, object>(delta);

            return request;

        }

        throw new InvalidOperationException($"Request type: ({obj.GetType().Name}) does not implment IUpdateEntityRequest");

    }



    public virtual IDeleteEntityRequest GetDeleteRequest(Type entity, [NotNull] string uid)
    {

        if (string.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(uid));

        using var logger = EnterMethod();


        var obj = _makeRequest(typeof(DeleteEntityRequest<>), entity);
        if (obj is IDeleteEntityRequest request)
        {
            request.Uid = uid;
            return request;
        }

        throw new InvalidOperationException($"Request type: ({obj.GetType().Name}) does not implement IDeleteEntityRequest");

    }


}
